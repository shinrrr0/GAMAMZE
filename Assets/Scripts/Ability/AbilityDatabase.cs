using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// База данных способностей. Содержит все доступные способности с описаниями и спрайтами.
/// </summary>
public static class AbilityDatabase
{
    private static List<Ability> allAbilities;

    /// <summary>
    /// Инициализирует базу данных способностей
    /// </summary>
    public static void Initialize()
    {
        if (allAbilities != null)
            return;

        allAbilities = new List<Ability>
        {
            new Ability("Переговоры", null, "Улучшает дипломатические отношения с соседними странами. Снижает вероятность конфликта на 15%.", new Color(1f, 0.8f, 0f, 1f)), // Золото
            new Ability("Лидерство", null, "Вдохновляет армию и население. Увеличивает боевой дух на 20%.", new Color(1f, 0.2f, 0.2f, 1f)), // Красный
            new Ability("Стратегия", null, "Разрабатывает долгосрочные планы. Дает +2 к принятию важных решений.", new Color(0.2f, 0.8f, 1f, 1f)), // Голубой
            new Ability("Экономика", null, "Управляет финансами государства. Увеличивает доход на 25% в этом ходу.", new Color(0.2f, 1f, 0.2f, 1f)), // Зелёный
            new Ability("Дипломатия", null, "Налаживает политические связи. Улучшает отношения с союзниками.", new Color(0.8f, 0.2f, 0.8f, 1f)), // Пурпур
            new Ability("Интриги", null, "Организует тайные операции. Раскрывает планы врагов с вероятностью 40%.", new Color(0.4f, 0.4f, 0.4f, 1f)), // Серый
            new Ability("Медицина", null, "Развивает здравоохранение. Восстанавливает 10 HP в конце хода.", new Color(1f, 0.6f, 0.8f, 1f)), // Розовый
            new Ability("Техника", null, "Улучшает технологии. Разблокирует новые технологии для армии.", new Color(0.6f, 0.6f, 0.6f, 1f)), // Светло-серый
            new Ability("Разведка", null, "Собирает информацию. Выявляет скрытые угрозы и кризисы на ход раньше.", new Color(0.2f, 0.2f, 0.8f, 1f)), // Синий
            new Ability("Риторика", null, "Убедительно говорит. Улучшает переговоры и пропаганду на 30%.", new Color(1f, 0.5f, 0.2f, 1f)), // Оранжевый
            new Ability("Благосостояние", null, "Заботится о народе. Снижает недовольство населения на 10%.", new Color(0.8f, 1f, 0.2f, 1f)), // Жёлто-зелёный
            new Ability("Наука", null, "Инвестирует в науку. Ускоряет исследования на 2 хода.", new Color(0.6f, 0.2f, 1f, 1f)), // Фиолетовый

            new Ability("Воровство", null, "Проверка интеллекта. При провале: коррупционер. При успехе: коррупционер +2 финансы. При крит: +2 финансы.", new Color(0.95f, 0.83f, 0.1f, 1f)),
            new Ability("Лобирование", null, "Проверка финансов. При провале: -1 влияние. При успехе: +1 влияние. При крит: +2 влияние + нет кризиса в след ход.", new Color(0.2f, 0.8f, 0.2f, 1f)),
            new Ability("Обращение важное", null, "Проверка воли. Можно потратить деньги на преимущество. При успехе +1 интеллект, крит +1 интеллект +1 финансы.", new Color(0.2f, 0.7f, 1f, 1f)),
            new Ability("Интриги", null, "Проверка интриги. При провале: непопулярный. При успехе: снимает непопулярный или коррупционер у цели. При крит: ничего.", new Color(0.8f, 0.3f, 0.6f, 1f)),
            new Ability("Дебаты", null, "Проверка интеллект vs интеллект другого кандидата. При провале/успехе -1 за проигравшего +1 победителю; крит +2 победителю.", new Color(0.5f, 0.3f, 0.9f, 1f))
        };

        // Логируем инициализацию
        Debug.Log($"[AbilityDatabase] Инициализирована база данных с {allAbilities.Count} способностями");
    }

    /// <summary>
    /// Возвращает все способности
    /// </summary>
    public static List<Ability> GetAllAbilities()
    {
        if (allAbilities == null)
            Initialize();
        return allAbilities;
    }

    /// <summary>
    /// Возвращает случайную способность
    /// </summary>
    public static Ability GetRandomAbility()
    {
        if (allAbilities == null)
            Initialize();
        return allAbilities[Random.Range(0, allAbilities.Count)];
    }

    /// <summary>
    /// Возвращает N случайных способностей без повторений
    /// </summary>
    public static List<Ability> GetRandomAbilities(int count)
    {
        if (allAbilities == null)
            Initialize();

        List<Ability> result = new List<Ability>();
        List<int> usedIndices = new List<int>();

        count = Mathf.Min(count, allAbilities.Count);

        for (int i = 0; i < count; i++)
        {
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, allAbilities.Count);
            } while (usedIndices.Contains(randomIndex));

            usedIndices.Add(randomIndex);
            result.Add(allAbilities[randomIndex]);
        }

        return result;
    }
}
