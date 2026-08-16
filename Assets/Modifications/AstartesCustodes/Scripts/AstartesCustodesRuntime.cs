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
            GuardianSpearProgressionController.EnsureController();
            modification.OnSetEnabled += enabled =>
            {
                if (enabled) GuardianSpearProgressionController.EnsureController();
            };
            Debug.Log("[AstartesCustodes] Runtime initialized.");
        }
    }
}
