using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CandidateCardsController : MonoBehaviour
{
    [Header("Optional: Автогенерация спрайтов")]
    [SerializeField] private CharacterGenerator2D generator;

    [Header("Options")]
    [SerializeField] private bool generateOnStart = true;

    private void Start()
    {
        Debug.Log("[CandidateCardsController] Start() called!");
        if (generateOnStart)
        {
            Debug.Log("[CandidateCardsController] generateOnStart = true, отлаживаю GenerateCandidates() на первый фрейм");
            StartCoroutine(GenerateCandidatesNextFrame());
        }
        else
            Debug.Log("[CandidateCardsController] generateOnStart = false, пропускаю генерацию");
    }

    private IEnumerator GenerateCandidatesNextFrame()
    {
        yield return null; // Ждём следующего фрейма
        GenerateCandidates();
    }

    [ContextMenu("Generate Candidates")]
    public void GenerateCandidates()
    {
        Debug.Log("[CandidateCardsController] GenerateCandidates() called!");
        
        // Автоматически находим все CharacterCardUI среди дочерних объектов
        List<CharacterCardUI> cardUIs = new List<CharacterCardUI>();
        
        Debug.Log($"[CandidateCardsController] Ищу CharacterCardUI в {transform.childCount} дочерних объектах");
        
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Debug.Log($"[CandidateCardsController] [{i}] {child.name}");
            
            // Пропускаем President
            if (child.name == "President")
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

        // Генерируем спрайты перед заполнением данных
        if (generator != null)
            generator.GenerateUICharacters();

        // Заполняем каждую карточку новым кандидатом
        for (int i = 0; i < cardUIs.Count; i++)
        {
            Candidate candidate = new Candidate();

            // Собираем названия способностей для отображения
            string[] abilityNames = new string[candidate.Abilities.Count];
            for (int j = 0; j < candidate.Abilities.Count; j++)
            {
                abilityNames[j] = candidate.Abilities[j].name;
            }

            // Подготавливаем доступные действия для игрока — из текущих способностей кандидата
            List<ActionOption> playerActionsForCard = new List<ActionOption>();
            foreach (var ability in candidate.Abilities)
            {
                if (ability == null || string.IsNullOrEmpty(ability.name))
                    continue;

                playerActionsForCard.Add(new ActionOption
                {
                    title = ability.name,
                    description = ability.description
                });
            }

            // Если способностей нет, добавим базовые для теста
            if (playerActionsForCard.Count == 0)
            {
                playerActionsForCard.Add(new ActionOption { title = "Воровство", description = "Проверка интеллекта." });
                playerActionsForCard.Add(new ActionOption { title = "Лобирование", description = "Проверка финансов." });
                playerActionsForCard.Add(new ActionOption { title = "Обращение важное", description = "Проверка воли." });
            }

            // AI-боевой выбор: максимум 3 рандомных действия из списка ключевых
            string[] coreActions = { "Воровство", "Лобирование", "Обращение важное", "Интриги", "Дебаты" };
            List<ActionOption> aiActionsForCard = new List<ActionOption>();
            List<int> used = new List<int>();
            for (int k = 0; k < 3; k++)
            {
                int idx;
                do { idx = Random.Range(0, coreActions.Length); } while (used.Contains(idx) && used.Count < coreActions.Length);
                used.Add(idx);

                aiActionsForCard.Add(new ActionOption
                {
                    title = coreActions[idx],
                    description = $"AI: {coreActions[idx]}"
                });
            }

            CharacterData data = new CharacterData
            {
                characterName = candidate.Name,
                // Заполняем 4 поля навыков значениями характеристик
                skills = new[]
                {
                    $"Влияние: {candidate.Influence}",
                    $"Интеллект: {candidate.Intellect}",
                    $"Воля: {candidate.Willpower}",
                    $"Деньги: {candidate.Money}"
                },
                // Передаем способности в CharacterData
                abilities = candidate.Abilities.ToArray(),
                abilityCount = candidate.Abilities.Count, // Количество способностей для отображения кружков
                hp = candidate.Influence,
                insanity = candidate.Intellect,
                age = candidate.Age,
                candidate = candidate, // Передаем ссылку на кандидата для вызова действий
                playerActions = playerActionsForCard.ToArray(),
                aiActions = aiActionsForCard.ToArray()
            };

            Debug.Log($"[CandidateCardsController] Кандидат {i}: {candidate.Name}");
            Debug.Log($"[CandidateCardsController] Способности: {string.Join(", ", abilityNames)}");
            cardUIs[i].Apply(data);
        }
    }
}
