using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// База данных кризисов. Содержит все доступные кризисы с описаниями и цветами.
/// </summary>
public static class CrisisDatabase
{
    private static List<Crisis> allCrises;
    private static readonly Color crisisColor = new Color(0.8f, 0.2f, 0.8f, 1f); // Фиолетовый

    /// <summary>
    /// Инициализирует базу данных кризисов
    /// </summary>
    public static void Initialize()
    {
        if (allCrises != null)
            return;

        allCrises = new List<Crisis>
        {
            new Crisis("Экономический кризис", "Тип: экономический.\nСюжет: Казначейство внезапно вспомнило, что бумага терпит всё, кроме пустого бюджета.\nМеханика: 30% доходов теряется в этом ходу.", crisisColor),
            new Crisis("Война", "Тип: социальный.\nСюжет: На границе заговорили громче обычного, и вся страна сразу стала жить в режиме тревожной сводки.\nМеханика: -20 HP в этом ходу.", crisisColor),
            new Crisis("Бунт", "Тип: социальный.\nСюжет: На улицах снова решили, что кричать проще, чем ждать.\nМеханика: -15 влияния.", crisisColor),
            new Crisis("Эпидемия", "Тип: социальный.\nСюжет: Министерство здравоохранения просит не паниковать, поэтому все уже слегка паникуют.\nМеханика: -25 HP в этом ходу.", crisisColor),
            new Crisis("Землетрясение", "Тип: экономический.\nСюжет: Стихия напомнила, что инфраструктура — это не абстракция из презентации.\nМеханика: -20 HP и -10 доходов.", crisisColor),
            new Crisis("Голод", "Тип: социальный.\nСюжет: Пустые склады и длинные очереди быстро превращают недовольство в национальный спорт.\nМеханика: -15 HP в этом ходу.", crisisColor),
            new Crisis("Мятеж", "Тип: социальный.\nСюжет: Часть элит решила, что вертикаль власти — это рекомендация, а не конструкция.\nМеханика: до 30 HP потерь, если не подавить.", crisisColor),
            new Crisis("Санкции", "Тип: экономический.\nСюжет: За рубежом снова нашли повод объяснить чужие проблемы вашими решениями.\nМеханика: доходы -20% на 3 хода.", crisisColor),
            new Crisis("Плохой урожай", "Тип: экономический.\nСюжет: Поля подвели, а чиновники срочно начали искать виноватых среди погоды и статистики.\nМеханика: -12 доходов за ход.", crisisColor),
            new Crisis("Восстание", "Тип: социальный.\nСюжет: Провинция решила напомнить столице о своём существовании максимально шумным способом.\nМеханика: -20 HP в этом ходу.", crisisColor),
            new Crisis("Паника", "Тип: социальный.\nСюжет: Слухи бегут быстрее официальных заявлений, а официальные заявления обычно даже помогают слухам.\nМеханика: -15 HP в этом ходу.", crisisColor),
            new Crisis("Дефолт", "Тип: экономический.\nСюжет: Государство торжественно обнаружило, что обещаний было больше, чем возможностей.\nМеханика: -40 HP сразу.", crisisColor)
        };

        Debug.Log($"[CrisisDatabase] Инициализирована база данных с {allCrises.Count} кризисами");
    }

    public static List<Crisis> GetAllCrises()
    {
        if (allCrises == null)
            Initialize();
        return allCrises;
    }

    public static Crisis GetRandomCrisis()
    {
        if (allCrises == null)
            Initialize();
        return CreateRuntimeCopy(allCrises[Random.Range(0, allCrises.Count)]);
    }

    public static Crisis GetRandomCrisisExcluding(IEnumerable<string> excludedNames)
    {
        if (allCrises == null)
            Initialize();

        HashSet<string> excluded = excludedNames != null
            ? new HashSet<string>(excludedNames.Where(n => !string.IsNullOrWhiteSpace(n)), System.StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

        List<Crisis> available = allCrises
            .Where(c => c != null && !excluded.Contains(c.name))
            .ToList();

        if (available.Count == 0)
            return null;

        return CreateRuntimeCopy(available[Random.Range(0, available.Count)]);
    }

    public static Crisis CreateRuntimeCopyByName(string crisisName)
    {
        if (allCrises == null)
            Initialize();

        if (string.IsNullOrWhiteSpace(crisisName))
            return null;

        Crisis source = allCrises.FirstOrDefault(c => c != null && c.name.Equals(crisisName, System.StringComparison.OrdinalIgnoreCase));
        return CreateRuntimeCopy(source);
    }

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
            result.Add(CreateRuntimeCopy(allCrises[randomIndex]));
        }

        return result;
    }

    private static Crisis CreateRuntimeCopy(Crisis source)
    {
        if (source == null)
            return null;

        Crisis copy = new Crisis(source.name, source.description, source.color)
        {
            icon = source.icon
        };

        return copy;
    }
}
