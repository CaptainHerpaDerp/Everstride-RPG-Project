using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace DataSave
{
    public static class SaveLoadManager
    {
        private static string savePath = Application.persistentDataPath + "/NPCData.json";

        public static void SaveNPCData(object npcData, string npcID)
        {
            string npcSavePath = Application.persistentDataPath + $"/NPC_{npcID}.json";
            string json = JsonConvert.SerializeObject(npcData);
            File.WriteAllText(npcSavePath, json);
        }

        public static T LoadNPCData<T>(string npcID)
        {
            string npcSavePath = Application.persistentDataPath + $"/NPC_{npcID}.json";

            if (File.Exists(npcSavePath))
            {
                string json = File.ReadAllText(npcSavePath);
                return JsonConvert.DeserializeObject<T>(json);
            }
            else
            {
                Debug.LogWarning($"No saved NPC data found for NPC with ID: {npcID}");
                return default;
            }
        }

        public static void SaveInventoryData(object InventoryData)
        {
            string json = JsonConvert.SerializeObject(InventoryData);
            string filePath = Application.persistentDataPath + "/InventoryData.json";
            File.WriteAllText(filePath, json);
        }

        public static T LoadInventoryData<T>()
        {
            string filePath = Application.persistentDataPath + "/InventoryData.json";

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<T>(json);
            }
            else
            {
                Debug.LogWarning("No saved inventory data found.");
                return default;
            }
        }

        public static T LoadItemData<T>(string itemID)
        {
            string itemSavePath = Application.persistentDataPath + $"/Item_{itemID}.json";

            if (File.Exists(itemSavePath))
            {
                string json = File.ReadAllText(itemSavePath);
                return JsonConvert.DeserializeObject<T>(json);
            }
            else
            {
                Debug.LogWarning($"No saved item data found for item with ID: {itemID}");
                return default;
            }
        }
    }
}
