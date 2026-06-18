using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    private enum PauseState
    {
        Main,
        Options
    }

    [Header("Configuração")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private Key pauseKey = Key.Escape;
    [SerializeField] private bool lockCursorDuringGameplay = true;

    [Header("Estilo Visual")]
    [SerializeField] private Font customFont;

    private bool isPaused = false;
    private PauseState currentState = PauseState.Main;

    private Texture2D pixelTexture;
    private Texture2D panelTexture;
    private Texture2D buttonTexture;
    private Texture2D buttonHoverTexture;
    private Texture2D buttonActiveTexture;
    private Texture2D sliderTexture;
    private Texture2D sliderThumbTexture;

    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;
    private GUIStyle panelStyle;
    private GUIStyle labelStyle;
    private GUIStyle sliderStyle;
    private GUIStyle sliderThumbStyle;

    private bool stylesCreated = false;

    // Propriedades de opções de áudio e vídeo
    private float masterVolume = 1f;
    private bool fullscreen;
    private bool vSync;
    private int qualityIndex;

    private const string VolumeKey = "ArcaneMenu.Volume";
    private const string FullscreenKey = "ArcaneMenu.Fullscreen";
    private const string VSyncKey = "ArcaneMenu.VSync";
    private const string QualityKey = "ArcaneMenu.Quality";

    private void Start()
    {
        // Se estiver no Menu Principal, desativa o script de pausa
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            enabled = false;
            return;
        }

        // Garante que o jogo começa rodando e com o cursor travado/escondido
        Time.timeScale = 1f;
        isPaused = false;
        currentState = PauseState.Main;

        if (lockCursorDuringGameplay)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        CreateTextures();
        LoadSettings();
    }

    private void Update()
    {
        if (!enabled)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (!keyboard[pauseKey].wasPressedThisFrame)
            return;

        if (currentState == PauseState.Options)
        {
            currentState = PauseState.Main;
            return;
        }

        SetPaused(!isPaused);
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
        if (isPaused)
        {
            Time.timeScale = 0f; // Pausa o tempo do jogo
            
            // Mostra o cursor para o jogador poder clicar nos botões
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            currentState = PauseState.Main;
        }
        else
        {
            Time.timeScale = 1f; // Volta o tempo do jogo ao normal
            
            if (lockCursorDuringGameplay)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    private void OnGUI()
    {
        if (!isPaused) return;

        CreateStyles();

        // Escurece levemente o fundo do jogo
        Color previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.5f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), pixelTexture);
        GUI.color = previousColor;

        if (currentState == PauseState.Main)
        {
            DrawMainPauseMenu();
        }
        else if (currentState == PauseState.Options)
        {
            DrawOptionsPauseMenu();
        }
    }

    private void DrawMainPauseMenu()
    {
        float boxWidth = 350f;
        float boxHeight = 350f;
        float halfScreenWidth = Screen.width / 2f;
        float halfScreenHeight = Screen.height / 2f;

        Rect menuRect = new Rect(halfScreenWidth - (boxWidth / 2f), halfScreenHeight - (boxHeight / 2f), boxWidth, boxHeight);

        GUI.Box(menuRect, GUIContent.none, panelStyle);

        GUILayout.BeginArea(new Rect(menuRect.x + 30f, menuRect.y + 35f, menuRect.width - 60f, menuRect.height - 70f));

        GUILayout.Label("JOGO PAUSADO", titleStyle);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("CONTINUAR", buttonStyle, GUILayout.Height(55f)))
        {
            SetPaused(false);
        }

        GUILayout.Space(15f);

        if (GUILayout.Button("OPÇÕES", buttonStyle, GUILayout.Height(55f)))
        {
            currentState = PauseState.Options;
        }

        GUILayout.Space(15f);

        if (GUILayout.Button("MENU PRINCIPAL", buttonStyle, GUILayout.Height(55f)))
        {
            ReturnToMainMenu();
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
    }

    private void DrawOptionsPauseMenu()
    {
        float boxWidth = 450f;
        float boxHeight = 450f;
        float halfScreenWidth = Screen.width / 2f;
        float halfScreenHeight = Screen.height / 2f;

        Rect menuRect = new Rect(halfScreenWidth - (boxWidth / 2f), halfScreenHeight - (boxHeight / 2f), boxWidth, boxHeight);

        GUI.Box(menuRect, GUIContent.none, panelStyle);

        GUILayout.BeginArea(new Rect(menuRect.x + 35f, menuRect.y + 35f, menuRect.width - 70f, menuRect.height - 70f));

        GUILayout.Label("OPÇÕES DE PAUSA", titleStyle);

        GUILayout.FlexibleSpace();

        GUILayout.Label($"VOLUME GERAL: {Mathf.RoundToInt(masterVolume * 100f)}%", labelStyle);

        float newVolume = GUILayout.HorizontalSlider(
            masterVolume,
            0f,
            1f,
            sliderStyle,
            sliderThumbStyle,
            GUILayout.Height(30f)
        );

        if (!Mathf.Approximately(newVolume, masterVolume))
        {
            masterVolume = newVolume;
            AudioListener.volume = masterVolume;
            SaveSettings();
        }

        GUILayout.Space(20f);

        bool newFullscreen = GUILayout.Toggle(fullscreen, " TELA CHEIA", labelStyle);
        if (newFullscreen != fullscreen)
        {
            fullscreen = newFullscreen;
            Screen.fullScreen = fullscreen;
            SaveSettings();
        }

        GUILayout.Space(15f);

        bool newVSync = GUILayout.Toggle(vSync, " SINCRONIZAÇÃO VERTICAL", labelStyle);
        if (newVSync != vSync)
        {
            vSync = newVSync;
            QualitySettings.vSyncCount = vSync ? 1 : 0;
            SaveSettings();
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("VOLTAR", buttonStyle, GUILayout.Height(50f)))
        {
            currentState = PauseState.Main;
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void CreateTextures()
    {
        pixelTexture = CreateSolidTexture(Color.white);
        panelTexture = CreateSolidTexture(new Color(0.035f, 0.025f, 0.075f, 0.94f));
        buttonTexture = CreateSolidTexture(new Color(0.15f, 0.08f, 0.25f, 0.98f));
        buttonHoverTexture = CreateSolidTexture(new Color(0.32f, 0.16f, 0.54f, 1f));
        buttonActiveTexture = CreateSolidTexture(new Color(0.47f, 0.24f, 0.7f, 1f));
        sliderTexture = CreateSolidTexture(new Color(0.12f, 0.08f, 0.18f, 1f));
        sliderThumbTexture = CreateSolidTexture(new Color(0.65f, 0.38f, 0.95f, 1f));
    }

    private Texture2D CreateSolidTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.name = "GeneratedPauseTexture";
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private void CreateStyles()
    {
        if (stylesCreated) return;

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.86f, 0.76f, 1f) }
        };

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.89f, 0.86f, 0.95f) }
        };

        panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.normal.background = panelTexture;
        panelStyle.padding = new RectOffset(25, 25, 25, 25);

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };
        buttonStyle.normal.background = buttonTexture;
        buttonStyle.normal.textColor = new Color(0.92f, 0.88f, 1f);
        buttonStyle.hover.background = buttonHoverTexture;
        buttonStyle.hover.textColor = Color.white;
        buttonStyle.active.background = buttonActiveTexture;
        buttonStyle.active.textColor = Color.white;

        sliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
        sliderStyle.normal.background = sliderTexture;
        sliderStyle.fixedHeight = 10f;

        sliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
        sliderThumbStyle.normal.background = sliderThumbTexture;
        sliderThumbStyle.hover.background = buttonHoverTexture;
        sliderThumbStyle.active.background = buttonActiveTexture;
        sliderThumbStyle.fixedWidth = 20f;
        sliderThumbStyle.fixedHeight = 20f;

        if (customFont != null)
        {
            titleStyle.font = customFont;
            labelStyle.font = customFont;
            buttonStyle.font = customFont;
        }

        stylesCreated = true;
    }

    private void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(VolumeKey, AudioListener.volume);
        fullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
        vSync = PlayerPrefs.GetInt(VSyncKey, QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;
        qualityIndex = PlayerPrefs.GetInt(QualityKey, QualitySettings.GetQualityLevel());
        AudioListener.volume = masterVolume;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat(VolumeKey, masterVolume);
        PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
        PlayerPrefs.SetInt(VSyncKey, vSync ? 1 : 0);
        PlayerPrefs.SetInt(QualityKey, qualityIndex);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (pixelTexture != null) Destroy(pixelTexture);
        if (panelTexture != null) Destroy(panelTexture);
        if (buttonTexture != null) Destroy(buttonTexture);
        if (buttonHoverTexture != null) Destroy(buttonHoverTexture);
        if (buttonActiveTexture != null) Destroy(buttonActiveTexture);
        if (sliderTexture != null) Destroy(sliderTexture);
        if (sliderThumbTexture != null) Destroy(sliderThumbTexture);
    }
}
