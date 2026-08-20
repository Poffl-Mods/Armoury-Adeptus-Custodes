using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AstartesCustodes.Runtime
{
    /// <summary>
    /// Visual-only pose bridge for the hybrid weapon. The gameplay ability remains a normal single
    /// shot; only the prefab visual and its muzzle locator rotate during this specific ability.
    /// </summary>
    public sealed class GuardianSpearShotPoseController : MonoBehaviour, IAbilityExecutionProcessHandler
    {
        private static readonly HashSet<string> GuardianSpearGuids = new HashSet<string>
        {
            "107fbf6e543d41c9ab5783dd7f761be6",
            "0ea483280c3c4f68b2894debf9f5930c", "53fd9030471a47ec832b7f3a9bf5d889",
            "b81b2c7c002e44dc8a6a711c82fdb2d7", "3b5e0fc217f441ed815b70dcdffdc72a",
            "c2e60e63e79749a9a71fcbff8db63d59",
            "69a10b7bc7a94c5cb59cd91a6d88d160", "57fe8f5911961a2f7520902d00a08ade",
            "daa6cb6d22409135822d23800e71f1ea", "db7158b0a3ab06fef59f60cb255d3b20",
            "6e3e198f6446b1a9de3fea683517ff69", "bdb166b828685110dff677d0791a0bda"
        };
        private static readonly Dictionary<string, int> AmmoCosts = new Dictionary<string, int>
        {
            ["747e419a3f9c43579f51b27f41e88b35"] = 1,
            ["5986262441d6940a7ba74e7199b17222"] = 1,
            ["728e8542d63b078a690053d7bf9805f7"] = 1,
            ["a6c669d9e59ac497dbdd5d87f4f1c5c1"] = 1,
            ["f8f4c3ee58372fb4ac8c365577b43af9"] = 1,
            ["e2d02670d099362458097355bf04adb4"] = 1,
            ["c4e65f9139c74d54bc08a87caf2bb381"] = 3,
            ["c7c3a20b3589b3c6fe4829a670e1a917"] = 4,
            ["e3b3153d60971a366702b2cd235743b1"] = 5,
            ["bd48e166afa8ec7d8747d67ce9f4ba90"] = 6,
            ["c4636e1c3018bbaf41f8b82d32b37593"] = 8,
            ["b277a41d4a5beb42441122718a5816bc"] = 9
        };
        private static readonly HashSet<string> AmmoFreeBolterAbilities = new HashSet<string>
        {
            "67b07b4445b348168d63903592a5ee50", "b78d52e76e84478f8868bef1e882cf6f",
            "3ae119d574d54650a27e75904c81e1de", "841570aa8b6249bba3dbd46175d4946b",
            "1462ce0843384098a4fb2c3f5a4246c9", "5635cd61d63541258a977ffeb7134026"
        };
        private static readonly HashSet<AbilityExecutionContext> AmmoSpentContexts = new HashSet<AbilityExecutionContext>();
        private static readonly Quaternion IdleRotation = Quaternion.Euler(0f, 0f, 45f);
        // Z lowers the upright staff into the rifle pose. The additional Y quarter-turn aligns
        // the spear's longitudinal axis with the rifle animation's forward/target axis.
        private static readonly Quaternion ShotRotation = Quaternion.Euler(0f, -90f, -45f);

        private Transform m_PoseRoot;
        private bool m_ShotActive;
        private float m_ReturnDelay;
        private bool m_BackAlignmentApplied;
        private static readonly MethodInfo SubscribeGlobalMethod = typeof(EventBus).GetMethod(
            "SubscribeGlobal", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo UnsubscribeGlobalMethod = typeof(EventBus).GetMethod(
            "UnsubscribeGlobal", BindingFlags.Static | BindingFlags.NonPublic);

        private void Awake()
        {
            m_PoseRoot = transform.Find("GuardianSpear_Visual");
            if (m_PoseRoot != null) m_PoseRoot.localRotation = IdleRotation;
        }

        private void OnEnable()
        {
            // Ability execution events are raised on an entity-scoped bus. A weapon prefab has no
            // subscription entity of its own, so listen globally and filter by our unique ability GUID.
            SubscribeGlobalMethod?.Invoke(null, new object[] { this, null });
        }

        private void OnDisable()
        {
            UnsubscribeGlobalMethod?.Invoke(null, new object[] { this, null });
            m_ShotActive = false;
            m_ReturnDelay = 0f;
            m_BackAlignmentApplied = false;
            if (m_PoseRoot != null) m_PoseRoot.localRotation = IdleRotation;
        }

        private void Update()
        {
            if (m_PoseRoot == null) return;
            if (!m_ShotActive && m_ReturnDelay > 0f) m_ReturnDelay -= Time.deltaTime;
            Quaternion target = m_ShotActive || m_ReturnDelay > 0f ? ShotRotation : IdleRotation;
            m_PoseRoot.localRotation = Quaternion.RotateTowards(
                m_PoseRoot.localRotation, target, 540f * Time.deltaTime);
        }

        private void LateUpdate()
        {
            TryAlignSheathedModelToBack();
        }

        private void TryAlignSheathedModelToBack()
        {
            if (m_BackAlignmentApplied) return;
            Transform parent = transform.parent;
            if (parent == null || parent.name.IndexOf("back_weapon_slot", System.StringComparison.OrdinalIgnoreCase) < 0)
            {
                m_BackAlignmentApplied = false;
                return;
            }

            Transform spine = FindAncestor(parent, "Spine_3");
            if (spine == null) return;

            Vector3 fromSpine = transform.position - spine.position;
            Vector2 horizontal = new Vector2(fromSpine.x, fromSpine.z);
            if (horizontal.sqrMagnitude < 0.0001f) return;

            const float distanceFromSpine = 0.16f;
            Vector2 direction = horizontal.normalized;
            Vector3 target = new Vector3(
                spine.position.x + direction.x * distanceFromSpine,
                transform.position.y,
                spine.position.z + direction.y * distanceFromSpine);
            if (name.IndexOf("CastellanAxe", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // Move across the back, not away from it. This horizontal tangent is perpendicular
                // to the already-known spine-to-weapon radial direction and is independent of the
                // rig's unusually oriented Spine_3 local axes.
                Vector3 leftAcrossBack = new Vector3(direction.y, 0f, -direction.x);
                target += leftAcrossBack * 0.25f;
            }
            transform.position = target;
            m_BackAlignmentApplied = true;
        }

        private static Transform FindAncestor(Transform current, string ancestorName)
        {
            while (current != null)
            {
                if (current.name == ancestorName) return current;
                current = current.parent;
            }
            return null;
        }

        public void HandleExecutionProcessStart(AbilityExecutionContext context)
        {
            if (!IsOurBolterAttack(context)) return;
            SpendAmmoOnce(context);
            m_ShotActive = true;
            m_ReturnDelay = 0f;
            // Execution start is already close to the actual delivery frame. Snap here so the
            // model and its child muzzle locator are aligned before projectile/VFX emission.
            if (m_PoseRoot != null) m_PoseRoot.localRotation = ShotRotation;
        }

        public void HandleExecutionProcessEnd(AbilityExecutionContext context)
        {
            if (!IsOurBolterAttack(context)) return;
            AmmoSpentContexts.Remove(context);
            m_ShotActive = false;
            m_ReturnDelay = 0.45f;
        }

        private bool IsOurBolterAttack(AbilityExecutionContext context)
        {
            if (context?.AbilityBlueprint == null) return false;
            string guid = context.AbilityBlueprint.AssetGuid.ToString();
            return AmmoCosts.ContainsKey(guid) || AmmoFreeBolterAbilities.Contains(guid);
        }

        private static void SpendAmmoOnce(AbilityExecutionContext context)
        {
            if (!AmmoSpentContexts.Add(context)) return;
            var sourceWeapon = context.Ability?.SourceWeapon;
            if (sourceWeapon?.Blueprint == null || !GuardianSpearGuids.Contains(sourceWeapon.Blueprint.AssetGuid.ToString())) return;
            if (!AmmoCosts.TryGetValue(context.AbilityBlueprint.AssetGuid.ToString(), out int cost)) return;
            sourceWeapon.CurrentAmmo = Mathf.Max(0, sourceWeapon.CurrentAmmo - cost);
        }
    }
}
