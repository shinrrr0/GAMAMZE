using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// База данных классов персонажей.
/// Каждый персонаж начинает с одним из этих классов.
/// Класс - это основная профессия/роль персонажа, которая определяет его направление развития.
/// </summary>
public static class ClassDatabase
{
    private static List<Ability> allClasses;

    /// <summary>
    /// Инициализирует базу данных классов
    /// </summary>
    public static void Initialize()
    {
        if (allClasses != null)
            return;

        allClasses = new List<Ability>
        {
            new Ability(
                "суверенный... философ..",
                null,
                "носитель оригинальной... философской... мысли... его посты в... социальных сетях... опасны... для мозга....",
                new Color(0.8f, 0.6f, 1f, 1f) // Фиолетовый
            ),
            new Ability(
                "Антикоррупционер",
                null,
                "Не любит ворующих чиновников. Может разоблочать коррупционеров. Так просто его не съесть.",
                new Color(0.2f, 1f, 0.2f, 1f) // Зелёный
            ),
            new Ability(
                "Боевой повар",
                null,
                "Сторонник жестких решений. Борец за простой народ. Его действия тем более эффективны, чем больше в стране кризисов.",
                new Color(1f, 0.2f, 0.2f, 1f) // Красный
            ),
            new Ability(
                "Пожилой стример",
                null,
                "Когда-то он задавал мейнстрим в интернете. Теперь это просто объект насмешек. Сам не знает что он делает в политике.",
                new Color(0.8f, 0.2f, 0.8f, 1f) // Пурпур
            ),
            new Ability(
                "Национал-Либерал",
                null,
                "Любит страну даже больше президента. Его прогнозы звучат абсолютно шизофренично, но иногда сбываются.",
                new Color(0.6f, 0.8f, 1f, 1f) // Голубой
            ),
            new Ability(
                "Профессиональный доносчик",
                null,
                "Всегда готов помочь стране. Не терпит посягательств на безопасность государства. Может написать донос на другого кандидата.",
                new Color(1f, 0.8f, 0f, 1f) // Золото
            ),
            new Ability(
                "еврейский фанат суфлера",
                null,
                "Все, что он говорит, - это слова его суфлера. Любой неудобный вопрос без заготовки, может поставить в ступор.",
                new Color(0.4f, 0.4f, 0.4f, 1f) // Серый
            ),
            new Ability(
                "Моральный богач",
                null,
                "Ебучий красавчик и прав во всем. Точно знает как торговать на бирже.",
                new Color(1f, 0.6f, 0.8f, 1f) // Розовый
            )
        };

        Debug.Log($"[ClassDatabase] Инициализирована база данных с {allClasses.Count} классами");
    }

    /// <summary>
    /// Возвращает все доступные классы
    /// </summary>
    public static List<Ability> GetAllClasses()
    {
        if (allClasses == null)
            Initialize();
        return allClasses;
    }

    /// <summary>
    /// Возвращает случайный класс
    /// </summary>
    public static Ability GetRandomClass()
    {
        if (allClasses == null)
            Initialize();
        return allClasses[Random.Range(0, allClasses.Count)];
    }

    /// <summary>
    /// Возвращает случайный класс, исключая уже использованные
    /// </summary>
    public static Ability GetRandomClassExcluding(HashSet<string> usedClasses)
    {
        if (allClasses == null)
            Initialize();

        List<Ability> availableClasses = new List<Ability>();

        foreach (var classAbility in allClasses)
        {
            if (usedClasses == null || !usedClasses.Contains(classAbility.name))
            {
                availableClasses.Add(classAbility);
            }
        }

        if (availableClasses.Count == 0)
        {
            Debug.LogWarning("[ClassDatabase] Нет доступных классов! Все классы已 использованы.");
            return allClasses[Random.Range(0, allClasses.Count)]; // Вернём случайный, если нет доступных
        }

        return availableClasses[Random.Range(0, availableClasses.Count)];
    }

    /// <summary>
    /// Возвращает класс по имени
    /// </summary>
    public static Ability GetClassByName(string className)
    {
        if (allClasses == null)
            Initialize();

        foreach (var classAbility in allClasses)
        {
            if (classAbility.name.Equals(className, System.StringComparison.OrdinalIgnoreCase))
            {
                return classAbility;
            }
        }

        Debug.LogWarning($"[ClassDatabase] Класс '{className}' не найден!");
        return null;
    }

    /// <summary>
    /// Проверяет, является ли способность классом
    /// </summary>
    public static bool IsClass(string abilityName)
    {
        if (allClasses == null)
            Initialize();

        foreach (var classAbility in allClasses)
        {
            if (classAbility.name.Equals(abilityName, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
