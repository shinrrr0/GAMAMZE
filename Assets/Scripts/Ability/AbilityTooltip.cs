using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Панель для отображения информации о способности
/// </summary>
public class AbilityTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private TextMeshProUGUI abilityDescriptionText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Image panelImage;
    [SerializeField] private Color panelBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.95f); // Тёмно-серый

    private CanvasGroup canvasGroup;
    private bool isOpen = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Устанавливаем цвет фона панели
        if (panelImage != null)
        {
            panelImage.color = panelBackgroundColor;
            panelImage.type = Image.Type.Simple;
            Debug.Log($"[AbilityTooltip] Установлен цвет панели: {panelBackgroundColor}");
        }
        else
        {
            Debug.LogError("[AbilityTooltip] panelImage не привязана в инспекторе!");
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        Close();
    }

    /// <summary>
    /// Открывает окно с информацией о способности
    /// </summary>
    public void ShowAbility(Ability ability)
    {
        if (ability == null)
            return;

        if (abilityNameText != null)
            abilityNameText.text = ability.name;

        if (abilityDescriptionText != null)
            abilityDescriptionText.text = ability.description;

        Open();
        Debug.Log($"[AbilityTooltip] Показываю способность: {ability.name}");
    }

    /// <summary>
    /// Открывает панель
    /// </summary>
    public void Open()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        isOpen = true;
    }

    /// <summary>
    /// Закрывает панель
    /// </summary>
    public void Close()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        isOpen = false;
    }

    /// <summary>
    /// Переключает состояние (открыто/закрыто)
    /// </summary>
    public void Toggle(Ability ability = null)
    {
        if (isOpen)
        {
            Close();
        }
        else if (ability != null)
        {
            ShowAbility(ability);
        }
    }

    public bool IsOpen => isOpen;
}
