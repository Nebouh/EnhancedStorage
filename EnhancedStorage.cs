using System.Collections;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: MelonInfo(typeof(EnhancedStorageMod.EnhancedStorage), "Enhanced Storage", "1.1.6", "Nebouh")]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace EnhancedStorageMod
{
    public class EnhancedStorage : MelonMod
    {
        private static bool isPatched = false;
        public override void OnInitializeMelon()
        {
            if (!StorageSettings.LoadPreferences()) return;

            StorageSettings.InitializeSettings();

            StoragePatcher.PatchStorage();

            MelonLogger.Msg("[Enhanced Storage] Successfully initialized.");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!scene.name.Equals("Main", StringComparison.OrdinalIgnoreCase))
                return;
            if (isPatched) return;
            MelonCoroutines.Start(DelayedAfterLoad());

            MelonLogger.Msg($"[Enhanced Vehicules] Scene loaded: {scene.name}, now patching...");
            StoragePatcher.PatchStorage();

            isPatched = true;
        }
        private IEnumerator DelayedAfterLoad()
        {
            yield return new WaitForSeconds(2f);
        }
    }
}
