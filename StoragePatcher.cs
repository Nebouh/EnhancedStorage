using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

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
                {
                    MelonLogger.Error("[Enhanced Storage] StorageEntity type not found!");
                    return;
                }

                var harmony = new HarmonyLib.Harmony("com.enhancedstorage.patch");
                harmony.Patch(
                    storageEntityType.GetMethod("Awake", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
                    new HarmonyMethod(typeof(StoragePatcher), nameof(StorageEntityInitialize_Prefix))
                );

                MelonLogger.Msg("[Enhanced Storage] StorageEntity patched successfully.");
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
                if (__instance is not MonoBehaviour val)
                    return true;

                foreach (var entry in StorageSettings.RackConfigs)
                {
                    if (!val.gameObject.name.Contains(entry.Key))
                        continue;

                    int slots = Mathf.Min(entry.Value.Slots, StorageSettings.GameMaxSlots.Value);
                    int rows = entry.Value.Rows;

                    SetFieldOrProperty(__instance, "SlotCount", slots);
                    SetFieldOrProperty(__instance, "DisplayRowCount", rows);

                    var refreshMethod = __instance.GetType().GetMethod("SetupSlots", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    refreshMethod?.Invoke(__instance, null);

                    MelonLogger.Msg($"[Enhanced Storage] Patched {val.gameObject.name} (Slots: {slots}, Rows: {rows})");
                    break;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Enhanced Storage] Prefix error: {ex.Message}");
            }

            return true;
        }

        private static void SetFieldOrProperty(object target, string name, object value)
        {
            var type = storageEntityType;
            var prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null)
            {
                prop.SetValue(target, value);
                return;
            }

            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(target, value);
            }
            else
            {
                MelonLogger.Warning($"[Enhanced Storage] Field or property '{name}' not found on {type.Name}");
            }
        }

        [HarmonyPatch(typeof(Il2CppScheduleOne.UI.StorageMenu), "Awake")]
        private static class StorageMenuPatch
        {
            [HarmonyPrefix]
            private static void Prefix(Il2CppScheduleOne.UI.StorageMenu __instance)
            {
                try
                {
                    int maxSlots = StorageSettings.GameMaxSlots.Value;
                    if (maxSlots <= 20) return;

                    var slotsTransform = __instance.Container?.Find("Slots");
                    if (slotsTransform == null) return;

                    var slotsUIsList = new List<Il2CppScheduleOne.UI.ItemSlotUI>(__instance.SlotsUIs);

                    while (slotsTransform.childCount < maxSlots)
                    {
                        var firstChild = slotsTransform.GetChild(0).gameObject;
                        var clonedSlot = Object.Instantiate(firstChild, slotsTransform, true);
                        clonedSlot.name = clonedSlot.name.Replace("Clone", $"Extra-[{slotsTransform.childCount - 1}]");

                        clonedSlot.SetActive(true);
                        clonedSlot.transform.localScale = Vector3.one;

                        var itemSlotUI = clonedSlot.GetComponent<Il2CppScheduleOne.UI.ItemSlotUI>() ?? clonedSlot.AddComponent<Il2CppScheduleOne.UI.ItemSlotUI>();
                        slotsUIsList.Add(itemSlotUI);
                    }

                    __instance.SlotsUIs = slotsUIsList.ToArray();

                    MelonLogger.Msg($"[Enhanced Vehicules] StorageMenu slots expanded to {slotsTransform.childCount}.");
                }
                catch (System.Exception ex)
                {
                    MelonLogger.Error($"[Enhanced Vehicules] StorageMenu Patch error: {ex.Message}");
                }
            }
        }
    }
}
