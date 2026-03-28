using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PresidentDisplay : MonoBehaviour
{
    public President presidentData;

    [Header("UI Text")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI insanityText;
    public TextMeshProUGUI ageText;
    public TextMeshProUGUI turnText;

    [Header("Optional refs")]
    [SerializeField] private Button nextTurnButtonOverride;

    [Header("Tooltips")]
    [SerializeField] private AbilityTooltip abilityTooltip;
    [SerializeField] private CrisisTooltip crisisTooltip;

    [Header("Crisis row settings")]
    [SerializeField] private float gapAfterAge = 18f;
    [SerializeField] private float gapBeforeTurnButton = 14f;
    [SerializeField] private float rowHeight = 56f;
    [SerializeField] private float dotPreferredSize = 48f;
    [SerializeField] private float dotMinSize = 22f;
    [SerializeField] private float dotSpacing = 6f;
    [SerializeField] private float fallbackWidth = 220f;

    private RectTransform panelRect;
    private RectTransform crisisRowRect;
    private Button nextTurnButton;

    private readonly List<Image> crisisImages = new List<Image>();

    void Start()
    {
        CrisisDatabase.Initialize();
        ResolveReferences();
        EnsureCrisisRow();
        UpdateUI();
    }

    void LateUpdate()
    {
        if (presidentData == null)
            return;

        UpdateUI();
    }

    private void ResolveReferences()
    {
        panelRect = transform as RectTransform;

        if (nextTurnButtonOverride != null)
        {
            nextTurnButton = nextTurnButtonOverride;
            return;
        }

        Transform btn = transform.Find("NextTurnButton");
        if (btn != null)
        {
            nextTurnButton = btn.GetComponent<Button>();
            if (nextTurnButton != null)
                return;
        }

        nextTurnButton = GetComponentInChildren<Button>(true);
    }

    private void EnsureCrisisRow()
    {
        if (crisisRowRect != null)
            return;

        Transform existing = transform.Find("RuntimeCrisisRow");
        if (existing != null)
        {
            crisisRowRect = existing as RectTransform;
        }
        else
        {
            GameObject rowGO = new GameObject("RuntimeCrisisRow", typeof(RectTransform));
            rowGO.transform.SetParent(transform, false);
            crisisRowRect = rowGO.GetComponent<RectTransform>();
        }

        // Удаляем всё, что может ломать ручную раскладку
        LayoutGroup lg = crisisRowRect.GetComponent<LayoutGroup>();
        if (lg != null) Destroy(lg);

        ContentSizeFitter csf = crisisRowRect.GetComponent<ContentSizeFitter>();
        if (csf != null) Destroy(csf);

        HorizontalOrVerticalLayoutGroup hov = crisisRowRect.GetComponent<HorizontalOrVerticalLayoutGroup>();
        if (hov != null) Destroy(hov);

        GridLayoutGroup gl = crisisRowRect.GetComponent<GridLayoutGroup>();
        if (gl != null) Destroy(gl);

        crisisRowRect.anchorMin = new Vector2(0f, 1f);
        crisisRowRect.anchorMax = new Vector2(0f, 1f);
        crisisRowRect.pivot = new Vector2(0f, 0.5f);
        crisisRowRect.SetAsLastSibling();
    }

    public void UpdateUI()
    {
        if (presidentData == null)
        {
            Debug.LogWarning("[PresidentDisplay] presidentData is NULL");
            return;
        }

        if (hpText == null || insanityText == null || ageText == null || turnText == null)
        {
            Debug.LogWarning("[PresidentDisplay] hpText / insanityText / ageText / turnText not assigned");
            return;
        }

        hpText.text = $"HP: {presidentData.hp}";
        insanityText.text = $"Insanity: {presidentData.insanity}";
        ageText.text = $"Age: {presidentData.age}";
        turnText.text = $"Turn: {presidentData.turnCount}";

        hpText.color = Color.green;
        insanityText.color = Color.magenta;
        ageText.color = Color.cyan;
        turnText.color = Color.yellow;

        ResolveReferences();
        EnsureCrisisRow();

        PositionCrisisRow();
        SyncCrisisIcons();
        LayoutCrisisIconsSingleRow();
    }

    private void PositionCrisisRow()
    {
        if (panelRect == null || ageText == null || crisisRowRect == null)
            return;

        RectTransform ageRect = ageText.rectTransform;
        RectTransform buttonRect = nextTurnButton != null ? nextTurnButton.GetComponent<RectTransform>() : null;

        // Получаем правую границу Age в координатах панели
        Vector3[] ageCorners = new Vector3[4];
        ageRect.GetWorldCorners(ageCorners);
        Vector2 ageRightLocal = panelRect.InverseTransformPoint(ageCorners[3]);

        float left = ageRightLocal.x + gapAfterAge;
        float right;

        if (buttonRect != null)
        {
            Vector3[] btnCorners = new Vector3[4];
            buttonRect.GetWorldCorners(btnCorners);
            Vector2 btnLeftLocal = panelRect.InverseTransformPoint(btnCorners[0]);
            right = btnLeftLocal.x - gapBeforeTurnButton;
        }
        else
        {
            right = left + fallbackWidth;
        }

        float width = Mathf.Max(60f, right - left);

        // Вертикально выравниваем по Age
        Vector2 ageCenterLocal = panelRect.InverseTransformPoint(ageRect.transform.position);

        crisisRowRect.anchorMin = new Vector2(0f, 1f);
        crisisRowRect.anchorMax = new Vector2(0f, 1f);
        crisisRowRect.pivot = new Vector2(0f, 0.5f);
        crisisRowRect.sizeDelta = new Vector2(width, rowHeight);
        crisisRowRect.anchoredPosition = new Vector2(left, ageCenterLocal.y);
    }

    private void SyncCrisisIcons()
    {
        int needed = presidentData.activeCrises != null ? presidentData.activeCrises.Count : 0;

        while (crisisImages.Count < needed)
        {
            GameObject dotGO = new GameObject($"CrisisDot_{crisisImages.Count}", typeof(RectTransform), typeof(Image), typeof(Button));
            dotGO.transform.SetParent(crisisRowRect, false);

            RectTransform rt = dotGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);

            Image img = dotGO.GetComponent<Image>();
            img.color = Color.white;
            img.raycastTarget = true;

            Button btn = dotGO.GetComponent<Button>();
            btn.targetGraphic = img;

            crisisImages.Add(img);
        }

        for (int i = 0; i < crisisImages.Count; i++)
        {
            bool active = i < needed;
            Image img = crisisImages[i];

            if (img == null)
                continue;

            img.gameObject.SetActive(active);

            if (!active)
                continue;

            Crisis crisis = presidentData.activeCrises[i];
            img.sprite = crisis != null ? crisis.icon : null;
            img.color = crisis != null ? crisis.color : Color.white;

            int capturedIndex = i;
            Button btn = img.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnCrisisDotClicked(capturedIndex));
        }
    }

    private void LayoutCrisisIconsSingleRow()
    {
        if (crisisRowRect == null)
            return;

        int activeCount = 0;
        for (int i = 0; i < crisisImages.Count; i++)
        {
            if (crisisImages[i] != null && crisisImages[i].gameObject.activeSelf)
                activeCount++;
        }

        if (activeCount == 0)
            return;

        float width = crisisRowRect.rect.width;
        if (width <= 0f)
            width = crisisRowRect.sizeDelta.x;

        float height = crisisRowRect.rect.height;
        if (height <= 0f)
            height = crisisRowRect.sizeDelta.y;

        float totalSpacing = dotSpacing * (activeCount - 1);
        float preferredTotal = activeCount * dotPreferredSize + totalSpacing;

        float dotSize = dotPreferredSize;
        if (preferredTotal > width)
            dotSize = (width - totalSpacing) / activeCount;

        dotSize = Mathf.Clamp(dotSize, dotMinSize, dotPreferredSize);
        dotSize = Mathf.Min(dotSize, height);

        float totalUsedWidth = activeCount * dotSize + totalSpacing;
        float startX = Mathf.Max(0f, (width - totalUsedWidth) * 0.5f);

        int visibleIndex = 0;
        for (int i = 0; i < crisisImages.Count; i++)
        {
            Image img = crisisImages[i];
            if (img == null || !img.gameObject.activeSelf)
                continue;

            RectTransform rt = img.rectTransform;
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(dotSize, dotSize);
            rt.anchoredPosition = new Vector2(startX + visibleIndex * (dotSize + dotSpacing), 0f);

            visibleIndex++;
        }
    }

    private void OnCrisisDotClicked(int index)
    {
        if (presidentData == null || presidentData.activeCrises == null)
            return;

        if (index < 0 || index >= presidentData.activeCrises.Count)
            return;

        Crisis crisis = presidentData.activeCrises[index];
        if (crisis != null && crisisTooltip != null)
        {
            crisisTooltip.ShowCrisis(crisis);
            Debug.Log($"[PresidentDisplay] Clicked on crisis: {crisis.name}");
        }
        else
        {
            Debug.LogWarning($"[PresidentDisplay] Crisis is null or tooltip is null at index {index}");
        }
    }
}