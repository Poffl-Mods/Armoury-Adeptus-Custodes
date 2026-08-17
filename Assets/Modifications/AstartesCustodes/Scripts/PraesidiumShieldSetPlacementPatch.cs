using System.Collections.Generic;
using HarmonyLib;
using Kingmaker.Items;
using UnityEngine;

namespace AstartesCustodes.Runtime
{
    /// <summary>
    /// Vanilla mirrors every shield's secondary-hand slot into all weapon sets. Suppress that
    /// operation only when the source set contains our Praesidium Shield, allowing another set
    /// to retain a two-handed weapon without changing the behaviour of any vanilla shield.
    /// </summary>
    [HarmonyPatch(typeof(HandsEquipmentSet), nameof(HandsEquipmentSet.ClearForShieldInOtherSet))]
    internal static class PraesidiumShieldSetPlacementPatch
    {
        private static readonly HashSet<string> PraesidiumShieldGuids = new HashSet<string>
        {
            "e44e715029c9490ab7df80ad4366996b", "b269c2b1beef4958b6fb80512a9c84d9",
            "361f5576f230408b86f7232808b6e4b7", "945a4bb3c4b34454ba12db26cf025653",
            "c540d8eb059a42c8aacdd2a3eb540c8e", "36b54ba6bf994d2e9863c6a42b53cf1d"
        };
        private const string HarmonyId = "Poffl.AstartesCustodes.PraesidiumShieldSetPlacement";
        private static Harmony s_Harmony;
        private static bool s_Patched;

        internal static void SetEnabled(bool enabled)
        {
            if (enabled && !s_Patched)
            {
                s_Harmony ??= new Harmony(HarmonyId);
                s_Harmony.PatchAll(typeof(PraesidiumShieldSetPlacementPatch).Assembly);
                s_Patched = true;
                Debug.Log("[AstartesCustodes][PraesidiumShield] Set-local shield patch applied.");
            }
            else if (!enabled && s_Patched)
            {
                s_Harmony?.UnpatchAll(HarmonyId);
                s_Patched = false;
            }
        }

        private static bool Prefix(HandsEquipmentSet setWithShield)
        {
            ItemEntity shield = setWithShield?.SecondaryHand?.MaybeItem;
            bool isPraesidiumShield = IsPraesidiumShield(shield);
            if (isPraesidiumShield)
            {
                Debug.Log("[AstartesCustodes][PraesidiumShield] Prevented cross-set offhand override.");
            }

            return !isPraesidiumShield;
        }

        internal static void ClearStaleOverrides(PartUnitBody body)
        {
            if (body?.HandsEquipmentSets == null) return;

            foreach (HandsEquipmentSet set in body.HandsEquipmentSets)
            {
                if (set == null || !set.IsOverrideSecondaryHand) continue;

                // Vanilla can restore its cross-set shield override before modification patches
                // become active during save loading. Such an override makes a two-handed weapon
                // appear to share its set with the shield until the shield is re-equipped.
                var overriddenSlot = set.SecondaryHand;
                if (!IsPraesidiumShield(overriddenSlot?.MaybeItem)) continue;

                set.OverrideSecondaryHand(null);
                overriddenSlot.IsDirty = true;
                if (set.SecondaryHand != null) set.SecondaryHand.IsDirty = true;
                Debug.Log("[AstartesCustodes][PraesidiumShield] Removed stale cross-set shield override after loading.");
            }
        }

        private static bool IsPraesidiumShield(ItemEntity item)
        {
            string guid = item?.Blueprint?.AssetGuid.ToString();
            return guid != null && PraesidiumShieldGuids.Contains(guid);
        }
    }
}
