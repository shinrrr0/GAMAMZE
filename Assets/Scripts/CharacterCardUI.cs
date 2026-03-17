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

    public ActionOption[] playerActions;
    public ActionOption[] aiActions;

    public int hp;
    public int insanity;
    public int age;
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

    [Header("Dot colors")]
    [SerializeField] private Color activeDotColor = Color.red;
    [SerializeField] private Color inactiveDotColor = new Color(1f, 1f, 1f, 0.15f);

    private ActionOption[] currentPlayerActions = Array.Empty<ActionOption>();
    private ActionOption[] currentAiActions = Array.Empty<ActionOption>();

    public void Apply(CharacterData data)
    {
        if (faceImage != null) faceImage.sprite = data.face;
        if (clothesImage != null) clothesImage.sprite = data.clothes;
        if (headImage != null) headImage.sprite = data.head;

        if (nameText != null)
            nameText.text = data.characterName ?? string.Empty;

        ApplySkills(data.skills);
        ApplyDots(hpDots, data.hp);
        ApplyDots(insanityDots, data.insanity);
        ApplyDots(ageDots, data.age);

        SetupPlayerDropdown(data.playerActions);
        SetupAiDropdown(data.aiActions);
    }

    private void ApplySkills(string[] skills)
    {
        if (skillTexts == null)
            return;

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
        playerActionDropdown.onValueChanged.AddListener(OnPlayerActionChanged);

        OnPlayerActionChanged(0);
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
        aiActionDropdown.value = 0;
        aiActionDropdown.RefreshShownValue();
        aiActionDropdown.onValueChanged.AddListener(OnAiActionChanged);

        OnAiActionChanged(0);
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
    }
}   