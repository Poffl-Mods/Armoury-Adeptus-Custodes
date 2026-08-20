using HarmonyLib;
using Kingmaker.Modding;
using UnityEngine;

namespace AstartesCustodes.Runtime
{
    /// <summary>Guaranteed runtime entry point invoked by Owlcat's modification loader.</summary>
    public static class AstartesCustodesRuntime
    {
        [OwlcatModificationEnterPoint]
        public static void Initialize(OwlcatModification modification)
        {
            PraesidiumShieldSetPlacementPatch.SetEnabled(true);
            GuardianSpearProgressionController.EnsureController();
            CustodesArmouryCacheController.EnsureController();
            modification.OnSetEnabled += enabled =>
            {
                PraesidiumShieldSetPlacementPatch.SetEnabled(enabled);
                if (enabled)
                {
                    GuardianSpearProgressionController.EnsureController();
                    CustodesArmouryCacheController.EnsureController();
                }
            };
            Debug.Log("[AstartesCustodes] Runtime initialized.");
        }
    }
}
