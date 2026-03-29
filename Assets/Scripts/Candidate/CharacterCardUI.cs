using System;
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

    public Ability[] abilities;
    public int abilityCount;

            public ActionOption[] playerActions;
    public ActionOption[] aiActions;

    public int hp;
    public int insanity;
    public int age;

    public Candidate candidate;
    public Candidate target;
}

public class CharacterCardUI : MonoBehaviour
{
    [Header("Portrait layers")]
    [SerializeField] private Image faceImage;
    [SerializeField] private Image clothesImage;
    [SerializeField] private Image headImage;

    [Header("Optional portrait click target")]
    [SerializeField] private Button portraitButton;

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

    [Header("Dot colors")]
    [SerializeField] private Color activeDotColor = Color.red;
    [SerializeField] private Color inactiveDotColor = new Color(1f, 1f, 1f, 0.15f);

    [Header("Perks")]
    [SerializeField] private AbilityTooltip abilityTooltip;

    public Action<int> OnPlayerActionSelected;

                    private ActionOption[] currentPlayerActions = Array.Empty<ActionOption>();
    private ActionOption[] currentAiActions = Array.Empty<ActionOption>();
    private Candidate currentTarget = null;
    private bool fieldsSearched = false;

    public void Apply(CharacterData data)
    {
        if (!fieldsSearched)
        {
            AutoFindFields();
            fieldsSearched = true;
        }

        if (faceImage != null && data.face != null) faceImage.sprite = data.face;
        if (clothesImage != null && data.clothes != null) clothesImage.sprite = data.clothes;
        if (headImage != null && data.head != null) headImage.sprite = data.head;

        if (nameText != null)
            nameText.text = data.characterName ?? string.Empty;

                                ApplySkills(data.skills);
        ApplyPerks(data.abilities);
        ApplyDots(hpDots, data.hp);
        ApplyDots(insanityDots, data.insanity);
        ApplyDots(ageDots, data.age);

        currentTarget = data.target;
        SetupPlayerDropdown(data.playerActions);
        SetupAiDropdown(data.aiActions);
    }

    private void AutoFindFields()
    {
        if (nameText == null)
        {
            TMP_Text[] allTexts = GetComponentsInChildren<TMP_Text>(true);
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

        if (skillTexts == null || skillTexts.Length == 0)
        {
            Transform skillsContainer = transform.Find("Skills and Perks");

            if (skillsContainer != null)
            {
                List<TMP_Text> foundSkills = new List<TMP_Text>();
                TMP_Text[] skillsInContainer = skillsContainer.GetComponentsInChildren<TMP_Text>(true);

                foreach (var txt in skillsInContainer)
                {
                    if (!txt.gameObject.name.Contains("Description") &&
                        !txt.gameObject.name.Contains("Header") &&
                        !txt.gameObject.name.Contains("Label") &&
                        !txt.gameObject.name.Contains("Perk") &&
                        !txt.gameObject.name.Contains("perk"))
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

        if (playerActionDropdown == null || aiActionDropdown == null)
        {
            Transform actionsContainer = transform.Find("Actions");
            if (actionsContainer != null)
            {
                TMP_Dropdown[] dropdowns = actionsContainer.GetComponentsInChildren<TMP_Dropdown>(true);

                foreach (var dd in dropdowns)
                {
                    if (playerActionDropdown == null &&
                        (dd.gameObject.name.Contains("Player") || dd.gameObject.name.Contains("player")))
                    {
                        playerActionDropdown = dd;
                        Debug.Log($"[CharacterCardUI] Found playerActionDropdown: {dd.gameObject.name}");
                    }
                    else if (aiActionDropdown == null &&
                             (dd.gameObject.name.Contains("AI") || dd.gameObject.name.Contains("Ai") || dd.gameObject.name.Contains("ai")))
                    {
                        aiActionDropdown = dd;
                        Debug.Log($"[CharacterCardUI] Found aiActionDropdown: {dd.gameObject.name}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("[CharacterCardUI] 'Actions' контейнер не найден!");
            }
        }

        Debug.Log($"[CharacterCardUI] AutoFind: nameText={nameText?.gameObject.name}, skillTexts={skillTexts?.Length ?? 0}, playerDropdown={playerActionDropdown?.gameObject.name}");
    }

    private void ApplyPerks(Ability[] abilities)
    {
        Transform perksContainer = transform.Find("Skills and Perks/Perks");
        if (perksContainer == null)
        {
            Transform skillsContainer = transform.Find("Skills and Perks");
            if (skillsContainer != null)
                perksContainer = skillsContainer.Find("Perks");
        }

        if (perksContainer == null)
            return;

        for (int i = perksContainer.childCount - 1; i >= 0; i--)
            Destroy(perksContainer.GetChild(i).gameObject);

        if (abilities == null || abilities.Length == 0)
            return;

        for (int i = 0; i < abilities.Length; i++)
        {
            Ability ability = abilities[i];
            if (ability == null) continue;

            GameObject iconGO = new GameObject($"PerkIcon_{i}");
            iconGO.transform.SetParent(perksContainer, false);

            RectTransform rect = iconGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(30f, 30f);

            Image img = iconGO.AddComponent<Image>();
            img.color = ability.color;
            if (ability.icon != null)
                img.sprite = ability.icon;

            Button btn = iconGO.AddComponent<Button>();
            btn.targetGraphic = img;

            Ability captured = ability;
            btn.onClick.AddListener(() =>
            {
                if (abilityTooltip != null)
                    abilityTooltip.ShowAbility(captured);
            });
        }
    }

    private void ApplySkills(string[] skills)
    {
        if (skillTexts == null)
        {
            Debug.LogWarning("[CharacterCardUI] skillTexts is NULL!");
            return;
        }

        for (int i = 0; i < skillTexts.Length; i++)
        {
            if (skillTexts[i] == null)
                continue;

            if (skills != null && i < skills.Length)
                skillTexts[i].text = skills[i];
            else
                skillTexts[i].text = string.Empty;
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

    private void SetupPlayerDropdown(ActionOption[] actions)
    {
        currentPlayerActions = actions ?? Array.Empty<ActionOption>();

        if (playerActionDropdown == null)
            return;

        playerActionDropdown.onValueChanged.RemoveAllListeners();
        playerActionDropdown.ClearOptions();

        List<string> options = new List<string>();
        options.Add("выберите действие");

        for (int i = 0; i < currentPlayerActions.Length; i++)
            options.Add(currentPlayerActions[i].title);

        if (options.Count <= 1)
            options.Add("-");

        playerActionDropdown.AddOptions(options);
        playerActionDropdown.value = 0;
        playerActionDropdown.RefreshShownValue();

        if (playerActionDescriptionText != null)
            playerActionDescriptionText.text = string.Empty;

        playerActionDropdown.onValueChanged.AddListener(OnPlayerActionChanged);
    }

    private void SetupAiDropdown(ActionOption[] actions)
    {
        currentAiActions = actions ?? Array.Empty<ActionOption>();

        if (aiActionDropdown != null)
            aiActionDropdown.gameObject.SetActive(false);

        if (aiActionDescriptionText != null)
            aiActionDescriptionText.gameObject.SetActive(false);

                string actionName = (currentAiActions.Length > 0) ? currentAiActions[0].title : "—";
        string targetInfo = (currentTarget != null) ? $" (Цель: {currentTarget.Name})" : "";

        Transform parent = aiActionDropdown != null
            ? aiActionDropdown.transform.parent
            : transform;

        TMP_Text aiLabel = null;
        Transform existing = parent.Find("AiActionLabel");
        if (existing != null)
            aiLabel = existing.GetComponent<TMP_Text>();

        if (aiLabel == null)
        {
            GameObject labelGO = new GameObject("AiActionLabel");
            labelGO.transform.SetParent(parent, false);

            RectTransform dstRect = labelGO.AddComponent<RectTransform>();

            if (aiActionDropdown != null)
            {
                RectTransform srcRect = aiActionDropdown.GetComponent<RectTransform>();
                dstRect.anchorMin = srcRect.anchorMin;
                dstRect.anchorMax = srcRect.anchorMax;
                dstRect.anchoredPosition = srcRect.anchoredPosition;
                dstRect.sizeDelta = srcRect.sizeDelta;
                dstRect.pivot = srcRect.pivot;
            }
            else
            {
                dstRect.anchorMin = new Vector2(0f, 0f);
                dstRect.anchorMax = new Vector2(1f, 0.5f);
                dstRect.offsetMin = Vector2.zero;
                dstRect.offsetMax = Vector2.zero;
            }

            aiLabel = labelGO.AddComponent<TextMeshProUGUI>();
            aiLabel.fontSize = 16;
            aiLabel.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            aiLabel.alignment = TextAlignmentOptions.MidlineLeft;
        }

        aiLabel.text = "Кандидат предпримет: " + actionName + targetInfo;
    }

    private void OnPlayerActionChanged(int index)
    {
        if (playerActionDescriptionText != null)
        {
            if (index <= 0 || index - 1 >= currentPlayerActions.Length)
                playerActionDescriptionText.text = string.Empty;
            else
                playerActionDescriptionText.text = currentPlayerActions[index - 1].description ?? string.Empty;
        }

        OnPlayerActionSelected?.Invoke(index);
    }

    public void ResetActionDropdown()
    {
        if (playerActionDropdown == null)
            return;

        playerActionDropdown.onValueChanged.RemoveListener(OnPlayerActionChanged);
        playerActionDropdown.value = 0;
        playerActionDropdown.RefreshShownValue();

        if (playerActionDescriptionText != null)
            playerActionDescriptionText.text = string.Empty;

        playerActionDropdown.onValueChanged.AddListener(OnPlayerActionChanged);
    }

    public Button GetOrCreatePortraitButton()
    {
        if (portraitButton != null)
            return portraitButton;

        Image targetImage = headImage != null ? headImage : (faceImage != null ? faceImage : clothesImage);
        if (targetImage == null)
            targetImage = GetComponentInChildren<Image>(true);

        if (targetImage == null)
            return null;

        portraitButton = targetImage.GetComponent<Button>();
        if (portraitButton == null)
            portraitButton = targetImage.gameObject.AddComponent<Button>();

        portraitButton.targetGraphic = targetImage;
        return portraitButton;
    }
}