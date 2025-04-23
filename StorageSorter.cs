using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UnityEngine;

namespace EnhancedStorageMod
{
    internal static class StorageSorter
    {
        private static readonly Type storageEntityType = Type.GetType("Il2CppScheduleOne.Storage.StorageEntity, Assembly-CSharp");

        public static void SortAlphabetically(object storageEntity)
        {
            try
            {
                if (storageEntity == null) return;

                var itemsField = storageEntityType.GetField("items", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (itemsField == null) throw new Exception("Field 'items' not found.");

                var items = itemsField.GetValue(storageEntity) as IList;
                if (items == null || items.Count < 2) return;

                var nameProp = items[0].GetType().GetProperty("name");
                if (nameProp == null) throw new Exception("Property 'name' not found on stored item.");

                var sorted = items.Cast<object>().OrderBy(item => nameProp.GetValue(item)?.ToString()).ToList();

                for (int i = 0; i < items.Count; i++)
                {
                    items[i] = sorted[i];
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Enhanced Storage] Error sorting items: {ex.Message}");
            }
        }
    }
}
