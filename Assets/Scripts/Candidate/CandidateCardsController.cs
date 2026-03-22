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

    private void Start()
    {
        ActionDatabase.Initialize();

        Debug.Log("[CandidateCardsController] Start() called!");
        if (generateOnStart)
        {
            Debug.Log("[CandidateCardsController] generateOnStart = true, откладываю на следующий фрейм");
            StartCoroutine(GenerateCandidatesNextFrame());
        }
        else
            Debug.Log("[CandidateCardsController] generateOnStart = false, пропускаю генерацию");
    }

    private IEnumerator GenerateCandidatesNextFrame()
    {
        yield return null;
        GenerateCandidates();
    }

    [ContextMenu("Generate Candidates")]
    public void GenerateCandidates()
    {
        Debug.Log("[CandidateCardsController] GenerateCandidates() called!");

        cardUIs.Clear();
        candidates.Clear();

        Debug.Log($"[CandidateCardsController] Ищу CharacterCardUI в {transform.childCount} дочерних объектах");

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Debug.Log($"[CandidateCardsController] [{i}] {child.name}");

            if (child.name == "President" || child.name == "President Panel")
            {
                Debug.Log($"[CandidateCardsController]     -> Пропускаю President");
                continue;
            }

            CharacterCardUI cardUI = child.GetComponent<CharacterCardUI>();
            Debug.Log($"[CandidateCardsController]     -> CharacterCardUI: {(cardUI != null ? "НАЙДЕН" : "НЕ НАЙДЕН")}");

            if (cardUI != null)
                cardUIs.Add(cardUI);
        }

        if (cardUIs.Count == 0)
        {
            Debug.LogError("[CandidateCardsController] Не найдено CharacterCardUI компонентов!");
            return;
        }

        Debug.Log($"[CandidateCardsController] Найдено {cardUIs.Count} карточек");

        if (generator != null)
            generator.GenerateUICharacters();

        List<GameAction> allActions = ActionDatabase.GetAll();

        for (int i = 0; i < cardUIs.Count; i++)
        {
            Candidate candidate = new Candidate();
            candidates.Add(candidate);

            // Собираем ActionOption для дропдауна из базы действий
            ActionOption[] playerActions = new ActionOption[allActions.Count];
            for (int j = 0; j < allActions.Count; j++)
            {
                playerActions[j] = new ActionOption
                {
                    title = allActions[j].name,
                    description = allActions[j].description
                };
            }

            // AI: 3 случайных действия
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
                playerActions = playerActions,
                aiActions = aiActionsForCard.ToArray()
            };

            Debug.Log($"[CandidateCardsController] Кандидат {i}: {candidate.Name}");
            cardUIs[i].Apply(data);

            // Привязываем обработчик выбора действия к карточке
            int cardIndex = i;
            cardUIs[i].OnPlayerActionSelected = (actionIndex) =>
            {
                if (actionIndex < 0 || actionIndex >= allActions.Count)
                    return;

                GameAction selectedAction = allActions[actionIndex];
                Candidate actor = candidates[cardIndex];

                // Цель — первый другой кандидат (заглушка, логику выбора цели можно расширить)
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
                    Debug.LogWarning("[CandidateCardsController] ActionTooltip не привязан в инспекторе!");
            };
        }
    }
}
