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

    private CanvasGroup canvasGroup;
    private GameAction currentAction;
    private Candidate currentActor;
    private Candidate currentTarget;

    // Колбэк — вызывается когда игрок подтверждает действие
    // Передаёт действие, актора и цель наружу для сохранения
    public Action<GameAction, Candidate, Candidate> OnActionConfirmed;

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
        // Сохраняем выбор через колбэк — не выполняем сразу
        if (currentAction != null)
            OnActionConfirmed?.Invoke(currentAction, currentActor, currentTarget);

        Close();
    }

    private void OnCancel()
    {
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