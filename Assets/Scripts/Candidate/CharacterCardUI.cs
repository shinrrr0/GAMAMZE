using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct ActionOption
{
    public string title;

    [TextArea(2, 5)]
    public string description;
}

[Serializable]
public class CharacterData
{
    public Sprite face;
    public Sprite clothes;
    public Sprite head;

    public string characterName;
    public string[] skills;
    
    // Способности кандидата
    public Ability[] abilities;

    public ActionOption[] playerActions;
    public ActionOption[] aiActions;

    public int hp;
    public int insanity;
    public int age;
    public int abilityCount; // Количество способностей для отображения кружков
    
    // Ссылка на кандидата для вызова действий
    public Candidate candidate;
}

public class CharacterCardUI : MonoBehaviour
{
    [Header("Portrait layers")]
    [SerializeField] private Image faceImage;
    [SerializeField] private Image clothesImage;
    [SerializeField] private Image headImage;

    [Header("Texts")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text[] skillTexts;

    [Header("Action UI")]
    [SerializeField] private TMP_Dropdown playerActionDropdown;
    [SerializeField] private TMP_Text playerActionDescriptionText;
    [SerializeField] private TMP_Dropdown aiActionDropdown;
    [SerializeField] private TMP_Text aiActionDescriptionText;

    [Header("Dots")]
    [SerializeField] private Image[] hpDots;
    [SerializeField] private Image[] insanityDots;
    [SerializeField] private Image[] ageDots;
    [SerializeField] private Image[] abilityDots; // Кружки для отображения способностей

    [Header("Ability Tooltip")]
    [SerializeField] private AbilityTooltip abilityTooltip;

    [Header("Dot colors")]
    [SerializeField] private Color activeDotColor = Color.red;
    [SerializeField] private Color inactiveDotColor = new Color(1f, 1f, 1f, 0.15f);

    private ActionOption[] currentPlayerActions = Array.Empty<ActionOption>();
    private ActionOption[] currentAiActions = Array.Empty<ActionOption>();
    private Ability[] currentAbilities = Array.Empty<Ability>();
    private bool fieldsSearched = false;
    private Candidate currentCandidate; // Ссылка на кандидата

    public void Apply(CharacterData data)
    {
        // Первый раз ищем поля
        if (!fieldsSearched)
        {
            AutoFindFields();
            fieldsSearched = true;
        }

        // Сохраняем ссылку на кандидата
        currentCandidate = data.candidate;
        string candidateName = currentCandidate != null ? currentCandidate.Name : "NULL";
        Debug.Log($"[CharacterCardUI] Установлен кандидат: {candidateName}");

        // Если спрайты не переданы - не перезаписываем уже установленные (например, сгенерированные другим скриптом)
        if (faceImage != null && data.face != null) faceImage.sprite = data.face;
        if (clothesImage != null && data.clothes != null) clothesImage.sprite = data.clothes;
        if (headImage != null && data.head != null) headImage.sprite = data.head;

        if (nameText != null)
            nameText.text = data.characterName ?? string.Empty;

        ApplySkills(data.skills);
        ApplyDots(hpDots, data.hp);
        ApplyDots(insanityDots, data.insanity);
        ApplyDots(ageDots, data.age);
        
        // Сохраняем способности и отображаем кружки
        currentAbilities = data.abilities ?? Array.Empty<Ability>();
        ApplyAbilityDots();

        SetupPlayerDropdown(data.playerActions);
        SetupAiDropdown(data.aiActions);
    }

    private void AutoFindFields()
    {
        // Ищем Name через GetComponentsInChildren
        if (nameText == null)
        {
            TMP_Text[] allTexts = GetComponentsInChildren<TMP_Text>();
            foreach (var txt in allTexts)
            {
                if (txt.gameObject.name == "Name" || txt.gameObject.name.Contains("Name"))
                {
                    nameText = txt;
                    Debug.Log($"[CharacterCardUI] Found nameText: {txt.gameObject.name}");
                    break;
                }
            }
        }

        // Ищем skillTexts: сначала ищем контейнер "Skills and Perks", потом его дочерних элементов
        if (skillTexts == null || skillTexts.Length == 0)
        {
            Transform skillsContainer = transform.Find("Skills and Perks");
            
            if (skillsContainer != null)
            {
                // Находим все TMP_Text компоненты в контейнере
                List<TMP_Text> foundSkills = new List<TMP_Text>();
                TMP_Text[] skillsInContainer = skillsContainer.GetComponentsInChildren<TMP_Text>();
                
                foreach (var txt in skillsInContainer)
                {
                    // Пропускаем если это заголовок или описание по другим признакам
                    if (!txt.gameObject.name.Contains("Description") && 
                        !txt.gameObject.name.Contains("Header") &&
                        !txt.gameObject.name.Contains("Label"))
                    {
                        foundSkills.Add(txt);
                        Debug.Log($"[CharacterCardUI] Found skill field: {txt.gameObject.name}");
                    }
                }
                
                if (foundSkills.Count > 0)
                {
                    skillTexts = foundSkills.ToArray();
                    Debug.Log($"[CharacterCardUI] Total skill fields found: {skillTexts.Length}");
                }
            }
            else
            {
                Debug.LogWarning("[CharacterCardUI] 'Skills and Perks' контейнер не найден!");
            }
        }

        // Ищем abilityDots: генерируем их динамически
        if (abilityDots == null || abilityDots.Length == 0)
        {
            GenerateAbilityDots();
        }

        Debug.Log($"[CharacterCardUI] AutoFind: nameText={nameText?.gameObject.name}, skillTexts={skillTexts?.Length ?? 0}, abilityDots={abilityDots?.Length ?? 0}");
    }

    /// <summary>
    /// Генерирует кружки для способностей программно
    /// </summary>
    private void GenerateAbilityDots()
    {
        Transform perksContainer = transform.Find("Skill and Perks/Perks");
        if (perksContainer == null)
            perksContainer = transform.Find("Skills and Perks/Perks");
        
        if (perksContainer == null)
        {
            Debug.LogWarning("[CharacterCardUI] Perks container not found");
            return;
        }
        
        const int maxDots = 3;
        List<Image> dotsList = new List<Image>();
        
        // Переиспользуем существующие Image компоненты
        for (int i = 0; i < perksContainer.childCount; i++)
        {
            Image img = perksContainer.GetChild(i).GetComponent<Image>();
            if (img != null)
            {
                dotsList.Add(img);
                Debug.Log($"[CharacterCardUI] Found existing perk dot: {perksContainer.GetChild(i).gameObject.name}");
            }
        }
        
        // Если недостаточно, создаём новые
        while (dotsList.Count < maxDots)
        {
            GameObject dotObj = new GameObject($"Perk_{dotsList.Count}");
            RectTransform rectTransform = dotObj.AddComponent<RectTransform>();
            rectTransform.SetParent(perksContainer, false);
            rectTransform.sizeDelta = new Vector2(30, 30);
            rectTransform.anchoredPosition = new Vector2(40 * dotsList.Count, 0);
            
            Image img = dotObj.AddComponent<Image>();
            img.color = activeDotColor;
            
            // Добавляем Button компонент для обработки кликов
            Button btn = dotObj.AddComponent<Button>();
            btn.targetGraphic = img;
            
            dotsList.Add(img);
            Debug.Log($"[CharacterCardUI] Created ability dot: {dotObj.name}");
        }
        
        abilityDots = dotsList.ToArray();
        SetupAbilityDotClickHandlers();
        Debug.Log($"[CharacterCardUI] Total ability dots: {abilityDots.Length}");
    }

    /// <summary>
    /// Привязывает обработчики кликов к кружкам способностей
    /// </summary>
    private void SetupAbilityDotClickHandlers()
    {
        if (abilityDots == null || abilityTooltip == null)
            return;

        for (int i = 0; i < abilityDots.Length; i++)
        {
            int index = i; // Локальная копия для замыкания
            Button btn = abilityDots[i].GetComponent<Button>();
            
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnAbilityDotClicked(index));
                Debug.Log($"[CharacterCardUI] Setup click handler for ability dot {index}");
            }
        }
    }

    /// <summary>
    /// Вызывается при клике на кружок способности
    /// </summary>
    private void OnAbilityDotClicked(int abilityIndex)
    {
        if (abilityIndex < 0 || abilityIndex >= currentAbilities.Length)
            return;

        abilityTooltip.ShowAbility(currentAbilities[abilityIndex]);
        Debug.Log($"[CharacterCardUI] Clicked on ability: {currentAbilities[abilityIndex].name}");
    }

    private void ApplySkills(string[] skills)
    {
        if (skillTexts == null)
        {
            Debug.LogWarning("[CharacterCardUI] skillTexts is NULL!");
            return;
        }

        Debug.Log($"[CharacterCardUI] ApplySkills: skills count={skills?.Length ?? 0}, skillTexts count={skillTexts.Length}");
        
        for (int i = 0; i < skillTexts.Length; i++)
        {
            if (skillTexts[i] == null)
                continue;

            if (skills != null && i < skills.Length)
            {
                skillTexts[i].text = skills[i];
                Debug.Log($"[CharacterCardUI] skillTexts[{i}] = '{skills[i]}'");
            }
            else
            {
                skillTexts[i].text = string.Empty;
                Debug.Log($"[CharacterCardUI] skillTexts[{i}] = '' (empty)");
            }
        }
    }

    private void ApplyDots(Image[] dots, int value)
    {
        if (dots == null)
            return;

        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] == null)
                continue;

            dots[i].color = i < value ? activeDotColor : inactiveDotColor;
        }
    }

    private void RefreshCandidateFromData()
    {
        if (currentCandidate == null)
            return;

        if (nameText != null)
            nameText.text = currentCandidate.Name;

        ApplySkills(new[]
        {
            $"Влияние: {currentCandidate.Influence}",
            $"Интеллект: {currentCandidate.Intellect}",
            $"Воля: {currentCandidate.Willpower}",
            $"Деньги: {currentCandidate.Money}"
        });

        ApplyDots(hpDots, currentCandidate.Influence);
        ApplyDots(insanityDots, currentCandidate.Intellect);
        ApplyDots(ageDots, currentCandidate.Age);

        currentAbilities = currentCandidate.Abilities?.ToArray() ?? Array.Empty<Ability>();
        ApplyAbilityDots();
    }

    private void SetupPlayerDropdown(ActionOption[] actions)
    {
        currentPlayerActions = actions ?? Array.Empty<ActionOption>();

        if (playerActionDropdown == null)
            return;

        playerActionDropdown.onValueChanged.RemoveListener(OnPlayerActionChanged);
        playerActionDropdown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 0; i < currentPlayerActions.Length; i++)
            options.Add(currentPlayerActions[i].title);

        if (options.Count == 0)
            options.Add("-");

        playerActionDropdown.AddOptions(options);
        playerActionDropdown.value = 0;
        playerActionDropdown.RefreshShownValue();
        
        // Обновляем описание
        if (playerActionDescriptionText != null && currentPlayerActions.Length > 0)
            playerActionDescriptionText.text = currentPlayerActions[0].description;
        
        // Привязываем слушателя после инициализации
        playerActionDropdown.onValueChanged.AddListener(OnPlayerActionChanged);
    }

    private void SetupAiDropdown(ActionOption[] actions)
    {
        currentAiActions = actions ?? Array.Empty<ActionOption>();

        if (aiActionDropdown == null)
            return;

        aiActionDropdown.onValueChanged.RemoveListener(OnAiActionChanged);
        aiActionDropdown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 0; i < currentAiActions.Length; i++)
            options.Add(currentAiActions[i].title);

        if (options.Count == 0)
            options.Add("-");

        aiActionDropdown.AddOptions(options);
        
        // Не выбираем случайное действие здесь (может быть во время сериализации)
        // Вместо этого установим первое действие
        aiActionDropdown.value = 0;
        aiActionDropdown.RefreshShownValue();
        
        // Обновляем описание первого действия
        if (aiActionDescriptionText != null && currentAiActions.Length > 0)
            aiActionDescriptionText.text = currentAiActions[0].description;
        
        // Привязываем слушателя после инициализации
        aiActionDropdown.onValueChanged.AddListener(OnAiActionChanged);
        
        // Отложим выбор случайного действия на один фрейм
        StartCoroutine(ApplyRandomAiActionNextFrame());
    }
    
    /// <summary>
    /// Выбирает случайное AI действие после инициализации
    /// </summary>
    private System.Collections.IEnumerator ApplyRandomAiActionNextFrame()
    {
        yield return null; // Ждём следующего фрейма
        
        if (currentAiActions.Length > 0 && aiActionDropdown != null)
        {
            int randomIndex = UnityEngine.Random.Range(0, currentAiActions.Length);
            aiActionDropdown.value = randomIndex;
            aiActionDropdown.RefreshShownValue();
        }
    }

    private void OnPlayerActionChanged(int index)
    {
        if (playerActionDescriptionText == null)
            return;

        if (currentPlayerActions.Length == 0 || index < 0 || index >= currentPlayerActions.Length)
        {
            playerActionDescriptionText.text = string.Empty;
            return;
        }

        playerActionDescriptionText.text = currentPlayerActions[index].description;
        
        // Вызовем соответствующее действие кандидата
        if (currentCandidate != null)
        {
            ExecutePlayerAction(index);
        }
    }

    private void OnAiActionChanged(int index)
    {
        if (aiActionDescriptionText == null)
            return;

        if (currentAiActions.Length == 0 || index < 0 || index >= currentAiActions.Length)
        {
            aiActionDescriptionText.text = string.Empty;
            return;
        }

        aiActionDescriptionText.text = currentAiActions[index].description;
        
        // Вызовем соответствующее действие кандидата для AI
        if (currentCandidate != null)
        {
            ExecuteAiAction(index);
        }
    }

    /// <summary>
    /// Вызывает действие кандидата по индексу из playerActions
    /// </summary>
    private void ExecutePlayerAction(int actionIndex)
    {
        if (currentPlayerActions.Length == 0 || actionIndex < 0 || actionIndex >= currentPlayerActions.Length)
            return;

        string actionTitle = currentPlayerActions[actionIndex].title;
        Debug.Log($"[CharacterCardUI] Вызываю действие игрока: {actionTitle}");

        switch (actionTitle)
        {
            case "Воровство":
                CandidateActions.Steal(currentCandidate);
                break;
            case "Лобирование":
                CandidateActions.Lobby(currentCandidate);
                break;
            case "Обращение важное":
                CandidateActions.MajorAppeal(currentCandidate, 0);
                break;
            case "Интриги":
                Debug.Log("[CharacterCardUI] Интриги требуют выбора цели (не реализовано в этом UI)");
                break;
            case "Дебаты":
                Debug.Log("[CharacterCardUI] Дебаты требуют выбора оппонента (не реализовано в этом UI)");
                break;
            default:
                Debug.LogWarning($"[CharacterCardUI] Неизвестное действие: {actionTitle}");
                break;
        }

        RefreshCandidateFromData();
    }

    /// <summary>
    /// Вызывает действие кандидата по индексу из aiActions (AI/случайное)
    /// </summary>
    private void ExecuteAiAction(int actionIndex)
    {
        if (currentAiActions.Length == 0 || actionIndex < 0 || actionIndex >= currentAiActions.Length)
            return;

        string actionTitle = currentAiActions[actionIndex].title;
        Debug.Log($"[CharacterCardUI] Вызываю действие AI: {actionTitle}");

        switch (actionTitle)
        {
            case "Воровство":
                CandidateActions.Steal(currentCandidate);
                break;
            case "Лобирование":
                CandidateActions.Lobby(currentCandidate);
                break;
            case "Обращение важное":
                CandidateActions.MajorAppeal(currentCandidate, 0);
                break;
            case "Интриги":
                Debug.Log("[CharacterCardUI] AI Интриги требуют выбора цели (не реализовано)");
                break;
            case "Дебаты":
                Debug.Log("[CharacterCardUI] AI Дебаты требуют выбора оппонента (не реализовано)");
                break;
            default:
                Debug.LogWarning($"[CharacterCardUI] Неизвестное AI действие: {actionTitle}");
                break;
        }

        RefreshCandidateFromData();
    }

    /// <summary>
    /// Отображает способности кандидата как кружки с их цветами (включает/выключает их)
    /// </summary>
    private void ApplyAbilityDots()
    {
        if (abilityDots == null)
            return;

        for (int i = 0; i < abilityDots.Length; i++)
        {
            if (abilityDots[i] == null)
                continue;

            if (i < currentAbilities.Length)
            {
                // Включаем кружок и устанавливаем цвет из способности
                abilityDots[i].gameObject.SetActive(true);
                abilityDots[i].color = currentAbilities[i].color;
            }
            else
            {
                // Отключаем неиспользуемые кружки
                abilityDots[i].gameObject.SetActive(false);
            }
        }
    }
}   