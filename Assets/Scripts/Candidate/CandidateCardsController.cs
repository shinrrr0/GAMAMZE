using System.Collections;
using System.Collections.Generic;
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

    private List<Candidate> candidates = new List<Candidate>();
    private List<CharacterCardUI> cardUIs = new List<CharacterCardUI>();
    private List<GameAction> allActions = new List<GameAction>();

    private GameAction[] pendingActions;
    private Candidate[] pendingTargets;
    
    private int currentActionCardIndex = -1; // Для обработки отмены действия

    private GameAction[] plannedCandidateActions;
    private Candidate[] plannedCandidateTargets;

    // Система выбора кандидата
    private bool isInCandidateSelection = false;
    private System.Action<Candidate> onCandidateSelected;

    private void Start()
    {
        ActionDatabase.Initialize();

        if (generateOnStart)
            StartCoroutine(GenerateCandidatesNextFrame());
    }

    private void Update()
    {
        // Обработка ESC для отмены выбора кандидата
        if (isInCandidateSelection && Input.GetKeyDown(KeyCode.Escape))
        {
            CancelCandidateSelection();
        }
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
            
            // Обработка отмены - возвращаем dropdown текущей карты на 0
            actionTooltip.OnActionCancelled = () =>
            {
                if (currentActionCardIndex >= 0 && currentActionCardIndex < cardUIs.Count)
                {
                    cardUIs[currentActionCardIndex].ResetActionDropdown();
                    currentActionCardIndex = -1;
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

        // Если кандидат в тюрьме - доступны только тюремные действия
        if (candidate.IsInPrison)
        {
            // Добавляем только "Жить по закону" и "Жить по понятиям"
            if (allActions != null)
            {
                foreach (var action in allActions)
                {
                    if (action.name == "Жить по закону" || action.name == "Жить по понятиям")
                        actions.Add(action);
                }
            }
            return actions;
        }

        // Обычный случай - добавляем все базовые действия
        if (allActions != null)
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
            
            // Добавляем обработчик для клика на портрет (для выбора цели действия)
            Image portraitImage = cardUIs[i].GetComponentInChildren<Image>();
            if (portraitImage != null)
            {
                Button portraitButton = portraitImage.GetComponent<Button>();
                if (portraitButton == null)
                    portraitButton = portraitImage.gameObject.AddComponent<Button>();
                
                portraitButton.onClick.RemoveAllListeners();
                portraitButton.onClick.AddListener(() => OnCandidatePortraitClicked(capturedCandidate));
            }
            
            cardUIs[i].OnPlayerActionSelected = (actionIndex) =>
            {
                // actionIndex 0 is "выберите действие" - do nothing
                if (actionIndex == 0)
                    return;

                // Real action index is actionIndex - 1 (because of the default option)
                int realActionIndex = actionIndex - 1;
                List<GameAction> candidateActions = BuildActionListForCandidate(candidates[cardIndex]);
                if (realActionIndex < 0 || realActionIndex >= candidateActions.Count)
                    return;

                GameAction selectedAction = candidateActions[realActionIndex];
                Candidate actor = candidates[cardIndex];
                Candidate target = GetDefaultTargetFor(cardIndex);

                if (actionTooltip != null)
                {
                    currentActionCardIndex = cardIndex; // Сохраняем индекс для обработки отмены
                    actionTooltip.Show(selectedAction, actor, target);
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

    /// <summary>
    /// Начинает процесс выбора кандидата. Все элементы кроме портретов становятся неинтерактивными.
    /// По клику на портрет вызывается callback. По ESC отменяется.
    /// </summary>
    public void StartCandidateSelection(System.Action<Candidate> callback)
    {
        if (isInCandidateSelection)
            return;

        isInCandidateSelection = true;
        onCandidateSelected = callback;

        // Деактивируем все CanvasGroup в иерархии, кроме cardUIs
        DeactivateAllUIExcept();

        Debug.Log("[CandidateCardsController] Начат режим выбора кандидата. Нажмите ESC для отмены.");
    }

    private void DeactivateAllUIExcept()
    {
        // Деактивируем все Buttons в иерархии, кроме portrait buttons
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var btn in allButtons)
        {
            bool isPortraitButton = false;
            
            // Проверяем, является ли этот button портретом одной из карточек
            foreach (var cardUI in cardUIs)
            {
                if (btn.transform.IsChildOf(cardUI.transform))
                {
                    isPortraitButton = true;
                    break;
                }
            }
            
            if (!isPortraitButton)
            {
                btn.interactable = false;
            }
        }

        // Также деактивируем Toggles, InputFields и другие интерактивные элементы
        Selectable[] allSelectables = FindObjectsByType<Selectable>(FindObjectsSortMode.None);
        foreach (var selectable in allSelectables)
        {
            if (selectable is Button)
                continue; // Buttons уже обработаны

            bool isInCardUI = false;
            foreach (var cardUI in cardUIs)
            {
                if (selectable.transform.IsChildOf(cardUI.transform))
                {
                    isInCardUI = true;
                    break;
                }
            }
            
            if (!isInCardUI)
            {
                selectable.interactable = false;
            }
        }
    }

    private void ReactivateUI()
    {
        // Включаем все Buttons обратно
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var btn in allButtons)
        {
            btn.interactable = true;
        }

        // Включаем остальные интерактивные элементы
        Selectable[] allSelectables = FindObjectsByType<Selectable>(FindObjectsSortMode.None);
        foreach (var selectable in allSelectables)
        {
            selectable.interactable = true;
        }
    }

    private void CancelCandidateSelection()
    {
        isInCandidateSelection = false;
        onCandidateSelected = null;
        ReactivateUI();
        Debug.Log("[CandidateCardsController] Выбор кандидата отменен (ESC).");
    }

    private void OnCandidatePortraitClicked(Candidate candidate)
    {
        if (!isInCandidateSelection)
            return;

        isInCandidateSelection = false;
        var callback = onCandidateSelected;
        onCandidateSelected = null;
        ReactivateUI();

        if (callback != null)
            callback.Invoke(candidate);

        Debug.Log($"[CandidateCardsController] Выбран кандидат: {candidate.Name}");
    }
}
