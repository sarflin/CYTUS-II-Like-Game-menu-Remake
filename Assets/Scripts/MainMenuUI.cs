using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        startButton.onClick.AddListener(OnStartClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        AddHoverSound(startButton);
        AddHoverSound(settingsButton);
        AddHoverSound(quitButton);

        if (AudioSystemManager.Instance != null)
            AudioSystemManager.Instance.SetMusicState(AudioSystemManager.MusicState.MainMenu);
    }

    private void AddHoverSound(Button button)
    {
        var trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entry.callback.AddListener(_ =>
        {
            if (AudioSystemManager.Instance != null)
                AudioSystemManager.Instance.PlayHover();
        });
        trigger.triggers.Add(entry);
    }

    private void OnStartClicked()
    {
        if (AudioSystemManager.Instance != null) AudioSystemManager.Instance.PlayClick();
        MenuManager.Instance.GoToCharacterSelect();
    }

    private void OnSettingsClicked()
    {
        if (AudioSystemManager.Instance != null) AudioSystemManager.Instance.PlayClick();
        MenuManager.Instance.OpenSettings();
    }

    private void OnQuitClicked()
    {
        if (AudioSystemManager.Instance != null) AudioSystemManager.Instance.PlayClick();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
