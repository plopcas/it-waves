using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
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
        
        [Header("Panels")]
        [SerializeField, Tooltip("Main menu panel.")]
        private GameObject mainMenuPanel;
        
        [SerializeField, Tooltip("Options panel.")]
        private GameObject optionsPanel;
        
        [Header("Narrative")]
        [SerializeField, Tooltip("Narrative text.")]
        private TextMeshProUGUI narrativeText;
        
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
            
            // Show main menu
            ShowMainMenu();
        }
        
        private void HandleStart()
        {
            // Start new game from wave 1 and reset save progress
            SaveManager.ResetProgress();
            SceneManager.LoadScene("Game");
        }

        private void HandleContinue()
        {
            // Continue from highest wave reached - score is already saved in SaveManager
            SceneManager.LoadScene("Game");
        }
        
        private void HandleOptions()
        {
            ShowOptions();
        }
        
        private void HandleQuit()
        {
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
        }
        
        public void HandleBackToMenu()
        {
            ShowMainMenu();
        }
    }
}

