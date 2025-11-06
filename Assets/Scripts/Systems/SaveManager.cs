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
        
        public static int GetHighestWaveReached()
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }
            Debug.Log($"GetHighestWaveReached: returning {cachedSaveData.highestWaveReached}");
            return cachedSaveData.highestWaveReached;
        }

        public static void UpdateHighestWave(int wave)
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }

            Debug.Log($"UpdateHighestWave called: wave={wave}, current highest={cachedSaveData.highestWaveReached}");

            if (wave > cachedSaveData.highestWaveReached)
            {
                cachedSaveData.highestWaveReached = wave;
                Save(cachedSaveData);
                Debug.Log($"Saved new highest wave: {wave}");
            }
            else
            {
                Debug.Log($"Wave {wave} not higher than current highest {cachedSaveData.highestWaveReached}, not saving");
            }
        }

        public static void ResetProgress()
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }

            Debug.Log("Resetting progress to wave 1");
            cachedSaveData.highestWaveReached = 1;
            cachedSaveData.currentScore = 0;
            cachedSaveData.deathScore = 0;
            cachedSaveData.deathWave = 1;
            cachedSaveData.collectedTreasures.Clear(); // Clear all collected treasures
            cachedSaveData.fireRateBoost = 0f; // Reset fire rate boost on new game
            cachedSaveData.snakePauseEnabled = false; // Reset snake pause ability
            cachedSaveData.totalDeaths = 0; // Reset death counter
            cachedSaveData.boxesDestroyed = 0; // Reset box counter
            cachedSaveData.crawlersKilled = 0; // Reset crawler counter
            cachedSaveData.skitterersKilled = 0; // Reset skitterer counter
            cachedSaveData.totalPlaytimeSeconds = 0f; // Reset playtime
            Save(cachedSaveData);
        }

        public static bool IsTreasureCollected(int treasureNumber)
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }
            return cachedSaveData.collectedTreasures.Contains(treasureNumber);
        }

        public static void SetTreasureCollected(int treasureNumber)
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }

            if (!cachedSaveData.collectedTreasures.Contains(treasureNumber))
            {
                cachedSaveData.collectedTreasures.Add(treasureNumber);
                Debug.Log($"Treasure {treasureNumber} collected! Total treasures: {cachedSaveData.collectedTreasures.Count}");
                Save(cachedSaveData);
            }
        }

        public static int GetHighestTreasureCollected()
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }

            if (cachedSaveData.collectedTreasures.Count == 0)
            {
                return 0;
            }

            int highest = 0;
            foreach (int treasureNum in cachedSaveData.collectedTreasures)
            {
                if (treasureNum > highest)
                {
                    highest = treasureNum;
                }
            }
            return highest;
        }

        public static bool HasAllTreasuresUpTo(int upToTreasureNumber)
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }

            for (int i = 1; i <= upToTreasureNumber; i++)
            {
                if (!cachedSaveData.collectedTreasures.Contains(i))
                {
                    return false;
                }
            }
            return true;
        }

        public static float GetFireRateBoost()
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }
            return cachedSaveData.fireRateBoost;
        }

        public static void AddFireRateBoost(float boost)
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }

            cachedSaveData.fireRateBoost += boost;
            Debug.Log($"Fire rate boost increased by {boost}. Total boost: {cachedSaveData.fireRateBoost}");
            Save(cachedSaveData);
        }

        public static bool IsSnakePauseEnabled()
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }
            return cachedSaveData.snakePauseEnabled;
        }

        public static void SetSnakePauseEnabled(bool enabled)
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }

            cachedSaveData.snakePauseEnabled = enabled;
            Debug.Log($"Snake pause ability set to: {enabled}");
            Save(cachedSaveData);
        }

        public static int GetCurrentScore()
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }
            return cachedSaveData.currentScore;
        }

        public static void SaveScore(int score, int wave)
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }

            cachedSaveData.currentScore = score;
            cachedSaveData.deathWave = wave;
            Save(cachedSaveData);
        }

        public static int GetDeathWave()
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }
            return cachedSaveData.deathWave;
        }

        public static int GetDeathScore()
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }
            return cachedSaveData.deathScore;
        }

        public static void SaveDeathScore(int score)
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }

            cachedSaveData.deathScore = score;
            Save(cachedSaveData);
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

        // Game statistics methods
        public static void IncrementDeaths()
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }

            cachedSaveData.totalDeaths++;
            Save(cachedSaveData);
        }

        public static void IncrementBoxesDestroyed()
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }

            cachedSaveData.boxesDestroyed++;
            Save(cachedSaveData);
        }

        public static void IncrementCrawlersKilled()
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }

            cachedSaveData.crawlersKilled++;
            Save(cachedSaveData);
        }

        public static void IncrementSkitterersKilled()
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }

            cachedSaveData.skitterersKilled++;
            Save(cachedSaveData);
        }

        public static int GetTotalDeaths()
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }
            return cachedSaveData.totalDeaths;
        }

        public static int GetBoxesDestroyed()
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }
            return cachedSaveData.boxesDestroyed;
        }

        public static int GetCrawlersKilled()
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }
            return cachedSaveData.crawlersKilled;
        }

        public static int GetSkitterersKilled()
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }
            return cachedSaveData.skitterersKilled;
        }

        public static void AddPlaytime(float seconds)
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }

            cachedSaveData.totalPlaytimeSeconds += seconds;
            Save(cachedSaveData);
        }

        public static float GetTotalPlaytime()
        {
            if (cachedSaveData == null)
            {
                cachedSaveData = Load();
            }
            return cachedSaveData.totalPlaytimeSeconds;
        }
    }
}

