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
            new GameAction("Воровство",        "Тихая экспроприация в пользу кампании. Механика: ИНТ; провал — статус «Коррупционер», успех — «Коррупционер» и +2 ФИН, крит — +2 ФИН без статуса.", (actor, target) => CandidateActions.Steal(actor)),
            new GameAction("Лоббирование",     "Деньги в конверте видеть значительно приятнее, чем письма. Механика: ФИН; провал — -1 ВЛН, успех — +1 ВЛН и без кризиса в следующий ход, крит — +2 ВЛН и без кризиса в следующий ход.", (actor, target) => CandidateActions.Lobby(actor)),
            new GameAction("Образование",      "Попытка отсидеть несколько лет ради корочки. Механика: ВОЛ; успех — +1 ИНТ, крит — +1 ИНТ и +1 ФИН.", (actor, target) => CandidateActions.MajorAppeal(actor, 0)),
            new GameAction("Интриги",          "Аппаратная возня, слухи и точечные вбросы. Механика: ИНТ против ИНТ цели; провал — «Непопулярный», успех — снимает отрицательный статус у цели.", (actor, target) => CandidateActions.Intrigue(actor, target)),
            new GameAction("Дебаты",           "То, ради чего люди смотрят телевизор: публичный срач двух политиков. Механика: ИНТ против ИНТ цели; победитель получает ВЛН, проигравший теряет.", (actor, target) => CandidateActions.Debate(actor, target)),
            new GameAction("Терпеть",          "Сжать зубы, не нарываться и досидеть срок с максимально каменным лицом. Механика: без доп. эффекта.", (actor, target) => CandidateActions.Endure(actor)),
            new GameAction("Жить по закону",   "Образцовый арестант: выполнять все приказы начальства и игнорировать понятия. Механика: ИНТ; провал — -1 ВОЛ и -1 ВЛН, крит — +1 ВЛН.", (actor, target) => CandidateActions.LiveByLaw(actor)),
            new GameAction("Жить по понятиям", "Уйти в отрицалово и жить по понятиям воровским. Может закончится плохо. Механика: ВОЛ; провал — -1 ко всем статам, успех — +1 ВОЛ, крит — +1 ВОЛ и +1 ИНТ.", (actor, target) => CandidateActions.LiveByCode(actor)),
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
                    "что бы... такого... написать.... чтобы всех... покорежило... Механика: ИНТ; провал — -1 ВЛН и -1 ВОЛ, успех — +2 ВЛН и +2 безумия президенту, крит — +2 ВЛН, +1 к остальным статам и +1 безумия президенту.",
                    (actor, target) => CandidateActions.PhilosopherPost(actor));

            case "Антикоррупционер":
                return new GameAction(
                    "Сделать разоблачение",
                    "Большое антикоррупционное шоу на популярнои видеохостинге. Механика: ВОЛ по коррупционерам; провал — -1 ФИН и -1 ВЛН, успех — награда за каждого коррупционера, крит — двойная награда и посадки.",
                    (actor, target) => CandidateActions.AntiCorruptionExpose(actor));

            case "Боевой повар":
                return new GameAction(
                    "Поднять мятеж",
                    "Попытка решить кризисы в стране простыми методами. Механика: ВОЛ + число кризисов; успех/крит дают ВЛН и снижают безумие президента, затем дуэль против безумия президента.",
                    (actor, target) => CandidateActions.RaiseMutiny(actor));

            case "Пожилой стример":
                return new GameAction(
                    "Провести полит стрим",
                    "Совместный треш-стрим. В наше время это добавляет политических очков. Механика: ИНТ, +2 если в этом ходу был кризис; исход влияет на ВЛН/ФИН обеих сторон.",
                    (actor, target) => CandidateActions.PoliticalStream(actor, target));

            case "Национал-Либерал":
                return new GameAction(
                    "Сделать предсказание",
                    "Смелое политическое пророчество. Поначалу люди всегда смеются. Механика: на следующем ходу либо +1 ко всем статам, если кризис будет, либо -1 ко всем, если нет.",
                    (actor, target) => CandidateActions.MakePrediction(actor));

            case "Профессиональный доносчик":
                return new GameAction(
                    "Написать донос",
                    "Пост в социальных сетях с требованием обратить внимание.. Механика: ВЛН против ВЛН цели; крит может отправить цель в тюрьму.",
                    (actor, target) => CandidateActions.FileReport(actor, target));

            case "Моральный богач":
                return new GameAction(
                    "Фиксируем прибыль",
                    "Избавиться от вложений, чтобы выйти в плюс. Механика: ИНТ; провал — потеря инвестиций, успех — +n ФИН, крит — +n² ФИН.",
                    (actor, target) => CandidateActions.CashOutInvestments(actor));

            case "еврейский фанат суфлера":
                return new GameAction(
                    "Зачитать по бумажке",
                    "Выступление строго по заготовке, без права на импровизацию. Механика: ИНТ; провал — -1 ВЛН и «Непопулярный», успех — +2 ВЛН, крит — +2 ВЛН и +1 ФИН.",
                    (actor, target) => CandidateActions.ReadFromScript(actor));

            default:
                return null;
        }
    }
}
