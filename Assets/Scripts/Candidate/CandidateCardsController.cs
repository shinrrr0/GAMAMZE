using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandidateCardsController : MonoBehaviour
{
    [Header("Optional: Автогенерация спрайтов")]
    [SerializeField] private CharacterGenerator2D generator;

    [Header("Options")]
    [SerializeField] private bool generateOnStart = true;

    [Header("Tooltip для действий")]
    [SerializeField] private ActionTooltip actionTooltip;

    private List<Candidate> candidates = new List<Candidate>();
    private List<CharacterCardUI> cardUIs = new List<CharacterCardUI>();
    private List<GameAction> allActions = new List<GameAction>();

    private GameAction[] pendingActions;
    private Candidate[] pendingTargets;

    private GameAction[] plannedCandidateActions;
    private Candidate[] plannedCandidateTargets;

    private void Start()
    {
        ActionDatabase.Initialize();

        if (generateOnStart)
            StartCoroutine(GenerateCandidatesNextFrame());
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

        allActions = ActionDatabase.GetAll();

        pendingActions = new GameAction[cardUIs.Count];
        pendingTargets = new Candidate[cardUIs.Count];

        plannedCandidateActions = new GameAction[cardUIs.Count];
        plannedCandidateTargets = new Candidate[cardUIs.Count];

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

        RebuildPlannedCandidateActions();
        ApplyAllCards();
    }

    private void RebuildPlannedCandidateActions()
    {
        if (candidates == null)
            return;

        for (int i = 0; i < candidates.Count; i++)
        {
            List<GameAction> available = BuildActionListForCandidate(candidates[i]);
            if (available.Count == 0)
                continue;

            int randomIdx = Random.Range(0, available.Count);
            plannedCandidateActions[i] = available[randomIdx];
            plannedCandidateTargets[i] = GetDefaultTargetFor(i);
        }
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

        if (allActions != null)
            actions.AddRange(allActions);

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

    private ActionOption[] BuildCandidateActionPreview(int index)
    {
        if (plannedCandidateActions == null || index < 0 || index >= plannedCandidateActions.Length)
            return new ActionOption[0];

        GameAction planned = plannedCandidateActions[index];
        if (planned == null)
            return new ActionOption[0];

        return new[]
        {
            new ActionOption
            {
                title = planned.name,
                description = planned.description
            }
        };
    }

    private void ApplyAllCards()
    {
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
                aiActions = BuildCandidateActionPreview(i)
            };

            cardUIs[i].Apply(data);

            int cardIndex = i;
            cardUIs[i].OnPlayerActionSelected = (actionIndex) =>
            {
                List<GameAction> candidateActions = BuildActionListForCandidate(candidates[cardIndex]);
                if (actionIndex < 0 || actionIndex >= candidateActions.Count)
                    return;

                GameAction selectedAction = candidateActions[actionIndex];
                Candidate actor = candidates[cardIndex];
                Candidate target = GetDefaultTargetFor(cardIndex);

                if (actionTooltip != null)
                    actionTooltip.Show(selectedAction, actor, target);
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
            ActionExecutionResult aiResult = new ActionExecutionResult();

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

            if (plannedCandidateActions != null && plannedCandidateActions[i] != null)
                aiResult = ExecuteActionByName(plannedCandidateActions[i].name, candidate, plannedCandidateTargets[i]);

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
                aiActionResult = aiResult
            };

            if (change.HasChanges || !string.IsNullOrEmpty(playerResult.actionName) || !string.IsNullOrEmpty(aiResult.actionName))
                changes.Add(change);
        }

        RebuildPlannedCandidateActions();
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
            "Лобирование" => CandidateActions.Lobby(actor),
            "Обращение важное" => CandidateActions.MajorAppeal(actor, 0),
            "Интриги" => CandidateActions.Intrigue(actor, target),
            "Дебаты" => CandidateActions.Debate(actor, target),
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
}
