using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject characterSelectUI;
    [SerializeField] private GameObject songSelectUI;

    [Header("Transition")]
    [SerializeField] private float loadDuration = 0.8f;
    [SerializeField] private float fadeInDuration = 0.3f;

    [Header("State")]
    private CharacterData selectedCharacter;
    private GameObject currentUI;

    private ScreenTransition transition;
    private bool isTransitioning;
    private SettingsUI settingsUI;
    private GameObject settingsUIGameObject;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            transition = ScreenTransition.Create(canvas);
            settingsUI = SettingsUI.Create(canvas);
            settingsUIGameObject = settingsUI.gameObject;
            settingsUIGameObject.SetActive(false);
        }
        ShowMainMenu();
    }

    // --- 导航 ---

    public void ShowMainMenu()
    {
        HideAllUI();
        mainMenuUI.SetActive(true);
        currentUI = mainMenuUI;
        StopSongPreview();
        StopCharacterTheme();
        SetMusic(AudioSystemManager.MusicState.MainMenu);
        StartCoroutine(FadeInUI(mainMenuUI));
    }

    /// <summary>从主菜单进入选角：主菜单主题曲不中断，继续循环。</summary>
    public void GoToCharacterSelect()
    {
        StartCoroutine(Navigate(characterSelectUI, null, () =>
        {
            var charSelect = characterSelectUI.GetComponent<CharacterSelectUI>();
            if (charSelect != null)
                charSelect.Initialize(null);
        }));
    }

    /// <summary>从选曲界面返回选角：退出时停预览，加载完成后播放对应角色主题曲。</summary>
    public void ReturnToCharacterSelect()
    {
        StartCoroutine(Navigate(characterSelectUI, StopSongPreview, () =>
        {
            var charSelect = characterSelectUI.GetComponent<CharacterSelectUI>();
            if (selectedCharacter != null)
            {
                if (AudioSystemManager.Instance != null)
                    AudioSystemManager.Instance.PlayCharacterTheme(selectedCharacter);
                if (charSelect != null)
                    charSelect.Initialize(selectedCharacter);
            }
            else
            {
                StopCharacterTheme();
                if (charSelect != null)
                    charSelect.Initialize(null);
            }
        }));
    }

    public void SelectCharacter(CharacterData character)
    {
        selectedCharacter = character;
        StartCoroutine(Navigate(songSelectUI, StopCharacterTheme, () =>
        {
            SetMusic(AudioSystemManager.MusicState.SongSelect);
            var songSelect = songSelectUI.GetComponent<SongSelectUI>();
            if (songSelect != null)
                songSelect.Initialize(character);
        }));
    }

    public void GoToMainMenu()
    {
        StartCoroutine(Navigate(mainMenuUI, StopCharacterTheme, () =>
        {
            SetMusic(AudioSystemManager.MusicState.MainMenu);
        }));
    }

    public void OpenSettings()
    {
        StartCoroutine(Navigate(settingsUIGameObject, null, null));
    }

    public void CloseSettings()
    {
        StartCoroutine(Navigate(mainMenuUI, null, null));
    }

    public void StartGame(SongData song)
    {
        if (AudioSystemManager.Instance != null)
        {
            AudioSystemManager.Instance.StopSongPreview();
            AudioSystemManager.Instance.SetMusicState(AudioSystemManager.MusicState.InGame);
        }
        Debug.Log($"Start Game: {song.songName}");
    }

    // --- 过渡 ---

    private IEnumerator Navigate(GameObject targetUI, Action onExit, Action onArrival)
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        onExit?.Invoke();
        HideAllUI();

        if (transition != null)
            yield return transition.PlayLoading(loadDuration);
        else
            yield return null;

        targetUI.SetActive(true);
        currentUI = targetUI;
        onArrival?.Invoke();
        yield return FadeInUI(targetUI);

        isTransitioning = false;
    }

    private IEnumerator FadeInUI(GameObject ui)
    {
        var cg = ui.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = ui.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    // --- 音频辅助 ---

    private void SetMusic(AudioSystemManager.MusicState state)
    {
        if (AudioSystemManager.Instance != null)
            AudioSystemManager.Instance.SetMusicState(state);
    }

    private void StopCharacterTheme()
    {
        if (AudioSystemManager.Instance != null)
            AudioSystemManager.Instance.StopCharacterTheme();
    }

    private void StopSongPreview()
    {
        if (AudioSystemManager.Instance != null)
            AudioSystemManager.Instance.StopSongPreview();
    }

    private void HideAllUI()
    {
        mainMenuUI.SetActive(false);
        characterSelectUI.SetActive(false);
        songSelectUI.SetActive(false);
        if (settingsUIGameObject != null)
            settingsUIGameObject.SetActive(false);
    }
}
