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

    // Все доступные действия (нужны для восстановления дропдаунов при RefreshAllCards)
    private List<GameAction> allActions = new List<GameAction>();

    // Сохранённые действия — по одному на каждого кандидата
    private GameAction[] pendingActions;
    private Candidate[] pendingTargets;

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

        // Подписываемся на колбэк подтверждения действия
        if (actionTooltip != null)
            actionTooltip.OnActionConfirmed = (action, actor, target) =>
            {
                int idx = candidates.IndexOf(actor);
                if (idx >= 0)
                {
                    pendingActions[idx] = action;
                    pendingTargets[idx] = target;
                    Debug.Log($"[CandidateCardsController] Сохранено действие '{action.name}' для {actor.Name}");
                }
            };

        // Создаём HashSet для отслеживания использованных классов (для уникальности)
        HashSet<string> usedClasses = new HashSet<string>();

        for (int i = 0; i < cardUIs.Count; i++)
        {
            Candidate candidate = new Candidate(usedClasses);
            candidates.Add(candidate);

            // Добавляем класс этого персонажа в HashSet для следующих персонажей
            if (candidate.Abilities.Count > 0)
            {
                usedClasses.Add(candidate.Abilities[0].name);
            }

            ActionOption[] playerActions = new ActionOption[allActions.Count];
            for (int j = 0; j < allActions.Count; j++)
            {
                playerActions[j] = new ActionOption
                {
                    title = allActions[j].name,
                    description = allActions[j].description
                };
            }

            List<ActionOption> aiActionsForCard = new List<ActionOption>();
            List<int> used = new List<int>();
            for (int k = 0; k < 3; k++)
            {
                int idx;
                do { idx = Random.Range(0, allActions.Count); } while (used.Contains(idx) && used.Count < allActions.Count);
                used.Add(idx);
                aiActionsForCard.Add(new ActionOption
                {
                    title = allActions[idx].name,
                    description = allActions[idx].description
                });
            }

            CharacterData data = new CharacterData
            {
                characterName = candidate.Name,
                skills = new[]
                {
                    $"ВЛН: {candidate.Influence}",
                    $"ИНТ: {candidate.Intellect}",
                    $"ВОЛ: {candidate.Willpower}",
                    $"ФИН: {candidate.Money}"
                },
                abilities = candidate.Abilities.ToArray(),
                abilityCount = candidate.Abilities.Count,
                hp = candidate.Influence,
                insanity = candidate.Intellect,
                age = candidate.Age,
                candidate = candidate,
                playerActions = playerActions,
                aiActions = aiActionsForCard.ToArray()
            };

            cardUIs[i].Apply(data);

            int cardIndex = i;
            cardUIs[i].OnPlayerActionSelected = (actionIndex) =>
            {
                if (actionIndex < 0 || actionIndex >= allActions.Count)
                    return;

                GameAction selectedAction = allActions[actionIndex];
                Candidate actor = candidates[cardIndex];

                Candidate target = null;
                for (int t = 0; t < candidates.Count; t++)
                {
                    if (t != cardIndex)
                    {
                        target = candidates[t];
                        break;
                    }
                }

                if (actionTooltip != null)
                    actionTooltip.Show(selectedAction, actor, target);
                else
                    Debug.LogWarning("[CandidateCardsController] ActionTooltip не привязан!");
            };
        }
    }

    /// <summary>
    /// Выполняет все сохранённые действия и обновляет UI карточек.
    /// Вызывается из President.NextTurn()
    /// </summary>
    public void ExecuteAllActions()
    {
        if (pendingActions == null) return;

        for (int i = 0; i < candidates.Count; i++)
        {
            if (pendingActions[i] != null)
            {
                Debug.Log($"[CandidateCardsController] Выполняю '{pendingActions[i].name}' для {candidates[i].Name}");
                pendingActions[i].Execute(candidates[i], pendingTargets[i]);
                pendingActions[i] = null;
                pendingTargets[i] = null;
            }
        }

        // Обновляем UI всех карточек
        RefreshAllCards();
    }

    private void RefreshAllCards()
    {
        ActionOption[] playerActionOptions = null;
        if (allActions != null && allActions.Count > 0)
        {
            playerActionOptions = new ActionOption[allActions.Count];
            for (int j = 0; j < allActions.Count; j++)
                playerActionOptions[j] = new ActionOption
                {
                    title = allActions[j].name,
                    description = allActions[j].description
                };
        }

        for (int i = 0; i < cardUIs.Count && i < candidates.Count; i++)
        {
            Candidate c = candidates[i];

            // Каждый ход — новое случайное действие для AI
            ActionOption[] aiAction = null;
            if (allActions != null && allActions.Count > 0)
            {
                int randomIdx = Random.Range(0, allActions.Count);
                aiAction = new ActionOption[]
                {
                    new ActionOption
                    {
                        title = allActions[randomIdx].name,
                        description = allActions[randomIdx].description
                    }
                };
            }

            CharacterData data = new CharacterData
            {
                characterName = c.Name,
                skills = new[]
                {
                    $"Влияние: {c.Influence}",
                    $"Интеллект: {c.Intellect}",
                    $"Воля: {c.Willpower}",
                    $"Деньги: {c.Money}"
                },
                abilities = c.Abilities.ToArray(),
                abilityCount = c.Abilities.Count,
                hp = c.Influence,
                insanity = c.Intellect,
                age = c.Age,
                candidate = c,
                playerActions = playerActionOptions,
                aiActions = aiAction
            };
            cardUIs[i].Apply(data);
        }
    }
}