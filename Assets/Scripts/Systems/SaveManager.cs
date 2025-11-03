using System;
using System.IO;
using UnityEngine;

namespace ITWaves.Systems
{
    /// <summary>
    /// Manages save and load operations with integrity checking.
    /// </summary>
    public static class SaveManager
    {
        private const string SAVE_FILE_NAME = "savegame.json";
        private const string HASH_FILE_NAME = "savegame.hash";
        
        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        private static string HashFilePath => Path.Combine(Application.persistentDataPath, HASH_FILE_NAME);
        
        private static SaveData cachedSaveData;
        
        /// <summary>
        /// Check if a save file exists.
        /// </summary>
        public static bool HasSave()
        {
            return File.Exists(SaveFilePath) && File.Exists(HashFilePath);
        }
        
        /// <summary>
        /// Load save data from disk.
        /// </summary>
        public static SaveData Load()
        {
            if (!HasSave())
            {
                Debug.Log("No save file found. Creating new save data.");
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
                Debug.Log($"Save data loaded. Highest level: {cachedSaveData.highestLevelReached}");
                return cachedSaveData;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load save data: {e.Message}");
                cachedSaveData = new SaveData();
                return cachedSaveData;
            }
        }
        
        /// <summary>
        /// Save data to disk with integrity hash.
        /// </summary>
        public static void Save(SaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                string hash = CryptoUtils.ComputeHMAC(json);
                
                File.WriteAllText(SaveFilePath, json);
                File.WriteAllText(HashFilePath, hash);
                
                cachedSaveData = data;
                Debug.Log($"Save data written. Highest level: {data.highestLevelReached}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save data: {e.Message}");
            }
        }
        
        /// <summary>
        /// Get the highest level reached from save data.
        /// </summary>
        public static int GetHighestLevelReached()
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }
            return cachedSaveData.highestLevelReached;
        }
        
        /// <summary>
        /// Update the highest level reached if new level is higher.
        /// </summary>
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
        
        /// <summary>
        /// Delete save file.
        /// </summary>
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
                Debug.Log("Save data deleted.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete save data: {e.Message}");
            }
        }
    }
}

