using UnityEngine;

public enum CheckOutcome
{
    Fail,
    Success,
    CriticalSuccess
}

public static class CandidateActions
{
    private const string CorruptAbilityName = "Коррупционер";
    private const string UnpopularAbilityName = "Непопулярный";

    private static CheckOutcome ResolveCheck(int stat, bool advantage, out int selectedRoll, out int totalValue)
    {
        int roll1 = Random.Range(1, 11);
        int roll2 = advantage ? Random.Range(1, 11) : 0;
        selectedRoll = advantage ? Mathf.Max(roll1, roll2) : roll1;
        totalValue = selectedRoll + stat;

        CheckOutcome outcome;
        if (totalValue >= 15)
            outcome = CheckOutcome.CriticalSuccess;
        else if (totalValue >= 10)
            outcome = CheckOutcome.Success;
        else
            outcome = CheckOutcome.Fail;

        Debug.Log($"[CandidateActions] Проверка стата={stat}, d10=({roll1}{(advantage ? "/" + roll2 : "")}) => {selectedRoll}, итого={totalValue} => {outcome}");
        return outcome;
    }

    public static void Steal(Candidate actor)
    {
        if (actor == null)
            return;

        var outcome = ResolveCheck(actor.Intellect, false, out int roll, out int total);

        switch (outcome)
        {
            case CheckOutcome.Fail:
                actor.AddAbility(CorruptAbilityName);
                Debug.Log($"[CandidateActions:Steal] {actor.Name} провалил воровство (roll={roll}, tot={total}). Получил '{CorruptAbilityName}'.");
                break;
            case CheckOutcome.Success:
                actor.AddAbility(CorruptAbilityName);
                actor.Money += 2;
                Debug.Log($"[CandidateActions:Steal] {actor.Name} успешно украл (roll={roll}, tot={total}). +2 к деньгам, получил '{CorruptAbilityName}'.");
                break;
            case CheckOutcome.CriticalSuccess:
                actor.Money += 2;
                Debug.Log($"[CandidateActions:Steal] {actor.Name} критически успешно украл (roll={roll}, tot={total}). +2 к деньгам.");
                break;
        }
    }

    public static void Lobby(Candidate actor)
    {
        if (actor == null)
            return;

        var outcome = ResolveCheck(actor.Money, false, out int roll, out int total);

        switch (outcome)
        {
            case CheckOutcome.Fail:
                actor.Influence -= 1;
                Debug.Log($"[CandidateActions:Lobby] {actor.Name} провалил лобирование (roll={roll}, tot={total}). -1 влияния.");
                break;
            case CheckOutcome.Success:
                actor.Influence += 1;
                Debug.Log($"[CandidateActions:Lobby] {actor.Name} успешно лобировал (roll={roll}, tot={total}). +1 влияния.");
                break;
            case CheckOutcome.CriticalSuccess:
                actor.Influence += 2;
                actor.NoCrisisNextTurn = true;
                Debug.Log($"[CandidateActions:Lobby] {actor.Name} критически успешно лобировал (roll={roll}, tot={total}). +2 влияния, кризис в следующем ходу отменяется.");
                break;
        }
    }

    public static void MajorAppeal(Candidate actor, int spendMoney = 0)
    {
        if (actor == null)
            return;

        bool advantage = false;
        if (spendMoney > 0)
        {
            int actualSpend = Mathf.Min(spendMoney, actor.Money);
            if (actualSpend > 0)
            {
                actor.Money -= actualSpend;
                advantage = true;
                Debug.Log($"[CandidateActions:MajorAppeal] {actor.Name} тратит {actualSpend} денег для преимущества в проверке.");
            }
            else
            {
                Debug.Log($"[CandidateActions:MajorAppeal] {actor.Name} не удалось потратить деньги (нет средств), проверка без преимущества.");
            }
        }

        var outcome = ResolveCheck(actor.Willpower, advantage, out int roll, out int total);

        switch (outcome)
        {
            case CheckOutcome.Fail:
                Debug.Log($"[CandidateActions:MajorAppeal] {actor.Name} провалил обращение (roll={roll}, tot={total}). Без эффектов.");
                break;
            case CheckOutcome.Success:
                actor.Intellect += 1;
                Debug.Log($"[CandidateActions:MajorAppeal] {actor.Name} успешно (roll={roll}, tot={total}). +1 интеллект.");
                break;
            case CheckOutcome.CriticalSuccess:
                actor.Intellect += 1;
                actor.Money += 1;
                Debug.Log($"[CandidateActions:MajorAppeal] {actor.Name} критически успешно (roll={roll}, tot={total}). +1 интеллект, +1 деньги.");
                break;
        }
    }

    public static void Intrigue(Candidate actor, Candidate target)
    {
        if (actor == null || target == null)
        {
            Debug.LogWarning("[CandidateActions:Intrigue] Требуется исходный кандидат и цель.");
            return;
        }

        var outcome = ResolveCheck(actor.Intellect, false, out int roll, out int total);

        switch (outcome)
        {
            case CheckOutcome.Fail:
                actor.AddAbility(UnpopularAbilityName);
                Debug.Log($"[CandidateActions:Intrigue] {actor.Name} провалил интриги (roll={roll}, tot={total}). +'{UnpopularAbilityName}'.");
                break;
            case CheckOutcome.Success:
                if (!target.RemoveAbility(UnpopularAbilityName) && !target.RemoveAbility(CorruptAbilityName))
                {
                    Debug.Log($"[CandidateActions:Intrigue] {actor.Name} успешно интриговал (roll={roll}, tot={total}), но у {target.Name} нет непопулярного/коррупционного статуса.");
                }
                else
                {
                    Debug.Log($"[CandidateActions:Intrigue] {actor.Name} успешно интриговал (roll={roll}, tot={total}), снята беда у {target.Name}.");
                }
                break;
            case CheckOutcome.CriticalSuccess:
                Debug.Log($"[CandidateActions:Intrigue] {actor.Name} критически успешно интриговал (roll={roll}, tot={total}). Эффект: отсутствует / чистый успех.");
                break;
        }
    }

    public static void Debate(Candidate actor, Candidate opponent)
    {
        if (actor == null || opponent == null)
        {
            Debug.LogWarning("[CandidateActions:Debate] Требуется оба кандидата для дебатов.");
            return;
        }

        int actorValue = Random.Range(1, 11) + actor.Intellect;
        int opponentValue = Random.Range(1, 11) + opponent.Intellect;

        string line = $"[CandidateActions:Debate] {actor.Name} ({actorValue}) vs {opponent.Name} ({opponentValue}).";

        int diff = actorValue - opponentValue;
        CheckOutcome outcome = CheckOutcome.Fail;

        if (diff >= 5)
            outcome = CheckOutcome.CriticalSuccess;
        else if (diff >= 0)
            outcome = CheckOutcome.Success;
        else
            outcome = CheckOutcome.Fail;

        if (outcome == CheckOutcome.Fail)
        {
            actor.Influence -= 1;
            opponent.Influence += 1;
            Debug.Log(line + $" Провал: {actor.Name} -1, {opponent.Name} +1.");
        }
        else if (outcome == CheckOutcome.Success)
        {
            actor.Influence += 1;
            opponent.Influence -= 1;
            Debug.Log(line + $" Успех: {actor.Name} +1, {opponent.Name} -1.");
        }
        else if (outcome == CheckOutcome.CriticalSuccess)
        {
            actor.Influence += 2;
            opponent.Influence -= 1;
            Debug.Log(line + $" Крит успех: {actor.Name} +2, {opponent.Name} -1.");
        }
    }
}
