using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(EnhancedStorageMod.EnhancedStorage), "Enhanced Storage", "0.2.0", "Nebouh")]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace EnhancedStorageMod
{
    public class EnhancedStorage : MelonMod
    {
        public override void OnInitializeMelon()
        {
            if (!StorageSettings.LoadPreferences()) return;

            StorageSettings.InitializeSettings();

            StoragePatcher.PatchStorage();
            StorageUIPatch.PatchStorageMenu();

            MelonLogger.Msg("[Enhanced Storage] Successfully initialized.");
        }
    }
}
