using UnityEngine;
using UnityEngine.SceneManagement;

namespace ITWaves.Systems
{
    // Manages background music across scenes with smooth transitions.
    // Singleton pattern ensures music persists across scene loads.
    [RequireComponent(typeof(AudioSource))]
    public class MusicManager : MonoBehaviour
    {
        private static MusicManager instance;
        public static MusicManager Instance => instance;

        [Header("Menu Music")]
        [SerializeField, Tooltip("Music for Boot and MainMenu scenes.")]
        private AudioClip menuMusic;

        [Header("Gameplay Music")]
        [SerializeField, Tooltip("Music for gameplay (Game scene).")]
        private AudioClip gameplayMusic;

        [Header("Settings")]
        [SerializeField, Tooltip("Volume of the music (0-1).")]
        [Range(0f, 1f)]
        private float musicVolume = 0.5f;

        [SerializeField, Tooltip("Fade duration when transitioning between tracks (seconds).")]
        private float fadeDuration = 1f;

        private AudioSource audioSource;
        private AudioClip currentClip;
        private bool isFading;
        private float fadeTimer;
        private float startVolume;
        private float targetVolume;
        private AudioClip nextClip;

        private void Awake()
        {
            // Singleton pattern - persist across scenes
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            // Get or add AudioSource
            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.volume = musicVolume;

            // Subscribe to scene changes
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void Start()
        {
            // Start playing appropriate music for the current scene
            PlayMusicForScene(SceneManager.GetActiveScene().name);
        }

        private void Update()
        {
            // Handle fade transitions
            if (isFading)
            {
                fadeTimer += Time.deltaTime;
                float t = Mathf.Clamp01(fadeTimer / fadeDuration);

                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);

                if (t >= 1f)
                {
                    isFading = false;

                    // If fading out, switch to next clip and fade in
                    if (targetVolume == 0f && nextClip != null)
                    {
                        audioSource.clip = nextClip;
                        currentClip = nextClip;
                        audioSource.Play();
                        nextClip = null;

                        // Fade in
                        StartFade(0f, musicVolume);
                    }
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayMusicForScene(scene.name);
        }

        private void PlayMusicForScene(string sceneName)
        {
            AudioClip targetClip = null;

            // Determine which music to play based on scene
            switch (sceneName)
            {
                case "Boot":
                case "MainMenu":
                case "GameOver":
                case "Win":
                    targetClip = menuMusic;
                    break;

                case "Game":
                    targetClip = gameplayMusic;
                    break;
            }

            // Only change music if it's different from current
            if (targetClip != null && targetClip != currentClip)
            {
                PlayMusic(targetClip);
            }
        }

        // Play a music clip with smooth transition.
        public void PlayMusic(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogWarning("[MusicManager] Attempted to play null music clip.");
                return;
            }

            // If already playing this clip, do nothing
            if (currentClip == clip && audioSource.isPlaying)
            {
                return;
            }

            // If nothing is playing, start immediately
            if (!audioSource.isPlaying)
            {
                audioSource.clip = clip;
                currentClip = clip;
                audioSource.volume = 0f;
                audioSource.Play();
                StartFade(0f, musicVolume);
            }
            else
            {
                // Fade out current, then switch to new clip
                nextClip = clip;
                StartFade(audioSource.volume, 0f);
            }
        }

        // Stop music with fade out.
        public void StopMusic()
        {
            if (audioSource.isPlaying)
            {
                StartFade(audioSource.volume, 0f);
            }
        }

        // Set music volume.
        public void SetVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (!isFading)
            {
                audioSource.volume = musicVolume;
            }
        }

        // Get current music volume.
        public float GetVolume()
        {
            return musicVolume;
        }

        private void StartFade(float from, float to)
        {
            isFading = true;
            fadeTimer = 0f;
            startVolume = from;
            targetVolume = to;
        }
    }
}

