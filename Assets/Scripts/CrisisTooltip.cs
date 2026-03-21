using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Панель для отображения информации о кризисе
/// </summary>
public class CrisisTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI crisisNameText;
    [SerializeField] private TextMeshProUGUI crisisDescriptionText;
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
        }
        else
        {
            Debug.LogError("[CrisisTooltip] panelImage не привязана в инспекторе!");
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        Close();
    }

    /// <summary>
    /// Открывает окно с информацией о кризисе
    /// </summary>
    public void ShowCrisis(Crisis crisis)
    {
        if (crisis == null)
            return;

        if (crisisNameText != null)
            crisisNameText.text = crisis.name;

        if (crisisDescriptionText != null)
            crisisDescriptionText.text = crisis.description;

        // Применяем цвет кризиса к фону панели
        if (panelImage != null)
        {
            panelImage.color = crisis.color;
        }

        Open();
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
}
