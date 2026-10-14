using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Cytus2/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Info")]
    public string characterName;
    [TextArea(2, 5)]
    public string description;
    public Sprite portrait;
    public Sprite fullArt;

    [Header("Sound")]
    public string wwiseEventSelect;
    public string wwiseEventConfirm;
    [Tooltip("Wwise Character_Theme Switch 名。留空时用 characterName。Wwise 要求合法标识符（无空格/特殊字符），如 Nai Long 需填 NaiLong")]
    public string wwiseSwitchName;

    public string GetAudioSwitchName()
    {
        return string.IsNullOrEmpty(wwiseSwitchName) ? characterName : wwiseSwitchName;
    }

    [Header("UI")]
    public int sortOrder;
    public Color themeColor = Color.white;
    public RuntimeAnimatorController animator;
}
