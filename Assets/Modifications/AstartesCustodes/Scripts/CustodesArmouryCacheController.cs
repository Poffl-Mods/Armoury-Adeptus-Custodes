using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace AstartesCustodes.Runtime
{
    internal sealed class CustodesArmouryCacheController : MonoBehaviour
    {
        private const string TargetAreaGuid = "8a2d1ed55f694366b2d512e122bd19a7";
        private const string CacheBlueprintGuid = "88f437341af34d15ad9d9e24c41dd34d";
        private static readonly Vector3 CachePosition = new Vector3(116.304f, 2.419f, -203.103f);
        private static readonly Quaternion CacheRotation = Quaternion.Euler(0f, 35f, 0f);
        private static readonly string[] TierOneItemGuids =
        {
            "69a10b7bc7a94c5cb59cd91a6d88d160", "94d7497e0b1941c1910a6b29ed8911c2",
            "e44e715029c9490ab7df80ad4366996b", "107fbf6e543d41c9ab5783dd7f761be6"
        };
        private float m_NextCheck;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        internal static void EnsureController()
        {
            if (FindFirstObjectByType<CustodesArmouryCacheController>() != null) return;
            var host = new GameObject("AstartesCustodes_ArmouryCacheController");
            DontDestroyOnLoad(host);
            host.AddComponent<CustodesArmouryCacheController>();
        }

        private void Update()
        {
            if (Time.unscaledTime >= m_NextCheck)
            {
                m_NextCheck = Time.unscaledTime + 1f;
                TrySpawnCache();
            }
        }

        private static void TrySpawnCache()
        {
            Game game = Game.Instance;
            AreaPersistentState areaState = game?.LoadedAreaState;
            if (game?.CurrentlyLoadedArea == null || areaState == null ||
                !string.Equals(game.CurrentlyLoadedArea.AssetGuid.ToString(), TargetAreaGuid, StringComparison.OrdinalIgnoreCase)) return;
            BlueprintDynamicMapObject blueprint = ResourcesLibrary.TryGetBlueprint<BlueprintDynamicMapObject>(CacheBlueprintGuid);
            if (blueprint == null) return;
            bool alreadyExists = areaState.AllEntityData.OfType<DynamicMapObjectView.EntityData>()
                .Any(entity => entity.Blueprint != null && entity.Blueprint.AssetGuid == blueprint.AssetGuid);
            if (alreadyExists) return;
            SceneEntitiesState state = FindLoadedSceneState(areaState) ?? areaState.MainState;
            DynamicMapObjectView.EntityData entity = game.EntitySpawner.SpawnMapObject(blueprint, CachePosition, CacheRotation, state);
            InteractionLootPart loot = entity?.GetOptional<InteractionLootPart>();
            if (loot == null)
            {
                Debug.LogError("[AstartesCustodes] Native armoury cache spawned without InteractionLootPart.");
                return;
            }
            foreach (string guid in TierOneItemGuids)
            {
                BlueprintItem item = ResourcesLibrary.TryGetBlueprint<BlueprintItem>(guid);
                if (item != null) loot.Loot.Add(item);
            }
            Debug.Log("[AstartesCustodes] Native persistent armoury cache spawned and filled in the Warrant Chamber.");
        }

        private static SceneEntitiesState FindLoadedSceneState(AreaPersistentState areaState)
        {
            foreach (SceneEntitiesState state in areaState.GetAllSceneStates())
                if (state != null && state.IsSceneLoaded && state.SceneName.StartsWith("VoidshipOfficersDeck", StringComparison.OrdinalIgnoreCase))
                    return state;
            return null;
        }

    }
}
