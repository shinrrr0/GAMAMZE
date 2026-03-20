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

    [Header("UI Crisises")]
    public Image[] crisisImages;

    [Header("Tooltips")]
    [SerializeField] private AbilityTooltip abilityTooltip;
    [SerializeField] private CrisisTooltip crisisTooltip;

    private Crisis[] currentCrises = new Crisis[3];

    void Start()
    {
        // Инициализируем кризис доты при старте
        GenerateCrisisDots();
        
        // Инициализируем CrisisDatabase если еще не инициализирована
        CrisisDatabase.Initialize();
    }

    void Update()
    {
        if (presidentData == null) return;
        UpdateUI();
    }

    public void UpdateUI()
    {
        // Дополнительная проверка
        if (presidentData == null)
        {
            Debug.LogWarning("[PresidentDisplay] presidentData is NULL");
            return;
        }

        // Закрываем tooltip при обновлении хода
        if (abilityTooltip != null)
            abilityTooltip.Close();
        if (crisisTooltip != null)
            crisisTooltip.Close();

        if (hpText == null || insanityText == null || ageText == null || turnText == null) 
        {
            Debug.LogWarning("hp, insanity, age or turn not specified");
            return;
        }


        hpText.text = $"HP: {presidentData.hp}";
        insanityText.text = $"Insanity: {presidentData.insanity}";
        ageText.text = $"Age: {presidentData.age}";
        turnText.text = $"Turn: {presidentData.turnCount}";

        // Цвета (лучше задать один раз в Start, но для теста пойдет)
        hpText.color = Color.green;
        insanityText.color = Color.magenta;
        ageText.color = Color.cyan;
        turnText.color = Color.yellow; 

        // Отображаем ровно столько кризисов, сколько нужно
        if (crisisImages == null || crisisImages.Length == 0)
        {
            return;
        }
        
        if (presidentData.activeCrises == null)
        {
            return;
        }

        // Динамически создаем новые доты если активных кризисов больше чем дотов
        if (presidentData.activeCrises.Count > crisisImages.Length)
        {
            ExpandCrisisDots();
            // Расширяем массив текущих кризисов
            System.Array.Resize(ref currentCrises, presidentData.activeCrises.Count);
        }

        for (int i = 0; i < crisisImages.Length; i++)
        {
            // Проверяем, не null ли сам элемент массива
            if (crisisImages[i] == null)
                continue;

            if (i < presidentData.activeCrises.Count)
            {
                Crisis currentCrisis = presidentData.activeCrises[i];
                crisisImages[i].gameObject.SetActive(true);
                
                // Сохраняем ссылку для onClick handler
                currentCrises[i] = currentCrisis;
                
                // Установим иконку если она есть
                if (currentCrisis != null && currentCrisis.icon != null)
                {
                    crisisImages[i].sprite = currentCrisis.icon;
                }
                
                // Устанавливаем цвет кризиса
                if (currentCrisis != null)
                {
                    crisisImages[i].color = currentCrisis.color;
                }
            }
            else
            {
                // Скрываем неиспользуемые иконки
                crisisImages[i].gameObject.SetActive(false);
                currentCrises[i] = null;
            }
        }
        
        // Устанавливаем click handlers для кризисов
        SetupCrisisDotClickHandlers();
    }

    /// <summary>
    /// Генерирует кружки для кризисов программно (подобно способностям)
    /// </summary>
    private void GenerateCrisisDots()
    {
        // Ищем контейнер для кризисов
        Transform crisisContainer = transform.Find("CrisisInfoRoot");
        
        if (crisisContainer == null)
        {
            crisisContainer = transform.Find("Crisis");
            
            if (crisisContainer == null)
            {
                crisisContainer = transform.Find("Crises");
            }
        }
        
        if (crisisContainer == null)
        {
            return;
        }

        // Получаем существующие Image компоненты (НЕ создаем)
        List<Image> dotsList = new List<Image>();

        int childCount = crisisContainer.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Image img = crisisContainer.GetChild(i).GetComponent<Image>();
            if (img != null)
            {
                dotsList.Add(img);
            }
        }

        crisisImages = dotsList.ToArray();
        if (dotsList.Count > 0)
            Debug.Log($"[PresidentDisplay] Found {crisisImages.Length} pre-made crisis dots in hierarchy");
    }

    /// <summary>
    /// Расширяет массив кризис-дотов когда активных кризисов больше чем дотов
    /// </summary>
    private void ExpandCrisisDots()
    {
        Transform crisisContainer = transform.Find("CrisisInfoRoot");
        
        if (crisisContainer == null)
        {
            crisisContainer = transform.Find("Crisis");
            
            if (crisisContainer == null)
            {
                crisisContainer = transform.Find("Crises");
            }
        }
        
        if (crisisContainer == null)
            return;

        // Расширяем массив
        List<Image> dotsList = new List<Image>(crisisImages);

        // Создаем новые доты до нужного количества
        int newDotsNeeded = presidentData.activeCrises.Count - crisisImages.Length;
        for (int i = 0; i < newDotsNeeded; i++)
        {
            int index = crisisImages.Length + i;
            GameObject dotGO = new GameObject($"CrisisDot_{index}");
            dotGO.transform.SetParent(crisisContainer, false);

            RectTransform rectTransform = dotGO.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(50, 50);
            rectTransform.anchoredPosition = new Vector2(index * 60, 0);

            Image img = dotGO.AddComponent<Image>();
            img.color = Color.white;

            dotsList.Add(img);
        }

        crisisImages = dotsList.ToArray();
    }

    /// <summary>
    /// Устанавливает click handlers для иконок кризисов
    /// </summary>
    private void SetupCrisisDotClickHandlers()
    {
        if (crisisImages == null || crisisImages.Length == 0)
        {
            Debug.LogWarning("[PresidentDisplay] crisisImages is empty or not initialized");
            return;
        }

        for (int i = 0; i < crisisImages.Length; i++)
        {
            if (crisisImages[i] == null)
            {
                Debug.LogWarning($"[PresidentDisplay] crisisImages[{i}] is NULL");
                continue;
            }

            int index = i; // Локальная копия для замыкания
            Button btn = crisisImages[i].GetComponent<Button>();
            
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnCrisisDotClicked(index));
                Debug.Log($"[PresidentDisplay] Setup click handler for crisis dot {index}");
            }
            else
            {
                Debug.LogWarning($"[PresidentDisplay] Crisis dot {i} doesn't have Button component! Add it in the scene.");
            }
        }
    }

    /// <summary>
    /// Обработчик клика по иконке кризиса
    /// </summary>
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
