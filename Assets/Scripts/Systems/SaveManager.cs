using System;
using System.IO;
using UnityEngine;

namespace ITWaves.Systems
{
    public static class SaveManager
    {
        private const string SAVE_FILE_NAME = "savegame.json";
        private const string HASH_FILE_NAME = "savegame.hash";
        
        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        private static string HashFilePath => Path.Combine(Application.persistentDataPath, HASH_FILE_NAME);
        
        private static SaveData cachedSaveData;
        
        public static bool HasSave()
        {
            return File.Exists(SaveFilePath) && File.Exists(HashFilePath);
        }
        
        public static SaveData Load()
        {
            if (!HasSave())
            {
                cachedSaveData = new SaveData();
                return cachedSaveData;
            }
            
            try
            {
                string json = File.ReadAllText(SaveFilePath);
                string hash = File.ReadAllText(HashFilePath);
                
                // Verify integrity
                if (!CryptoUtils.VerifyHMAC(json, hash))
                {
                    Debug.LogWarning("Save file integrity check failed. Creating new save data.");
                    cachedSaveData = new SaveData();
                    return cachedSaveData;
                }
                
                cachedSaveData = JsonUtility.FromJson<SaveData>(json);
                return cachedSaveData;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load save data: {e.Message}");
                cachedSaveData = new SaveData();
                return cachedSaveData;
            }
        }
        
        public static void Save(SaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                string hash = CryptoUtils.ComputeHMAC(json);
                
                File.WriteAllText(SaveFilePath, json);
                File.WriteAllText(HashFilePath, hash);
                
                cachedSaveData = data;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save data: {e.Message}");
            }
        }
        
        public static int GetHighestLevelReached()
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }
            return cachedSaveData.highestLevelReached;
        }
        
        public static void UpdateHighestLevel(int level)
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }
            
            if (level > cachedSaveData.highestLevelReached)
            {
                cachedSaveData.highestLevelReached = level;
                Save(cachedSaveData);
            }
        }
        
        public static void DeleteSave()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    File.Delete(SaveFilePath);
                }
                if (File.Exists(HashFilePath))
                {
                    File.Delete(HashFilePath);
                }
                cachedSaveData = new SaveData();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete save data: {e.Message}");
            }
        }
    }
}

