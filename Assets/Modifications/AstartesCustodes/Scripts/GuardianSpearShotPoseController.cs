using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
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
        private const string BoltShotGuid = "747e419a3f9c43579f51b27f41e88b35";
        private static readonly Quaternion IdleRotation = Quaternion.Euler(0f, 0f, 45f);
        // Z lowers the upright staff into the rifle pose. The additional Y quarter-turn aligns
        // the spear's longitudinal axis with the rifle animation's forward/target axis.
        private static readonly Quaternion ShotRotation = Quaternion.Euler(0f, -90f, -45f);

        private Transform m_PoseRoot;
        private bool m_ShotActive;
        private float m_ReturnDelay;
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

        public void HandleExecutionProcessStart(AbilityExecutionContext context)
        {
            if (!IsOurBoltShot(context)) return;
            m_ShotActive = true;
            m_ReturnDelay = 0f;
            // Execution start is already close to the actual delivery frame. Snap here so the
            // model and its child muzzle locator are aligned before projectile/VFX emission.
            if (m_PoseRoot != null) m_PoseRoot.localRotation = ShotRotation;
        }

        public void HandleExecutionProcessEnd(AbilityExecutionContext context)
        {
            if (!IsOurBoltShot(context)) return;
            m_ShotActive = false;
            m_ReturnDelay = 0.45f;
        }

        private bool IsOurBoltShot(AbilityExecutionContext context)
        {
            return context?.AbilityBlueprint != null && context.AbilityBlueprint.AssetGuid.ToString() == BoltShotGuid;
        }
    }
}
