using UnityEngine;

namespace AstartesCustodes.Runtime
{
    internal sealed class PraesidiumShieldBackPositionController : MonoBehaviour
    {
        private static readonly Quaternion HolsteredRotation = Quaternion.Euler(0f, 0f, 45f);
        private bool m_BackPositionApplied;

        private void OnEnable()
        {
            m_BackPositionApplied = false;
        }

        private void LateUpdate()
        {
            if (m_BackPositionApplied) return;

            // Slot 10 applies this rotation only while the shield is holstered. The drawn
            // off-hand state uses m_OffHand instead and remains at the prefab's zero rotation.
            if (Quaternion.Angle(transform.localRotation, HolsteredRotation) > 2f)
            {
                m_BackPositionApplied = false;
                return;
            }

            Transform spine = FindAncestor(transform.parent, "Spine_3");
            if (spine == null) return;

            Vector3 fromSpine = transform.position - spine.position;
            Vector2 horizontal = new Vector2(fromSpine.x, fromSpine.z);
            if (horizontal.sqrMagnitude < 0.0001f) return;

            // The spear uses 0.16 m. The shield is substantially thicker and needs extra
            // clearance so its rear face does not intersect the torso or head.
            const float distanceFromSpine = 0.24f;
            Vector2 direction = horizontal.normalized;
            transform.position = new Vector3(
                spine.position.x + direction.x * distanceFromSpine,
                transform.position.y,
                spine.position.z + direction.y * distanceFromSpine);
            m_BackPositionApplied = true;
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
    }
}
