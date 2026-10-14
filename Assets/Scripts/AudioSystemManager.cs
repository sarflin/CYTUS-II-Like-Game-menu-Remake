using UnityEngine;

/// <summary>
/// 全局音频管理：加载 SoundBank、按菜单状态切换互动音乐（Cytus 风格）、播放 UI 音效。
/// 所有事件名可在 Inspector 中配置，需与 Wwise 工程中的事件一致。
/// </summary>
public class AudioSystemManager : MonoBehaviour
{
    public static AudioSystemManager Instance { get; private set; }

    [Header("SoundBanks")]
    [Tooltip("启动时自动加载的 Bank 名列表（不含 Init，Init 自动加载）")]
    [SerializeField] private string[] startupBanks = { "UI", "Music", "SongPreviews" };

    [Header("Menu Music Switch")]
    [Tooltip("启动互动音乐的事件（Post 一次后，后续只靠 Switch 换段）")]
    [SerializeField] private string menuMusicEvent = "Play_MenuMusic";
    [Tooltip("互动音乐的 Switch Group 名")]
    [SerializeField] private string musicSwitchGroup = "Menu_Music";
    [SerializeField] private string mainMenuState = "MainMenu";
    [SerializeField] private string characterSelectState = "CharacterSelect";
    [SerializeField] private string songSelectState = "SongSelect";
    [SerializeField] private string gameState = "InGame";

    [Header("Character Theme Switch")]
    [Tooltip("角色主题曲的 Switch Group 名（角色选择界面按角色切歌）")]
    [SerializeField] private string characterThemeSwitchGroup = "Character_Theme";
    [Tooltip("未选择角色时的 Switch 状态名，需与 Wwise Character_Theme 中的状态名完全一致（当前工程为 Non）")]
    [SerializeField] private string noCharacterThemeState = "Non";

    [Header("UI SFX Events")]
    [SerializeField] private string clickSound = "UI_Click";
    [SerializeField] private string hoverSound = "UI_Hover";
    [SerializeField] private string backSound = "UI_Back";
    [SerializeField] private string characterConfirmSound = "Character_Confirm";
    [SerializeField] private string songSelectSound = "Song_Select";

    public enum MusicState
    {
        MainMenu,
        CharacterSelect,
        SongSelect,
        InGame
    }

    private MusicState currentMusicState = MusicState.MainMenu;

    private uint currentPreviewPlayingId;
    private bool previewingWwise;

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
            return;
        }
    }

    private void Start()
    {
        LoadBanks();
        SetMusicState(MusicState.MainMenu);
        // 启动互动音乐：Play_MenuMusic 只 Post 一次，之后靠 Switch 换段（主菜单/选人/选曲共用）。
        WwiseIntegration.PostEvent(menuMusicEvent);
    }

    private void LoadBanks()
    {
        // Init 由 AkInitializer 在初始化时自动加载，这里只加载自定义 Bank。
        if (startupBanks != null)
        {
            foreach (var bank in startupBanks)
            {
                if (!string.IsNullOrEmpty(bank))
                    WwiseIntegration.LoadBank(bank);
            }
        }
    }

    /// <summary>切换到指定音乐状态，触发 Wwise Switch 实现互动音乐。</summary>
    public void SetMusicState(MusicState state)
    {
        currentMusicState = state;
        string switchState = state switch
        {
            MusicState.MainMenu => mainMenuState,
            MusicState.CharacterSelect => characterSelectState,
            MusicState.SongSelect => songSelectState,
            MusicState.InGame => gameState,
            _ => mainMenuState,
        };
        WwiseIntegration.SwitchMusic(musicSwitchGroup, switchState);
    }

    public MusicState GetMusicState() => currentMusicState;

    /// <summary>切换角色主题曲（角色选择界面，把 Switch 打到对应角色状态）；传空则切到“未选角色”状态。</summary>
    public void SetCharacterTheme(string characterName)
    {
        if (string.IsNullOrEmpty(characterName))
        {
            WwiseIntegration.SwitchMusic(characterThemeSwitchGroup, noCharacterThemeState);
            return;
        }
        WwiseIntegration.SwitchMusic(characterThemeSwitchGroup, characterName);
    }

    public void SetCharacterTheme(CharacterData character)
    {
        if (character == null) return;
        SetCharacterTheme(character.GetAudioSwitchName());
    }

    /// <summary>玩家在选角界面选中角色：切到 CharacterSelect 音乐状态并播放该角色主题曲（方案B：进选角不切音乐，选中才切）。</summary>
    public void PlayCharacterTheme(CharacterData character)
    {
        if (character == null) return;
        SetMusicState(MusicState.CharacterSelect);
        SetCharacterTheme(character);
    }

    /// <summary>把角色主题曲 Switch 切回“未选角色”状态（离开角色选择界面 / 未选角色时调用）。</summary>
    public void StopCharacterTheme()
    {
        WwiseIntegration.SwitchMusic(characterThemeSwitchGroup, noCharacterThemeState);
    }

    // --- UI SFX ---

    public void PlayClick() => WwiseIntegration.PostEvent(clickSound);
    public void PlayHover() => WwiseIntegration.PostEvent(hoverSound);
    public void PlayBack() => WwiseIntegration.PostEvent(backSound);
    public void PlayCharacterConfirm() => WwiseIntegration.PostEvent(characterConfirmSound);
    public void PlaySongSelect() => WwiseIntegration.PostEvent(songSelectSound);

    /// <summary>播放歌曲预览：先精确停止上一首，再播放当前（Wwise 事件优先，回退到 AudioClip）。</summary>
    public void PlaySongPreview(SongData song)
    {
        if (song == null) return;

        StopSongPreview();

        if (!string.IsNullOrEmpty(song.wwiseEventPreview))
        {
            uint playingId = WwiseIntegration.PostEventGetID(song.wwiseEventPreview);
            if (playingId != AkUnitySoundEngine.AK_INVALID_PLAYING_ID)
            {
                previewingWwise = true;
                currentPreviewPlayingId = playingId;
                return;
            }
        }

        previewingWwise = false;
        WwiseIntegration.PlayClip(song.previewClip);
    }

    /// <summary>停止当前预览（同时停止 Wwise 实例与回退 AudioSource）。</summary>
    public void StopSongPreview()
    {
        if (previewingWwise)
        {
            WwiseIntegration.StopPlayingID(currentPreviewPlayingId);
            previewingWwise = false;
        }
        currentPreviewPlayingId = 0;
        WwiseIntegration.StopClip();
    }
}
