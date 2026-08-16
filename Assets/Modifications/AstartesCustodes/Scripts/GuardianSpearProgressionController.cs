using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using UnityEngine;

namespace AstartesCustodes.Runtime
{
    /// <summary>
    /// Replaces Guardian Spear tiers according to party level. Replacement is intentionally deferred
    /// during combat and is also checked periodically so old saves and inventory transfers self-heal.
    /// </summary>
    public sealed class GuardianSpearProgressionController : MonoBehaviour,
        ILevelUpCompleteUIHandler, IItemsCollectionHandler, IPartyCombatHandler
    {
        private static readonly string[] VisibleWeaponGuids =
        {
            "69a10b7bc7a94c5cb59cd91a6d88d160", "57fe8f5911961a2f7520902d00a08ade",
            "daa6cb6d22409135822d23800e71f1ea", "db7158b0a3ab06fef59f60cb255d3b20",
            "6e3e198f6446b1a9de3fea683517ff69", "bdb166b828685110dff677d0791a0bda"
        };

        private static readonly MethodInfo SubscribeGlobalMethod = typeof(EventBus).GetMethod(
            "SubscribeGlobal", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo UnsubscribeGlobalMethod = typeof(EventBus).GetMethod(
            "UnsubscribeGlobal", BindingFlags.Static | BindingFlags.NonPublic);
        private readonly HashSet<ItemsCollection> m_KnownCollections = new HashSet<ItemsCollection>();
        private float m_NextCheck;
        private bool m_ChangingItems;
        private bool m_DeferredByCombat;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void EnsureController()
        {
            if (FindFirstObjectByType<GuardianSpearProgressionController>() != null) return;
            GameObject host = new GameObject("AstartesCustodes_GuardianSpearProgression");
            DontDestroyOnLoad(host);
            host.AddComponent<GuardianSpearProgressionController>();
        }

        private void OnEnable() => SubscribeGlobalMethod?.Invoke(null, new object[] { this, null });

        private void OnDisable() => UnsubscribeGlobalMethod?.Invoke(null, new object[] { this, null });

        private void Update()
        {
            if (Time.unscaledTime < m_NextCheck) return;
            m_NextCheck = Time.unscaledTime + 1f;
            RememberPlayerCollections();
            TryUpgradeAll();
        }

        public void HandleLevelUpComplete(bool isChargen) => TryUpgradeAll();

        public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
        {
            if (collection != null) m_KnownCollections.Add(collection);
            if (!m_ChangingItems) TryUpgradeAll();
        }

        public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
        {
            if (collection != null) m_KnownCollections.Add(collection);
        }

        public void HandlePartyCombatStateChanged(bool inCombat)
        {
            if (inCombat) return;
            if (m_DeferredByCombat) TryUpgradeAll();
        }

        private void RememberPlayerCollections()
        {
            Player player = Game.Instance?.Player;
            if (player == null) return;
            if (player.Inventory != null) m_KnownCollections.Add(player.Inventory);
            if (player.SharedStash != null) m_KnownCollections.Add(player.SharedStash);
        }

        private void TryUpgradeAll()
        {
            Player player = Game.Instance?.Player;
            if (player == null || m_ChangingItems) return;
            if (IsCombatActive(player))
            {
                m_DeferredByCombat = true;
                return;
            }

            m_DeferredByCombat = false;
            int targetTier = TierForLevel(player.PartyLevel);
            m_ChangingItems = true;
            try
            {
                foreach (ItemsCollection collection in m_KnownCollections.Where(item => item != null).ToArray())
                foreach (ItemEntity item in collection.Items.ToArray())
                    TryUpgradeItem(item, targetTier);
                foreach (BaseUnitEntity unit in EnumeratePartyUnits(player))
                {
                    if (unit.Inventory?.Collection != null) m_KnownCollections.Add(unit.Inventory.Collection);
                    PropertyInfo bodyProperty = unit.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .FirstOrDefault(property => typeof(PartUnitBody).IsAssignableFrom(property.PropertyType));
                    if (bodyProperty?.GetValue(unit) is PartUnitBody body)
                    foreach (ItemEntity item in body.Items.ToArray())
                        TryUpgradeItem(item, targetTier);
                }
            }
            finally { m_ChangingItems = false; }
        }

        private static IEnumerable<BaseUnitEntity> EnumeratePartyUnits(Player player)
        {
            var result = new HashSet<BaseUnitEntity>();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (PropertyInfo property in player.GetType().GetProperties(flags)
                .Where(property => property.Name.IndexOf("Party", StringComparison.OrdinalIgnoreCase) >= 0 && property.GetIndexParameters().Length == 0))
            {
                object value;
                try { value = property.GetValue(player); } catch { continue; }
                AddUnits(value, result);
            }
            foreach (FieldInfo field in player.GetType().GetFields(flags)
                .Where(field => field.Name.IndexOf("Party", StringComparison.OrdinalIgnoreCase) >= 0))
                AddUnits(field.GetValue(player), result);
            return result;
        }

        private static bool IsCombatActive(Player player)
        {
            if (player.IsInCombat) return true;
            // Player.IsInCombat can lag behind the unit combat state around attack startup.
            // Never replace an equipped weapon while any party member is actively fighting.
            return EnumeratePartyUnits(player).Any(unit => unit != null && unit.IsInCombat);
        }

        private static void AddUnits(object value, HashSet<BaseUnitEntity> result)
        {
            if (value is BaseUnitEntity unit) result.Add(unit);
            else if (value is IEnumerable enumerable)
            foreach (object item in enumerable)
                if (item is BaseUnitEntity partyUnit) result.Add(partyUnit);
        }

        private void TryUpgradeItem(ItemEntity item, int targetTier)
        {
            if (item?.Blueprint == null) return;
            int currentTier = Array.IndexOf(VisibleWeaponGuids, item.Blueprint.AssetGuid.ToString());
            if (currentTier < 0 || currentTier >= targetTier) return;

            BlueprintItemWeapon target = ResourcesLibrary.TryGetBlueprint(
                VisibleWeaponGuids[targetTier]) as BlueprintItemWeapon;
            if (target == null)
            {
                Debug.LogError("[AstartesCustodes] Guardian Spear tier blueprint was not found: " + VisibleWeaponGuids[targetTier]);
                return;
            }

            var slot = item.HoldingSlot;
            ItemsCollection collection = item.Collection ?? slot?.MaybeOwnerInventory?.Collection;
            if (collection == null) return;

            if (slot != null && !slot.RemoveItem(false, false)) return;
            if (item.Collection != null) item.Collection.Remove(item);
            ItemEntity replacement = collection.Add(target);
            if (slot != null) slot.InsertItem(replacement, false);
            Debug.Log($"[AstartesCustodes] Guardian Spear upgraded from V{currentTier + 1} to V{targetTier + 1} at party level {Game.Instance.Player.PartyLevel}.");
        }

        private static int TierForLevel(int level)
        {
            if (level >= 50) return 5;
            if (level >= 40) return 4;
            if (level >= 30) return 3;
            if (level >= 20) return 2;
            if (level >= 10) return 1;
            return 0;
        }
    }
}
