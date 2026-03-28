using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CandidateTurnChange
{
    public string candidateName;

    public int influenceBefore;
    public int influenceAfter;

    public int intellectBefore;
    public int intellectAfter;

    public int willpowerBefore;
    public int willpowerAfter;

    public int moneyBefore;
    public int moneyAfter;

    // Информация о действии игрока
    public ActionExecutionResult playerActionResult;
    
    // Информация о действии AI
    public ActionExecutionResult aiActionResult;

    public bool HasChanges =>
        influenceBefore != influenceAfter ||
        intellectBefore != intellectAfter ||
        willpowerBefore != willpowerAfter ||
        moneyBefore != moneyAfter;

    public string GetSummaryText()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"<b>{candidateName}</b>");
        sb.AppendLine();

        // Действие игрока
        if (!string.IsNullOrEmpty(playerActionResult.actionName))
        {
            sb.AppendLine($"<color=#FFD700>🎮 Действие игрока: {playerActionResult.actionName}</color>");
            sb.AppendLine($"  Проверка: {playerActionResult.statChecked} ({playerActionResult.statValue})");
            sb.AppendLine($"  Бросок d10: {playerActionResult.diceRoll} → Итого: {playerActionResult.checkTotal}");
            sb.AppendLine($"  {playerActionResult.outcomeText}");
            sb.AppendLine($"  Сюжет: {(string.IsNullOrWhiteSpace(playerActionResult.narrativeDescription) ? playerActionResult.resultDescription : playerActionResult.narrativeDescription)}");
            if (!string.IsNullOrWhiteSpace(playerActionResult.mechanicsDescription))
                sb.AppendLine($"  <color=#A0A0A0>Разбор: {playerActionResult.mechanicsDescription}</color>");
            sb.AppendLine();
        }

        // Действие AI
        if (!string.IsNullOrEmpty(aiActionResult.actionName))
        {
            sb.AppendLine($"<color=#00FF00>🤖 Действие кандидата: {aiActionResult.actionName}</color>");
            sb.AppendLine($"  Проверка: {aiActionResult.statChecked} ({aiActionResult.statValue})");
            sb.AppendLine($"  Бросок d10: {aiActionResult.diceRoll} → Итого: {aiActionResult.checkTotal}");
            sb.AppendLine($"  {aiActionResult.outcomeText}");
            sb.AppendLine($"  Сюжет: {(string.IsNullOrWhiteSpace(aiActionResult.narrativeDescription) ? aiActionResult.resultDescription : aiActionResult.narrativeDescription)}");
            if (!string.IsNullOrWhiteSpace(aiActionResult.mechanicsDescription))
                sb.AppendLine($"  <color=#A0A0A0>Разбор: {aiActionResult.mechanicsDescription}</color>");
            sb.AppendLine();
        }

        // Итоговые изменения характеристик
        sb.AppendLine("<b>📊 Итоговые изменения:</b>");
        
        bool any = false;

        if (influenceBefore != influenceAfter)
        {
            string change = influenceAfter > influenceBefore ? $"<color=#00FF00>+{influenceAfter - influenceBefore}</color>" : $"<color=#FF0000>{influenceAfter - influenceBefore}</color>";
            sb.AppendLine($"  Влияние: {influenceBefore} → {influenceAfter} {change}");
            any = true;
        }

        if (intellectBefore != intellectAfter)
        {
            string change = intellectAfter > intellectBefore ? $"<color=#00FF00>+{intellectAfter - intellectBefore}</color>" : $"<color=#FF0000>{intellectAfter - intellectBefore}</color>";
            sb.AppendLine($"  Интеллект: {intellectBefore} → {intellectAfter} {change}");
            any = true;
        }

        if (willpowerBefore != willpowerAfter)
        {
            string change = willpowerAfter > willpowerBefore ? $"<color=#00FF00>+{willpowerAfter - willpowerBefore}</color>" : $"<color=#FF0000>{willpowerAfter - willpowerBefore}</color>";
            sb.AppendLine($"  Воля: {willpowerBefore} → {willpowerAfter} {change}");
            any = true;
        }

        if (moneyBefore != moneyAfter)
        {
            string change = moneyAfter > moneyBefore ? $"<color=#00FF00>+{moneyAfter - moneyBefore}</color>" : $"<color=#FF0000>{moneyAfter - moneyBefore}</color>";
            sb.AppendLine($"  Деньги: {moneyBefore} → {moneyAfter} {change}");
            any = true;
        }

        if (!any)
            sb.AppendLine("  Нет изменений");

        return sb.ToString().TrimEnd();
    }
}

public struct CandidateSnapshot
{
    public int influence;
    public int intellect;
    public int willpower;
    public int money;

    public CandidateSnapshot(Candidate candidate)
    {
        influence = candidate.Influence;
        intellect = candidate.Intellect;
        willpower = candidate.Willpower;
        money = candidate.Money;
    }
}

public struct PresidentTurnChange
{
    public int hpBefore;
    public int hpAfter;

    public int insanityBefore;
    public int insanityAfter;

    public bool HasHpChange => hpBefore != hpAfter;
    public bool HasInsanityChange => insanityBefore != insanityAfter;
}

public class TurnSummaryPopup : MonoBehaviour
{
    [Header("Optional manual bindings")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button closeButton;
    [SerializeField] private ScrollRect scrollRect;

    private RectTransform panelRect;

    private void Awake()
    {
        EnsureRoot();
        EnsureFullUI();
        BindButton();
        Close();
    }

    private void EnsureRoot()
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect == null)
            rect = gameObject.AddComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        Image bg = GetComponent<Image>();
        if (bg == null)
            bg = gameObject.AddComponent<Image>();

        bg.color = new Color(0f, 0f, 0f, 0.55f);
        bg.raycastTarget = true;
    }

    private void EnsureFullUI()
    {
        Transform panel = transform.Find("Panel");
        if (panel == null)
        {
            GameObject panelGO = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panelGO.transform.SetParent(transform, false);
            panel = panelGO.transform;

            RectTransform pr = panel.GetComponent<RectTransform>();
            pr.anchorMin = new Vector2(0.5f, 0.5f);
            pr.anchorMax = new Vector2(0.5f, 0.5f);
            pr.pivot = new Vector2(0.5f, 0.5f);
            pr.sizeDelta = new Vector2(900f, 650f);
            pr.anchoredPosition = Vector2.zero;

            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.14f, 0.14f, 0.14f, 0.96f);
        }

        panelRect = panel.GetComponent<RectTransform>();

        if (titleText == null)
        {
            Transform title = panel.Find("Title");
            if (title == null)
            {
                GameObject titleGO = new GameObject("Title", typeof(RectTransform));
                titleGO.transform.SetParent(panel, false);

                RectTransform tr = titleGO.GetComponent<RectTransform>();
                tr.anchorMin = new Vector2(0f, 1f);
                tr.anchorMax = new Vector2(1f, 1f);
                tr.pivot = new Vector2(0.5f, 1f);
                tr.offsetMin = new Vector2(24f, -90f);
                tr.offsetMax = new Vector2(-24f, -24f);

                titleText = titleGO.AddComponent<TextMeshProUGUI>();
                titleText.fontSize = 34;
                titleText.alignment = TextAlignmentOptions.TopLeft;
                titleText.color = Color.white;
                titleText.textWrappingMode = TextWrappingModes.NoWrap;
            }
            else
            {
                titleText = title.GetComponent<TMP_Text>();
            }
        }

        if (scrollRect == null || bodyText == null)
            EnsureScrollArea(panel);

        if (closeButton == null)
        {
            Transform close = panel.Find("CloseButton");
            if (close == null)
            {
                GameObject buttonGO = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
                buttonGO.transform.SetParent(panel, false);

                RectTransform br = buttonGO.GetComponent<RectTransform>();
                br.anchorMin = new Vector2(0.5f, 0f);
                br.anchorMax = new Vector2(0.5f, 0f);
                br.pivot = new Vector2(0.5f, 0f);
                br.sizeDelta = new Vector2(220f, 56f);
                br.anchoredPosition = new Vector2(0f, 20f);

                Image btnImage = buttonGO.GetComponent<Image>();
                btnImage.color = new Color(0.28f, 0.28f, 0.28f, 1f);

                closeButton = buttonGO.GetComponent<Button>();

                GameObject labelGO = new GameObject("Text", typeof(RectTransform));
                labelGO.transform.SetParent(buttonGO.transform, false);

                RectTransform lr = labelGO.GetComponent<RectTransform>();
                lr.anchorMin = Vector2.zero;
                lr.anchorMax = Vector2.one;
                lr.offsetMin = Vector2.zero;
                lr.offsetMax = Vector2.zero;

                TextMeshProUGUI label = labelGO.AddComponent<TextMeshProUGUI>();
                label.text = "Закрыть";
                label.fontSize = 24;
                label.alignment = TextAlignmentOptions.Center;
                label.color = Color.white;
            }
            else
            {
                closeButton = close.GetComponent<Button>();
            }
        }
    }

    private void EnsureScrollArea(Transform panel)
    {
        Transform scroll = panel.Find("Scroll View");
        if (scroll == null)
        {
            GameObject scrollGO = new GameObject("Scroll View", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollGO.transform.SetParent(panel, false);

            RectTransform sr = scrollGO.GetComponent<RectTransform>();
            sr.anchorMin = new Vector2(0f, 0f);
            sr.anchorMax = new Vector2(1f, 1f);
            sr.offsetMin = new Vector2(24f, 90f);
            sr.offsetMax = new Vector2(-24f, -95f);

            Image scrollBg = scrollGO.GetComponent<Image>();
            scrollBg.color = new Color(0f, 0f, 0f, 0.15f);

            scrollRect = scrollGO.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 30f;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            GameObject viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportGO.transform.SetParent(scrollGO.transform, false);

            RectTransform vr = viewportGO.GetComponent<RectTransform>();
            vr.anchorMin = Vector2.zero;
            vr.anchorMax = Vector2.one;
            vr.offsetMin = Vector2.zero;
            vr.offsetMax = new Vector2(-18f, 0f);

            Image viewportImage = viewportGO.GetComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f);

            Mask mask = viewportGO.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject contentGO = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGO.transform.SetParent(viewportGO.transform, false);

            RectTransform cr = contentGO.GetComponent<RectTransform>();
            cr.anchorMin = new Vector2(0f, 1f);
            cr.anchorMax = new Vector2(1f, 1f);
            cr.pivot = new Vector2(0.5f, 1f);
            cr.offsetMin = Vector2.zero;
            cr.offsetMax = Vector2.zero;

            VerticalLayoutGroup vlg = contentGO.GetComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.spacing = 0;

            ContentSizeFitter csf = contentGO.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            GameObject bodyGO = new GameObject("Body", typeof(RectTransform), typeof(ContentSizeFitter));
            bodyGO.transform.SetParent(contentGO.transform, false);

            bodyText = bodyGO.AddComponent<TextMeshProUGUI>();
            bodyText.fontSize = 24;
            bodyText.color = Color.white;
            bodyText.alignment = TextAlignmentOptions.TopLeft;
            bodyText.textWrappingMode = TextWrappingModes.Normal;
            bodyText.overflowMode = TextOverflowModes.Overflow;

            ContentSizeFitter bodyFitter = bodyGO.GetComponent<ContentSizeFitter>();
            bodyFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            bodyFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            scrollRect.viewport = vr;
            scrollRect.content = cr;

            GameObject scrollbarGO = new GameObject("Scrollbar Vertical", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            scrollbarGO.transform.SetParent(scrollGO.transform, false);

            RectTransform sbr = scrollbarGO.GetComponent<RectTransform>();
            sbr.anchorMin = new Vector2(1f, 0f);
            sbr.anchorMax = new Vector2(1f, 1f);
            sbr.pivot = new Vector2(1f, 1f);
            sbr.sizeDelta = new Vector2(18f, 0f);
            sbr.anchoredPosition = Vector2.zero;

            Image sbBg = scrollbarGO.GetComponent<Image>();
            sbBg.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

            Scrollbar scrollbar = scrollbarGO.GetComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            GameObject slidingArea = new GameObject("Sliding Area", typeof(RectTransform));
            slidingArea.transform.SetParent(scrollbarGO.transform, false);

            RectTransform sar = slidingArea.GetComponent<RectTransform>();
            sar.anchorMin = Vector2.zero;
            sar.anchorMax = Vector2.one;
            sar.offsetMin = new Vector2(2f, 2f);
            sar.offsetMax = new Vector2(-2f, -2f);

            GameObject handleGO = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handleGO.transform.SetParent(slidingArea.transform, false);

            RectTransform hr = handleGO.GetComponent<RectTransform>();
            hr.anchorMin = Vector2.zero;
            hr.anchorMax = Vector2.one;
            hr.offsetMin = Vector2.zero;
            hr.offsetMax = Vector2.zero;

            Image handleImage = handleGO.GetComponent<Image>();
            handleImage.color = new Color(0.75f, 0.75f, 0.75f, 1f);

            scrollbar.handleRect = hr;
            scrollbar.targetGraphic = handleImage;

            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        }
        else
        {
            scrollRect = scroll.GetComponent<ScrollRect>();
            if (scrollRect != null && bodyText == null)
                bodyText = scroll.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    private void BindButton()
    {
        if (closeButton == null)
            return;

        closeButton.onClick.RemoveListener(Close);
        closeButton.onClick.AddListener(Close);
    }

    public void ShowSummary(
        int turnNumber,
        PresidentTurnChange presidentChange,
        List<CandidateTurnChange> candidateChanges,
        Crisis newCrisis)
    {
        if (titleText != null)
            titleText.text = $"Итоги хода {turnNumber}";

        if (bodyText != null)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Изменения президента:");
            sb.AppendLine();

            bool presidentSectionHasData = false;

            if (presidentChange.HasHpChange)
            {
                sb.AppendLine($"- Здоровье: {presidentChange.hpBefore} → {presidentChange.hpAfter}");
                presidentSectionHasData = true;
            }

            if (presidentChange.HasInsanityChange)
            {
                sb.AppendLine($"- Безумие: {presidentChange.insanityBefore} → {presidentChange.insanityAfter}");
                presidentSectionHasData = true;
            }

            if (!presidentSectionHasData)
                sb.AppendLine("- без изменений");

            sb.AppendLine();
            sb.AppendLine("Кризисы:");
            sb.AppendLine();

            if (newCrisis != null)
            {
                sb.AppendLine($"- Новый кризис: {newCrisis.name}");
                if (!string.IsNullOrWhiteSpace(newCrisis.description))
                    sb.AppendLine($"  {newCrisis.description.Replace("\n", "\n  ")}");
            }
            else
                sb.AppendLine("- новых кризисов нет");

            sb.AppendLine();
            sb.AppendLine("Изменения характеристик:");
            sb.AppendLine();

            if (candidateChanges != null && candidateChanges.Count > 0)
            {
                foreach (var change in candidateChanges)
                {
                    sb.AppendLine(change.GetSummaryText());
                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("- изменений характеристик не было");
            }

            bodyText.text = sb.ToString().TrimEnd();
        }

        Open();

        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    public void Open()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Close()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}