using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CandidateCardsController : MonoBehaviour
{
    [Header("Optional: Автогенерация спрайтов")]
    [SerializeField] private CharacterGenerator2D generator;

    [Header("Options")]
    [SerializeField] private bool generateOnStart = true;

    [Header("Tooltip для действий")]
    [SerializeField] private ActionTooltip actionTooltip;

    private readonly List<Candidate> candidates = new List<Candidate>();
    private readonly List<CharacterCardUI> cardUIs = new List<CharacterCardUI>();
    private readonly List<GameAction> allActions = new List<GameAction>();

    private GameAction[] pendingActions;
    private Candidate[] pendingTargets;

    private int currentActionCardIndex = -1;

    private bool isInCandidateSelection = false;
    private System.Action<Candidate> onCandidateSelected;

    private readonly List<Button> portraitButtons = new List<Button>();
    private readonly List<Selectable> disabledForTargetSelection = new List<Selectable>();

    private void Start()
    {
        ActionDatabase.Initialize();

        if (generateOnStart)
            StartCoroutine(GenerateCandidatesNextFrame());
    }

    private void Update()
    {
        if (isInCandidateSelection && Input.GetKeyDown(KeyCode.Escape))
            CancelCandidateSelection();
    }

    private IEnumerator GenerateCandidatesNextFrame()
    {
        yield return null;
        GenerateCandidates();
    }

    [ContextMenu("Generate Candidates")]
    public void GenerateCandidates()
    {
        cardUIs.Clear();
        candidates.Clear();
        portraitButtons.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            if (child.name == "President" || child.name == "President Panel")
                continue;

            CharacterCardUI cardUI = child.GetComponent<CharacterCardUI>();
            if (cardUI != null)
                cardUIs.Add(cardUI);
        }

        if (cardUIs.Count == 0)
        {
            Debug.LogError("[CandidateCardsController] Не найдено CharacterCardUI компонентов!");
            return;
        }

        if (generator != null)
            generator.GenerateUICharacters();

        allActions.Clear();
        allActions.AddRange(ActionDatabase.GetAll());

        pendingActions = new GameAction[cardUIs.Count];
        pendingTargets = new Candidate[cardUIs.Count];

        if (actionTooltip != null)
        {
            actionTooltip.OnActionConfirmed = (action, actor, target) =>
            {
                int idx = candidates.IndexOf(actor);
                if (idx >= 0)
                {
                    pendingActions[idx] = action;
                    pendingTargets[idx] = target;
                }

                currentActionCardIndex = -1;
            };

            actionTooltip.OnActionCancelled = () =>
            {
                ResetCurrentActionSelection();
            };
        }

        HashSet<string> usedClasses = new HashSet<string>();

        for (int i = 0; i < cardUIs.Count; i++)
        {
            Candidate candidate = new Candidate(usedClasses);
            candidates.Add(candidate);

            if (candidate.Abilities.Count > 0)
                usedClasses.Add(candidate.Abilities[0].name);
        }

        ApplyAllCards();
    }

    private Candidate GetDefaultTargetFor(int actorIndex)
    {
        if (candidates == null || candidates.Count <= 1)
            return null;

        for (int t = 0; t < candidates.Count; t++)
        {
            if (t != actorIndex && !candidates[t].IsInPrison)
                return candidates[t];
        }

        for (int t = 0; t < candidates.Count; t++)
        {
            if (t != actorIndex)
                return candidates[t];
        }

        return null;
    }

    private List<GameAction> BuildActionListForCandidate(Candidate candidate)
    {
        List<GameAction> actions = new List<GameAction>();

        if (candidate.IsInPrison)
        {
            foreach (var action in allActions)
            {
                if (action.name == "Жить по закону" || action.name == "Жить по понятиям")
                    actions.Add(action);
            }

            return actions;
        }

        actions.AddRange(allActions);

        if (candidate != null)
        {
            if (candidate.Willpower >= 5)
            {
                actions.Add(new GameAction(
                    "Экстренное урегулирование (социальное)",
                    "Жёсткое вмешательство в общественную повестку и попытка быстро сбить самый опасный социальный кризис. Механика: ВОЛ; провал — -1 ВОЛ, успех — убрать худший социальный кризис, крит — убрать его и +1 ВОЛ.",
                    (actor, target) => CandidateActions.ResolveSocialRegulation(actor)));
            }

            if (candidate.Intellect >= 5)
            {
                actions.Add(new GameAction(
                    "Экстренное урегулирование (экономическое)",
                    "Пакет срочных мер для цифр, поставок и чиновников с очень нервными лицами. Механика: ИНТ; провал — -1 ИНТ, успех — убрать худший экономический кризис, крит — убрать его и +1 ИНТ.",
                    (actor, target) => CandidateActions.ResolveEconomicRegulation(actor)));
            }
        }

        GameAction classAction = ActionDatabase.GetClassAction(candidate);
        if (classAction != null)
            actions.Insert(0, classAction);

        return actions;
    }

    private ActionOption[] BuildPlayerActions(Candidate candidate)
    {
        List<GameAction> actions = BuildActionListForCandidate(candidate);
        ActionOption[] playerActions = new ActionOption[actions.Count];

        for (int j = 0; j < actions.Count; j++)
        {
            playerActions[j] = new ActionOption
            {
                title = actions[j].name,
                description = actions[j].description
            };
        }

        return playerActions;
    }

    private void ApplyAllCards()
    {
        portraitButtons.Clear();

        for (int i = 0; i < cardUIs.Count && i < candidates.Count; i++)
        {
            Candidate candidate = candidates[i];

            CharacterData data = new CharacterData
            {
                characterName = candidate.IsInPrison ? $"{candidate.Name} [ТЮРЬМА]" : candidate.Name,
                skills = new[]
                {
                    $"Влияние: {candidate.Influence}",
                    $"Интеллект: {candidate.Intellect}",
                    $"Воля: {candidate.Willpower}",
                    $"Деньги: {candidate.Money}"
                },
                abilities = candidate.Abilities.ToArray(),
                abilityCount = candidate.Abilities.Count,
                hp = candidate.Influence,
                insanity = candidate.Intellect,
                age = candidate.Age,
                candidate = candidate,
                playerActions = BuildPlayerActions(candidate),
                aiActions = new ActionOption[0]
            };

            cardUIs[i].Apply(data);

            int cardIndex = i;
            Candidate capturedCandidate = candidate;

            Button portraitButton = cardUIs[i].GetOrCreatePortraitButton();
            if (portraitButton != null)
            {
                portraitButton.onClick.RemoveAllListeners();
                portraitButton.onClick.AddListener(() => OnCandidatePortraitClicked(capturedCandidate));
                portraitButtons.Add(portraitButton);
            }

            cardUIs[i].OnPlayerActionSelected = (actionIndex) =>
            {
                if (isInCandidateSelection)
                    return;

                if (actionIndex == 0)
                    return;

                int realActionIndex = actionIndex - 1;
                List<GameAction> candidateActions = BuildActionListForCandidate(candidates[cardIndex]);
                if (realActionIndex < 0 || realActionIndex >= candidateActions.Count)
                    return;

                GameAction selectedAction = candidateActions[realActionIndex];
                Candidate actor = candidates[cardIndex];
                Candidate target = GetDefaultTargetFor(cardIndex);

                if (actionTooltip != null)
                {
                    currentActionCardIndex = cardIndex;
                    actionTooltip.Show(selectedAction, actor, target);
                }
                else
                {
                    Debug.LogWarning("[CandidateCardsController] ActionTooltip не привязан!");
                }
            };
        }
    }

    public List<CandidateTurnChange> ExecuteAllActions()
    {
        List<CandidateTurnChange> changes = new List<CandidateTurnChange>();

        if (candidates == null || candidates.Count == 0)
            return changes;

        for (int i = 0; i < candidates.Count; i++)
        {
            Candidate candidate = candidates[i];
            CandidateSnapshot before = new CandidateSnapshot(candidate);
            ActionExecutionResult playerResult = new ActionExecutionResult();

            if (candidate.IsInPrison)
            {
                candidate.TickPrison();
                changes.Add(new CandidateTurnChange
                {
                    candidateName = candidate.Name,
                    influenceBefore = before.influence,
                    influenceAfter = candidate.Influence,
                    intellectBefore = before.intellect,
                    intellectAfter = candidate.Intellect,
                    willpowerBefore = before.willpower,
                    willpowerAfter = candidate.Willpower,
                    moneyBefore = before.money,
                    moneyAfter = candidate.Money,
                    playerActionResult = new ActionExecutionResult { actionName = "—", resultDescription = "Кандидат в тюрьме и пропускает ход" },
                    aiActionResult = new ActionExecutionResult()
                });
                continue;
            }

            if (pendingActions != null && pendingActions[i] != null)
            {
                playerResult = ExecuteActionByName(pendingActions[i].name, candidate, pendingTargets[i]);
                pendingActions[i] = null;
                pendingTargets[i] = null;
            }

            CandidateSnapshot after = new CandidateSnapshot(candidate);

            CandidateTurnChange change = new CandidateTurnChange
            {
                candidateName = candidate.Name,
                influenceBefore = before.influence,
                influenceAfter = after.influence,
                intellectBefore = before.intellect,
                intellectAfter = after.intellect,
                willpowerBefore = before.willpower,
                willpowerAfter = after.willpower,
                moneyBefore = before.money,
                moneyAfter = after.money,
                playerActionResult = playerResult,
                aiActionResult = new ActionExecutionResult()
            };

            if (change.HasChanges || !string.IsNullOrEmpty(playerResult.actionName))
                changes.Add(change);
        }

        ApplyAllCards();
        return changes;
    }

    public void ResolveEndOfTurnEffects(bool crisisHappened)
    {
        if (candidates == null)
            return;

        foreach (Candidate candidate in candidates)
        {
            CandidateActions.ResolvePredictionOutcome(candidate, crisisHappened);
            CandidateActions.ResolvePassiveTurnEffects(candidate);
        }

        ApplyAllCards();
    }

    private ActionExecutionResult ExecuteActionByName(string actionName, Candidate actor, Candidate target)
    {
        return actionName switch
        {
            "Воровство" => CandidateActions.Steal(actor),
            "Лоббирование" => CandidateActions.Lobby(actor),
            "Образование" => CandidateActions.MajorAppeal(actor, 0),
            "Экстренное урегулирование (социальное)" => CandidateActions.ResolveSocialRegulation(actor),
            "Экстренное урегулирование (экономическое)" => CandidateActions.ResolveEconomicRegulation(actor),
            "Зачитать по бумажке" => CandidateActions.ReadFromScript(actor),
            "Интриги" => CandidateActions.Intrigue(actor, target),
            "Дебаты" => CandidateActions.Debate(actor, target),
            "Терпеть" => CandidateActions.Endure(actor),
            "Жить по закону" => CandidateActions.LiveByLaw(actor),
            "Жить по понятиям" => CandidateActions.LiveByCode(actor),
            "Написать пост в тг" => CandidateActions.PhilosopherPost(actor),
            "Сделать разоблачение" => CandidateActions.AntiCorruptionExpose(actor),
            "Поднять мятеж" => CandidateActions.RaiseMutiny(actor),
            "Провести полит стрим" => CandidateActions.PoliticalStream(actor, target),
            "Сделать предсказание" => CandidateActions.MakePrediction(actor),
            "Написать донос" => CandidateActions.FileReport(actor, target),
            "Фиксируем прибыль" => CandidateActions.CashOutInvestments(actor),
            _ => new ActionExecutionResult { actionName = actionName, resultDescription = "Действие не найдено" }
        };
    }

    public List<Candidate> GetCandidates()
    {
        return new List<Candidate>(candidates);
    }

    public CharacterCardUI GetCardUIForCandidate(Candidate candidate)
    {
        int index = candidates.IndexOf(candidate);
        if (index >= 0 && index < cardUIs.Count)
            return cardUIs[index];
        return null;
    }

    public bool AreAllActionsSelected()
    {
        if (candidates == null || candidates.Count == 0)
            return true;

        for (int i = 0; i < candidates.Count; i++)
        {
            if (candidates[i].IsInPrison)
                continue;

            if (pendingActions == null || pendingActions[i] == null)
                return false;
        }

        return true;
    }

    public void StartCandidateSelection(System.Action<Candidate> callback)
    {
        if (isInCandidateSelection)
            return;

        isInCandidateSelection = true;
        onCandidateSelected = callback;

        DeactivateAllUIExceptPortraits();

        Debug.Log("[CandidateCardsController] Начат режим выбора цели. Активны только портреты. ESC — отмена.");
    }

    private void DeactivateAllUIExceptPortraits()
    {
        disabledForTargetSelection.Clear();

        Selectable[] allSelectables = FindObjectsByType<Selectable>(FindObjectsSortMode.None);
        foreach (Selectable selectable in allSelectables)
        {
            if (selectable == null || !selectable.interactable)
                continue;

            bool keepEnabled = false;

            if (selectable is Button button)
                keepEnabled = portraitButtons.Contains(button);

            if (keepEnabled)
                continue;

            selectable.interactable = false;
            disabledForTargetSelection.Add(selectable);
        }

        foreach (Button portraitButton in portraitButtons)
        {
            if (portraitButton != null)
                portraitButton.interactable = true;
        }
    }

    private void ReactivateUI()
    {
        for (int i = 0; i < disabledForTargetSelection.Count; i++)
        {
            if (disabledForTargetSelection[i] != null)
                disabledForTargetSelection[i].interactable = true;
        }

        disabledForTargetSelection.Clear();
    }

    private void ResetCurrentActionSelection()
    {
        if (currentActionCardIndex >= 0 && currentActionCardIndex < cardUIs.Count)
        {
            pendingActions[currentActionCardIndex] = null;
            pendingTargets[currentActionCardIndex] = null;
            cardUIs[currentActionCardIndex].ResetActionDropdown();
        }

        currentActionCardIndex = -1;
    }

    private void CancelCandidateSelection()
    {
        isInCandidateSelection = false;
        onCandidateSelected = null;
        ReactivateUI();
        ResetCurrentActionSelection();

        Debug.Log("[CandidateCardsController] Выбор цели отменён.");
    }

    private void OnCandidatePortraitClicked(Candidate candidate)
    {
        if (!isInCandidateSelection)
            return;

        isInCandidateSelection = false;
        var callback = onCandidateSelected;
        onCandidateSelected = null;

        ReactivateUI();
        currentActionCardIndex = -1;

        callback?.Invoke(candidate);

        Debug.Log($"[CandidateCardsController] Выбрана цель: {candidate?.Name}");
    }
}