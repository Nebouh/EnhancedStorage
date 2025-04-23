using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MelonLoader;


namespace EnhancedStorageMod
{
    internal static class StorageSorter
    {
        private static readonly Type storageEntityType = Type.GetType("Il2CppScheduleOne.Storage.StorageEntity, Assembly-CSharp");

        public static void SortAlphabetically(object storageEntity)
        {
            MelonLogger.Msg("Entered in SortAlphabetically");
            try
            {
                if (storageEntity == null)
                {
                    MelonLogger.Error("storageEntity is null.");
                    return;
                }

                // Get ItemSlots property
                var itemsProp = storageEntityType.GetProperty("ItemSlots", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var itemsRaw = itemsProp?.GetValue(storageEntity);
                if (itemsRaw == null)
                {
                    MelonLogger.Error("ItemSlots is null.");
                    return;
                }

                // Get count of slots
                var countProp = itemsRaw.GetType().GetProperty("Count");
                int count = countProp != null ? (int)countProp.GetValue(itemsRaw) : 0;
                MelonLogger.Msg($"ItemSlots Count: {count}");

                // Get indexer (Item[i])
                var indexer = itemsRaw.GetType().GetProperty("Item");
                if (indexer == null)
                {
                    MelonLogger.Error("Could not access indexer on ItemSlots.");
                    return;
                }

                // Build list of used slots
                var usedSlots = new List<object>();
                var itemSlotToItem = new Dictionary<object, object>();
                for (int i = 0; i < count; i++)
                {
                    var slot = indexer.GetValue(itemsRaw, new object[] { i });
                    if (slot == null) continue;

                    var itemProp = slot.GetType().GetProperty("Item");
                    var item = itemProp?.GetValue(slot);

                    if (item != null)
                    {
                        usedSlots.Add(slot);
                        itemSlotToItem[slot] = item;
                    }
                }

                MelonLogger.Msg($"Used item slots: {usedSlots.Count}");
                if (usedSlots.Count < 2)
                {
                    MelonLogger.Msg("Not enough items to sort.");
                    return;
                }

                // Get name property from one sample item
                var sampleItem = itemSlotToItem[usedSlots[0]];
                var nameProp = sampleItem.GetType().GetProperty("name");
                if (nameProp == null)
                {
                    MelonLogger.Error("Property 'name' not found on item.");
                    return;
                }

                // Log before sorting
                MelonLogger.Msg("Before sorting:");
                foreach (var slot in usedSlots)
                {
                    var item = itemSlotToItem[slot];
                    MelonLogger.Msg($"- {nameProp.GetValue(item) ?? "null"}");
                }

                // Sort by name
                var sortedItems = usedSlots
                    .Select(s => itemSlotToItem[s])
                    .OrderBy(i => nameProp.GetValue(i)?.ToString())
                    .ToList();

                // Re-assign sorted items to slots from 0 onward
                int index = 0;
                for (int i = 0; i < count; i++)
                {
                    var slot = indexer.GetValue(itemsRaw, new object[] { i });
                    if (slot == null) continue;

                    var itemProp = slot.GetType().GetProperty("Item");
                    var item = itemProp?.GetValue(slot);

                    // Only overwrite slots that had an item before
                    if (item != null && index < sortedItems.Count)
                    {
                        itemProp?.SetValue(slot, sortedItems[index]);
                        index++;
                    }
                }

                // Log after sorting
                MelonLogger.Msg("After sorting:");
                foreach (var item in sortedItems)
                {
                    MelonLogger.Msg($"- {nameProp.GetValue(item) ?? "null"}");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Enhanced Storage] Error sorting items: {ex.Message}");
            }
        }
    }
}
