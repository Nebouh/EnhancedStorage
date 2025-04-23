using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Il2CppTMPro;
using MelonLoader;

namespace EnhancedStorageMod
{
    internal static class StorageUIPatch
    {
        public static void PatchStorageMenu()
        {
            try
            {
                var storageMenuType = Type.GetType("Il2CppScheduleOne.UI.StorageMenu, Assembly-CSharp");
                if (storageMenuType == null)
                    throw new Exception("StorageMenu type not found");

                var openMethod = storageMenuType
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(m =>
                        m.Name == "Open" &&
                        m.GetParameters().Length == 1 &&
                        m.GetParameters()[0].ParameterType.Name.Contains("StorageEntity"));

                var closeMethod = storageMenuType.GetMethod("Close", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                var postfixOpen = typeof(StorageUIPatch).GetMethod(nameof(OnStorageMenuOpen), BindingFlags.Static | BindingFlags.NonPublic);
                var prefixClose = typeof(StorageUIPatch).GetMethod(nameof(OnStorageMenuClose), BindingFlags.Static | BindingFlags.NonPublic);

                var harmony = new HarmonyLib.Harmony("com.mod.enhancedstorage.ui");
                harmony.Patch(openMethod, postfix: new HarmonyMethod(postfixOpen));
                harmony.Patch(closeMethod, prefix: new HarmonyMethod(prefixClose));
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Enhanced Storage] UI Patch error: {ex.Message}");
            }
        }

        private static void OnStorageMenuOpen(object __instance)
        {
            try
            {
                if (GameObject.Find("SortAZButton") != null)
                    return;

                var go = (__instance as MonoBehaviour)?.gameObject;
                if (go == null) return;

                var prop = __instance.GetType().GetProperty("CloseButton", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                var closeButton = prop?.GetValue(__instance) as RectTransform;
                if (closeButton == null) return;

                var sortButton = GameObject.Instantiate(closeButton.gameObject, closeButton.parent);
                sortButton.name = "SortAZButton";

                var rect = sortButton.GetComponent<RectTransform>();
                rect.anchoredPosition += new Vector2(-rect.sizeDelta.x - 10f, 0);

                var tmpText = sortButton.GetComponentInChildren<TextMeshProUGUI>();
                if (tmpText != null)
                    tmpText.text = "SORT";

                var button = sortButton.GetComponent<Button>();
                button.onClick.RemoveAllListeners();

                var storageEntityField = __instance.GetType().GetField("storage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                var capturedStorageEntity = storageEntityField?.GetValue(__instance);
                button.onClick.AddListener(() => StorageSorter.SortAlphabetically(capturedStorageEntity));
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Enhanced Storage] Error creating sort button: {ex.Message}");
            }
        }

        private static void OnStorageMenuClose()
        {
            var existing = GameObject.Find("SortAZButton");
            if (existing != null)
            {
                GameObject.Destroy(existing);
            }
        }
    }
}
