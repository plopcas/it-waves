using System;

namespace ITWaves.Systems
{
    [Serializable]
    public class SaveData
    {
        public int highestWaveReached = 1;
        public int currentScore = 0; // Score at wave start (used when continuing)
        public int deathScore = 0; // Score when player died (for GameOver display)
        public int deathWave = 1; // Wave player died on (for GameOver display)
        public float masterVolume = 1f;
        public float sfxVolume = 1f;
        public float musicVolume = 1f;
        public float mouseSensitivity = 1f;
        public int qualityLevel = 2;
        public bool treasureCollected = false; // Track if treasure box was collected in current run
        public float fireRateBoost = 0f; // Track accumulated fire rate boost from treasure boxes

        public SaveData()
        {
            // Default values
            highestWaveReached = 1;
            currentScore = 0;
            deathScore = 0;
            deathWave = 1;
            masterVolume = 1f;
            sfxVolume = 1f;
            musicVolume = 1f;
            mouseSensitivity = 1f;
            qualityLevel = 2;
            treasureCollected = false;
            fireRateBoost = 0f;
        }
    }
}

