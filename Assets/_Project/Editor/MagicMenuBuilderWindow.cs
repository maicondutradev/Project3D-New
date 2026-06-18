#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public sealed class MagicMenuBuilderWindow : EditorWindow
{
    private Sprite backgroundSprite;
    private string menuSceneName = "MainMenu";
    private string mainSceneName = "MainScene";
    private string infernoSceneName = "Inferno";

    private static readonly Color Gold = new Color(0.94f, 0.73f, 0.32f, 1f);
    private static readonly Color Cream = new Color(1f, 0.91f, 0.69f, 1f);
    private static readonly Color DarkPanel = new Color(0.035f, 0.025f, 0.045f, 0.96f);
    private static readonly Color Brown = new Color(0.19f, 0.10f, 0.055f, 0.98f);
    private static readonly Color Blue = new Color(0.02f, 0.35f, 0.88f, 0.92f);

    [MenuItem("Tools/Mago Arcano/Criar menus do jogo")]
    public static void OpenWindow()
    {
        GetWindow<MagicMenuBuilderWindow>("Mago Arcano UI");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Construtor do Menu Mago Arcano", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Este construtor cria o Canvas do menu principal usando a imagem como fundo e coloca botões transparentes exatamente sobre os botões desenhados.",
            MessageType.Info);

        backgroundSprite = (Sprite)EditorGUILayout.ObjectField(
            "Imagem do menu",
            backgroundSprite,
            typeof(Sprite),
            false);

        menuSceneName = EditorGUILayout.TextField("Cena do menu", menuSceneName);
        mainSceneName = EditorGUILayout.TextField("Cena principal", mainSceneName);
        infernoSceneName = EditorGUILayout.TextField("Cena Inferno", infernoSceneName);

        EditorGUILayout.Space(10f);

        GUI.backgroundColor = new Color(0.35f, 0.75f, 1f);
        if (GUILayout.Button("CRIAR MENU PRINCIPAL NESTA CENA", GUILayout.Height(42f)))
            BuildMainMenu();

        GUI.backgroundColor = new Color(0.73f, 0.49f, 0.93f);
        if (GUILayout.Button("CRIAR MENU DE PAUSA NESTA CENA", GUILayout.Height(42f)))
            BuildPauseMenu();

        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(10f);
        EditorGUILayout.HelpBox(
            "Antes de usar a imagem: selecione o PNG no Project, altere Texture Type para Sprite (2D and UI) e clique em Apply.",
            MessageType.Warning);
    }

    private void BuildMainMenu()
    {
        RemoveExisting("MagoArcano_MainMenu");
        EnsureInputSystemEventSystem();

        GameObject root = new GameObject("MagoArcano_MainMenu");
        Undo.RegisterCreatedObjectUndo(root, "Criar Menu Mago Arcano");

        MainMenuController controller = root.AddComponent<MainMenuController>();
        AudioSource audioSource = root.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        Canvas canvas = CreateCanvas("MainMenuCanvas", root.transform);
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        Image background = CreateImage("Background", canvasRect, backgroundSprite, Color.white);
        Stretch(background.rectTransform);
        background.preserveAspect = false;
        background.raycastTarget = false;

        GameObject hotspots = CreateRectObject("Hotspots", canvasRect);
        Stretch(hotspots.GetComponent<RectTransform>());

        Button playButton = CreateHotspotButton("Jogar", hotspots.transform, 198f, 346f, 356f, 70f);
        Button continueButton = CreateHotspotButton("Continuar", hotspots.transform, 198f, 426f, 356f, 69f);
        Button mapButton = CreateHotspotButton("SelecionarMapa", hotspots.transform, 198f, 505f, 356f, 69f);
        Button optionsButton = CreateHotspotButton("Opcoes", hotspots.transform, 198f, 581f, 356f, 69f);
        Button creditsButton = CreateHotspotButton("Creditos", hotspots.transform, 198f, 657f, 356f, 69f);
        Button exitButton = CreateHotspotButton("Sair", hotspots.transform, 198f, 734f, 356f, 69f);

        GameObject mapPanel = CreateMapPanel(canvasRect, controller);
        GameObject optionsPanel = CreateOptionsPanel(canvasRect, out Slider volumeSlider, out Toggle fullscreenToggle, out Toggle vSyncToggle);
        GameObject creditsPanel = CreateCreditsPanel(canvasRect);
        GameObject exitPanel = CreateExitPanel(canvasRect, controller);

        Text statusText = CreateText("StatusText", canvasRect, string.Empty, 20, Cream, TextAnchor.MiddleCenter);
        SetRect(statusText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 38f), new Vector2(760f, 42f));
        statusText.raycastTarget = false;

        GameObject fadeObject = CreateRectObject("FadeOverlay", canvasRect);
        RectTransform fadeRect = fadeObject.GetComponent<RectTransform>();
        Stretch(fadeRect);
        Image fadeImage = fadeObject.AddComponent<Image>();
        fadeImage.color = new Color(0.005f, 0.01f, 0.025f, 1f);
        CanvasGroup fadeGroup = fadeObject.AddComponent<CanvasGroup>();
        fadeGroup.alpha = 0f;
        fadeGroup.interactable = false;
        fadeGroup.blocksRaycasts = false;

        Text loadingText = CreateText("LoadingText", fadeRect, "Abrindo portal mágico...", 30, Gold, TextAnchor.MiddleCenter);
        SetRect(loadingText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 80f));

        UnityEventTools.AddPersistentListener(playButton.onClick, controller.PlayNewGame);
        UnityEventTools.AddPersistentListener(continueButton.onClick, controller.ContinueGame);
        UnityEventTools.AddPersistentListener(mapButton.onClick, controller.OpenMapPanel);
        UnityEventTools.AddPersistentListener(optionsButton.onClick, controller.OpenOptionsPanel);
        UnityEventTools.AddPersistentListener(creditsButton.onClick, controller.OpenCreditsPanel);
        UnityEventTools.AddPersistentListener(exitButton.onClick, controller.OpenExitPanel);

        AssignMainControllerReferences(
            controller,
            hotspots,
            mapPanel,
            optionsPanel,
            creditsPanel,
            exitPanel,
            continueButton,
            volumeSlider,
            fullscreenToggle,
            vSyncToggle,
            statusText,
            fadeGroup,
            loadingText,
            audioSource);

        SetPrivateString(controller, "menuSceneName", menuSceneName);
        SetPrivateString(controller, "defaultGameScene", mainSceneName);
        SetPrivateString(controller, "infernoScene", infernoSceneName);

        mapPanel.SetActive(false);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        exitPanel.SetActive(false);

        Selection.activeGameObject = root;
        EditorUtility.SetDirty(root);
        Debug.Log("Menu principal Mago Arcano criado com sucesso.");
    }

    private void BuildPauseMenu()
    {
        RemoveExisting("MagoArcano_PauseMenu");
        EnsureInputSystemEventSystem();

        GameObject root = new GameObject("MagoArcano_PauseMenu");
        Undo.RegisterCreatedObjectUndo(root, "Criar Pause Menu");

        PauseMenuController controller = root.AddComponent<PauseMenuController>();

        Canvas canvas = CreateCanvas("PauseCanvas", root.transform);
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        Image dim = CreateImage("Dim", canvasRect, null, new Color(0f, 0f, 0.025f, 0.72f));
        Stretch(dim.rectTransform);

        GameObject mainPanel = CreatePanel("PauseMainPanel", canvasRect, new Vector2(0f, 0f), new Vector2(560f, 540f));
        Text title = CreateText("Title", mainPanel.transform, "JOGO PAUSADO", 38, new Color(0.89f, 0.76f, 1f), TextAnchor.MiddleCenter);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -66f), new Vector2(500f, 66f));

        Button resume = CreateVisibleButton("Continuar", mainPanel.transform, "CONTINUAR", new Vector2(0f, 80f));
        Button options = CreateVisibleButton("Opcoes", mainPanel.transform, "OPÇÕES", new Vector2(0f, 0f));
        Button restart = CreateVisibleButton("Reiniciar", mainPanel.transform, "REINICIAR FASE", new Vector2(0f, -80f));
        Button menu = CreateVisibleButton("MenuPrincipal", mainPanel.transform, "MENU PRINCIPAL", new Vector2(0f, -160f));

        GameObject optionsPanel = CreatePanel("PauseOptionsPanel", canvasRect, Vector2.zero, new Vector2(620f, 560f));
        Text optionsTitle = CreateText("Title", optionsPanel.transform, "OPÇÕES", 36, new Color(0.89f, 0.76f, 1f), TextAnchor.MiddleCenter);
        SetRect(optionsTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(500f, 60f));

        Text volumeLabel = CreateText("VolumeLabel", optionsPanel.transform, "VOLUME GERAL", 20, Cream, TextAnchor.MiddleLeft);
        SetRect(volumeLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-125f, 105f), new Vector2(260f, 42f));
        Slider pauseVolume = CreateSlider("VolumeSlider", optionsPanel.transform, new Vector2(135f, 105f));

        Text fullLabel = CreateText("FullscreenLabel", optionsPanel.transform, "TELA CHEIA", 20, Cream, TextAnchor.MiddleLeft);
        SetRect(fullLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-125f, 30f), new Vector2(260f, 42f));
        Toggle pauseFullscreen = CreateToggle("FullscreenToggle", optionsPanel.transform, new Vector2(185f, 30f));

        Text syncLabel = CreateText("VSyncLabel", optionsPanel.transform, "SINCRONIZAÇÃO VERTICAL", 20, Cream, TextAnchor.MiddleLeft);
        SetRect(syncLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-125f, -45f), new Vector2(300f, 42f));
        Toggle pauseVSync = CreateToggle("VSyncToggle", optionsPanel.transform, new Vector2(185f, -45f));

        Button back = CreateVisibleButton("Voltar", optionsPanel.transform, "VOLTAR", new Vector2(0f, -190f));

        UnityEventTools.AddPersistentListener(resume.onClick, controller.ResumeGame);
        UnityEventTools.AddPersistentListener(options.onClick, controller.OpenOptions);
        UnityEventTools.AddPersistentListener(restart.onClick, controller.RestartCurrentScene);
        UnityEventTools.AddPersistentListener(menu.onClick, controller.ReturnToMainMenu);
        UnityEventTools.AddPersistentListener(back.onClick, controller.ShowPauseMain);

        AssignPauseControllerReferences(
            controller,
            canvas.gameObject,
            mainPanel,
            optionsPanel,
            pauseVolume,
            pauseFullscreen,
            pauseVSync);

        SetPrivateString(controller, "mainMenuSceneName", menuSceneName);

        optionsPanel.SetActive(false);
        canvas.gameObject.SetActive(false);

        Selection.activeGameObject = root;
        EditorUtility.SetDirty(root);
        Debug.Log("Menu de pausa Mago Arcano criado com sucesso.");
    }

    private GameObject CreateMapPanel(RectTransform parent, MainMenuController controller)
    {
        GameObject panel = CreatePanel("MapPanel", parent, new Vector2(360f, 0f), new Vector2(580f, 530f));

        Text title = CreateText("Title", panel.transform, "SELECIONAR MAPA", 32, Gold, TextAnchor.MiddleCenter);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(520f, 60f));

        Text description = CreateText("Description", panel.transform,
            "Escolha o reino onde a aventura do mago começará.",
            18,
            new Color(0.8f, 0.84f, 0.9f),
            TextAnchor.MiddleCenter);
        SetRect(description.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -108f), new Vector2(500f, 55f));

        Button mainMap = CreateVisibleButton("VilaEncantada", panel.transform, "VILA ENCANTADA", new Vector2(0f, 70f));
        Button infernoMap = CreateVisibleButton("ReinoInfernal", panel.transform, "REINO INFERNAL", new Vector2(0f, -10f));
        Button back = CreateVisibleButton("Voltar", panel.transform, "VOLTAR", new Vector2(0f, -165f));

        UnityEventTools.AddPersistentListener(mainMap.onClick, controller.LoadMainMap);
        UnityEventTools.AddPersistentListener(infernoMap.onClick, controller.LoadInfernoMap);
        UnityEventTools.AddPersistentListener(back.onClick, controller.ClosePanels);

        return panel;
    }

    private GameObject CreateOptionsPanel(
        RectTransform parent,
        out Slider volumeSlider,
        out Toggle fullscreenToggle,
        out Toggle vSyncToggle)
    {
        GameObject panel = CreatePanel("OptionsPanel", parent, new Vector2(360f, 0f), new Vector2(620f, 570f));

        Text title = CreateText("Title", panel.transform, "OPÇÕES", 32, Gold, TextAnchor.MiddleCenter);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(500f, 60f));

        Text volumeLabel = CreateText("VolumeLabel", panel.transform, "VOLUME GERAL", 20, Cream, TextAnchor.MiddleLeft);
        SetRect(volumeLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-130f, 115f), new Vector2(260f, 42f));
        volumeSlider = CreateSlider("VolumeSlider", panel.transform, new Vector2(145f, 115f));

        Text fullLabel = CreateText("FullscreenLabel", panel.transform, "TELA CHEIA", 20, Cream, TextAnchor.MiddleLeft);
        SetRect(fullLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-130f, 35f), new Vector2(260f, 42f));
        fullscreenToggle = CreateToggle("FullscreenToggle", panel.transform, new Vector2(205f, 35f));

        Text vSyncLabel = CreateText("VSyncLabel", panel.transform, "SINCRONIZAÇÃO VERTICAL", 20, Cream, TextAnchor.MiddleLeft);
        SetRect(vSyncLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-130f, -45f), new Vector2(310f, 42f));
        vSyncToggle = CreateToggle("VSyncToggle", panel.transform, new Vector2(205f, -45f));

        Button back = CreateVisibleButton("Voltar", panel.transform, "VOLTAR", new Vector2(0f, -190f));
        MainMenuController controller = parent.GetComponentInParent<MainMenuController>();
        if (controller != null)
            UnityEventTools.AddPersistentListener(back.onClick, controller.ClosePanels);

        return panel;
    }

    private GameObject CreateCreditsPanel(RectTransform parent)
    {
        GameObject panel = CreatePanel("CreditsPanel", parent, new Vector2(360f, 0f), new Vector2(580f, 500f));

        Text title = CreateText("Title", panel.transform, "CRÉDITOS", 32, Gold, TextAnchor.MiddleCenter);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(500f, 60f));

        Text body = CreateText(
            "Body",
            panel.transform,
            "MAGO ARCANO\n\nConceito, programação e direção criativa\nErick Matheus\n\nMenu construído com Unity Canvas e o novo Input System.",
            20,
            Cream,
            TextAnchor.MiddleCenter);
        SetRect(body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(500f, 260f));

        Button back = CreateVisibleButton("Voltar", panel.transform, "VOLTAR", new Vector2(0f, -165f));
        MainMenuController controller = parent.GetComponentInParent<MainMenuController>();
        if (controller != null)
            UnityEventTools.AddPersistentListener(back.onClick, controller.ClosePanels);

        return panel;
    }

    private GameObject CreateExitPanel(RectTransform parent, MainMenuController controller)
    {
        GameObject panel = CreatePanel("ExitPanel", parent, new Vector2(360f, 0f), new Vector2(560f, 380f));

        Text title = CreateText("Title", panel.transform, "SAIR DO JOGO?", 32, Gold, TextAnchor.MiddleCenter);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(500f, 60f));

        Text question = CreateText("Question", panel.transform,
            "Deseja realmente abandonar sua jornada?",
            20,
            Cream,
            TextAnchor.MiddleCenter);
        SetRect(question.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 35f), new Vector2(480f, 80f));

        Button cancel = CreateVisibleButton("Cancelar", panel.transform, "CANCELAR", new Vector2(-125f, -100f), new Vector2(220f, 62f));
        Button confirm = CreateVisibleButton("Confirmar", panel.transform, "SAIR", new Vector2(125f, -100f), new Vector2(220f, 62f));

        UnityEventTools.AddPersistentListener(cancel.onClick, controller.ClosePanels);
        UnityEventTools.AddPersistentListener(confirm.onClick, controller.ConfirmExit);

        return panel;
    }

    private static Canvas CreateCanvas(string name, Transform parent)
    {
        GameObject canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(parent, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color)
    {
        GameObject go = CreateRectObject(name, parent);
        Image image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        return image;
    }

    private static Button CreateHotspotButton(string name, Transform parent, float x, float y, float width, float height)
    {
        GameObject go = CreateRectObject(name, parent);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);

        Image image = go.AddComponent<Image>();
        image.color = new Color(0.08f, 0.45f, 1f, 0.01f);
        image.raycastTarget = true;

        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        button.colors = new ColorBlock
        {
            normalColor = new Color(0.08f, 0.45f, 1f, 0.01f),
            highlightedColor = new Color(0.20f, 0.52f, 1f, 0.12f),
            pressedColor = new Color(0.26f, 0.67f, 1f, 0.18f),
            selectedColor = new Color(0.20f, 0.52f, 1f, 0.12f),
            disabledColor = new Color(0f, 0f, 0f, 0.40f),
            colorMultiplier = 1f,
            fadeDuration = 0.18f
        };

        go.AddComponent<MenuHotspotEffect>();
        return button;
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject panel = CreateRectObject(name, parent);
        RectTransform rect = panel.GetComponent<RectTransform>();
        SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, size);

        Image image = panel.AddComponent<Image>();
        image.color = DarkPanel;

        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(0.64f, 0.39f, 0.12f, 0.95f);
        outline.effectDistance = new Vector2(3f, -3f);

        return panel;
    }

    private static Button CreateVisibleButton(
        string name,
        Transform parent,
        string label,
        Vector2 anchoredPosition,
        Vector2? size = null)
    {
        GameObject go = CreateRectObject(name, parent);
        RectTransform rect = go.GetComponent<RectTransform>();
        SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, size ?? new Vector2(420f, 64f));

        Image image = go.AddComponent<Image>();
        image.color = Brown;

        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0.61f, 0.39f, 0.14f, 0.95f);
        outline.effectDistance = new Vector2(2f, -2f);

        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        button.colors = new ColorBlock
        {
            normalColor = Color.white,
            highlightedColor = new Color(0.76f, 0.60f, 0.30f, 1f),
            pressedColor = new Color(0.56f, 0.42f, 0.21f, 1f),
            selectedColor = new Color(0.76f, 0.60f, 0.30f, 1f),
            disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.6f),
            colorMultiplier = 1f,
            fadeDuration = 0.18f
        };

        Text text = CreateText("Label", go.transform, label, 22, Cream, TextAnchor.MiddleCenter);
        Stretch(text.rectTransform);
        text.raycastTarget = false;

        go.AddComponent<ButtonAnimationEffect>();

        return button;
    }

    private static Slider CreateSlider(string name, Transform parent, Vector2 anchoredPosition)
    {
        GameObject root = CreateRectObject(name, parent);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        SetRect(rootRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, new Vector2(250f, 36f));

        Slider slider = root.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.8f;

        Image background = CreateImage("Background", rootRect, null, new Color(0.12f, 0.08f, 0.15f, 1f));
        SetRect(background.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(0f, 10f));

        GameObject fillAreaObject = CreateRectObject("Fill Area", rootRect);
        RectTransform fillArea = fillAreaObject.GetComponent<RectTransform>();
        fillArea.anchorMin = new Vector2(0f, 0.5f);
        fillArea.anchorMax = new Vector2(1f, 0.5f);
        fillArea.offsetMin = new Vector2(5f, -5f);
        fillArea.offsetMax = new Vector2(-15f, 5f);

        Image fill = CreateImage("Fill", fillArea, null, Blue);
        fill.rectTransform.anchorMin = new Vector2(0f, 0f);
        fill.rectTransform.anchorMax = new Vector2(1f, 1f);
        fill.rectTransform.offsetMin = Vector2.zero;
        fill.rectTransform.offsetMax = Vector2.zero;

        GameObject handleAreaObject = CreateRectObject("Handle Slide Area", rootRect);
        RectTransform handleArea = handleAreaObject.GetComponent<RectTransform>();
        handleArea.anchorMin = Vector2.zero;
        handleArea.anchorMax = Vector2.one;
        handleArea.offsetMin = new Vector2(10f, 0f);
        handleArea.offsetMax = new Vector2(-10f, 0f);

        Image handle = CreateImage("Handle", handleArea, null, new Color(0.43f, 0.84f, 1f, 1f));
        SetRect(handle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(24f, 24f));

        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    private static Toggle CreateToggle(string name, Transform parent, Vector2 anchoredPosition)
    {
        GameObject root = CreateRectObject(name, parent);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        SetRect(rootRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, new Vector2(52f, 52f));

        Image background = root.AddComponent<Image>();
        background.color = new Color(0.12f, 0.07f, 0.12f, 1f);

        Outline outline = root.AddComponent<Outline>();
        outline.effectColor = new Color(0.67f, 0.43f, 0.15f, 1f);
        outline.effectDistance = new Vector2(2f, -2f);

        Toggle toggle = root.AddComponent<Toggle>();
        toggle.targetGraphic = background;

        Image checkmark = CreateImage("Checkmark", rootRect, null, new Color(0.18f, 0.70f, 1f, 1f));
        SetRect(checkmark.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(34f, 34f));
        toggle.graphic = checkmark;
        toggle.isOn = true;

        return toggle;
    }

    private static Text CreateText(string name, Transform parent, string content, int fontSize, Color color, TextAnchor alignment)
    {
        GameObject go = CreateRectObject(name, parent);
        Text text = go.AddComponent<Text>();
        text.text = content;
        text.font = GetBuiltinFont();
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    private static GameObject CreateRectObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static Font GetBuiltinFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return font;
    }

    private static void EnsureInputSystemEventSystem()
    {
        EventSystem eventSystem = Object.FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventObject = new GameObject("EventSystem", typeof(EventSystem));
            Undo.RegisterCreatedObjectUndo(eventObject, "Criar EventSystem");
            eventSystem = eventObject.GetComponent<EventSystem>();
        }

        StandaloneInputModule oldModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (oldModule != null)
            DestroyImmediate(oldModule);

        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();

        inputModule.AssignDefaultActions();
    }

    private static void RemoveExisting(string objectName)
    {
        GameObject existing = GameObject.Find(objectName);
        if (existing != null)
            Undo.DestroyObjectImmediate(existing);
    }

    private static void AssignMainControllerReferences(
        MainMenuController controller,
        GameObject mainPanel,
        GameObject mapPanel,
        GameObject optionsPanel,
        GameObject creditsPanel,
        GameObject exitPanel,
        Button continueButton,
        Slider volumeSlider,
        Toggle fullscreenToggle,
        Toggle vSyncToggle,
        Text statusText,
        CanvasGroup fadeOverlay,
        Text loadingText,
        AudioSource audioSource)
    {
        SerializedObject serialized = new SerializedObject(controller);
        Assign(serialized, "mainPanel", mainPanel);
        Assign(serialized, "mapPanel", mapPanel);
        Assign(serialized, "optionsPanel", optionsPanel);
        Assign(serialized, "creditsPanel", creditsPanel);
        Assign(serialized, "exitPanel", exitPanel);
        Assign(serialized, "continueButton", continueButton);
        Assign(serialized, "volumeSlider", volumeSlider);
        Assign(serialized, "fullscreenToggle", fullscreenToggle);
        Assign(serialized, "vSyncToggle", vSyncToggle);
        Assign(serialized, "statusText", statusText);
        Assign(serialized, "fadeOverlay", fadeOverlay);
        Assign(serialized, "loadingText", loadingText);
        Assign(serialized, "uiAudioSource", audioSource);
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AssignPauseControllerReferences(
        PauseMenuController controller,
        GameObject pauseRoot,
        GameObject pauseMainPanel,
        GameObject pauseOptionsPanel,
        Slider volumeSlider,
        Toggle fullscreenToggle,
        Toggle vSyncToggle)
    {
        SerializedObject serialized = new SerializedObject(controller);
        Assign(serialized, "pauseRoot", pauseRoot);
        Assign(serialized, "pauseMainPanel", pauseMainPanel);
        Assign(serialized, "pauseOptionsPanel", pauseOptionsPanel);
        Assign(serialized, "volumeSlider", volumeSlider);
        Assign(serialized, "fullscreenToggle", fullscreenToggle);
        Assign(serialized, "vSyncToggle", vSyncToggle);
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void Assign(SerializedObject serialized, string propertyName, Object value)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
            property.objectReferenceValue = value;
    }

    private static void SetPrivateString(Object target, string propertyName, string value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
