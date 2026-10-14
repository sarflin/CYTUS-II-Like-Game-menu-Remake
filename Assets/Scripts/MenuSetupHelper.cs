using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class MenuSetupHelper : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Cytus2/Create Menu System")]
    public static void CreateMenuSystem()
    {
        // 创建主Canvas
        var canvasObj = new GameObject("MenuCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // 创建EventSystem
        if (UnityEngine.Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 创建主菜单
        CreateMainMenu(canvasObj.transform);

        // 创建角色选择界面
        CreateCharacterSelect(canvasObj.transform);

        // 创建曲目选择界面
        CreateSongSelect(canvasObj.transform);

        // 创建MenuManager
        var managerObj = new GameObject("MenuManager");
        var manager = managerObj.AddComponent<MenuManager>();

        // 创建WwiseGlobal（AkInitializer）
        if (UnityEngine.Object.FindObjectOfType<AkInitializer>() == null)
        {
            var wwiseGlobal = new GameObject("WwiseGlobal");
            wwiseGlobal.AddComponent<AkInitializer>();
        }

        // 创建WwiseIntegration
        var wwiseObj = new GameObject("WwiseIntegration");
        wwiseObj.AddComponent<WwiseIntegration>();

        // 创建AudioSystemManager（加载Bank、切换音乐状态、UI音效）
        if (UnityEngine.Object.FindObjectOfType<AudioSystemManager>() == null)
        {
            var audioManager = new GameObject("AudioSystemManager");
            audioManager.AddComponent<AudioSystemManager>();
        }

        Debug.Log("Menu system created!");
    }

    private static void CreateMainMenu(Transform parent)
    {
        var mainMenu = CreateUIPanel(parent, "MainMenu");
        var verticalLayout = mainMenu.AddComponent<VerticalLayoutGroup>();
        verticalLayout.spacing = 20;
        verticalLayout.childAlignment = TextAnchor.MiddleCenter;

        // 标题
        CreateTextElement(mainMenu.transform, "Title", "CYTUS II", 72);

        // 按钮容器
        var buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(mainMenu.transform, false);
        var layout = buttonContainer.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 15;

        // Start button
        CreateButton(buttonContainer.transform, "StartButton", "Start");
        // Settings button
        CreateButton(buttonContainer.transform, "SettingsButton", "Settings");
        // Quit button
        CreateButton(buttonContainer.transform, "QuitButton", "Quit");
    }

    private static void CreateCharacterSelect(Transform parent)
    {
        var characterSelect = CreateUIPanel(parent, "CharacterSelectUI");

        // 角色画像容器（居中，脚本控制位置）
        var container = new GameObject("CharacterContainer");
        container.transform.SetParent(characterSelect.transform, false);
        var containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(1920, 500);

        // 底部信息面板
        var infoPanel = CreateUIPanel(characterSelect.transform, "CharacterInfoPanel");
        var infoRect = infoPanel.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.5f, 0f);
        infoRect.anchorMax = new Vector2(0.5f, 0f);
        infoRect.pivot = new Vector2(0.5f, 0f);
        infoRect.sizeDelta = new Vector2(600, 300);
        infoRect.anchoredPosition = new Vector2(0f, 80f);

        var infoLayout = infoPanel.AddComponent<VerticalLayoutGroup>();
        infoLayout.childAlignment = TextAnchor.MiddleCenter;
        infoLayout.spacing = 10;

        CreateTextElement(infoPanel.transform, "CharacterName", "Character Name", 48);
        CreateTextElement(infoPanel.transform, "CharacterDesc", "Description", 24);

        var buttonContainer = new GameObject("Buttons");
        buttonContainer.transform.SetParent(infoPanel.transform, false);
        var buttonRect = buttonContainer.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(200, 50);
        CreateButton(buttonContainer.transform, "BackButton", "Back");
    }

    private static void CreateSongSelect(Transform parent)
    {
        var songSelect = CreateUIPanel(parent, "SongSelectUI");

        // 左侧选曲面板 (35%)
        var leftPanel = new GameObject("SongListPanel");
        leftPanel.transform.SetParent(songSelect.transform, false);
        var leftRect = leftPanel.AddComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(0f, 0f);
        leftRect.anchorMax = new Vector2(0.35f, 1f);
        leftRect.sizeDelta = Vector2.zero;

        var container = new GameObject("SongContainer");
        container.transform.SetParent(leftPanel.transform, false);
        var containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(400, 800);

        // 右侧详情面板 (65%)
        var rightPanel = new GameObject("SongDetailPanel");
        rightPanel.transform.SetParent(songSelect.transform, false);
        var rightRect = rightPanel.AddComponent<RectTransform>();
        rightRect.anchorMin = new Vector2(0.35f, 0f);
        rightRect.anchorMax = new Vector2(1f, 1f);
        rightRect.sizeDelta = Vector2.zero;

        var rightContent = rightPanel.AddComponent<VerticalLayoutGroup>();
        rightContent.childAlignment = TextAnchor.MiddleCenter;
        rightContent.spacing = 15;

        // 曲绘
        var coverObj = new GameObject("CoverArt");
        coverObj.transform.SetParent(rightPanel.transform, false);
        var coverImage = coverObj.AddComponent<Image>();
        var coverRect = coverObj.GetComponent<RectTransform>();
        coverRect.sizeDelta = new Vector2(500, 500);

        // 曲名
        CreateTextElement(rightPanel.transform, "SongName", "Song Name", 42);
        // 艺术家
        CreateTextElement(rightPanel.transform, "Artist", "Artist", 28);

        // 难度面板
        var diffPanel = new GameObject("DifficultyPanel");
        diffPanel.transform.SetParent(rightPanel.transform, false);
        var diffRect = diffPanel.AddComponent<RectTransform>();
        diffRect.sizeDelta = new Vector2(600, 120);
        var diffLayout = diffPanel.AddComponent<HorizontalLayoutGroup>();
        diffLayout.spacing = 20;
        diffLayout.childAlignment = TextAnchor.MiddleCenter;

        CreateDifficultyButton(diffPanel.transform, "EasyButton", "Easy");
        CreateDifficultyButton(diffPanel.transform, "HardButton", "Hard");
        CreateDifficultyButton(diffPanel.transform, "ChaosButton", "Chaos");
        CreateDifficultyButton(diffPanel.transform, "GlitchButton", "Glitch");

        // 返回按钮
        var backContainer = new GameObject("BackButton");
        backContainer.transform.SetParent(songSelect.transform, false);
        var backRect2 = backContainer.AddComponent<RectTransform>();
        backRect2.anchorMin = new Vector2(0f, 1f);
        backRect2.anchorMax = new Vector2(0f, 1f);
        backRect2.pivot = new Vector2(0f, 1f);
        backRect2.anchoredPosition = new Vector2(20f, -20f);
        backRect2.sizeDelta = new Vector2(100, 40);
        CreateButton(backContainer.transform, "BackText", "Back");
    }

    private static void CreateDifficultyButton(Transform parent, string name, string label)
    {
        var btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        var rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100, 100);
        var btn = btnObj.AddComponent<Button>();
        var img = btnObj.AddComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

        var layout = btnObj.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 5;

        var nameText = new GameObject("NameText");
        nameText.transform.SetParent(btnObj.transform, false);
        var nm = nameText.AddComponent<TextMeshProUGUI>();
        nm.text = label;
        nm.fontSize = 18;
        nm.alignment = TextAlignmentOptions.Center;
        nm.color = Color.white;

        var levelText = new GameObject("LevelText");
        levelText.transform.SetParent(btnObj.transform, false);
        var lv = levelText.AddComponent<TextMeshProUGUI>();
        lv.text = "0";
        lv.fontSize = 28;
        lv.alignment = TextAlignmentOptions.Center;
        lv.color = Color.white;
    }

    private static GameObject CreateUIPanel(Transform parent, string name)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var rectTransform = panel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        return panel;
    }

    private static void CreateTextElement(Transform parent, string name, string text, int fontSize)
    {
        var textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        var textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = Color.white;
    }

    private static void CreateButton(Transform parent, string name, string label)
    {
        var buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        var button = buttonObj.AddComponent<Button>();
        var image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        var rectTransform = buttonObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 50);

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
    }
#endif
}
