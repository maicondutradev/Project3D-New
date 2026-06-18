using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private enum MenuPage
    {
        Main,
        Maps,
        Options,
        Credits,
        ExitConfirmation
    }

    [Header("Cenas")]
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private string mainSceneName = "MainScene";
    [SerializeField] private string infernoSceneName = "Inferno";

    [Header("Textos")]
    [SerializeField] private string gameTitle = "REINO ARCANO";
    [SerializeField] private string gameSubtitle = "A Jornada do Mago";
    [SerializeField] private string versionText = "Versão 1.0";

    [Header("Imagens")]
    [SerializeField] private Texture2D backgroundImage;
    [SerializeField] private Texture2D logoImage;
    [SerializeField] private Font menuFont;

    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    private const float ReferenceWidth = 1920f;
    private const float ReferenceHeight = 1080f;

    private const string VolumeKey = "MagicGame.Volume";
    private const string FullscreenKey = "MagicGame.Fullscreen";
    private const string VSyncKey = "MagicGame.VSync";
    private const string QualityKey = "MagicGame.Quality";

    private static MainMenu instance;

    private MenuPage currentPage = MenuPage.Main;

    private float masterVolume = 1f;

    private bool isLoading;
    private float fadeAlpha;

    private string warningMessage = "";
    private float warningTime;

    private Texture2D whiteTexture;
    private Texture2D fallbackBackground;
    private Texture2D panelTexture;
    private Texture2D buttonTexture;
    private Texture2D buttonHoverTexture;
    private Texture2D buttonPressedTexture;
    private Texture2D secondaryButtonTexture;
    private Texture2D sliderTexture;
    private Texture2D sliderThumbTexture;

    private GUIStyle panelStyle;
    private GUIStyle titleStyle;
    private GUIStyle subtitleStyle;
    private GUIStyle sectionStyle;
    private GUIStyle descriptionStyle;
    private GUIStyle buttonStyle;
    private GUIStyle secondaryButtonStyle;
    private GUIStyle optionLabelStyle;
    private GUIStyle smallButtonStyle;
    private GUIStyle warningStyle;
    private GUIStyle sliderStyle;
    private GUIStyle sliderThumbStyle;

    private bool stylesCreated;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name != menuSceneName)
        {
            enabled = false;
            return;
        }

        if (instance != null && instance != this)
        {
            enabled = false;
            return;
        }

        instance = this;

        CreateTextures();
        LoadSettings();
    }

    private void Start()
    {
        if (!enabled)
            return;

        Time.timeScale = 1f;
        AudioListener.pause = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        if (!enabled || isLoading)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (!keyboard.escapeKey.wasPressedThisFrame)
            return;

        if (currentPage == MenuPage.Main)
            return;

        PlayClick();
        currentPage = MenuPage.Main;
    }

    private void OnGUI()
    {
        if (!enabled)
            return;

        CreateStyles();
        DrawBackground();

        Matrix4x4 oldMatrix = GUI.matrix;

        float scale = Mathf.Min(
            Screen.width / ReferenceWidth,
            Screen.height / ReferenceHeight
        );

        float offsetX = (Screen.width - ReferenceWidth * scale) * 0.5f;
        float offsetY = (Screen.height - ReferenceHeight * scale) * 0.5f;

        GUI.matrix = Matrix4x4.TRS(
            new Vector3(offsetX, offsetY, 0f),
            Quaternion.identity,
            new Vector3(scale, scale, 1f)
        );

        DrawHeader();

        switch (currentPage)
        {
            case MenuPage.Main:
                DrawMainPage();
                break;

            case MenuPage.Maps:
                DrawMapPage();
                break;

            case MenuPage.Options:
                DrawOptionsPage();
                break;

            case MenuPage.Credits:
                DrawCreditsPage();
                break;

            case MenuPage.ExitConfirmation:
                DrawExitConfirmation();
                break;
        }

        GUI.matrix = oldMatrix;

        if (fadeAlpha > 0f)
        {
            Color oldColor = GUI.color;

            GUI.color = new Color(0f, 0f, 0f, fadeAlpha);

            GUI.DrawTexture(
                new Rect(0f, 0f, Screen.width, Screen.height),
                whiteTexture
            );

            GUI.color = oldColor;
        }
    }

    private void DrawBackground()
    {
        Texture background = backgroundImage != null
            ? backgroundImage
            : fallbackBackground;

        GUI.DrawTexture(
            new Rect(0f, 0f, Screen.width, Screen.height),
            background,
            ScaleMode.ScaleAndCrop
        );

        Color oldColor = GUI.color;

        GUI.color = new Color(0.01f, 0.015f, 0.035f, 0.25f);

        GUI.DrawTexture(
            new Rect(0f, 0f, Screen.width, Screen.height),
            whiteTexture
        );

        GUI.color = oldColor;
    }

    private void DrawHeader()
    {
        if (logoImage != null)
        {
            GUI.DrawTexture(
                new Rect(ReferenceWidth * 0.5f - 65f, 32f, 130f, 130f),
                logoImage,
                ScaleMode.ScaleToFit
            );
        }

        float titleY = logoImage != null ? 148f : 55f;

        GUI.Label(
            new Rect(0f, titleY, ReferenceWidth, 70f),
            gameTitle,
            titleStyle
        );

        GUI.Label(
            new Rect(0f, titleY + 67f, ReferenceWidth, 42f),
            gameSubtitle,
            subtitleStyle
        );
    }

    private void DrawMainPage()
    {
        Rect panelRect = GetCenteredPanel(570f, 650f, 95f);

        GUI.Box(panelRect, GUIContent.none, panelStyle);

        GUILayout.BeginArea(new Rect(
            panelRect.x + 45f,
            panelRect.y + 38f,
            panelRect.width - 90f,
            panelRect.height - 76f
        ));

        GUILayout.Label("MENU PRINCIPAL", sectionStyle);

        GUILayout.Space(12f);

        GUILayout.Label(
            "Prepare seus feitiços e escolha o próximo destino.",
            descriptionStyle
        );

        GUILayout.Space(38f);

        if (DrawMainButton("JOGAR"))
            LoadScene(mainSceneName);

        GUILayout.Space(18f);

        if (DrawMainButton("CONTINUAR"))
            LoadScene(mainSceneName);

        GUILayout.Space(18f);

        if (DrawMainButton("SELECIONAR MAPA"))
            currentPage = MenuPage.Maps;

        GUILayout.Space(18f);

        if (DrawMainButton("OPÇÕES"))
            currentPage = MenuPage.Options;

        GUILayout.Space(18f);

        if (DrawMainButton("CRÉDITOS"))
            currentPage = MenuPage.Credits;

        GUILayout.Space(18f);

        if (DrawMainButton("SAIR"))
            currentPage = MenuPage.ExitConfirmation;

        GUILayout.FlexibleSpace();

        DrawWarning();

        GUILayout.Label(versionText, descriptionStyle);

        GUILayout.EndArea();
    }

    private void DrawMapPage()
    {
        Rect panelRect = GetCenteredPanel(660f, 650f, 90f);

        GUI.Box(panelRect, GUIContent.none, panelStyle);

        GUILayout.BeginArea(new Rect(
            panelRect.x + 48f,
            panelRect.y + 38f,
            panelRect.width - 96f,
            panelRect.height - 76f
        ));

        GUILayout.Label("ESCOLHA O DESTINO", sectionStyle);

        GUILayout.Space(12f);

        GUILayout.Label(
            "Selecione o mapa onde a jornada do mago começará.",
            descriptionStyle
        );

        GUILayout.Space(40f);

        if (DrawMainButton("VILA ENCANTADA"))
            LoadScene(mainSceneName);

        GUILayout.Space(18f);

        if (DrawMainButton("REINO INFERNAL"))
            LoadScene(infernoSceneName);

        GUILayout.FlexibleSpace();

        if (DrawSecondaryButton("VOLTAR"))
            currentPage = MenuPage.Main;

        DrawWarning();

        GUILayout.EndArea();
    }

    private void DrawOptionsPage()
    {
        Rect panelRect = GetCenteredPanel(720f, 350f, 90f);

        GUI.Box(panelRect, GUIContent.none, panelStyle);

        GUILayout.BeginArea(new Rect(
            panelRect.x + 55f,
            panelRect.y + 38f,
            panelRect.width - 110f,
            panelRect.height - 76f
        ));

        GUILayout.Label("OPÇÕES", sectionStyle);

        GUILayout.Space(30f);

        GUILayout.Label(
            $"VOLUME: {Mathf.RoundToInt(masterVolume * 100f)}%",
            optionLabelStyle
        );

        float newVolume = GUILayout.HorizontalSlider(
            masterVolume,
            0f,
            1f,
            sliderStyle,
            sliderThumbStyle,
            GUILayout.Height(38f)
        );

        if (!Mathf.Approximately(newVolume, masterVolume))
        {
            masterVolume = newVolume;
            AudioListener.volume = masterVolume;
            SaveSettings();
        }

        GUILayout.FlexibleSpace();

        if (DrawSecondaryButton("VOLTAR"))
            currentPage = MenuPage.Main;

        GUILayout.EndArea();
    }

    private void DrawCreditsPage()
    {
        Rect panelRect = GetCenteredPanel(720f, 350f, 90f);

        GUI.Box(panelRect, GUIContent.none, panelStyle);

        GUILayout.BeginArea(new Rect(
            panelRect.x + 55f,
            panelRect.y + 38f,
            panelRect.width - 110f,
            panelRect.height - 76f
        ));

        GUILayout.Label("CRÉDITOS", sectionStyle);

        GUILayout.Space(30f);

        GUILayout.Label(
            "Jogo Mago Arcano\nObrigado por jogar!\n\nDesenvolvido com Unity.",
            descriptionStyle
        );

        GUILayout.FlexibleSpace();

        if (DrawSecondaryButton("VOLTAR"))
            currentPage = MenuPage.Main;

        GUILayout.EndArea();
    }

    private void DrawExitConfirmation()
    {
        Rect panelRect = GetCenteredPanel(590f, 390f, 90f);

        GUI.Box(panelRect, GUIContent.none, panelStyle);

        GUILayout.BeginArea(new Rect(
            panelRect.x + 48f,
            panelRect.y + 40f,
            panelRect.width - 96f,
            panelRect.height - 80f
        ));

        GUILayout.Label("SAIR DO JOGO?", sectionStyle);

        GUILayout.Space(20f);

        GUILayout.Label(
            "Deseja realmente abandonar sua jornada?",
            descriptionStyle
        );

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button(
                "CANCELAR",
                secondaryButtonStyle,
                GUILayout.Height(64f)))
        {
            PlayClick();
            currentPage = MenuPage.Main;
        }

        GUILayout.Space(15f);

        if (GUILayout.Button(
                "SAIR",
                buttonStyle,
                GUILayout.Height(64f)))
        {
            PlayClick();
            QuitGame();
        }

        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    private bool DrawMainButton(string text)
    {
        bool clicked = GUILayout.Button(
            text,
            buttonStyle,
            GUILayout.Height(70f)
        );

        if (clicked)
            PlayClick();

        return clicked;
    }

    private bool DrawSecondaryButton(string text)
    {
        bool clicked = GUILayout.Button(
            text,
            secondaryButtonStyle,
            GUILayout.Height(58f)
        );

        if (clicked)
            PlayClick();

        return clicked;
    }



    private void LoadScene(string sceneName)
    {
        if (isLoading)
            return;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            ShowWarning("O nome da cena não foi configurado.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            ShowWarning($"A cena \"{sceneName}\" não está no Build Profiles.");

            Debug.LogError(
                $"A cena {sceneName} não está adicionada ao Build Profiles."
            );

            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;

        Time.timeScale = 1f;
        AudioListener.pause = false;

        float duration = 0.45f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeAlpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
            yield return null;
    }

    private void ShowWarning(string message)
    {
        warningMessage = message;
        warningTime = Time.unscaledTime + 4f;
    }

    private void DrawWarning()
    {
        if (Time.unscaledTime > warningTime)
            return;

        GUILayout.Space(12f);
        GUILayout.Label(warningMessage, warningStyle);
    }

    private void PlayClick()
    {
        if (audioSource == null || buttonSound == null)
            return;

        audioSource.PlayOneShot(buttonSound);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        AudioListener.volume = masterVolume;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat(VolumeKey, masterVolume);
        PlayerPrefs.Save();
    }

    private Rect GetCenteredPanel(
        float width,
        float height,
        float verticalOffset
    )
    {
        return new Rect(
            (ReferenceWidth - width) * 0.5f,
            (ReferenceHeight - height) * 0.5f + verticalOffset,
            width,
            height
        );
    }

    private void CreateStyles()
    {
        if (stylesCreated)
            return;

        panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.normal.background = panelTexture;
        panelStyle.padding = new RectOffset(25, 25, 25, 25);

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 56,
            fontStyle = FontStyle.Bold,
            normal =
            {
                textColor = new Color(0.89f, 0.78f, 1f)
            }
        };

        subtitleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 23,
            fontStyle = FontStyle.Italic,
            normal =
            {
                textColor = new Color(0.93f, 0.9f, 1f)
            }
        };

        sectionStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 27,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal =
            {
                textColor = new Color(0.91f, 0.82f, 1f)
            }
        };

        descriptionStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            wordWrap = true,
            normal =
            {
                textColor = new Color(0.82f, 0.79f, 0.88f)
            }
        };

        optionLabelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 19,
            fontStyle = FontStyle.Bold,
            normal =
            {
                textColor = new Color(0.92f, 0.89f, 0.97f)
            }
        };

        warningStyle = new GUIStyle(descriptionStyle)
        {
            fontStyle = FontStyle.Bold,
            normal =
            {
                textColor = new Color(1f, 0.65f, 0.45f)
            }
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 22,
            fontStyle = FontStyle.Bold
        };

        buttonStyle.normal.background = buttonTexture;
        buttonStyle.normal.textColor = new Color(0.96f, 0.91f, 1f);
        buttonStyle.hover.background = buttonHoverTexture;
        buttonStyle.hover.textColor = Color.white;
        buttonStyle.active.background = buttonPressedTexture;
        buttonStyle.active.textColor = Color.white;

        secondaryButtonStyle = new GUIStyle(buttonStyle)
        {
            fontSize = 19
        };

        secondaryButtonStyle.normal.background = secondaryButtonTexture;

        smallButtonStyle = new GUIStyle(buttonStyle)
        {
            fontSize = 16
        };

        sliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
        sliderStyle.normal.background = sliderTexture;
        sliderStyle.fixedHeight = 10f;

        sliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
        sliderThumbStyle.normal.background = sliderThumbTexture;
        sliderThumbStyle.hover.background = buttonHoverTexture;
        sliderThumbStyle.active.background = buttonPressedTexture;
        sliderThumbStyle.fixedWidth = 26f;
        sliderThumbStyle.fixedHeight = 26f;

        ApplyFont(titleStyle);
        ApplyFont(subtitleStyle);
        ApplyFont(sectionStyle);
        ApplyFont(descriptionStyle);
        ApplyFont(optionLabelStyle);
        ApplyFont(warningStyle);
        ApplyFont(buttonStyle);
        ApplyFont(secondaryButtonStyle);
        ApplyFont(smallButtonStyle);

        stylesCreated = true;
    }

    private void ApplyFont(GUIStyle style)
    {
        if (menuFont != null)
            style.font = menuFont;
    }

    private void CreateTextures()
    {
        whiteTexture = CreateSolidTexture(Color.white);

        fallbackBackground = CreateGradientTexture(
            new Color(0.12f, 0.05f, 0.25f),
            new Color(0.01f, 0.015f, 0.04f)
        );

        panelTexture = CreateSolidTexture(
            new Color(0.015f, 0.01f, 0.055f, 0.94f)
        );

        buttonTexture = CreateSolidTexture(
            new Color(0.15f, 0.065f, 0.27f, 0.98f)
        );

        buttonHoverTexture = CreateSolidTexture(
            new Color(0.29f, 0.12f, 0.48f, 1f)
        );

        buttonPressedTexture = CreateSolidTexture(
            new Color(0.44f, 0.2f, 0.66f, 1f)
        );

        secondaryButtonTexture = CreateSolidTexture(
            new Color(0.09f, 0.045f, 0.17f, 0.98f)
        );

        sliderTexture = CreateSolidTexture(
            new Color(0.09f, 0.045f, 0.15f, 1f)
        );

        sliderThumbTexture = CreateSolidTexture(
            new Color(0.68f, 0.4f, 0.96f, 1f)
        );
    }

    private Texture2D CreateSolidTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.name = "GeneratedMainMenuTexture";
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixel(0, 0, color);
        texture.Apply();

        return texture;
    }

    private Texture2D CreateGradientTexture(Color top, Color bottom)
    {
        const int height = 256;

        Texture2D texture = new Texture2D(1, height);
        texture.name = "GeneratedMainMenuBackground";
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < height; y++)
        {
            float amount = y / (height - 1f);
            texture.SetPixel(0, y, Color.Lerp(bottom, top, amount));
        }

        texture.Apply();

        return texture;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;

        DestroyTexture(whiteTexture);
        DestroyTexture(fallbackBackground);
        DestroyTexture(panelTexture);
        DestroyTexture(buttonTexture);
        DestroyTexture(buttonHoverTexture);
        DestroyTexture(buttonPressedTexture);
        DestroyTexture(secondaryButtonTexture);
        DestroyTexture(sliderTexture);
        DestroyTexture(sliderThumbTexture);
    }

    private void DestroyTexture(Texture2D texture)
    {
        if (texture != null)
            Destroy(texture);
    }
}
