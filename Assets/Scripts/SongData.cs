using UnityEngine;

[CreateAssetMenu(fileName = "NewSong", menuName = "Cytus2/Song Data")]
public class SongData : ScriptableObject
{
    [Header("Info")]
    public string songName;
    public string artist;
    public int sortOrder;
    [TextArea(2, 5)]
    public string description;
    public Sprite coverArt;
    public float bpm;

    [Header("Difficulty")]
    public int easyLevel;
    public int hardLevel;
    public int chaosLevel;
    public int glitchLevel;

    [Header("Character")]
    public CharacterData character;

    [Header("Sound")]
    [Tooltip("Wwise 预览事件名，如 Play_Preview_<歌曲>")]
    public string wwiseEventPreview;
    [Tooltip("Wwise 开始游玩事件名")]
    public string wwiseEventPlay;
    [Tooltip("Wwise 事件缺失时的回退预览 AudioClip")]
    public AudioClip previewClip;

    [Header("Files")]
    public string audioFilePath;
    public RuntimeAnimatorController chartAnimator;
}
