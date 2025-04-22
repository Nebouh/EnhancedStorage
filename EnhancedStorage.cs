using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using EnhancedStorageMod;

[assembly: MelonInfo(typeof(EnhancedStorage), "Enhanced Storage", "0.2.0", "Nebouh")]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace EnhancedStorageMod;

public class EnhancedStorage : MelonMod
{
    private static MelonPreferences_Category category;
    private static MelonPreferences_Entry<bool> prefEnableMod;
    private static MelonPreferences_Entry<bool> prefUseDefaultSettings;
    private static MelonPreferences_Entry<int> prefGameMaxSlots;

    private static Type storageEntityType;

    private struct RackSettings
    {
        public string Key;
        public string Label;
        public int DefaultSlots;
        public int DefaultRows;
        public int Slots;
        public int Rows;
        public MelonPreferences_Entry<int> PrefSlots;
        public MelonPreferences_Entry<int> PrefRows;
    }

    private static readonly Dictionary<string, RackSettings> RackConfigs = new()
    {
        ["StorageRack_Small"] = new RackSettings { Key = "SmallRack", Label = "Small Rack", DefaultSlots = 4, DefaultRows = 1 },
        ["StorageRack_Medium"] = new RackSettings { Key = "MediumRack", Label = "Medium Rack", DefaultSlots = 6, DefaultRows = 1 },
        ["StorageRack_Large"] = new RackSettings { Key = "LargeRack", Label = "Large Rack", DefaultSlots = 8, DefaultRows = 2 },
        ["WallMountedShelf_Built"] = new RackSettings { Key = "WallMountShelf", Label = "Wall Mount Shelf", DefaultSlots = 4, DefaultRows = 1 },
        ["DisplayCabinet"] = new RackSettings { Key = "DisplayCabinet", Label = "Display Cabinet", DefaultSlots = 4, DefaultRows = 1 },
        ["WoodSquareTable"] = new RackSettings { Key = "WoodSquareTable", Label = "Wood Square Table", DefaultSlots = 3, DefaultRows = 1 },
        ["MetalSquareTable"] = new RackSettings { Key = "MetalSquareTable", Label = "Metal Square Table", DefaultSlots = 3, DefaultRows = 1 },
        ["Safe"] = new RackSettings { Key = "Safe", Label = "Safe", DefaultSlots = 8, DefaultRows = 2 },
        ["CoffeeTable"] = new RackSettings { Key = "CoffeeTable", Label = "CoffeeTable", DefaultSlots = 3, DefaultRows = 1 },
    };

    public override void OnInitializeMelon()
    {
        MelonLogger.Msg("[Enhanced Storage] Loading...");

        CreateSettings();
        LoadSettings();

        if (!prefEnableMod.Value)
        { 
            MelonLogger.Msg("[Enhanced Storage] Mod disabled via config.");
            return;
        }

        PatchStorage();
        MelonLogger.Msg("[Enhanced Storage] Successfully loaded.");
    }

    private static void CreateSettings()
    {
        category = MelonPreferences.CreateCategory("EnhancedStorage", "Enhanced Storage Settings");

        prefEnableMod = category.CreateEntry("EnableMod", true, "Enable This Mod");
        prefUseDefaultSettings = category.CreateEntry("UseDefaultSettings", false, "Use Default Rack Settings");
        prefGameMaxSlots = category.CreateEntry("GameMaxSlots", 20, "Maximum Total Slots in Game");

        foreach (var key in RackConfigs.Keys.ToList())
        {
            var cfg = RackConfigs[key];
            cfg.PrefSlots = category.CreateEntry($"{cfg.Key}Slots", cfg.DefaultSlots, $"{cfg.Label} - Number of Slots");
            cfg.PrefRows = category.CreateEntry($"{cfg.Key}Rows", cfg.DefaultRows, $"{cfg.Label} - Number of Rows");
            RackConfigs[key] = cfg;
        }
    }

    private static void LoadSettings()
    {
        foreach (var key in RackConfigs.Keys.ToList())
        {
            var cfg = RackConfigs[key];

            if (prefUseDefaultSettings.Value)
            {
                cfg.Slots = cfg.DefaultSlots;
                cfg.Rows = cfg.DefaultRows;
            }
            else
            {
                cfg.Slots = cfg.PrefSlots.Value;
                cfg.Rows = cfg.PrefRows.Value;
            }

            RackConfigs[key] = cfg;
        }
    }

    private static void PatchStorage()
    {
        try
        {
            storageEntityType = Type.GetType("Il2CppScheduleOne.Storage.StorageEntity, Assembly-CSharp");
            if (storageEntityType == null)
                throw new Exception("StorageEntity type not found");

            var method = storageEntityType.GetMethod("Awake", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var prefix = typeof(EnhancedStorage).GetMethod(nameof(StorageEntityInitialize_Prefix), BindingFlags.Static | BindingFlags.NonPublic);
            new HarmonyLib.Harmony("com.mod.largerstorages").Patch(method, new HarmonyMethod(prefix));
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
            //MelonLogger.Msg($"[Enhanced Storage] Found object: {val.gameObject.name}");

            foreach (var entry in RackConfigs)
            {
                if (!val.gameObject.name.Contains(entry.Key)) continue;

                int slots = Mathf.Min(entry.Value.Slots, prefGameMaxSlots.Value);
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
