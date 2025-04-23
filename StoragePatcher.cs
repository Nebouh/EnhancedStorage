using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace EnhancedStorageMod
{
    internal static class StoragePatcher
    {
        private static Type storageEntityType;

        public static void PatchStorage()
        {
            try
            {
                storageEntityType = Type.GetType("Il2CppScheduleOne.Storage.StorageEntity, Assembly-CSharp");
                if (storageEntityType == null)
                    throw new Exception("StorageEntity type not found");

                var method = storageEntityType.GetMethod("Awake", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var prefix = typeof(StoragePatcher).GetMethod(nameof(StorageEntityInitialize_Prefix), BindingFlags.Static | BindingFlags.NonPublic);
                new HarmonyLib.Harmony("com.mod.enhancedstorage").Patch(method, new HarmonyMethod(prefix));
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Enhanced Storage] Patch error: {ex.Message}");
            }
        }

        private static bool StorageEntityInitialize_Prefix(object __instance)
        {
            try
            {
                if (__instance is not MonoBehaviour val) return true;

                foreach (var entry in StorageSettings.RackConfigs)
                {
                    if (!val.gameObject.name.Contains(entry.Key)) continue;

                    int slots = Mathf.Min(entry.Value.Slots, StorageSettings.GameMaxSlots.Value);
                    int rows = entry.Value.Rows;

                    SetFieldOrProperty(__instance, "SlotCount", slots);
                    SetFieldOrProperty(__instance, "DisplayRowCount", rows);
                    break;
                }

                return true;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Enhanced Storage] Prefix error: {ex.Message}");
                return true;
            }
        }

        private static void SetFieldOrProperty(object target, string name, object value)
        {
            var prop = storageEntityType.GetProperty(name);
            if (prop != null) { prop.SetValue(target, value); return; }

            var field = storageEntityType.GetField(name);
            field?.SetValue(target, value);
        }
    }
}
