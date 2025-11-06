using System;
using System.Collections.Generic;

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

        public List<int> collectedTreasures = new List<int>();

        public float fireRateBoost = 0f; // Track accumulated fire rate boost from treasure boxes
        public bool snakePauseEnabled = false; // Track if player has snake pause ability (treasure 3)

        // Game statistics (for current playthrough)
        public int totalDeaths = 0; // Total times player died in current playthrough
        public int boxesDestroyed = 0; // Total boxes destroyed in current playthrough
        public int crawlersKilled = 0; // Total crawlers killed in current playthrough
        public int skitterersKilled = 0; // Total skitterers killed in current playthrough
        public float totalPlaytimeSeconds = 0f; // Total time spent in current playthrough (in seconds)

        public SaveData()
        {
            highestWaveReached = 1;
            currentScore = 0;
            deathScore = 0;
            deathWave = 1;
            masterVolume = 1f;
            sfxVolume = 1f;
            musicVolume = 1f;
            mouseSensitivity = 1f;
            qualityLevel = 2;
            collectedTreasures = new List<int>();
            fireRateBoost = 0f;
            snakePauseEnabled = false;
            totalDeaths = 0;
            boxesDestroyed = 0;
            crawlersKilled = 0;
            skitterersKilled = 0;
            totalPlaytimeSeconds = 0f;
        }
    }
}

