using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 设置面板：两个 0-100 滑块分别控制音乐音量与音效音量，通过 Wwise RTPC 实现。
/// 由 MenuManager 在运行时创建到主 Canvas 下，音量值用 PlayerPrefs 持久化。
/// </summary>
public class SettingsUI : MonoBehaviour
{
    [Header("RTPC Game Parameters")]
    [SerializeField] private string musicVolumeRTPC = "Music_Volume";
    [SerializeField] private string sfxVolumeRTPC = "SFX_Volume";

    private const string MusicVolumeKey = "Settings_MusicVolume";
    private const string SFXVolumeKey = "Settings_SFXVolume";

    private Slider musicSlider;
    private Slider sfxSlider;
    private TextMeshProUGUI musicLabel;
    private TextMeshProUGUI sfxLabel;
    private TextMeshProUGUI fullscreenButtonLabel;

    public static SettingsUI Create(Canvas canvas)
    {
        var go = new GameObject("SettingsUI");
        go.transform.SetParent(canvas.transform, false);
        var s = go.AddComponent<SettingsUI>();
        s.Build(canvas);
        return s;
    }

    private void Build(Canvas canvas)
    {
        var rootRect = gameObject.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;

        var dim = new GameObject("Dim");
        dim.transform.SetParent(transform, false);
        var dimRect = dim.AddComponent<RectTransform>();
        Stretch(dimRect);
        var dimImg = dim.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.5f);
        dimImg.raycastTarget = true;

        var canvasRect = ((RectTransform)canvas.transform).rect;
        float panelWidth = canvasRect.width * 0.5f;
        float panelHeight = canvasRect.height * 0.65f;
        float itemWidth = panelWidth - 100f;

        var panel = new GameObject("Panel");
        panel.transform.SetParent(transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.08f, 0.08f, 0.12f, 0.97f);

        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(50, 50, 40, 40);
        layout.spacing = 18;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        CreateText(panel.transform, "Title", "设置", 46, itemWidth, 70);

        musicLabel = CreateText(panel.transform, "MusicLabel", "音乐音量  100", 28, itemWidth, 40);
        musicSlider = CreateSlider(panel.transform, "MusicSlider", itemWidth, 40);

        sfxLabel = CreateText(panel.transform, "SFXLabel", "音效音量  100", 28, itemWidth, 40);
        sfxSlider = CreateSlider(panel.transform, "SFXSlider", itemWidth, 40);

        var fullscreenButton = CreateButton(panel.transform, "FullscreenButton", "", 320, 56, ToggleFullscreen);
        fullscreenButtonLabel = fullscreenButton.GetComponentInChildren<TextMeshProUGUI>();
        CreateButton(panel.transform, "BackButton", "返回", 240, 64, OnBackClicked);
        UpdateFullscreenLabel();

        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        float musicVol = PlayerPrefs.GetFloat(MusicVolumeKey, 100f);
        float sfxVol = PlayerPrefs.GetFloat(SFXVolumeKey, 100f);
        musicSlider.value = musicVol;
        sfxSlider.value = sfxVol;
        // 滑块值未变化时 onValueChanged 不会触发，这里强制应用一次，确保启动即设置 RTPC
        OnMusicVolumeChanged(musicVol);
        OnSFXVolumeChanged(sfxVol);
    }

    private void OnMusicVolumeChanged(float value)
    {
        int iv = Mathf.RoundToInt(value);
        if (musicLabel != null)
            musicLabel.text = $"音乐音量  {iv}";
        WwiseIntegration.SetRTPC(musicVolumeRTPC, value);
        PlayerPrefs.SetFloat(MusicVolumeKey, value);
        PlayerPrefs.Save();
    }

    private void OnSFXVolumeChanged(float value)
    {
        int iv = Mathf.RoundToInt(value);
        if (sfxLabel != null)
            sfxLabel.text = $"音效音量  {iv}";
        WwiseIntegration.SetRTPC(sfxVolumeRTPC, value);
        PlayerPrefs.SetFloat(SFXVolumeKey, value);
        PlayerPrefs.Save();
    }

    private void OnBackClicked()
    {
        if (AudioSystemManager.Instance != null)
            AudioSystemManager.Instance.PlayBack();
        MenuManager.Instance.CloseSettings();
    }

    private void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        UpdateFullscreenLabel();
    }

    private void UpdateFullscreenLabel()
    {
        if (fullscreenButtonLabel != null)
            fullscreenButtonLabel.text = Screen.fullScreen ? "切换到窗口模式" : "切换到全屏模式";
    }

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
    }

    private TextMeshProUGUI CreateText(Transform parent, string name, string text, int fontSize, float width, float height)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = fontSize;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;
        return t;
    }

    private Slider CreateSlider(Transform parent, string name, float width, float height)
    {
        var root = new GameObject(name);
        root.transform.SetParent(parent, false);
        var rootRect = root.AddComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(width, height);

        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(root.transform, false);
        var bgRect = bgGo.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        var bg = bgGo.AddComponent<Image>();
        bg.color = new Color(1f, 1f, 1f, 0.15f);

        var fillAreaGo = new GameObject("Fill Area");
        fillAreaGo.transform.SetParent(root.transform, false);
        var fillAreaRect = fillAreaGo.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRect.sizeDelta = Vector2.zero;

        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(fillAreaGo.transform, false);
        var fillRect = fillGo.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.sizeDelta = Vector2.zero;
        var fill = fillGo.AddComponent<Image>();
        fill.color = new Color(0.35f, 0.75f, 1f, 0.9f);

        var handleAreaGo = new GameObject("Handle Slide Area");
        handleAreaGo.transform.SetParent(root.transform, false);
        var handleAreaRect = handleAreaGo.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = new Vector2(0f, 0f);
        handleAreaRect.anchorMax = new Vector2(1f, 1f);
        handleAreaRect.sizeDelta = new Vector2(-20f, 0f);

        var handleGo = new GameObject("Handle");
        handleGo.transform.SetParent(handleAreaGo.transform, false);
        var handleRect = handleGo.AddComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0f, 0f);
        handleRect.anchorMax = new Vector2(0f, 1f);
        handleRect.sizeDelta = new Vector2(20f, 0f);
        var handle = handleGo.AddComponent<Image>();
        handle.color = Color.white;

        var slider = root.AddComponent<Slider>();
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.wholeNumbers = true;
        slider.value = 100f;

        return slider;
    }

    private Button CreateButton(Transform parent, string name, string label, float width, float height, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.25f, 1f);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        var text = textGo.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 28;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        if (onClick != null)
            btn.onClick.AddListener(onClick);
        return btn;
    }
}
