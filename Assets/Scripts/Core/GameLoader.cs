using UnityEngine;
using UnityEngine.SceneManagement;

namespace ITWaves.Core
{
    /// <summary>
    /// Handles game initialization and scene loading.
    /// </summary>
    public class GameLoader : MonoBehaviour
    {
        [Header("Boot Settings")]
        [SerializeField, Tooltip("Auto-load main menu on start.")]
        private bool autoLoadMainMenu = true;
        
        [SerializeField, Tooltip("Delay before loading main menu.")]
        private float loadDelay = 0.5f;
        
        private void Start()
        {
            // Initialize settings
            InitializeSettings();
            
            // Load main menu
            if (autoLoadMainMenu)
            {
                Invoke(nameof(LoadMainMenu), loadDelay);
            }
        }
        
        private void InitializeSettings()
        {
            // Set target frame rate for WebGL
            #if UNITY_WEBGL && !UNITY_EDITOR
            Application.targetFrameRate = 60;
            #else
            Application.targetFrameRate = -1; // Unlimited
            #endif
            
            // Set quality settings
            QualitySettings.vSyncCount = 1;
            
            // Initialize audio
            AudioListener.volume = 1f;
        }
        
        private void LoadMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
        
        /// <summary>
        /// Load game scene with specified level.
        /// </summary>
        public static void LoadGame(int levelIndex)
        {
            PlayerPrefs.SetInt("StartLevel", levelIndex);
            SceneManager.LoadScene("Game");
        }
    }
}

