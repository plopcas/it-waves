using UnityEngine;
using UnityEngine.SceneManagement;

namespace ITWaves.Core
{
    public class GameLoader : MonoBehaviour
    {
        [Header("Boot Settings")]
        [SerializeField, Tooltip("Auto-load main menu on start.")]
        private bool autoLoadMainMenu = true;

        [SerializeField, Tooltip("Delay before loading main menu.")]
        private float loadDelay = 2f;

        [Header("Music Manager")]
        [SerializeField, Tooltip("Music Manager prefab (will be instantiated if not in scene).")]
        private GameObject musicManagerPrefab;

        private void Start()
        {
            // Initialize settings
            InitializeSettings();

            // Initialize music manager
            InitializeMusicManager();

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

        private void InitializeMusicManager()
        {
            // Check if MusicManager already exists in scene
            if (Systems.MusicManager.Instance == null && musicManagerPrefab != null)
            {
                Instantiate(musicManagerPrefab);
            }
        }

        private void LoadMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}

