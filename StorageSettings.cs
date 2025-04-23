using System.Collections.Generic;
using System.Linq;
using MelonLoader;

namespace EnhancedStorageMod
{
    internal static class StorageSettings
    {
        public static MelonPreferences_Category Category;
        public static MelonPreferences_Entry<bool> EnableMod;
        public static MelonPreferences_Entry<bool> UseDefaultSettings;
        public static MelonPreferences_Entry<int> GameMaxSlots;

        public struct RackSettings
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

        public static readonly Dictionary<string, RackSettings> RackConfigs = new()
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
            ["PalletStand"] = new RackSettings { Key = "PalletStand", Label = "PalletStand", DefaultSlots = 3, DefaultRows = 1 },
        };

        public static bool LoadPreferences()
        {
            Category = MelonPreferences.CreateCategory("EnhancedStorage", "Enhanced Storage Settings");

            EnableMod = Category.CreateEntry("EnableMod", true, "Enable this mod");
            if (!EnableMod.Value)
            {
                MelonLogger.Warning("[Enhanced Storage] Mod is disabled via preferences.");
                return false;
            }

            UseDefaultSettings = Category.CreateEntry("UseDefaultSettings", false, "Use default rack settings");
            GameMaxSlots = Category.CreateEntry("GameMaxSlots", 20, "Maximum total slots in the game");

            return true;
        }

        public static void InitializeSettings()
        {
            foreach (var key in RackConfigs.Keys.ToList())
            {
                var cfg = RackConfigs[key];
                cfg.PrefSlots = Category.CreateEntry($"{cfg.Key}Slots", cfg.DefaultSlots, $"{cfg.Label} - Number of Slots");
                cfg.PrefRows = Category.CreateEntry($"{cfg.Key}Rows", cfg.DefaultRows, $"{cfg.Label} - Number of Rows");

                cfg.Slots = UseDefaultSettings.Value ? cfg.DefaultSlots : cfg.PrefSlots.Value;
                cfg.Rows = UseDefaultSettings.Value ? cfg.DefaultRows : cfg.PrefRows.Value;

                RackConfigs[key] = cfg;
            }
        }
    }
}
