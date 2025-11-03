using System;

namespace ITWaves.Systems
{
    [Serializable]
    public class SaveData
    {
        public int highestLevelReached = 1;
        public float masterVolume = 1f;
        public float sfxVolume = 1f;
        public float musicVolume = 1f;
        public float mouseSensitivity = 1f;
        public int qualityLevel = 2;
        
        public SaveData()
        {
            // Default values
            highestLevelReached = 1;
            masterVolume = 1f;
            sfxVolume = 1f;
            musicVolume = 1f;
            mouseSensitivity = 1f;
            qualityLevel = 2;
        }
    }
}

