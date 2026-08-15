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

            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
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
        }
    }
}
