using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Панель для отображения информации о кризисе
/// </summary>
public class CrisisTooltip : MonoBehaviour
{
    [Header("Основной popup кризиса")]
    [SerializeField] private TextMeshProUGUI crisisNameText;
    [SerializeField] private TextMeshProUGUI crisisDescriptionText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Image panelImage;
    [SerializeField] private Color panelBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);

    [Header("Отдельное меню финального кризиса (назначается через инспектор)")]
    [SerializeField] private GameObject finalCandidateMenuRoot;
    [SerializeField] private Button[] finalCandidateButtons = new Button[4];
    [SerializeField] private TextMeshProUGUI[] finalCandidateButtonTexts = new TextMeshProUGUI[4];
    [SerializeField] private TextMeshProUGUI finalResultText;

    private CanvasGroup canvasGroup;
    private bool isOpen = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (panelImage != null)
        {
            panelImage.color = panelBackgroundColor;
            panelImage.type = Image.Type.Sliced;
        }
        else
        {
            Debug.LogError("[CrisisTooltip] panelImage не привязана в инспекторе!");
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        HideFinalMenu();
        Close();
    }

    public void ShowCrisis(Crisis crisis)
    {
        if (crisis == null)
            return;

        if (crisisNameText != null)
            crisisNameText.text = crisis.name;

        if (crisisDescriptionText != null)
            crisisDescriptionText.text = crisis.description;

        if (panelImage != null)
            panelImage.color = crisis.color;

        Open();
    }

    public void ShowFinalCrisis(Crisis crisis, List<Candidate> candidates, Action<Candidate> onCandidateSelected)
    {
        ShowCrisis(crisis);
        ShowFinalCandidateMenu(candidates, onCandidateSelected);
    }

    public void ShowFinalCandidateMenu(List<Candidate> candidates, Action<Candidate> onCandidateSelected)
    {
        if (!ValidateFinalMenuBindings())
            return;

        Open();
        HideFinalMenu();
        finalCandidateMenuRoot.SetActive(true);

        int buttonCount = Mathf.Min(finalCandidateButtons.Length, finalCandidateButtonTexts.Length);
        int candidatesCount = candidates != null ? candidates.Count : 0;

        for (int i = 0; i < buttonCount; i++)
        {
            Button button = finalCandidateButtons[i];
            TextMeshProUGUI label = finalCandidateButtonTexts[i];

            if (button == null || label == null)
                continue;

            button.onClick.RemoveAllListeners();

            if (i < candidatesCount && candidates[i] != null)
            {
                Candidate candidate = candidates[i];
                label.text = FormatCandidateButtonText(candidate);
                button.gameObject.SetActive(true);
                button.interactable = true;

                button.onClick.AddListener(() =>
                {
                    Debug.Log($"[CrisisTooltip] Выбран кандидат: {candidate.Name}");
                    onCandidateSelected?.Invoke(candidate);
                });
            }
            else
            {
                label.text = "Нет кандидата";
                button.gameObject.SetActive(false);
                button.interactable = false;
            }
        }

        SetFinalResultText(string.Empty);
    }

    public void ShowFinalSkillCheckResult(SkillCheckResult result, bool playerWon)
    {
        if (finalCandidateMenuRoot == null)
        {
            Debug.LogWarning("[CrisisTooltip] finalCandidateMenuRoot не привязан в инспекторе. Результат будет выведен в основной popup.");
            SetFinalResultText(BuildResultText(result, playerWon));
            return;
        }

        Open();
        finalCandidateMenuRoot.SetActive(true);

        if (finalCandidateButtons != null)
        {
            for (int i = 0; i < finalCandidateButtons.Length; i++)
            {
                if (finalCandidateButtons[i] == null)
                    continue;

                finalCandidateButtons[i].onClick.RemoveAllListeners();
                finalCandidateButtons[i].interactable = false;
                finalCandidateButtons[i].gameObject.SetActive(false);
            }
        }

        SetFinalResultText(BuildResultText(result, playerWon));
    }


    public void HideFinalCandidateMenu()
    {
        HideFinalMenu();
    }

    public void Open()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        isOpen = true;
    }

    public void Close()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // Не скрываем меню выбора кандидата при закрытии окна кризиса:
        // финальное меню должно жить независимо от popup кризиса.
        isOpen = false;
    }

    public void Toggle(Crisis crisis = null)
    {
        if (isOpen)
        {
            Close();
        }
        else if (crisis != null)
        {
            ShowCrisis(crisis);
        }
    }

    public bool IsOpen => isOpen;

    private void HideFinalMenu()
    {
        if (finalCandidateMenuRoot != null)
            finalCandidateMenuRoot.SetActive(false);

        SetFinalResultText(string.Empty);

        if (finalCandidateButtons == null)
            return;

        for (int i = 0; i < finalCandidateButtons.Length; i++)
        {
            if (finalCandidateButtons[i] == null)
                continue;

            finalCandidateButtons[i].onClick.RemoveAllListeners();
            finalCandidateButtons[i].interactable = true;
        }
    }

    private bool ValidateFinalMenuBindings()
    {
        if (finalCandidateMenuRoot == null)
        {
            Debug.LogWarning("[CrisisTooltip] finalCandidateMenuRoot не привязан в инспекторе.");
            return false;
        }

        if (finalCandidateButtons == null || finalCandidateButtons.Length < 4)
        {
            Debug.LogWarning("[CrisisTooltip] Нужно привязать 4 кнопки финального меню в инспекторе.");
            return false;
        }

        if (finalCandidateButtonTexts == null || finalCandidateButtonTexts.Length < 4)
        {
            Debug.LogWarning("[CrisisTooltip] Нужно привязать 4 текстовых поля кнопок финального меню в инспекторе.");
            return false;
        }

        return true;
    }

    private void SetFinalResultText(string value)
    {
        if (finalResultText != null)
        {
            finalResultText.text = value;
            return;
        }

        if (!string.IsNullOrEmpty(value) && crisisDescriptionText != null)
        {
            crisisDescriptionText.text += "\n\n" + value;
            Debug.LogWarning("[CrisisTooltip] finalResultText не привязан. Результат выведен в crisisDescriptionText.");
        }
    }

    private string BuildResultText(SkillCheckResult result, bool playerWon)
    {
        string skillCheckText = result.outcome == CheckOutcome.Fail
            ? "Кандидат не прошел скиллчек"
            : "Кандидат прошел скиллчек";

        string endText = playerWon ? "Игрок выиграл" : "Игрок проиграл";
        return $"{skillCheckText}\n{endText}\nСтат: {result.statName} {result.statValue} | Бросок: {result.selectedRoll} | Сумма: {result.totalValue} | Итог: {result.outcome}";
    }

    private string FormatCandidateButtonText(Candidate candidate)
    {
        if (candidate == null)
            return "Нет кандидата";

        return $"{candidate.Name} | ВЛН {candidate.Influence} | ИНТ {candidate.Intellect} | ВОЛ {candidate.Willpower} | ФИН {candidate.Money}";
    }
}
