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
            new GameAction("Воровство",        "Проверка интеллекта. При провале: коррупционер. При успехе: коррупционер +2 финансы. При крит: +2 финансы.",                                                                   (actor, target) => CandidateActions.Steal(actor)),
            new GameAction("Лобирование",      "Проверка финансов. При провале: -1 влияние. При успехе: +1 влияние. При крит: +2 влияние + нет кризиса в след ход.",                                                          (actor, target) => CandidateActions.Lobby(actor)),
            new GameAction("Обращение важное", "Проверка воли. При успехе +1 интеллект, крит +1 интеллект +1 финансы.",                                                                                                        (actor, target) => CandidateActions.MajorAppeal(actor, 0)),
            new GameAction("Интриги",          "Проверка интеллекта vs цели. При провале: непопулярный. При успехе: снимает статус у цели.",                                                                                   (actor, target) => CandidateActions.Intrigue(actor, target)),
            new GameAction("Дебаты",           "Проверка интеллект vs интеллект цели. Победитель +1 влияние, проигравший -1. При крит победителю +2.",                                                                        (actor, target) => CandidateActions.Debate(actor, target)),
        };
    }

    public static List<GameAction> GetAll()
    {
        if (allActions == null)
            Initialize();
        return allActions;
    }
}
