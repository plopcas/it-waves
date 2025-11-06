using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using ITWaves.Systems;

namespace ITWaves.UI
{
    public class MenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField, Tooltip("Start button.")]
        private Button startButton;

        [SerializeField, Tooltip("Continue button.")]
        private Button continueButton;

        [SerializeField, Tooltip("Options button.")]
        private Button optionsButton;

        [SerializeField, Tooltip("Quit button.")]
        private Button quitButton;

        [SerializeField, Tooltip("Delete save button.")]
        private Button deleteSaveButton;

        [Header("Panels")]
        [SerializeField, Tooltip("Main menu panel.")]
        private GameObject mainMenuPanel;

        [SerializeField, Tooltip("Options panel.")]
        private GameObject optionsPanel;

        [SerializeField, Tooltip("Confirmation dialog panel.")]
        private GameObject confirmationPanel;

        [Header("Confirmation Dialog")]
        [SerializeField, Tooltip("Confirmation message text.")]
        private TextMeshProUGUI confirmationText;

        [SerializeField, Tooltip("Yes button in confirmation dialog.")]
        private Button confirmYesButton;

        [SerializeField, Tooltip("No button in confirmation dialog.")]
        private Button confirmNoButton;

        [Header("Narrative")]
        [SerializeField, Tooltip("Narrative text.")]
        private TextMeshProUGUI narrativeText;

        [Header("Audio")]
        [SerializeField, Tooltip("Sound to play when clicking menu buttons.")]
        private AudioClip menuClickSound;

        [SerializeField, Tooltip("Delay before quitting to allow sound to play (seconds).")]
        private float quitDelay = 0.2f;

        private const string NARRATIVE_TEXT = "LOST IN A HOUSE THAT HATES THE LIGHT, YOU HUNT WHAT HUNTS YOU.";

        private void Start()
        {
            // Set narrative text
            if (narrativeText != null)
            {
                narrativeText.text = NARRATIVE_TEXT;
            }

            // Setup buttons
            if (startButton != null)
            {
                startButton.onClick.AddListener(HandleStart);
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(HandleContinue);
                // Enable continue only if save exists
                continueButton.interactable = SaveManager.HasSave();
            }

            if (optionsButton != null)
            {
                optionsButton.onClick.AddListener(HandleOptions);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(HandleQuit);
                // Hide quit button in WebGL
                #if UNITY_WEBGL && !UNITY_EDITOR
                quitButton.gameObject.SetActive(false);
                #endif
            }

            if (deleteSaveButton != null)
            {
                deleteSaveButton.onClick.AddListener(HandleDeleteSave);
            }

            // Setup confirmation dialog buttons
            if (confirmYesButton != null)
            {
                confirmYesButton.onClick.AddListener(HandleConfirmYes);
            }

            if (confirmNoButton != null)
            {
                confirmNoButton.onClick.AddListener(HandleConfirmNo);
            }

            // Hide confirmation panel initially
            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(false);
            }

            // Show main menu
            ShowMainMenu();
        }
        
        private void HandleStart()
        {
            PlayMenuSound();
            // Start new game from wave 1 and reset save progress
            SaveManager.ResetProgress();
            SceneManager.LoadScene("Game");
        }

        private void HandleContinue()
        {
            PlayMenuSound();
            // Continue from highest wave reached - score is already saved in SaveManager
            SceneManager.LoadScene("Game");
        }

        private void HandleOptions()
        {
            PlayMenuSound();
            ShowOptions();
        }

        private void HandleQuit()
        {
            // Disable button to prevent multiple clicks
            if (quitButton != null)
            {
                quitButton.interactable = false;
            }

            PlayMenuSound();
            StartCoroutine(QuitAfterDelay());
        }

        private IEnumerator QuitAfterDelay()
        {
            // Wait for sound to play
            yield return new WaitForSeconds(quitDelay);

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        private void ShowMainMenu()
        {
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }
            
            if (optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }
        }
        
        private void ShowOptions()
        {
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(false);
            }

            if (optionsPanel != null)
            {
                optionsPanel.SetActive(true);
            }

            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(false);
            }
        }

        public void HandleBackToMenu()
        {
            PlayMenuSound();
            ShowMainMenu();
        }

        private void HandleDeleteSave()
        {
            PlayMenuSound();
            // Show confirmation dialog
            ShowConfirmationDialog("ARE YOU SURE?");
        }

        private void PlayMenuSound()
        {
            if (MusicManager.Instance != null && menuClickSound != null)
            {
                MusicManager.Instance.PlayUISound(menuClickSound);
            }
        }

        private void ShowConfirmationDialog(string message)
        {
            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(true);
            }

            if (confirmationText != null)
            {
                confirmationText.text = message;
            }

            // Hide options panel while showing confirmation
            if (optionsPanel != null)
            {
                optionsPanel.SetActive(false);
            }
        }

        private void HideConfirmationDialog()
        {
            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(false);
            }

            // Show options panel again
            if (optionsPanel != null)
            {
                optionsPanel.SetActive(true);
            }
        }

        private void HandleConfirmYes()
        {
            PlayMenuSound();
            // Delete the save
            SaveManager.DeleteSave();
            Debug.Log("Save data deleted successfully");

            // Update continue button state
            if (continueButton != null)
            {
                continueButton.interactable = SaveManager.HasSave();
            }

            // Hide confirmation and return to options
            HideConfirmationDialog();
        }

        private void HandleConfirmNo()
        {
            PlayMenuSound();
            // Just hide the confirmation dialog
            HideConfirmationDialog();
        }
    }
}

