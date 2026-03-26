using UnityEngine;

public enum CheckOutcome
{
    Fail,
    Success,
    CriticalSuccess
}

public struct SkillCheckResult
{
    public string statName;
    public int statValue;
    public int selectedRoll;
    public int totalValue;
    public CheckOutcome outcome;
}

/// <summary>
/// Результат выполнения действия с информацией о броске и эффектах
/// </summary>
public struct ActionExecutionResult
{
    public string actionName;
    public string statChecked;
    public int statValue;
    public int diceRoll;
    public int checkTotal;
    public CheckOutcome outcome;
    public string resultDescription;
    public string outcomeText;

    public string GetOutcomeText()
    {
        return outcome switch
        {
            CheckOutcome.Fail => "❌ Провал",
            CheckOutcome.Success => "✓ Успех",
            CheckOutcome.CriticalSuccess => "⭐ Крит",
            _ => "?"
        };
    }
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

    public static SkillCheckResult ResolveFinalCrisisSkillCheck(Candidate actor)
    {
        SkillCheckResult result = new SkillCheckResult
        {
            statName = "Воля",
            statValue = actor != null ? actor.Willpower : 0,
            selectedRoll = 0,
            totalValue = 0,
            outcome = CheckOutcome.Fail
        };

        if (actor == null)
        {
            Debug.LogWarning("[CandidateActions:FinalCrisis] Кандидат не передан.");
            return result;
        }

        result.outcome = ResolveCheck(actor.Willpower, false, out result.selectedRoll, out result.totalValue);
        Debug.Log($"[CandidateActions:FinalCrisis] {actor.Name} проходит финальный кризис по стату '{result.statName}'={result.statValue}. Бросок={result.selectedRoll}, итог={result.totalValue}, исход={result.outcome}");
        return result;
    }

    public static ActionExecutionResult Steal(Candidate actor)
    {
        ActionExecutionResult result = new ActionExecutionResult
        {
            actionName = "Воровство",
            statChecked = "Интеллект"
        };

        if (actor == null)
        {
            result.resultDescription = "Ошибка: кандидат не найден";
            return result;
        }

        result.statValue = actor.Intellect;
        var outcome = ResolveCheck(actor.Intellect, false, out int roll, out int total);
        result.diceRoll = roll;
        result.checkTotal = total;
        result.outcome = outcome;
        result.outcomeText = result.GetOutcomeText();

        switch (outcome)
        {
            case CheckOutcome.Fail:
                actor.AddAbility(CorruptAbilityName);
                result.resultDescription = $"Провал. {actor.Name} получил эффект '{CorruptAbilityName}'";
                Debug.Log($"[CandidateActions:Steal] {actor.Name} провалил воровство (roll={roll}, tot={total}). Получил '{CorruptAbilityName}'.");
                break;
            case CheckOutcome.Success:
                actor.AddAbility(CorruptAbilityName);
                actor.Money += 2;
                result.resultDescription = $"Успех. +2 деньги, получил '{CorruptAbilityName}'";
                Debug.Log($"[CandidateActions:Steal] {actor.Name} успешно украл (roll={roll}, tot={total}). +2 к деньгам, получил '{CorruptAbilityName}'.");
                break;
            case CheckOutcome.CriticalSuccess:
                actor.Money += 2;
                result.resultDescription = $"Крит! +2 деньги";
                Debug.Log($"[CandidateActions:Steal] {actor.Name} критически успешно украл (roll={roll}, tot={total}). +2 к деньгам.");
                break;
        }

        return result;
    }

    public static ActionExecutionResult Lobby(Candidate actor)
    {
        ActionExecutionResult result = new ActionExecutionResult
        {
            actionName = "Лобирование",
            statChecked = "Деньги"
        };

        if (actor == null)
        {
            result.resultDescription = "Ошибка: кандидат не найден";
            return result;
        }

        result.statValue = actor.Money;
        var outcome = ResolveCheck(actor.Money, false, out int roll, out int total);
        result.diceRoll = roll;
        result.checkTotal = total;
        result.outcome = outcome;
        result.outcomeText = result.GetOutcomeText();

        switch (outcome)
        {
            case CheckOutcome.Fail:
                actor.Influence -= 1;
                result.resultDescription = $"Провал. -1 влияние";
                Debug.Log($"[CandidateActions:Lobby] {actor.Name} провалил лобирование (roll={roll}, tot={total}). -1 влияния.");
                break;
            case CheckOutcome.Success:
                actor.Influence += 1;
                result.resultDescription = $"Успех. +1 влияние";
                Debug.Log($"[CandidateActions:Lobby] {actor.Name} успешно лобировал (roll={roll}, tot={total}). +1 влияния.");
                break;
            case CheckOutcome.CriticalSuccess:
                actor.Influence += 2;
                actor.NoCrisisNextTurn = true;
                result.resultDescription = $"Крит! +2 влияния, отмена кризиса в следующем ходу";
                Debug.Log($"[CandidateActions:Lobby] {actor.Name} критически успешно лобировал (roll={roll}, tot={total}). +2 влияния, кризис в следующем ходу отменяется.");
                break;
        }

        return result;
    }

    public static ActionExecutionResult MajorAppeal(Candidate actor, int spendMoney = 0)
    {
        ActionExecutionResult result = new ActionExecutionResult
        {
            actionName = "Обращение важное",
            statChecked = "Воля"
        };

        if (actor == null)
        {
            result.resultDescription = "Ошибка: кандидат не найден";
            return result;
        }

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
        }

        result.statValue = actor.Willpower;
        var outcome = ResolveCheck(actor.Willpower, advantage, out int roll, out int total);
        result.diceRoll = roll;
        result.checkTotal = total;
        result.outcome = outcome;
        result.outcomeText = result.GetOutcomeText();

        switch (outcome)
        {
            case CheckOutcome.Fail:
                result.resultDescription = $"Провал. Нет эффектов";
                Debug.Log($"[CandidateActions:MajorAppeal] {actor.Name} провалил обращение (roll={roll}, tot={total}). Без эффектов.");
                break;
            case CheckOutcome.Success:
                actor.Intellect += 1;
                result.resultDescription = $"Успех. +1 интеллект";
                Debug.Log($"[CandidateActions:MajorAppeal] {actor.Name} успешно (roll={roll}, tot={total}). +1 интеллект.");
                break;
            case CheckOutcome.CriticalSuccess:
                actor.Intellect += 1;
                actor.Money += 1;
                result.resultDescription = $"Крит! +1 интеллект, +1 деньги";
                Debug.Log($"[CandidateActions:MajorAppeal] {actor.Name} критически успешно (roll={roll}, tot={total}). +1 интеллект, +1 деньги.");
                break;
        }

        return result;
    }

    public static ActionExecutionResult Intrigue(Candidate actor, Candidate target)
    {
        ActionExecutionResult result = new ActionExecutionResult
        {
            actionName = "Интриги",
            statChecked = "Интеллект"
        };

        if (actor == null || target == null)
        {
            result.resultDescription = "Ошибка: требуется исходный кандидат и цель";
            Debug.LogWarning("[CandidateActions:Intrigue] Требуется исходный кандидат и цель.");
            return result;
        }

        result.statValue = actor.Intellect;
        var outcome = ResolveCheck(actor.Intellect, false, out int roll, out int total);
        result.diceRoll = roll;
        result.checkTotal = total;
        result.outcome = outcome;
        result.outcomeText = result.GetOutcomeText();

        switch (outcome)
        {
            case CheckOutcome.Fail:
                actor.AddAbility(UnpopularAbilityName);
                result.resultDescription = $"Провал. {actor.Name} получил '{UnpopularAbilityName}'";
                Debug.Log($"[CandidateActions:Intrigue] {actor.Name} провалил интриги (roll={roll}, tot={total}). +'{UnpopularAbilityName}'.");
                break;
            case CheckOutcome.Success:
                bool removed = target.RemoveAbility(UnpopularAbilityName);
                if (!removed)
                    removed = target.RemoveAbility(CorruptAbilityName);
                
                if (removed)
                {
                    result.resultDescription = $"Успех. Снят статус у {target.Name}";
                    Debug.Log($"[CandidateActions:Intrigue] {actor.Name} успешно интриговал (roll={roll}, tot={total}), снята беда у {target.Name}.");
                }
                else
                {
                    result.resultDescription = $"Успех. У {target.Name} нет статусов для снятия";
                    Debug.Log($"[CandidateActions:Intrigue] {actor.Name} успешно интриговал (roll={roll}, tot={total}), но у {target.Name} нет непопулярного/коррупционного статуса.");
                }
                break;
            case CheckOutcome.CriticalSuccess:
                result.resultDescription = $"Крит! Чистый успех";
                Debug.Log($"[CandidateActions:Intrigue] {actor.Name} критически успешно интриговал (roll={roll}, tot={total}).");
                break;
        }

        return result;
    }

    public static ActionExecutionResult Debate(Candidate actor, Candidate opponent)
    {
        ActionExecutionResult result = new ActionExecutionResult
        {
            actionName = "Дебаты",
            statChecked = "Интеллект"
        };

        if (actor == null || opponent == null)
        {
            result.resultDescription = "Ошибка: требуются оба кандидата для дебатов";
            Debug.LogWarning("[CandidateActions:Debate] Требуется оба кандидата для дебатов.");
            return result;
        }

        int actorRoll = Random.Range(1, 11);
        int opponentRoll = Random.Range(1, 11);
        int actorValue = actorRoll + actor.Intellect;
        int opponentValue = opponentRoll + opponent.Intellect;

        result.statValue = actor.Intellect;
        result.diceRoll = actorRoll;
        result.checkTotal = actorValue;

        int diff = actorValue - opponentValue;
        CheckOutcome outcome = CheckOutcome.Fail;

        if (diff >= 5)
            outcome = CheckOutcome.CriticalSuccess;
        else if (diff >= 0)
            outcome = CheckOutcome.Success;
        else
            outcome = CheckOutcome.Fail;

        result.outcome = outcome;
        result.outcomeText = result.GetOutcomeText();

        if (outcome == CheckOutcome.Fail)
        {
            actor.Influence -= 1;
            opponent.Influence += 1;
            result.resultDescription = $"Провал против {opponent.Name}. {actor.Name} -1 влияние, {opponent.Name} +1";
            Debug.Log($"[CandidateActions:Debate] {actor.Name} ({actorValue}) vs {opponent.Name} ({opponentValue}). Провал: {actor.Name} -1, {opponent.Name} +1.");
        }
        else if (outcome == CheckOutcome.Success)
        {
            actor.Influence += 1;
            opponent.Influence -= 1;
            result.resultDescription = $"Успех против {opponent.Name}. {actor.Name} +1 влияние, {opponent.Name} -1";
            Debug.Log($"[CandidateActions:Debate] {actor.Name} ({actorValue}) vs {opponent.Name} ({opponentValue}). Успех: {actor.Name} +1, {opponent.Name} -1.");
        }
        else if (outcome == CheckOutcome.CriticalSuccess)
        {
            actor.Influence += 2;
            opponent.Influence -= 1;
            result.resultDescription = $"Крит против {opponent.Name}! {actor.Name} +2 влияния, {opponent.Name} -1";
            Debug.Log($"[CandidateActions:Debate] {actor.Name} ({actorValue}) vs {opponent.Name} ({opponentValue}). Крит успех: {actor.Name} +2, {opponent.Name} -1.");
        }

        return result;
    }
}
