using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Cytus2/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Wwise Config")]
    public string defaultBank = "Init";
    public string musicSwitchGroup = "Music_State";
    public string sfxSwitchGroup = "SFX_State";

    [Header("UI Config")]
    public float transitionDuration = 0.5f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Sound Events")]
    public string uiHoverEvent = "UI_Hover";
    public string uiClickEvent = "UI_Click";
    public string uiConfirmEvent = "UI_Confirm";
    public string uiBackEvent = "UI_Back";
}
