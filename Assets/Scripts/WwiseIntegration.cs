using UnityEngine;

/// <summary>
/// Wwise 事件封装。向 AkSoundEngine 发送事件；同时提供 AudioSource 回退用于预览播放。
/// </summary>
public class WwiseIntegration : MonoBehaviour
{
    private static WwiseIntegration instance;

    [Header("Fallback Audio")]
    [Tooltip("Wwise 事件缺失时，用 AudioSource 播放预览曲")]
    [SerializeField] private AudioSource fallbackSource;

    private AudioSource _fallback;

    public static WwiseIntegration Instance => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (fallbackSource == null)
        {
            _fallback = gameObject.AddComponent<AudioSource>();
            _fallback.playOnAwake = false;
            _fallback.spatialBlend = 0f;
        }
        else
        {
            _fallback = fallbackSource;
        }
    }

    /// <summary>发送 Wwise 事件到全局对象。</summary>
    public static void PostEvent(string eventName)
    {
        if (string.IsNullOrEmpty(eventName)) return;
        if (instance == null)
        {
            Debug.Log($"[Wwise] PostEvent: {eventName}");
            return;
        }
        instance.PostEventInternal(eventName, instance.gameObject);
    }

    /// <summary>发送 Wwise 事件到指定游戏对象。</summary>
    public static void PostEvent(string eventName, GameObject target)
    {
        if (string.IsNullOrEmpty(eventName)) return;
        if (instance == null)
        {
            Debug.Log($"[Wwise] PostEvent: {eventName} on {(target != null ? target.name : "null")}");
            return;
        }
        instance.PostEventInternal(eventName, target != null ? target : instance.gameObject);
    }

    /// <summary>通过 AudioClip 播放（Wwise 事件缺失时的回退方案，循环播放）。</summary>
    public static void PlayClip(AudioClip clip)
    {
        if (clip == null || instance == null) return;
        instance._fallback.Stop();
        instance._fallback.clip = clip;
        instance._fallback.loop = true;
        instance._fallback.Play();
    }

    /// <summary>停止所有回退预览音频。</summary>
    public static void StopClip()
    {
        if (instance == null) return;
        instance._fallback.Stop();
        instance._fallback.clip = null;
    }

    /// <summary>设置 RTPC 参数（全局，作用于所有发声对象，兼容 Global / Game Object 两种作用域）。</summary>
    public static void SetRTPC(string paramName, float value)
    {
        if (string.IsNullOrEmpty(paramName)) return;

        if (instance != null)
            AkUnitySoundEngine.SetRTPCValue(paramName, value);
        else
            Debug.Log($"[Wwise] SetRTPC: {paramName} = {value}");
    }

    /// <summary>切换互动音乐状态（Cytus 风格音乐随界面切换）。</summary>
    public static void SwitchMusic(string switchGroup, string switchState)
    {
        if (string.IsNullOrEmpty(switchGroup) || string.IsNullOrEmpty(switchState)) return;

        if (instance != null)
            AkUnitySoundEngine.SetSwitch(switchGroup, switchState, instance.gameObject);
        else
            Debug.Log($"[Wwise] Switch: {switchGroup} -> {switchState}");
    }

    /// <summary>加载 SoundBank（按名字，Init 走 LoadInitBank）。</summary>
    public static void LoadBank(string bankName)
    {
        if (string.IsNullOrEmpty(bankName)) return;

        if (bankName == "Init")
            AkBankManager.LoadInitBank();
        else
            AkBankManager.LoadBank(bankName, false, false);
    }

    /// <summary>卸载 SoundBank。</summary>
    public static void UnloadBank(string bankName)
    {
        if (string.IsNullOrEmpty(bankName)) return;

        if (bankName == "Init")
            AkBankManager.UnloadInitBank();
        else
            AkBankManager.UnloadBank(bankName);
    }

    /// <summary>发送 Wwise 事件并返回 PlayingID（用于后续精确停止，如歌曲预览）。</summary>
    public static uint PostEventGetID(string eventName)
    {
        if (string.IsNullOrEmpty(eventName)) return AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
        if (instance == null)
        {
            Debug.Log($"[Wwise] PostEvent: {eventName}");
            return AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
        }
        return instance.PostEventInternal(eventName, instance.gameObject);
    }

    /// <summary>按 PlayingID 精确停止一个实例（不会误伤正在播放的背景音乐）。</summary>
    public static void StopPlayingID(uint playingID)
    {
        if (playingID == 0 || playingID == AkUnitySoundEngine.AK_INVALID_PLAYING_ID) return;
        AkUnitySoundEngine.StopPlayingID(playingID);
    }

    private uint PostEventInternal(string eventName, GameObject target)
    {
        if (target != null)
        {
            var playingId = AkUnitySoundEngine.PostEvent(eventName, target);
            if (playingId == AkUnitySoundEngine.AK_INVALID_PLAYING_ID)
                Debug.LogWarning($"[Wwise] 事件不存在: {eventName}");
            return playingId;
        }
        Debug.Log($"[Wwise] PostEvent: {eventName}");
        return AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
    }
}
