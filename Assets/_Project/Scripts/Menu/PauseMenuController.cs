using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class PauseMenuController : MonoBehaviour
{
    [Header("Cenas")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Entrada")]
    [SerializeField] private Key pauseKey = Key.Escape;
    [SerializeField] private bool lockCursorDuringGameplay = true;

    [Header("Interface")]
    [SerializeField] private GameObject pauseRoot;
    [SerializeField] private GameObject pauseMainPanel;
    [SerializeField] private GameObject pauseOptionsPanel;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private Dropdown qualityDropdown;

    private const string VolumeKey = "MagoArcano.Volume";
    private const string FullscreenKey = "MagoArcano.Fullscreen";
    private const string VSyncKey = "MagoArcano.VSync";
    private const string QualityKey = "MagoArcano.Quality";

    private bool isPaused;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == mainMenuSceneName)
        {
            enabled = false;
            if (pauseRoot != null)
                pauseRoot.SetActive(false);
            return;
        }
    }

    private void Start()
    {
        if (!enabled)
            return;

        ConfigureControls();
        ApplySavedSettings();
        ForceGameRunning();

        // Oculta opções que não são de áudio no menu de pausa
        if (fullscreenToggle != null)
        {
            fullscreenToggle.gameObject.SetActive(false);
            if (pauseOptionsPanel != null)
            {
                Transform fullLabel = pauseOptionsPanel.transform.Find("FullscreenLabel");
                if (fullLabel != null) fullLabel.gameObject.SetActive(false);
            }
        }
        if (vSyncToggle != null)
        {
            vSyncToggle.gameObject.SetActive(false);
            if (pauseOptionsPanel != null)
            {
                Transform vsyncLabel = pauseOptionsPanel.transform.Find("VSyncLabel");
                if (vsyncLabel != null) vsyncLabel.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (!enabled)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard[pauseKey].wasPressedThisFrame)
            return;

        if (pauseOptionsPanel != null && pauseOptionsPanel.activeSelf)
        {
            ShowPauseMain();
            return;
        }

        SetPaused(!isPaused);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(SetVolume);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);

        if (vSyncToggle != null)
            vSyncToggle.onValueChanged.RemoveListener(SetVSync);

        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.RemoveListener(SetQuality);
    }

    public void ResumeGame()
    {
        SetPaused(false);
    }

    public void OpenOptions()
    {
        if (pauseMainPanel != null)
            pauseMainPanel.SetActive(false);

        if (pauseOptionsPanel != null)
            pauseOptionsPanel.SetActive(true);
    }

    public void ShowPauseMain()
    {
        if (pauseOptionsPanel != null)
            pauseOptionsPanel.SetActive(false);

        if (pauseMainPanel != null)
            pauseMainPanel.SetActive(true);
    }

    public void RestartCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        RestoreBeforeSceneChange();
        SceneManager.LoadScene(currentScene);
    }

    public void ReturnToMainMenu()
    {
        if (!Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            Debug.LogError($"A cena '{mainMenuSceneName}' não está no Build Profiles.");
            return;
        }

        RestoreBeforeSceneChange();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        RestoreBeforeSceneChange();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetVolume(float value)
    {
        value = Mathf.Clamp01(value);
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool enabledValue)
    {
        Screen.fullScreen = enabledValue;
        PlayerPrefs.SetInt(FullscreenKey, enabledValue ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetVSync(bool enabledValue)
    {
        QualitySettings.vSyncCount = enabledValue ? 1 : 0;
        PlayerPrefs.SetInt(VSyncKey, enabledValue ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetQuality(int index)
    {
        if (QualitySettings.names == null || QualitySettings.names.Length == 0)
            return;

        index = Mathf.Clamp(index, 0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt(QualityKey, index);
        PlayerPrefs.Save();
    }

    private void SetPaused(bool paused)
    {
        isPaused = paused;

        if (pauseRoot != null)
            pauseRoot.SetActive(paused);

        if (paused)
            ShowPauseMain();

        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;

        if (paused)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = !lockCursorDuringGameplay;
            Cursor.lockState = lockCursorDuringGameplay
                ? CursorLockMode.Locked
                : CursorLockMode.None;
        }
    }

    private void ForceGameRunning()
    {
        isPaused = false;

        if (pauseRoot != null)
            pauseRoot.SetActive(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.visible = !lockCursorDuringGameplay;
        Cursor.lockState = lockCursorDuringGameplay
            ? CursorLockMode.Locked
            : CursorLockMode.None;
    }

    private void RestoreBeforeSceneChange()
    {
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void ConfigureControls()
    {
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
            qualityDropdown.onValueChanged.RemoveListener(SetQuality);
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(SetVolume);
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }

        if (vSyncToggle != null)
        {
            vSyncToggle.onValueChanged.RemoveListener(SetVSync);
            vSyncToggle.onValueChanged.AddListener(SetVSync);
        }
    }

    private void ApplySavedSettings()
    {
        float volume = PlayerPrefs.GetFloat(VolumeKey, 0.8f);
        bool fullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
        bool vSync = PlayerPrefs.GetInt(VSyncKey, QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;
        int quality = PlayerPrefs.GetInt(QualityKey, QualitySettings.GetQualityLevel());

        AudioListener.volume = Mathf.Clamp01(volume);
        Screen.fullScreen = fullscreen;
        QualitySettings.vSyncCount = vSync ? 1 : 0;

        if (QualitySettings.names != null && QualitySettings.names.Length > 0)
        {
            quality = Mathf.Clamp(quality, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(quality, true);
        }

        if (volumeSlider != null)
            volumeSlider.SetValueWithoutNotify(volume);

        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(fullscreen);

        if (vSyncToggle != null)
            vSyncToggle.SetIsOnWithoutNotify(vSync);

        if (qualityDropdown != null && qualityDropdown.options.Count > 0)
            qualityDropdown.SetValueWithoutNotify(quality);
    }
}
