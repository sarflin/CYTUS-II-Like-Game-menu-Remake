using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SongSelectUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [System.Serializable]
    public class DifficultyEntry
    {
        public string name;
        public Button button;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI levelText;
    }

    [Header("Left - Song List")]
    [SerializeField] private RectTransform songContainer;
    [SerializeField] private GameObject songItemPrefab;

    [Header("Right - Song Detail")]
    [SerializeField] private Image coverArt;
    [SerializeField] private TextMeshProUGUI songNameText;
    [SerializeField] private TextMeshProUGUI artistText;

    [Header("Right - Difficulty")]
    [SerializeField] private DifficultyEntry[] difficulties;
    [SerializeField] private float diffSelectedScale = 1.3f;
    [SerializeField] private float diffUnselectedScale = 0.8f;

    [Header("Carousel")]
    [SerializeField] private float centerScale = 1.2f;
    [SerializeField] private float farScale = 0.6f;
    [SerializeField] private float snapDuration = 0.25f;

    [Header("Data")]
    [SerializeField] private SongData[] allSongs;
    [Tooltip("Leave empty to auto-load from Resources/Songs/")]
    [SerializeField] private bool autoLoadFromResources = true;

    [Header("Navigation")]
    [SerializeField] private Button backButton;

    private CharacterData currentCharacter;
    private List<RectTransform> songItems = new List<RectTransform>();
    private List<SongData> filteredSongs = new List<SongData>();
    private int selectedIndex;
    private float currentOffset;
    private float itemHeight;
    private int maxVisible;
    private Coroutine snapAnim;
    private bool isSnapping;
    private int selectedDifficulty = -1;
    private SongData lastSelectedSong;

    private void Start()
    {
        backButton.onClick.AddListener(OnBackClicked);

        for (int i = 0; i < difficulties.Length; i++)
        {
            int ci = i;
            difficulties[i].button.onClick.AddListener(() => OnDifficultyClicked(ci));
            AddHoverSound(difficulties[i].button);
        }
    }

    private void AddHoverSound(Button button)
    {
        if (button == null || AudioSystemManager.Instance == null) return;
        var trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        var entry = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
        };
        entry.callback.AddListener(_ => AudioSystemManager.Instance.PlayHover());
        trigger.triggers.Add(entry);
    }

    public void Initialize(CharacterData character)
    {
        CalculateScreenScale();
        if ((allSongs == null || allSongs.Length == 0) && autoLoadFromResources)
        {
            allSongs = Resources.LoadAll<SongData>("Songs");
            System.Array.Sort(allSongs, (a, b) => a.sortOrder != 0 || b.sortOrder != 0
                ? a.sortOrder.CompareTo(b.sortOrder)
                : string.Compare(a.songName, b.songName));
        }
        currentCharacter = character;
        filteredSongs.Clear();
        foreach (var s in allSongs)
            if (s.character == character)
                filteredSongs.Add(s);

        foreach (var item in songItems)
            if (item != null) Destroy(item.gameObject);
        songItems.Clear();

        for (int i = 0; i < filteredSongs.Count; i++)
        {
            var obj = Instantiate(songItemPrefab, songContainer);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            var cover = obj.transform.Find("CoverArt")?.GetComponent<Image>();
            if (cover != null && filteredSongs[i].coverArt != null)
                cover.sprite = filteredSongs[i].coverArt;

            var nameText = obj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = filteredSongs[i].songName;

            var artistText = obj.transform.Find("Artist")?.GetComponent<TextMeshProUGUI>();
            if (artistText != null)
                artistText.text = filteredSongs[i].artist;

            if (obj.GetComponent<Button>() == null)
                obj.AddComponent<Button>();
            if (obj.GetComponent<CanvasGroup>() == null)
                obj.AddComponent<CanvasGroup>();

            var btn = obj.GetComponent<Button>();
            int ci = i;
            btn.onClick.AddListener(() => OnSongClicked(ci));
            AddHoverSound(btn);

            songItems.Add(rect);
        }

        selectedIndex = 0;
        if (lastSelectedSong != null)
        {
            int found = filteredSongs.FindIndex(s => s == lastSelectedSong);
            if (found >= 0)
                selectedIndex = found;
        }
        currentOffset = 0f;
        selectedDifficulty = -1;

        UpdateCarousel(0f);
        ShowSongDetail(selectedIndex);
    }

    private void CalculateScreenScale()
    {
        float screenHeight = ((RectTransform)transform).rect.height;
        itemHeight = screenHeight * 0.08f;
        maxVisible = Mathf.CeilToInt(screenHeight / itemHeight) + 1;
    }

    private void UpdateCarousel(float extraOffset)
    {
        float totalOffset = selectedIndex + extraOffset;
        for (int i = 0; i < songItems.Count; i++)
        {
            float offset = i - totalOffset;
            float absOff = Mathf.Abs(offset);

            float targetY = -offset * itemHeight;
            float t = Mathf.Clamp01(absOff / maxVisible);
            float scale = Mathf.Lerp(centerScale, farScale, t);
            float alpha = Mathf.Clamp01(1f - absOff * 0.3f);

            songItems[i].anchoredPosition = new Vector2(0f, targetY);
            songItems[i].localScale = Vector3.one * scale;
            songItems[i].GetComponent<CanvasGroup>().alpha = alpha;
        }
    }

    // --- Drag ---

    public void OnBeginDrag(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        if (isSnapping) return;
        currentOffset += eventData.delta.y / itemHeight;
        currentOffset = Mathf.Clamp(currentOffset, -0.4f, 0.4f);
        UpdateCarousel(currentOffset);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isSnapping) return;
        int dir = currentOffset > 0.15f ? 1 : currentOffset < -0.15f ? -1 : 0;
        int newIndex = Mathf.Clamp(selectedIndex + dir, 0, filteredSongs.Count - 1);
        selectedIndex = newIndex;
        SnapToSelected(currentOffset);
        currentOffset = 0f;
    }

    // --- Click ---

    private void OnSongClicked(int index)
    {
        if (index == selectedIndex)
        {
            // 重复点击同一首歌：预览继续播放不重启，仅重复 UI 音效
            if (AudioSystemManager.Instance != null)
                AudioSystemManager.Instance.PlayClick();
            return;
        }

        float fromOffset = (selectedIndex - index) + currentOffset;
        selectedIndex = index;
        currentOffset = 0f;
        SnapToSelected(fromOffset);
        ShowSongDetail(index);
    }

    private void SnapToSelected(float fromOffset)
    {
        if (snapAnim != null) StopCoroutine(snapAnim);
        snapAnim = StartCoroutine(AnimateSnap(fromOffset));
    }

    private IEnumerator AnimateSnap(float fromOffset)
    {
        isSnapping = true;
        float elapsed = 0f;
        while (elapsed < snapDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / snapDuration;
            float eased = t * t * (3f - 2f * t);
            UpdateCarousel(Mathf.Lerp(fromOffset, 0f, eased));
            yield return null;
        }
        UpdateCarousel(0f);
        isSnapping = false;
        snapAnim = null;
        if (selectedIndex >= 0 && selectedIndex < songItems.Count)
            songItems[selectedIndex].SetAsLastSibling();
    }

    // --- Detail ---

    private void ShowSongDetail(int index)
    {
        if (index < 0 || index >= filteredSongs.Count) return;
        var song = filteredSongs[index];
        lastSelectedSong = song;

        if (coverArt != null && song.coverArt != null)
            coverArt.sprite = song.coverArt;
        if (songNameText != null)
            songNameText.text = song.songName;
        if (artistText != null)
            artistText.text = song.artist;

        selectedDifficulty = -1;
        UpdateDifficultyButtons();

        if (AudioSystemManager.Instance != null)
            AudioSystemManager.Instance.PlaySongPreview(song);
    }

    // --- Difficulty ---

    private void OnDifficultyClicked(int index)
    {
        if (index < 0 || index >= difficulties.Length) return;

        if (selectedDifficulty == index)
        {
            StartGame();
        }
        else
        {
            selectedDifficulty = index;
            UpdateDifficultyButtons();
        }
    }

    private void UpdateDifficultyButtons()
    {
        for (int i = 0; i < difficulties.Length; i++)
        {
            bool isSelected = i == selectedDifficulty;
            float targetScale = isSelected ? diffSelectedScale : diffUnselectedScale;

            difficulties[i].button.transform.localScale = Vector3.one * targetScale;

            int level = GetDifficultyLevel(i);
            if (difficulties[i].levelText != null)
            {
                difficulties[i].levelText.text = level > 0 ? level.ToString() : "-";
                difficulties[i].levelText.color = isSelected ? Color.white : new Color(1, 1, 1, 0.5f);
            }
            if (difficulties[i].nameText != null)
                difficulties[i].nameText.color = isSelected ? Color.white : new Color(1, 1, 1, 0.5f);
        }
    }

    private int GetDifficultyLevel(int index)
    {
        if (selectedIndex < 0 || selectedIndex >= filteredSongs.Count) return 0;
        var song = filteredSongs[selectedIndex];
        switch (index)
        {
            case 0: return song.easyLevel;
            case 1: return song.hardLevel;
            case 2: return song.chaosLevel;
            case 3: return song.glitchLevel;
            default: return 0;
        }
    }

    private void StartGame()
    {
        if (selectedIndex < 0 || selectedIndex >= filteredSongs.Count) return;
        var song = filteredSongs[selectedIndex];

        if (AudioSystemManager.Instance != null)
            AudioSystemManager.Instance.PlayClick();

        if (!string.IsNullOrEmpty(song.wwiseEventPlay))
            WwiseIntegration.PostEvent(song.wwiseEventPlay);

        MenuManager.Instance.StartGame(song);
    }

    private void OnBackClicked()
    {
        if (AudioSystemManager.Instance != null) AudioSystemManager.Instance.PlayBack();
        MenuManager.Instance.ReturnToCharacterSelect();
    }
}
