using System.Collections.Generic;

public static class ActionDatabase
{
    private static List<GameAction> allActions;

    public static void Initialize()
    {
        if (allActions != null)
            return;

        allActions = new List<GameAction>
        {
            new GameAction("Воровство",        "Проверка интеллекта. При провале: коррупционер. При успехе: коррупционер +2 финансы. При крит: +2 финансы.", (actor, target) => CandidateActions.Steal(actor)),
            new GameAction("Лобирование",      "Проверка финансов. При провале: -1 влияние. При успехе: +1 влияние. При крит: +2 влияние + нет кризиса в след ход.", (actor, target) => CandidateActions.Lobby(actor)),
            new GameAction("Обращение важное", "Проверка воли. При успехе +1 интеллект, крит +1 интеллект +1 финансы.", (actor, target) => CandidateActions.MajorAppeal(actor, 0)),
            new GameAction("Интриги",          "Проверка интеллекта vs цели. При провале: непопулярный. При успехе: снимает статус у цели.", (actor, target) => CandidateActions.Intrigue(actor, target)),
            new GameAction("Дебаты",           "Проверка интеллект vs интеллект цели. Победитель +1 влияние, проигравший -1. При крит победителю +2.", (actor, target) => CandidateActions.Debate(actor, target)),
            new GameAction("Терпеть",          "Тюремное действие без дополнительных эффектов. Подходит для ИИ, если кандидат сидит в тюрьме.", (actor, target) => CandidateActions.Endure(actor)),
            new GameAction("Жить по закону",   "Тюрьма. Проверка ИНТ. Провал: -1 ВОЛ и -1 ВЛН. Успех: ничего. Крит: +1 ВЛН.", (actor, target) => CandidateActions.LiveByLaw(actor)),
            new GameAction("Жить по понятиям", "Тюрьма. Проверка ВОЛ. Провал: -1 ко всем характеристикам. Успех: +1 ВОЛ. Крит: +1 ВОЛ и +1 ИНТ.", (actor, target) => CandidateActions.LiveByCode(actor)),
        };
    }

    public static List<GameAction> GetAll()
    {
        if (allActions == null)
            Initialize();
        return allActions;
    }

    public static GameAction GetClassAction(Candidate candidate)
    {
        if (candidate == null || string.IsNullOrWhiteSpace(candidate.ClassName))
            return null;

        switch (candidate.ClassName)
        {
            case "суверенный... философ..":
                return new GameAction(
                    "Написать пост в тг",
                    "Проверка ИНТ. Провал: ВЛН -1, ВОЛ -1. Успех: +2 ВЛН, +2 безумия у президента. Крит: +2 ВЛН, +1 к остальным характеристикам, +1 безумия у президента.",
                    (actor, target) => CandidateActions.PhilosopherPost(actor));

            case "Антикоррупционер":
                return new GameAction(
                    "Сделать разоблачение",
                    "Проверка ВОЛ по каждому кандидату с чертой 'Коррупционер'. Провал: -1 ФИН, -1 ВЛН. Успех: +1 ФИН и +1 ВЛН за каждого коррупционера. Крит: +2 ФИН и +2 ВЛН за каждого коррупционера, коррупционеры получают -1 ФИН/-1 ВЛН и садятся в тюрьму.",
                    (actor, target) => CandidateActions.AntiCorruptionExpose(actor));

            case "Боевой повар":
                return new GameAction(
                    "Поднять мятеж",
                    "Проверка ВОЛ + число кризисов. Провал: -1 ВЛН, +2 безумия у президента. Успех: +1 ВЛН за каждый кризис, -1 безумия у президента. Крит: +1 ВЛН за каждый кризис, -2 безумия у президента. Затем ВЛН против БЕЗ президента: при провале кандидат выбывает.",
                    (actor, target) => CandidateActions.RaiseMutiny(actor));

            case "Пожилой стример":
                return new GameAction(
                    "Провести полит стрим",
                    "Проверка ИНТ (+2, если в этом ходу был кризис). Провал: у цели -1 ВЛН, у стримера -1 ВЛН и -1 ФИН. Успех: +1 ВЛН и +1 ФИН цели и стримеру. Крит: +1 ко всем характеристикам обеим сторонам.",
                    (actor, target) => CandidateActions.PoliticalStream(actor, target));

            case "Национал-Либерал":
                return new GameAction(
                    "Сделать предсказание",
                    "Если на следующем ходу будет кризис: +1 ко всем характеристикам. Если кризиса не будет: -1 ко всем характеристикам.",
                    (actor, target) => CandidateActions.MakePrediction(actor));

            case "Профессиональный доносчик":
                return new GameAction(
                    "Написать донос",
                    "Проверка ВЛН против другого кандидата. Провал: -1 ВЛН. Успех: +1 ВЛН себе, -1 ВЛН цели. Крит: +1 ВЛН себе, -1 ВЛН цели, цель в тюрьме.",
                    (actor, target) => CandidateActions.FileReport(actor, target));

            case "Моральный богач":
                return new GameAction(
                    "Фиксируем прибыль",
                    "Есть стартовая инвестиция. Каждый ход инвестиции могут сгореть с шансом кризисы*10%. Проверка ИНТ. Провал: теряет все инвестиции. Успех: +n ФИН. Крит: +n² ФИН.",
                    (actor, target) => CandidateActions.CashOutInvestments(actor));

            default:
                return null;
        }
    }
}
