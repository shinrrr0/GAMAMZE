using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionTooltip : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI actionNameText;
    [SerializeField] private TextMeshProUGUI actionDescriptionText;

    [Header("Buttons")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Panel")]
    [SerializeField] private Image panelImage;
    [SerializeField] private Color panelBackgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.95f);

    [Header("References")]
    [SerializeField] private CandidateCardsController candidateCardsController;

    private CanvasGroup canvasGroup;
    private GameAction currentAction;
    private Candidate currentActor;
    private Candidate currentTarget;

    // Колбэк — вызывается когда игрок подтверждает действие
    // Передаёт действие, актора и цель наружу для сохранения
    public Action<GameAction, Candidate, Candidate> OnActionConfirmed;
    
    // Колбэк — вызывается при отмене (должен вернуть dropdown на значение 0)
    public System.Action OnActionCancelled;

    // Список действий, требующих выбор цели
    private static readonly string[] ActionsRequiringTarget = { "Дебаты", "Интриги", "Написать донос", "Провести полит стрим" };

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (panelImage != null)
            panelImage.color = panelBackgroundColor;

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirm);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancel);

        if (candidateCardsController == null)
            candidateCardsController = FindObjectOfType<CandidateCardsController>();

        Close();
    }

    public void Show(GameAction action, Candidate actor, Candidate target)
    {
        currentAction = action;
        currentActor  = actor;
        currentTarget = target;

        if (actionNameText != null)
            actionNameText.text = action.name;

        if (actionDescriptionText != null)
            actionDescriptionText.text = action.description;

        Open();
    }

    private void OnConfirm()
    {
        // Проверяем, требует ли действие выбор цели
        if (ActionRequiresTarget(currentAction))
        {
            // Нужно выбрать цель
            if (candidateCardsController != null)
            {
                Close(); // Закрываем tooltip перед началом выбора
                                // Передаем currentActor, чтобы нельзя было выбрать себя
                                candidateCardsController.StartCandidateSelection(currentActor, (selectedTarget) =>
                                {
                                    currentTarget = selectedTarget;
                                    // Теперь сохраняем выбор через колбэк
                                    if (currentAction != null)
                                        OnActionConfirmed?.Invoke(currentAction, currentActor, currentTarget);
                                });
            }
            else
            {
                Debug.LogError("[ActionTooltip] CandidateCardsController не найден!");
                Close();
            }
        }
        else
        {
            // Сохраняем выбор через колбэк — не выполняем сразу
            if (currentAction != null)
                OnActionConfirmed?.Invoke(currentAction, currentActor, currentTarget);

            Close();
        }
    }

    private bool ActionRequiresTarget(GameAction action)
    {
        if (action == null)
            return false;

        foreach (string targetAction in ActionsRequiringTarget)
        {
            if (action.name == targetAction)
                return true;
        }

        return false;
    }

    private void OnCancel()
    {
        OnActionCancelled?.Invoke();
        Close();
    }

    private void Open()
    {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Close()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}