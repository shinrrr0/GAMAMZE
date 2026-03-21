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

    private Crisis[] currentCrises;

void Start()
{
    CrisisDatabase.Initialize();

    if (crisisImages == null)
        crisisImages = new Image[0];

    UpdateUI();
} 

    void Update()
    {
        if (presidentData == null) return;
        UpdateUI();
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
        Debug.LogWarning("hp, insanity, age or turn not specified");
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

    if (presidentData.activeCrises == null)
        return;

    if (crisisImages == null)
        crisisImages = new Image[0];

    // Если кризисов стало больше, чем есть UI-элементов — создаём новые
    if (presidentData.activeCrises.Count > crisisImages.Length)
    {
        ExpandCrisisDots();
    }

    for (int i = 0; i < crisisImages.Length; i++)
    {
        if (crisisImages[i] == null)
            continue;

        if (i < presidentData.activeCrises.Count)
        {
            Crisis currentCrisis = presidentData.activeCrises[i];
            crisisImages[i].gameObject.SetActive(true);

            if (currentCrisis != null && currentCrisis.icon != null)
                crisisImages[i].sprite = currentCrisis.icon;

            if (currentCrisis != null)
                crisisImages[i].color = currentCrisis.color;
        }
        else
        {
            crisisImages[i].gameObject.SetActive(false);
        }
    }

    SetupCrisisDotClickHandlers();
}

    /// <summary>
    /// Генерирует кружки для кризисов программно (подобно способностям)
    /// </summary>
private void GenerateCrisisDots(int count)
{
    Transform crisisContainer = transform.Find("CrisisInfoRoot");

    if (crisisContainer == null)
    {
        crisisContainer = transform.Find("Crisis");

        if (crisisContainer == null)
            crisisContainer = transform.Find("Crises");
    }

    if (crisisContainer == null)
        return;

    foreach (Transform child in crisisContainer)
    {
        Destroy(child.gameObject);
    }

    List<Image> dots = new List<Image>();

    for (int i = 0; i < count; i++)
    {
        GameObject dotGO = new GameObject($"CrisisDot_{i}");
        dotGO.transform.SetParent(crisisContainer, false);

        RectTransform rect = dotGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(32f, 32f);

        Image image = dotGO.AddComponent<Image>();
        image.color = Color.white;

        Button button = dotGO.AddComponent<Button>();
        button.targetGraphic = image;

        dots.Add(image);
    }

    crisisImages = dots.ToArray();
    SetupCrisisDotClickHandlers();
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
            crisisContainer = transform.Find("Crises");
    }

    if (crisisContainer == null)
        return;

    List<Image> dotsList = new List<Image>(crisisImages);

    int newDotsNeeded = presidentData.activeCrises.Count - crisisImages.Length;
    for (int i = 0; i < newDotsNeeded; i++)
    {
        int index = crisisImages.Length + i;
        GameObject dotGO = new GameObject($"CrisisDot_{index}");
        dotGO.transform.SetParent(crisisContainer, false);

        RectTransform rectTransform = dotGO.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(50, 50);

        Image img = dotGO.AddComponent<Image>();
        img.color = Color.white;

        Button btn = dotGO.AddComponent<Button>();
        btn.targetGraphic = img;

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
        return;

    for (int i = 0; i < crisisImages.Length; i++)
    {
        if (crisisImages[i] == null)
            continue;

        int index = i;
        Button btn = crisisImages[i].GetComponent<Button>();

        if (btn == null)
        {
            btn = crisisImages[i].gameObject.AddComponent<Button>();
            btn.targetGraphic = crisisImages[i];
        }

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => OnCrisisDotClicked(index));
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

