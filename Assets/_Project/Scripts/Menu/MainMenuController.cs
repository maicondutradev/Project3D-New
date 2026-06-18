using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public sealed class MainMenuController : MonoBehaviour
{
    [Header("Cenas")]
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private string defaultGameScene = "MainScene";
    [SerializeField] private string infernoScene = "Inferno";

    [Header("Áreas do menu")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject exitPanel;

    [Header("Controles")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private Dropdown qualityDropdown;
    [SerializeField] private Text statusText;

    [Header("Transição")]
    [SerializeField] private CanvasGroup fadeOverlay;
    [SerializeField] private Text loadingText;
    [SerializeField, Min(0.05f)] private float fadeDuration = 0.45f;

    [Header("Áudio opcional")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip clickClip;

    private const string LastSceneKey = "MagoArcano.LastScene";
    private const string HasSaveKey = "MagoArcano.HasSave";
    private const string VolumeKey = "MagoArcano.Volume";
    private const string FullscreenKey = "MagoArcano.Fullscreen";
    private const string VSyncKey = "MagoArcano.VSync";
    private const string QualityKey = "MagoArcano.Quality";

    private GameObject activeSecondaryPanel;
    private bool isLoading;
    private Coroutine statusRoutine;

    private void Awake()
    {
        AutoWireSceneReferences();

        if (SceneManager.GetActiveScene().name != menuSceneName)
        {
            enabled = false;
            return;
        }

        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Start()
    {
        if (!enabled)
            return;

        ConfigureOptionsControls();
        ApplySavedSettings();
        ClosePanels();
        UpdateContinueButton();
        EnsureButtonAnimations();

        // Oculta opções que não são de áudio (deixando apenas o volume geral)
        if (fullscreenToggle != null)
        {
            fullscreenToggle.gameObject.SetActive(false);
            if (optionsPanel != null)
            {
                Transform fullLabel = optionsPanel.transform.Find("FullscreenLabel");
                if (fullLabel != null) fullLabel.gameObject.SetActive(false);
            }
        }
        if (vSyncToggle != null)
        {
            vSyncToggle.gameObject.SetActive(false);
            if (optionsPanel != null)
            {
                Transform vsyncLabel = optionsPanel.transform.Find("VSyncLabel");
                if (vsyncLabel != null) vsyncLabel.gameObject.SetActive(false);
            }
        }

        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 0f;
            fadeOverlay.interactable = false;
            fadeOverlay.blocksRaycasts = false;
        }

        SetStatus(string.Empty);
    }

    private void Update()
    {
        if (!enabled || isLoading)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
            return;

        if (activeSecondaryPanel != null)
        {
            PlayClick();
            ClosePanels();
        }
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(SetVolume);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);

        if (vSyncToggle != null)
            vSyncToggle.onValueChanged.RemoveListener(SetVSync);

        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.RemoveListener(SetQuality);
    }

    public void PlayNewGame()
    {
        PlayClick();
        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.SetString(LastSceneKey, defaultGameScene);
        PlayerPrefs.Save();
        LoadScene(defaultGameScene);
    }

    public void ContinueGame()
    {
        PlayClick();

        if (PlayerPrefs.GetInt(HasSaveKey, 0) != 1)
        {
            ShowStatus("Nenhuma jornada salva foi encontrada.");
            UpdateContinueButton();
            return;
        }

        string sceneName = PlayerPrefs.GetString(LastSceneKey, defaultGameScene);
        LoadScene(sceneName);
    }

    public void OpenMapPanel()
    {
        PlayClick();
        OpenPanel(mapPanel);
    }

    public void OpenOptionsPanel()
    {
        PlayClick();
        OpenPanel(optionsPanel);
    }

    public void OpenCreditsPanel()
    {
        PlayClick();
        OpenPanel(creditsPanel);
    }

    public void OpenExitPanel()
    {
        PlayClick();
        OpenPanel(exitPanel);
    }

    public void ClosePanels()
    {
        SetPanelState(mapPanel, false);
        SetPanelState(optionsPanel, false);
        SetPanelState(creditsPanel, false);
        SetPanelState(exitPanel, false);

        activeSecondaryPanel = null;

        if (mainPanel != null)
            mainPanel.SetActive(true);
    }

    public void LoadMainMap()
    {
        PlayClick();
        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.SetString(LastSceneKey, defaultGameScene);
        PlayerPrefs.Save();
        LoadScene(defaultGameScene);
    }

    public void LoadInfernoMap()
    {
        PlayClick();
        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.SetString(LastSceneKey, infernoScene);
        PlayerPrefs.Save();
        LoadScene(infernoScene);
    }

    public void ConfirmExit()
    {
        PlayClick();

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

    private void ConfigureOptionsControls()
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

    private void UpdateContinueButton()
    {
        if (continueButton != null)
            continueButton.interactable = PlayerPrefs.GetInt(HasSaveKey, 0) == 1;
    }

    private void AutoWireSceneReferences()
    {
        mainPanel ??= FindChildObject("Hotspots") ?? FindChildObject("MainPanel");
        mapPanel ??= FindChildObject("MapPanel");
        optionsPanel ??= FindChildObject("OptionsPanel");
        creditsPanel ??= FindChildObject("CreditsPanel");
        exitPanel ??= FindChildObject("ExitPanel");

        continueButton ??= FindButton("Continuar");
        volumeSlider ??= FindComponentInChildrenByName<Slider>("VolumeSlider");
        fullscreenToggle ??= FindComponentInChildrenByName<Toggle>("FullscreenToggle");
        vSyncToggle ??= FindComponentInChildrenByName<Toggle>("VSyncToggle");
        qualityDropdown ??= FindComponentInChildrenByName<Dropdown>("QualityDropdown");
        statusText ??= FindComponentInChildrenByName<Text>("StatusText");
        fadeOverlay ??= FindComponentInChildrenByName<CanvasGroup>("FadeOverlay");
        loadingText ??= FindComponentInChildrenByName<Text>("LoadingText");
        uiAudioSource ??= GetComponent<AudioSource>();
    }

    private void EnsureButtonAnimations()
    {
        foreach (Button button in GetComponentsInChildren<Button>(true))
        {
            if (button.GetComponent<ButtonAnimationEffect>() == null &&
                button.GetComponent<MenuHotspotEffect>() == null)
            {
                button.gameObject.AddComponent<ButtonAnimationEffect>();
            }
        }
    }

    private GameObject FindChildObject(string objectName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        Transform found = children.FirstOrDefault(child => child.name == objectName);
        return found != null ? found.gameObject : null;
    }

    private Button FindButton(string objectName)
    {
        return FindComponentInChildrenByName<Button>(objectName);
    }

    private T FindComponentInChildrenByName<T>(string objectName) where T : Component
    {
        T[] components = GetComponentsInChildren<T>(true);
        return components.FirstOrDefault(component => component.name == objectName);
    }

    private void OpenPanel(GameObject panel)
    {
        if (panel == null)
            return;

        SetPanelState(mapPanel, false);
        SetPanelState(optionsPanel, false);
        SetPanelState(creditsPanel, false);
        SetPanelState(exitPanel, false);

        if (mainPanel != null)
            mainPanel.SetActive(false);

        panel.SetActive(true);
        activeSecondaryPanel = panel;
    }

    private static void SetPanelState(GameObject panel, bool state)
    {
        if (panel != null)
            panel.SetActive(state);
    }

    private void LoadScene(string sceneName)
    {
        if (isLoading)
            return;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            ShowStatus("O nome da cena está vazio.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            ShowStatus($"A cena '{sceneName}' não foi adicionada ao Build Profiles.");
            Debug.LogError($"Cena não encontrada no Build Profiles: {sceneName}");
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (loadingText != null)
            loadingText.text = "Abrindo o portão para a magia...";

        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.blocksRaycasts = true;
            fadeOverlay.interactable = true;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeOverlay.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        if (operation == null)
        {
            isLoading = false;
            ShowStatus("Não foi possível iniciar o carregamento da cena.");
            yield break;
        }

        while (!operation.isDone)
            yield return null;
    }

    private void PlayClick()
    {
        if (uiAudioSource != null && clickClip != null)
            uiAudioSource.PlayOneShot(clickClip);
    }

    private void ShowStatus(string message)
    {
        if (statusRoutine != null)
            StopCoroutine(statusRoutine);

        statusRoutine = StartCoroutine(StatusRoutine(message));
    }

    private IEnumerator StatusRoutine(string message)
    {
        SetStatus(message);
        yield return new WaitForSecondsRealtime(4f);
        SetStatus(string.Empty);
        statusRoutine = null;
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
