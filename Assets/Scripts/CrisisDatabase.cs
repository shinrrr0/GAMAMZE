using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// База данных кризисов. Содержит все доступные кризисы с описаниями и цветами.
/// </summary>
public static class CrisisDatabase
{
    private static List<Crisis> allCrises;
    private static Color crisisColor = new Color(0.8f, 0.2f, 0.8f, 1f); // Фиолетовый

    /// <summary>
    /// Инициализирует базу данных кризисов
    /// </summary>
    public static void Initialize()
    {
        if (allCrises != null)
            return;

        allCrises = new List<Crisis>
        {
            new Crisis("Экономический кризис", "Обвал валюты и финансовой системы. Теряется 30% доходов в этом ходу.", crisisColor),
            new Crisis("Война", "Вооружённый конфликт с соседней страной. Теряется 20 HP в ходу.", crisisColor),
            new Crisis("Бунт", "Восстание народа против власти. Теряется 15 влияния.", crisisColor),
            new Crisis("Эпидемия", "Распространение болезни по стране. Теряется 25 HP в ходу.", crisisColor),
            new Crisis("Землетрясение", "Стихийное бедствие разрушает инфраструктуру. Теряется 20 HP и 10 доходов.", crisisColor),
            new Crisis("Голод", "Неурожай приводит к нехватке продовольствия. Теряется 15 HP в ходу.", crisisColor),
            new Crisis("Мятеж", "Заговор против президента. Можема потерять до 30 HP если не подавить.", crisisColor),
            new Crisis("Санкции", "Международные санкции от других стран. Доходы снижаются на 20% на 3 хода.", crisisColor),
            new Crisis("Плохой урожай", "Сельскохозяйственный кризис. Теряется 12 доходов за ход.", crisisColor),
            new Crisis("Восстание", "Крупное восстание в провинции. Теряется 20 HP в ходу.", crisisColor),
            new Crisis("Паника", "Население охвачено паникой и страхом. Теряется 15 HP в ходу.", crisisColor),
            new Crisis("Дефолт", "Немогу погасить государственный долг. Теряется 40 HP сразу!", crisisColor)
        };

        Debug.Log($"[CrisisDatabase] Инициализирована база данных с {allCrises.Count} кризисами");
    }

    /// <summary>
    /// Возвращает все кризисы
    /// </summary>
    public static List<Crisis> GetAllCrises()
    {
        if (allCrises == null)
            Initialize();
        return allCrises;
    }

    /// <summary>
    /// Возвращает случайный кризис
    /// </summary>
    public static Crisis GetRandomCrisis()
    {
        if (allCrises == null)
            Initialize();
        return allCrises[Random.Range(0, allCrises.Count)];
    }

    /// <summary>
    /// Возвращает N случайных кризисов без повторений
    /// </summary>
    public static List<Crisis> GetRandomCrises(int count)
    {
        if (allCrises == null)
            Initialize();

        List<Crisis> result = new List<Crisis>();
        List<int> usedIndices = new List<int>();

        count = Mathf.Min(count, allCrises.Count);

        for (int i = 0; i < count; i++)
        {
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, allCrises.Count);
            } while (usedIndices.Contains(randomIndex));

            usedIndices.Add(randomIndex);
            result.Add(allCrises[randomIndex]);
        }

        return result;
    }
}
