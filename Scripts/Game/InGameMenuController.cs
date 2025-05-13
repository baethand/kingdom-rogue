using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject gameMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Menu Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backButton;

    [Header("Settings Controls")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Other References")]
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private AudioSource backgroundMusic;
    

    private bool isPaused = false;

    private void Awake()
    {
        // Назначаем обработчики кнопок
        resumeButton.onClick.AddListener(TogglePauseMenu);
        settingsButton.onClick.AddListener(OpenSettings);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        quitButton.onClick.AddListener(QuitGame);
        backButton.onClick.AddListener(CloseSettings);
    }

    private void Start()
    {
        gameMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        LoadSettings();
    }
    
    private void Update()
    {
        // Открытие/закрытие меню по клавише Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else
            {
                TogglePauseMenu();
            }
        }
    }
    
    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        
        gameMenuPanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
        
        if (inputHandler != null)
        {
            inputHandler.SetInputEnabled(!isPaused);
        }
        
        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }
    
    public void OpenSettings()
    {
        gameMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
    
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        gameMenuPanel.SetActive(true);
        SaveSettings();
    }
    
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    private void LoadSettings()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        ApplySettings();
    }
    
    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", volumeSlider.value);
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        ApplySettings();
    }
    
    private void ApplySettings()
    {
        AudioListener.volume = volumeSlider.value;
        Screen.fullScreen = fullscreenToggle.isOn;
    }
}