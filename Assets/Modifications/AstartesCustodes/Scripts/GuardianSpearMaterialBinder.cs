using UnityEngine;

namespace AstartesCustodes.Runtime
{
    /// <summary>
    /// Owlcat's official mod build deliberately strips every shader program from mod bundles.
    /// Rebind the material assets to the game's already-loaded Owlcat/Lit shader at runtime.
    /// This component is visual-only and carries no gameplay or Harmony logic.
    /// </summary>
    public sealed class GuardianSpearMaterialBinder : MonoBehaviour
    {
        private void Awake() => BindGameShader();
        private void OnEnable() => BindGameShader();

        private void BindGameShader()
        {
            Shader shader = Shader.Find("Owlcat/Lit");
            if (shader == null) return;

            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.materials;
                bool changed = false;
                foreach (Material material in materials)
                {
                    if (material == null || material.shader == shader) continue;
                    material.shader = shader;
                    changed = true;
                }
                if (changed) renderer.materials = materials;
            }

            if (name.IndexOf("SentinelSword", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string details = renderers.Length == 0
                    ? "no renderers found"
                    : string.Join(", ", System.Array.ConvertAll(renderers, renderer =>
                        renderer.name + " active=" + renderer.gameObject.activeInHierarchy +
                        " enabled=" + renderer.enabled +
                        " bounds=" + renderer.bounds.size));
                Debug.Log("[AstartesCustodes][SentinelSword] Visual instantiated: " + details);
            }
        }
    }
}
