using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CharacterSelectUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    [SerializeField] private RectTransform characterContainer;
    [SerializeField] private GameObject characterPortraitPrefab;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI characterDescText;
    [SerializeField] private Button backButton;

    [Header("Carousel")]
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float normalScale = 0.7f;
    [SerializeField] private float snapDuration = 0.3f;
    [Tooltip("画像高度占屏幕高度的比例")]
    [SerializeField] private float portraitHeightRatio = 0.35f;
    [Tooltip("画像宽高比")]
    [SerializeField] private float portraitAspect = 0.8f;
    [Tooltip("画像之间的间距 = 画像宽度 × 该倍数")]
    [SerializeField] private float spacingMultiplier = 1.25f;
    [Tooltip("画像整行的垂直偏移（正数上移）")]
    [SerializeField] private float portraitVerticalOffset = 0f;
    [Tooltip("未选中画像的透明度")]
    [SerializeField] private float normalAlpha = 0.6f;
    [Tooltip("无选中状态时画像的透明度")]
    [SerializeField] private float noSelectionAlpha = 0.85f;

    [Header("Data")]
    [SerializeField] private CharacterData[] characters;
    [Tooltip("Leave empty to auto-load from Resources/Characters/")]
    [SerializeField] private bool autoLoadFromResources = true;

    private List<RectTransform> portraitRects = new List<RectTransform>();
    private List<CharacterData> portraitData = new List<CharacterData>();
    private int selectedIndex = -1;
    private float currentOffset;
    private Coroutine snapAnim;
    private bool isSnapping;
    private bool populated;
    private float spacing;
    private float portraitWidth;
    private float portraitHeight;
    private int maxVisible;

    private void Start()
    {
        backButton.onClick.AddListener(OnBackClicked);
        EnsurePopulated();
    }

    private void EnsurePopulated()
    {
        if (populated) return;
        CalculateScreenScale();
        if ((characters == null || characters.Length == 0) && autoLoadFromResources)
        {
            characters = Resources.LoadAll<CharacterData>("Characters");
            System.Array.Sort(characters, (a, b) => a.sortOrder.CompareTo(b.sortOrder));
        }
        PopulateCarousel();
        populated = true;
    }

    /// <summary>进入选角界面时，按上次选中角色（可为空）恢复高亮。</summary>
    public void Initialize(CharacterData currentlySelected)
    {
        EnsurePopulated();
        int idx = (currentlySelected != null && characters != null)
            ? System.Array.IndexOf(characters, currentlySelected)
            : -1;
        selectedIndex = idx >= 0 ? idx : -1;
        currentOffset = 0f;
        UpdateCarousel(0f);
        RefreshInfo();
    }

    private void CalculateScreenScale()
    {
        var rect = (RectTransform)transform;
        float screenWidth = rect.rect.width;
        float screenHeight = rect.rect.height;
        portraitHeight = screenHeight * portraitHeightRatio;
        portraitWidth = portraitHeight * portraitAspect;
        spacing = portraitWidth * spacingMultiplier;
        maxVisible = Mathf.CeilToInt(screenWidth / spacing) + 1;
    }

    private void PopulateCarousel()
    {
        foreach (var rect in portraitRects)
            if (rect != null) Destroy(rect.gameObject);
        portraitRects.Clear();
        portraitData.Clear();

        for (int i = 0; i < characters.Length; i++)
        {
            var obj = Instantiate(characterPortraitPrefab, characterContainer);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(portraitWidth, portraitHeight);

            var image = obj.GetComponent<Image>();
            if (image != null && characters[i].portrait != null)
                image.sprite = characters[i].portrait;

            if (obj.GetComponent<Button>() == null)
                obj.AddComponent<Button>();

            if (obj.GetComponent<CanvasGroup>() == null)
                obj.AddComponent<CanvasGroup>();

            int ci = i;
            obj.GetComponent<Button>().onClick.AddListener(() => OnPortraitClicked(ci));

            portraitRects.Add(rect);
            portraitData.Add(characters[i]);
        }

        currentOffset = 0f;
        UpdateCarousel(0f);
    }

    private void UpdateCarousel(float smoothOffset)
    {
        int count = portraitRects.Count;
        // 无选中时整行居中；有选中时以选中项为焦点
        float focusIndex = selectedIndex >= 0 ? selectedIndex + smoothOffset : (count - 1) * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float offset = i - focusIndex;
            float absOffset = Mathf.Abs(offset);

            float targetX = offset * spacing;

            float scale;
            float alpha;
            if (selectedIndex >= 0)
            {
                float selectT = Mathf.Clamp01(1f - absOffset);
                scale = Mathf.Lerp(normalScale, selectedScale, selectT);
                alpha = Mathf.Lerp(normalAlpha, 1f, selectT);
            }
            else
            {
                scale = normalScale;
                alpha = noSelectionAlpha;
            }

            portraitRects[i].anchoredPosition = new Vector2(targetX, portraitVerticalOffset);
            portraitRects[i].localScale = Vector3.one * scale;
            portraitRects[i].GetComponent<CanvasGroup>().alpha = alpha;
        }
    }

    // --- Drag ---

    public void OnBeginDrag(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isSnapping) return;
        currentOffset += eventData.delta.x / spacing;
        currentOffset = Mathf.Clamp(currentOffset, -0.4f, 0.4f);
        UpdateCarousel(currentOffset);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isSnapping) return;
        int dir = currentOffset > 0.15f ? -1 : currentOffset < -0.15f ? 1 : 0;

        if (dir == 0)
        {
            SnapToSelected(currentOffset, false);
            currentOffset = 0f;
            return;
        }

        int baseIndex = selectedIndex >= 0 ? selectedIndex : (characters.Length - 1) / 2;
        int newIndex = Mathf.Clamp(baseIndex + dir, 0, characters.Length - 1);
        bool changed = newIndex != selectedIndex;
        selectedIndex = newIndex;
        SnapToSelected(currentOffset - dir, changed);
        currentOffset = 0f;
    }

    // --- Click ---

    private void OnPortraitClicked(int index)
    {
        if (index == selectedIndex)
        {
            OnCharacterConfirmed(portraitData[index]);
        }
        else
        {
            float fromOffset = selectedIndex >= 0 ? (selectedIndex - index) + currentOffset : 0f;
            selectedIndex = index;
            currentOffset = 0f;
            SnapToSelected(fromOffset, true);
        }
    }

    private void SnapToSelected(float fromOffset, bool playTheme)
    {
        if (snapAnim != null) StopCoroutine(snapAnim);
        snapAnim = StartCoroutine(AnimateSnap(fromOffset, playTheme));
    }

    private IEnumerator AnimateSnap(float fromOffset, bool playTheme)
    {
        isSnapping = true;
        float elapsed = 0f;

        while (elapsed < snapDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / snapDuration;
            float eased = t * t * (3f - 2f * t);

            float offset = Mathf.Lerp(fromOffset, 0f, eased);
            UpdateCarousel(offset);

            yield return null;
        }

        UpdateCarousel(0f);
        RefreshInfo();
        if (playTheme)
            PlayCharacterTheme(selectedIndex);
        BringSelectedToFront();
        isSnapping = false;
        snapAnim = null;
    }

    private void BringSelectedToFront()
    {
        if (selectedIndex >= 0 && selectedIndex < portraitRects.Count)
            portraitRects[selectedIndex].SetAsLastSibling();
    }

    // --- Info ---

    private void RefreshInfo()
    {
        if (selectedIndex >= 0 && selectedIndex < portraitData.Count)
        {
            characterNameText.text = portraitData[selectedIndex].characterName;
            characterDescText.text = portraitData[selectedIndex].description;
        }
        else
        {
            characterNameText.text = "";
            characterDescText.text = "";
        }
    }

    /// <summary>玩家明确选中（点击/拖动到）某个角色时，才切换到该角色主题曲并播选中音效。</summary>
    private void PlayCharacterTheme(int index)
    {
        if (index < 0 || index >= portraitData.Count) return;
        var data = portraitData[index];
        if (AudioSystemManager.Instance != null)
            AudioSystemManager.Instance.PlayCharacterTheme(data);
        if (!string.IsNullOrEmpty(data.wwiseEventSelect))
            WwiseIntegration.PostEvent(data.wwiseEventSelect);
    }

    private void OnCharacterConfirmed(CharacterData character)
    {
        if (!string.IsNullOrEmpty(character.wwiseEventConfirm))
            WwiseIntegration.PostEvent(character.wwiseEventConfirm);
        if (AudioSystemManager.Instance != null)
            AudioSystemManager.Instance.PlayCharacterConfirm();
        MenuManager.Instance.SelectCharacter(character);
    }

    private void OnBackClicked()
    {
        if (AudioSystemManager.Instance != null) AudioSystemManager.Instance.PlayBack();
        MenuManager.Instance.GoToMainMenu();
    }
}
