using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
    private const string JailedAbilityName = "В тюрьме";
    private const string InvestmentsAbilityName = "Инвестиции";
    private const string PredictionAbilityName = "Предсказание";

    private const string InfluenceStatName = "Влияние";
    private const string IntellectStatName = "Интеллект";
    private const string MoneyStatName = "Деньги";
    private const string WillpowerStatName = "Воля";

    private const string EndureActionName = "Терпеть";
    private const string LiveByLawActionName = "Жить по закону";
    private const string LiveByCodeActionName = "Жить по понятиям";

    private static readonly BindingFlags InstanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    private static ActionExecutionResult CreateBaseResult(string actionName, string statChecked)
    {
        return new ActionExecutionResult
        {
            actionName = actionName,
            statChecked = statChecked,
            resultDescription = string.Empty,
            outcomeText = string.Empty
        };
    }

    private static void FinalizeResult(ref ActionExecutionResult result, CheckOutcome outcome, int roll, int total, int statValue, string description)
    {
        result.statValue = statValue;
        result.diceRoll = roll;
        result.checkTotal = total;
        result.outcome = outcome;
        result.outcomeText = result.GetOutcomeText();
        result.resultDescription = description;
    }

    private static void LogResult(string actionName, Candidate actor, Candidate target, ActionExecutionResult result)
    {
        Debug.Log($"[CandidateActions:{actionName}] actor={GetCandidateName(actor)} target={GetCandidateName(target)} stat={result.statChecked}:{result.statValue} roll={result.diceRoll} total={result.checkTotal} outcome={result.outcome} desc=\"{result.resultDescription}\"");
    }

    private static void LogSimpleCheck(string label, int stat, bool advantage, int roll1, int roll2, int selectedRoll, int total, CheckOutcome outcome)
    {
        string rolls = advantage ? $"{roll1}/{roll2}" : roll1.ToString();
        Debug.Log($"[CandidateActions:{label}] type=simple stat={stat} rolls={rolls} selected={selectedRoll} total={total} outcome={outcome}");
    }

    private static void LogOpposedCheck(string label, Candidate actor, Candidate target, string actorStatName, string targetStatName, int actorStat, int targetStat, int actorRoll, int targetRoll, int actorTotal, int targetTotal, CheckOutcome outcome)
    {
        Debug.Log($"[CandidateActions:{label}] type=opposed actor={GetCandidateName(actor)} target={GetCandidateName(target)} actorStat={actorStatName}:{actorStat} targetStat={targetStatName}:{targetStat} actorRoll={actorRoll} targetRoll={targetRoll} actorTotal={actorTotal} targetTotal={targetTotal} outcome={outcome}");
    }

    private static string GetCandidateName(Candidate c) => c != null ? c.Name : "нет цели";

    private static CheckOutcome ResolveCheck(int stat, bool advantage, out int selectedRoll, out int totalValue)
    {
        int roll1 = UnityEngine.Random.Range(1, 11);
        int roll2 = advantage ? UnityEngine.Random.Range(1, 11) : 0;
        selectedRoll = advantage ? Mathf.Max(roll1, roll2) : roll1;
        totalValue = selectedRoll + stat;

        CheckOutcome outcome = totalValue >= 15
            ? CheckOutcome.CriticalSuccess
            : totalValue >= 10
                ? CheckOutcome.Success
                : CheckOutcome.Fail;

        LogSimpleCheck(nameof(ResolveCheck), stat, advantage, roll1, roll2, selectedRoll, totalValue, outcome);
        return outcome;
    }

    private static CheckOutcome ResolveOpposedCheck(Candidate actor, Candidate target, string actorStatName, int actorStat, string targetStatName, int targetStat, out int actorRoll, out int targetRoll, out int actorTotal, out int targetTotal)
    {
        actorRoll = UnityEngine.Random.Range(1, 11);
        targetRoll = UnityEngine.Random.Range(1, 11);
        actorTotal = actorRoll + actorStat;
        targetTotal = targetRoll + targetStat;

        int diff = actorTotal - targetTotal;
        CheckOutcome outcome = diff >= 5
            ? CheckOutcome.CriticalSuccess
            : diff >= 0
                ? CheckOutcome.Success
                : CheckOutcome.Fail;

        LogOpposedCheck(nameof(ResolveOpposedCheck), actor, target, actorStatName, targetStatName, actorStat, targetStat, actorRoll, targetRoll, actorTotal, targetTotal, outcome);
        return outcome;
    }

    private static int ClampNonNegative(int value) => Mathf.Max(0, value);

    private static void AddInfluence(Candidate c, int delta)
    {
        if (c == null) return;
        c.Influence = ClampNonNegative(c.Influence + delta);
    }

    private static void AddIntellect(Candidate c, int delta)
    {
        if (c == null) return;
        c.Intellect = ClampNonNegative(c.Intellect + delta);
    }

    private static void AddMoney(Candidate c, int delta)
    {
        if (c == null) return;
        c.Money = ClampNonNegative(c.Money + delta);
    }

    private static void AddWillpower(Candidate c, int delta)
    {
        if (c == null) return;
        c.Willpower = ClampNonNegative(c.Willpower + delta);
    }

    private static void AddAllStats(Candidate c, int delta)
    {
        AddInfluence(c, delta);
        AddIntellect(c, delta);
        AddMoney(c, delta);
        AddWillpower(c, delta);
    }

    private static bool AddAbilityIfMissing(Candidate c, string abilityName)
    {
        if (c == null || string.IsNullOrWhiteSpace(abilityName)) return false;
        bool had = c.HasAbility(abilityName);
        c.AddAbility(abilityName);
        return !had;
    }

    private static bool RemoveAbilityIfPresent(Candidate c, string abilityName)
    {
        if (c == null || string.IsNullOrWhiteSpace(abilityName)) return false;
        return c.RemoveAbility(abilityName);
    }

    private static bool RemoveFirstNegativeStatus(Candidate c, out string removed)
    {
        removed = null;
        if (c == null) return false;

        if (RemoveAbilityIfPresent(c, UnpopularAbilityName))
        {
            removed = UnpopularAbilityName;
            return true;
        }

        if (RemoveAbilityIfPresent(c, CorruptAbilityName))
        {
            removed = CorruptAbilityName;
            return true;
        }

        return false;
    }

    private static President GetPresident()
    {
#if UNITY_2023_1_OR_NEWER
        return UnityEngine.Object.FindFirstObjectByType<President>();
#else
        return UnityEngine.Object.FindObjectOfType<President>();
#endif
    }

    private static IList GetActiveCrises(President president)
    {
        if (president == null) return null;

        FieldInfo field = typeof(President).GetField("activeCrises", InstanceFlags);
        if (field != null)
            return field.GetValue(president) as IList;

        PropertyInfo prop = typeof(President).GetProperty("activeCrises", InstanceFlags);
        return prop?.GetValue(president) as IList;
    }

    private static int GetCrisisCount()
    {
        IList list = GetActiveCrises(GetPresident());
        return list?.Count ?? 0;
    }

    private static int GetPresidentInsanity()
    {
        President president = GetPresident();
        if (president == null) return 0;

        FieldInfo field = typeof(President).GetField("insanity", InstanceFlags);
        if (field != null && field.FieldType == typeof(int))
            return (int)field.GetValue(president);

        PropertyInfo prop = typeof(President).GetProperty("insanity", InstanceFlags);
        if (prop != null && prop.PropertyType == typeof(int))
            return (int)prop.GetValue(president);

        return 0;
    }

    private static void AddPresidentInsanity(int delta)
    {
        President president = GetPresident();
        if (president == null) return;

        FieldInfo field = typeof(President).GetField("insanity", InstanceFlags);
        if (field != null && field.FieldType == typeof(int))
        {
            field.SetValue(president, Math.Max(0, (int)field.GetValue(president) + delta));
            return;
        }

        PropertyInfo prop = typeof(President).GetProperty("insanity", InstanceFlags);
        if (prop != null && prop.PropertyType == typeof(int) && prop.CanRead && prop.CanWrite)
            prop.SetValue(president, Math.Max(0, (int)prop.GetValue(president) + delta));
    }

    private static int GetPresidentTurnCount()
    {
        President president = GetPresident();
        if (president == null) return 0;

        FieldInfo field = typeof(President).GetField("turnCount", InstanceFlags);
        if (field != null && field.FieldType == typeof(int))
            return (int)field.GetValue(president);

        PropertyInfo prop = typeof(President).GetProperty("turnCount", InstanceFlags);
        if (prop != null && prop.PropertyType == typeof(int))
            return (int)prop.GetValue(president);

        return 0;
    }

    private static bool GetPresidentLastTurnHadNewCrisis()
    {
        President president = GetPresident();
        if (president == null) return false;

        FieldInfo field = typeof(President).GetField("LastTurnHadNewCrisis", InstanceFlags);
        if (field != null && field.FieldType == typeof(bool))
            return (bool)field.GetValue(president);

        PropertyInfo prop = typeof(President).GetProperty("LastTurnHadNewCrisis", InstanceFlags);
        if (prop != null && prop.PropertyType == typeof(bool))
            return (bool)prop.GetValue(president);

        return false;
    }

    private static int GetIntPropertyOrField(object obj, string name, int fallback = 0)
    {
        if (obj == null) return fallback;
        Type t = obj.GetType();

        PropertyInfo prop = t.GetProperty(name, InstanceFlags);
        if (prop != null && prop.PropertyType == typeof(int) && prop.CanRead)
            return (int)prop.GetValue(obj);

        FieldInfo field = t.GetField(name, InstanceFlags);
        if (field != null && field.FieldType == typeof(int))
            return (int)field.GetValue(obj);

        return fallback;
    }

    private static void SetIntPropertyOrField(object obj, string name, int value)
    {
        if (obj == null) return;
        Type t = obj.GetType();

        PropertyInfo prop = t.GetProperty(name, InstanceFlags);
        if (prop != null && prop.PropertyType == typeof(int) && prop.CanWrite)
        {
            prop.SetValue(obj, value);
            return;
        }

        FieldInfo field = t.GetField(name, InstanceFlags);
        if (field != null && field.FieldType == typeof(int))
            field.SetValue(obj, value);
    }

    private static bool GetBoolPropertyOrField(object obj, string name, bool fallback = false)
    {
        if (obj == null) return fallback;
        Type t = obj.GetType();

        PropertyInfo prop = t.GetProperty(name, InstanceFlags);
        if (prop != null && prop.PropertyType == typeof(bool) && prop.CanRead)
            return (bool)prop.GetValue(obj);

        FieldInfo field = t.GetField(name, InstanceFlags);
        if (field != null && field.FieldType == typeof(bool))
            return (bool)field.GetValue(obj);

        return fallback;
    }

    private static void SetBoolPropertyOrField(object obj, string name, bool value)
    {
        if (obj == null) return;
        Type t = obj.GetType();

        PropertyInfo prop = t.GetProperty(name, InstanceFlags);
        if (prop != null && prop.PropertyType == typeof(bool) && prop.CanWrite)
        {
            prop.SetValue(obj, value);
            return;
        }

        FieldInfo field = t.GetField(name, InstanceFlags);
        if (field != null && field.FieldType == typeof(bool))
            field.SetValue(obj, value);
    }

    private static int GetInvestmentCount(Candidate c) => GetIntPropertyOrField(c, "InvestmentCount", c != null && c.HasAbility(InvestmentsAbilityName) ? 1 : 0);

    private static void SetInvestmentCount(Candidate c, int value)
    {
        if (c == null) return;
        int clamped = Mathf.Max(0, value);
        SetIntPropertyOrField(c, "InvestmentCount", clamped);

        if (clamped > 0)
            AddAbilityIfMissing(c, InvestmentsAbilityName);
        else
            RemoveAbilityIfPresent(c, InvestmentsAbilityName);
    }

    private static bool GetPredictedCrisisFlag(Candidate c) => GetBoolPropertyOrField(c, "PredictedCrisisNextTurn", c != null && c.HasAbility(PredictionAbilityName));
    private static void SetPredictedCrisisFlag(Candidate c, bool value)
    {
        SetBoolPropertyOrField(c, "PredictedCrisisNextTurn", value);
        if (c == null) return;
        if (value) AddAbilityIfMissing(c, PredictionAbilityName); else RemoveAbilityIfPresent(c, PredictionAbilityName);
    }

    private static int GetPredictionTurnIssued(Candidate c) => GetIntPropertyOrField(c, "PredictionTurnIssued", -1);
    private static void SetPredictionTurnIssued(Candidate c, int value) => SetIntPropertyOrField(c, "PredictionTurnIssued", value);

    private static bool IsInPrison(Candidate c)
    {
        if (c == null) return false;
        MethodInfo m = c.GetType().GetMethod("get_IsInPrison", InstanceFlags);
        if (m != null && m.ReturnType == typeof(bool))
            return (bool)m.Invoke(c, null);
        return c.HasAbility(JailedAbilityName) || GetIntPropertyOrField(c, "PrisonTurnsLeft", 0) > 0;
    }

    private static void SendToPrison(Candidate c, int turns = 1)
    {
        if (c == null) return;
        MethodInfo m = c.GetType().GetMethod("SendToPrison", InstanceFlags, null, new[] { typeof(int) }, null)
                    ?? c.GetType().GetMethod("SendToPrison", InstanceFlags);
        if (m != null)
        {
            var pars = m.GetParameters();
            if (pars.Length == 1) m.Invoke(c, new object[] { turns }); else m.Invoke(c, null);
        }
        else
        {
            int current = GetIntPropertyOrField(c, "PrisonTurnsLeft", 0);
            SetIntPropertyOrField(c, "PrisonTurnsLeft", Mathf.Max(current, turns));
            AddAbilityIfMissing(c, JailedAbilityName);
        }
    }

    private static void TickPrison(Candidate c)
    {
        if (c == null) return;
        MethodInfo m = c.GetType().GetMethod("TickPrison", InstanceFlags);
        if (m != null)
        {
            m.Invoke(c, null);
            return;
        }

        int turns = GetIntPropertyOrField(c, "PrisonTurnsLeft", 0);
        if (turns <= 0) return;
        turns--;
        SetIntPropertyOrField(c, "PrisonTurnsLeft", turns);
        if (turns <= 0)
            RemoveAbilityIfPresent(c, JailedAbilityName);
    }

    private static List<Candidate> GetAllCandidates()
    {
#if UNITY_2023_1_OR_NEWER
        CandidateCardsController controller = UnityEngine.Object.FindFirstObjectByType<CandidateCardsController>();
#else
        CandidateCardsController controller = UnityEngine.Object.FindObjectOfType<CandidateCardsController>();
#endif
        return controller != null ? controller.GetCandidates() : new List<Candidate>();
    }

    private static Candidate GetDefaultTarget(Candidate actor)
    {
        List<Candidate> all = GetAllCandidates();
        foreach (Candidate candidate in all)
        {
            if (candidate != null && candidate != actor && !IsInPrison(candidate))
                return candidate;
        }
        return null;
    }

    private static List<Candidate> GetCandidatesWithAbility(string abilityName, Candidate exclude = null)
    {
        List<Candidate> result = new List<Candidate>();
        foreach (Candidate candidate in GetAllCandidates())
        {
            if (candidate == null || candidate == exclude || IsInPrison(candidate))
                continue;
            if (candidate.HasAbility(abilityName))
                result.Add(candidate);
        }
        return result;
    }

    public static SkillCheckResult ResolveFinalCrisisSkillCheck(Candidate actor)
    {
        SkillCheckResult result = new SkillCheckResult
        {
            statName = WillpowerStatName,
            statValue = actor != null ? actor.Willpower : 0,
            selectedRoll = 0,
            totalValue = 0,
            outcome = CheckOutcome.Fail
        };

        if (actor == null)
        {
            Debug.LogWarning("[CandidateActions:ResolveFinalCrisisSkillCheck] actor is null");
            return result;
        }

        result.outcome = ResolveCheck(actor.Willpower, false, out result.selectedRoll, out result.totalValue);
        Debug.Log($"[CandidateActions:ResolveFinalCrisisSkillCheck] actor={actor.Name} stat={result.statName}:{result.statValue} roll={result.selectedRoll} total={result.totalValue} outcome={result.outcome}");
        return result;
    }

    public static ActionExecutionResult Steal(Candidate actor)
    {
        ActionExecutionResult result = CreateBaseResult("Воровство", IntellectStatName);
        if (actor == null)
        {
            result.resultDescription = "Ошибка: кандидат не найден.";
            return result;
        }

        CheckOutcome outcome = ResolveCheck(actor.Intellect, false, out int roll, out int total);
        switch (outcome)
        {
            case CheckOutcome.Fail:
                AddAbilityIfMissing(actor, CorruptAbilityName);
                FinalizeResult(ref result, outcome, roll, total, actor.Intellect, $"Провал. {actor.Name} получает статус '{CorruptAbilityName}'.");
                break;
            case CheckOutcome.Success:
                AddAbilityIfMissing(actor, CorruptAbilityName);
                AddMoney(actor, 2);
                FinalizeResult(ref result, outcome, roll, total, actor.Intellect, $"Успех. {actor.Name} получает статус '{CorruptAbilityName}' и +2 деньги.");
                break;
            case CheckOutcome.CriticalSuccess:
                AddMoney(actor, 2);
                FinalizeResult(ref result, outcome, roll, total, actor.Intellect, $"Крит. {actor.Name} получает +2 деньги без лишних следов.");
                break;
        }

        LogResult(nameof(Steal), actor, null, result);
        return result;
    }

    public static ActionExecutionResult Lobby(Candidate actor)
    {
        ActionExecutionResult result = CreateBaseResult("Лобирование", MoneyStatName);
        if (actor == null)
        {
            result.resultDescription = "Ошибка: кандидат не найден.";
            return result;
        }

        CheckOutcome outcome = ResolveCheck(actor.Money, false, out int roll, out int total);
        switch (outcome)
        {
            case CheckOutcome.Fail:
                AddInfluence(actor, -1);
                FinalizeResult(ref result, outcome, roll, total, actor.Money, "Провал. -1 влияние.");
                break;
            case CheckOutcome.Success:
                AddInfluence(actor, 1);
                FinalizeResult(ref result, outcome, roll, total, actor.Money, "Успех. +1 влияние.");
                break;
            case CheckOutcome.CriticalSuccess:
                AddInfluence(actor, 2);
                actor.NoCrisisNextTurn = true;
                FinalizeResult(ref result, outcome, roll, total, actor.Money, "Крит. +2 влияние и следующий кризис для кандидата отменён.");
                break;
        }

        LogResult(nameof(Lobby), actor, null, result);
        return result;
    }

    public static ActionExecutionResult MajorAppeal(Candidate actor, int spendMoney = 0)
    {
        ActionExecutionResult result = CreateBaseResult("Обращение важное", WillpowerStatName);
        if (actor == null)
        {
            result.resultDescription = "Ошибка: кандидат не найден.";
            return result;
        }

        int spend = Mathf.Max(0, spendMoney);
        int availableSpend = Mathf.Min(spend, actor.Money);
        bool advantage = availableSpend > 0;
        if (availableSpend > 0)
            AddMoney(actor, -availableSpend);

        CheckOutcome outcome = ResolveCheck(actor.Willpower, advantage, out int roll, out int total);
        switch (outcome)
        {
            case CheckOutcome.Fail:
                FinalizeResult(ref result, outcome, roll, total, actor.Willpower, availableSpend > 0 ? $"Провал. Потрачено {availableSpend} денег без результата." : "Провал. Без эффекта.");
                break;
            case CheckOutcome.Success:
                AddIntellect(actor, 1);
                FinalizeResult(ref result, outcome, roll, total, actor.Willpower, "+1 интеллект.");
                break;
            case CheckOutcome.CriticalSuccess:
                AddIntellect(actor, 1);
                AddMoney(actor, 1);
                FinalizeResult(ref result, outcome, roll, total, actor.Willpower, "Крит. +1 интеллект и +1 деньги.");
                break;
        }

        LogResult(nameof(MajorAppeal), actor, null, result);
        return result;
    }

    public static ActionExecutionResult Intrigue(Candidate actor, Candidate target)
    {
        ActionExecutionResult result = CreateBaseResult("Интриги", IntellectStatName);
        if (actor == null)
        {
            result.resultDescription = "Ошибка: кандидат не найден.";
            return result;
        }

        target ??= GetDefaultTarget(actor);
        if (target == null)
        {
            result.resultDescription = "Ошибка: не найдена цель для интриг.";
            return result;
        }

        CheckOutcome outcome = ResolveOpposedCheck(actor, target, IntellectStatName, actor.Intellect, IntellectStatName, target.Intellect, out int actorRoll, out int targetRoll, out int actorTotal, out int targetTotal);
        switch (outcome)
        {
            case CheckOutcome.Fail:
                AddAbilityIfMissing(actor, UnpopularAbilityName);
                FinalizeResult(ref result, outcome, actorRoll, actorTotal, actor.Intellect, $"Провал против {target.Name}: {actorRoll}+{actor.Intellect}={actorTotal} против {targetRoll}+{target.Intellect}={targetTotal}. {actor.Name} получает статус '{UnpopularAbilityName}'.");
                break;
            case CheckOutcome.Success:
            case CheckOutcome.CriticalSuccess:
                bool removed = RemoveFirstNegativeStatus(target, out string removedName);
                string extra = outcome == CheckOutcome.CriticalSuccess ? " Крит даёт особенно чистое исполнение." : string.Empty;
                FinalizeResult(ref result, outcome, actorRoll, actorTotal, actor.Intellect,
                    removed
                        ? $"Успех против {target.Name}: {actorRoll}+{actor.Intellect}={actorTotal} против {targetRoll}+{target.Intellect}={targetTotal}. Снят статус '{removedName}'.{extra}"
                        : $"Успех против {target.Name}: {actorRoll}+{actor.Intellect}={actorTotal} против {targetRoll}+{target.Intellect}={targetTotal}. У цели не было подходящих статусов для снятия.{extra}");
                break;
        }

        LogResult(nameof(Intrigue), actor, target, result);
        return result;
    }

    public static ActionExecutionResult Debate(Candidate actor, Candidate target)
    {
        ActionExecutionResult result = CreateBaseResult("Дебаты", IntellectStatName);
        if (actor == null)
        {
            result.resultDescription = "Ошибка: кандидат не найден.";
            return result;
        }

        target ??= GetDefaultTarget(actor);
        if (target == null)
        {
            result.resultDescription = "Ошибка: не найден оппонент для дебатов.";
            return result;
        }

        CheckOutcome outcome = ResolveOpposedCheck(actor, target, IntellectStatName, actor.Intellect, IntellectStatName, target.Intellect, out int actorRoll, out int targetRoll, out int actorTotal, out int targetTotal);
        switch (outcome)
        {
            case CheckOutcome.Fail:
                AddInfluence(actor, -1);
                AddInfluence(target, 1);
                FinalizeResult(ref result, outcome, actorRoll, actorTotal, actor.Intellect, $"Провал против {target.Name}: {actorRoll}+{actor.Intellect}={actorTotal} против {targetRoll}+{target.Intellect}={targetTotal}. {actor.Name} теряет 1 влияние, {target.Name} получает 1.");
                break;
            case CheckOutcome.Success:
                AddInfluence(actor, 1);
                AddInfluence(target, -1);
                FinalizeResult(ref result, outcome, actorRoll, actorTotal, actor.Intellect, $"Успех против {target.Name}: {actorRoll}+{actor.Intellect}={actorTotal} против {targetRoll}+{target.Intellect}={targetTotal}. {actor.Name} получает 1 влияние, {target.Name} теряет 1.");
                break;
            case CheckOutcome.CriticalSuccess:
                AddInfluence(actor, 2);
                AddInfluence(target, -1);
                FinalizeResult(ref result, outcome, actorRoll, actorTotal, actor.Intellect, $"Крит против {target.Name}: {actorRoll}+{actor.Intellect}={actorTotal} против {targetRoll}+{target.Intellect}={targetTotal}. {actor.Name} получает 2 влияния, {target.Name} теряет 1.");
                break;
        }

        LogResult(nameof(Debate), actor, target, result);
        return result;
    }

    public static ActionExecutionResult Whistleblow(Candidate actor, Candidate target)
    {
        return FileReport(actor, target);
    }

    public static ActionExecutionResult FileReport(Candidate actor, Candidate target)
    {
        ActionExecutionResult result = CreateBaseResult("Написать донос", WillpowerStatName);
        if (actor == null)
        {
            result.resultDescription = "Ошибка: кандидат не найден.";
            return result;
        }

        target ??= GetDefaultTarget(actor);
        if (target == null)
        {
            result.resultDescription = "Ошибка: не найдена цель для доноса.";
            return result;
        }

        CheckOutcome outcome = ResolveOpposedCheck(actor, target, WillpowerStatName, actor.Influence, WillpowerStatName, target.Influence, out int actorRoll, out int targetRoll, out int actorTotal, out int targetTotal);
        switch (outcome)
        {
            case CheckOutcome.Fail:
                AddInfluence(actor, -1);
                FinalizeResult(ref result, outcome, actorRoll, actorTotal, actor.Influence, $"Провал против {target.Name}: {actorRoll}+{actor.Influence}={actorTotal} против {targetRoll}+{target.Influence}={targetTotal}. {actor.Name} теряет 1 влияние.");
                break;
            case CheckOutcome.Success:
                AddInfluence(actor, 1);
                AddInfluence(target, -1);
                FinalizeResult(ref result, outcome, actorRoll, actorTotal, actor.Influence, $"Успех против {target.Name}: {actorRoll}+{actor.Influence}={actorTotal} против {targetRoll}+{target.Influence}={targetTotal}. {actor.Name} получает 1 влияние, {target.Name} теряет 1.");
                break;
            case CheckOutcome.CriticalSuccess:
                AddInfluence(actor, 1);
                AddInfluence(target, -1);
                SendToPrison(target, 1);
                FinalizeResult(ref result, outcome, actorRoll, actorTotal, actor.Influence, $"Крит против {target.Name}: {actorRoll}+{actor.Influence}={actorTotal} против {targetRoll}+{target.Influence}={targetTotal}. {actor.Name} получает 1 влияние, {target.Name} теряет 1 и отправляется в тюрьму.");
                break;
        }

        LogResult(nameof(FileReport), actor, target, result);
        return result;
    }

    public static ActionExecutionResult PhilosopherPost(Candidate actor)
    {
        ActionExecutionResult result = CreateBaseResult("Написать пост в тг", IntellectStatName);
        if (actor == null)
        {
            result.resultDescription = "Ошибка: кандидат не найден.";
            return result;
        }

        CheckOutcome outcome = ResolveCheck(actor.Intellect, false, out int roll, out int total);
        switch (outcome)
        {
            case CheckOutcome.Fail:
                AddInfluence(actor, -1);
                AddWillpower(actor, -1);
                FinalizeResult(ref result, outcome, roll, total, actor.Intellect, "Провал. -1 влияние, -1 воля.");
                break;
            case CheckOutcome.Success:
                AddInfluence(actor, 2);
                AddPresidentInsanity(2);
                FinalizeResult(ref result, outcome, roll, total, actor.Intellect, "+2 влияние, +2 безумия президенту.");
                break;
            case CheckOutcome.CriticalSuccess:
                AddInfluence(actor, 2);
                AddIntellect(actor, 1);
                AddMoney(actor, 1);
                AddWillpower(actor, 1);
                AddPresidentInsanity(1);
                FinalizeResult(ref result, outcome, roll, total, actor.Intellect, "Крит. +2 влияние, +1 интеллект, +1 деньги, +1 воля и +1 безумия президенту.");
                break;
        }

        LogResult(nameof(PhilosopherPost), actor, null, result);
        return result;
    }

    public static ActionExecutionResult AntiCorruptionExpose(Candidate actor)
    {
        ActionExecutionResult result = CreateBaseResult("Сделать разоблачение", WillpowerStatName);
        if (actor == null)
        {
            result.resultDescription = "Ошибка: кандидат не найден.";
            return result;
        }

        List<Candidate> corrupt = GetCandidatesWithAbility(CorruptAbilityName, actor);
        int count = corrupt.Count;
        CheckOutcome outcome = ResolveCheck(actor.Willpower, false, out int roll, out int total);
        switch (outcome)
        {
            case CheckOutcome.Fail:
                AddMoney(actor, -1);
                AddInfluence(actor, -1);
                FinalizeResult(ref result, outcome, roll, total, actor.Willpower, "Провал. -1 деньги, -1 влияние.");
                break;
            case CheckOutcome.Success:
                AddMoney(actor, count);
                AddInfluence(actor, count);
                FinalizeResult(ref result, outcome, roll, total, actor.Willpower, $"Успех. Найдено коррупционеров: {count}. +{count} деньги и +{count} влияние.");
                break;
            case CheckOutcome.CriticalSuccess:
                AddMoney(actor, count * 2);
                AddInfluence(actor, count * 2);
                foreach (Candidate c in corrupt)
                {
                    AddMoney(c, -1);
                    AddInfluence(c, -1);
                    SendToPrison(c, 1);
                }
                FinalizeResult(ref result, outcome, roll, total, actor.Willpower, $"Крит. Найдено коррупционеров: {count}. +{count * 2} деньги и +{count * 2} влияние. Коррупционеры наказаны и посажены.");
                break;
        }

        LogResult(nameof(AntiCorruptionExpose), actor, null, result);
        return result;
    }

    public static ActionExecutionResult RaiseMutiny(Candidate actor)
    {
        ActionExecutionResult result = CreateBaseResult("Поднять мятеж", WillpowerStatName);
        if (actor == null)
        {
            result.resultDescription = "Ошибка: кандидат не найден.";
            return result;
        }

        int crisisCount = GetCrisisCount();
        int effectiveStat = actor.Willpower + crisisCount;
        CheckOutcome outcome = ResolveCheck(effectiveStat, false, out int roll, out int total);
        switch (outcome)
        {
            case CheckOutcome.Fail:
                AddInfluence(actor, -1);
                AddPresidentInsanity(2);
                FinalizeResult(ref result, outcome, roll, total, effectiveStat, "Провал. -1 влияние, +2 безумия президенту.");
                break;
            case CheckOutcome.Success:
                AddInfluence(actor, crisisCount);
                AddPresidentInsanity(-1);
                FinalizeResult(ref result, outcome, roll, total, effectiveStat, $"Успех. +{crisisCount} влияние, -1 безумия президенту.");
                break;
            case CheckOutcome.CriticalSuccess:
                AddInfluence(actor, crisisCount);
                AddPresidentInsanity(-2);
                FinalizeResult(ref result, outcome, roll, total, effectiveStat, $"Крит. +{crisisCount} влияние, -2 безумия президенту.");
                break;
        }

        if (outcome != CheckOutcome.Fail)
        {
            int presidentInsanity = GetPresidentInsanity();
            CheckOutcome mutinyDuel = ResolveOpposedCheck(actor, null, InfluenceStatName, actor.Influence, "Безумие", presidentInsanity, out int actorRoll, out int presRoll, out int actorTotal, out int presTotal);
            if (mutinyDuel == CheckOutcome.Fail)
            {
                SendToPrison(actor, 99);
                result.resultDescription += $" Затем мятеж сорвался: {actorRoll}+{actor.Influence}={actorTotal} против {presRoll}+{presidentInsanity}={presTotal}. Кандидат выбыл.";
            }
            else
            {
                result.resultDescription += $" Затем мятеж удержан: {actorRoll}+{actor.Influence}={actorTotal} против {presRoll}+{presidentInsanity}={presTotal}.";
            }
        }

        LogResult(nameof(RaiseMutiny), actor, null, result);
        return result;
    }

    public static ActionExecutionResult PoliticalStream(Candidate actor, Candidate target)
    {
        ActionExecutionResult result = CreateBaseResult("Провести полит стрим", IntellectStatName);
        if (actor == null)
        {
            result.resultDescription = "Ошибка: кандидат не найден.";
            return result;
        }

        target ??= GetDefaultTarget(actor);
        if (target == null)
        {
            result.resultDescription = "Ошибка: не найдена цель для стрима.";
            return result;
        }

        int bonus = GetPresidentLastTurnHadNewCrisis() ? 2 : 0;
        int effectiveStat = actor.Intellect + bonus;
        CheckOutcome outcome = ResolveCheck(effectiveStat, false, out int roll, out int total);
        switch (outcome)
        {
            case CheckOutcome.Fail:
                AddInfluence(target, -1);
                AddInfluence(actor, -1);
                AddMoney(actor, -1);
                FinalizeResult(ref result, outcome, roll, total, effectiveStat, $"Провал. У {target.Name} -1 влияние, у {actor.Name} -1 влияние и -1 деньги.");
                break;
            case CheckOutcome.Success:
                AddInfluence(target, 1);
                AddMoney(target, 1);
                AddInfluence(actor, 1);
                AddMoney(actor, 1);
                FinalizeResult(ref result, outcome, roll, total, effectiveStat, $"Успех. {actor.Name} и {target.Name} получают +1 влияние и +1 деньги.");
                break;
            case CheckOutcome.CriticalSuccess:
                AddAllStats(actor, 1);
                AddAllStats(target, 1);
                FinalizeResult(ref result, outcome, roll, total, effectiveStat, $"Крит. {actor.Name} и {target.Name} получают +1 ко всем характеристикам.");
                break;
        }

        LogResult(nameof(PoliticalStream), actor, target, result);
        return result;
    }

    public static ActionExecutionResult MakePrediction(Candidate actor)
    {
        ActionExecutionResult result = CreateBaseResult("Сделать предсказание", IntellectStatName);
        if (actor == null)
        {
            result.resultDescription = "Ошибка: кандидат не найден.";
            return result;
        }

        SetPredictedCrisisFlag(actor, true);
        SetPredictionTurnIssued(actor, GetPresidentTurnCount());
        FinalizeResult(ref result, CheckOutcome.Success, 0, actor.Intellect, actor.Intellect, "Предсказание сделано. На следующем ходу эффект сработает автоматически.");
        LogResult(nameof(MakePrediction), actor, null, result);
        return result;
    }

    public static void ResolvePredictionOutcome(Candidate actor, bool crisisHappened)
    {
        if (actor == null || !GetPredictedCrisisFlag(actor))
            return;

        SetPredictedCrisisFlag(actor, false);
        if (crisisHappened)
        {
            AddAllStats(actor, 1);
            Debug.Log($"[CandidateActions:ResolvePredictionOutcome] actor={actor.Name} crisis=true effect=+1_all_stats");
        }
        else
        {
            AddAllStats(actor, -1);
            Debug.Log($"[CandidateActions:ResolvePredictionOutcome] actor={actor.Name} crisis=false effect=-1_all_stats");
        }
    }

    public static ActionExecutionResult CashOutInvestments(Candidate actor)
    {
        ActionExecutionResult result = CreateBaseResult("Фиксируем прибыль", IntellectStatName);
        if (actor == null)
        {
            result.resultDescription = "Ошибка: кандидат не найден.";
            return result;
        }

        int n = GetInvestmentCount(actor);
        CheckOutcome outcome = ResolveCheck(actor.Intellect, false, out int roll, out int total);
        switch (outcome)
        {
            case CheckOutcome.Fail:
                SetInvestmentCount(actor, 0);
                FinalizeResult(ref result, outcome, roll, total, actor.Intellect, "Провал. Все инвестиции потеряны.");
                break;
            case CheckOutcome.Success:
                AddMoney(actor, n);
                FinalizeResult(ref result, outcome, roll, total, actor.Intellect, $"Успех. +{n} деньги.");
                break;
            case CheckOutcome.CriticalSuccess:
                AddMoney(actor, n * n);
                FinalizeResult(ref result, outcome, roll, total, actor.Intellect, $"Крит. +{n * n} деньги.");
                break;
        }

        LogResult(nameof(CashOutInvestments), actor, null, result);
        return result;
    }

    public static ActionExecutionResult Endure(Candidate actor)
    {
        ActionExecutionResult result = CreateBaseResult(EndureActionName, WillpowerStatName);
        if (actor == null)
        {
            result.resultDescription = "Ошибка: кандидат не найден.";
            return result;
        }

        FinalizeResult(ref result, CheckOutcome.Success, 0, actor.Willpower, actor.Willpower, "Кандидат терпит тюремный срок. Ход проходит без дополнительных эффектов.");
        LogResult(nameof(Endure), actor, null, result);
        return result;
    }

    public static ActionExecutionResult LiveByLaw(Candidate actor)
    {
        ActionExecutionResult result = CreateBaseResult(LiveByLawActionName, IntellectStatName);
        if (actor == null)
        {
            result.resultDescription = "Ошибка: кандидат не найден.";
            return result;
        }

        CheckOutcome outcome = ResolveCheck(actor.Intellect, false, out int roll, out int total);
        switch (outcome)
        {
            case CheckOutcome.Fail:
                AddWillpower(actor, -1);
                AddInfluence(actor, -1);
                FinalizeResult(ref result, outcome, roll, total, actor.Intellect, "Провал. -1 воля и -1 влияние.");
                break;
            case CheckOutcome.Success:
                FinalizeResult(ref result, outcome, roll, total, actor.Intellect, "Успех. Ничего не происходит.");
                break;
            case CheckOutcome.CriticalSuccess:
                AddInfluence(actor, 1);
                FinalizeResult(ref result, outcome, roll, total, actor.Intellect, "Крит. +1 влияние.");
                break;
        }

        LogResult(nameof(LiveByLaw), actor, null, result);
        return result;
    }

    public static ActionExecutionResult LiveByCode(Candidate actor)
    {
        ActionExecutionResult result = CreateBaseResult(LiveByCodeActionName, WillpowerStatName);
        if (actor == null)
        {
            result.resultDescription = "Ошибка: кандидат не найден.";
            return result;
        }

        CheckOutcome outcome = ResolveCheck(actor.Willpower, false, out int roll, out int total);
        switch (outcome)
        {
            case CheckOutcome.Fail:
                AddAllStats(actor, -1);
                FinalizeResult(ref result, outcome, roll, total, actor.Willpower, "Провал. -1 ко всем характеристикам.");
                break;
            case CheckOutcome.Success:
                AddWillpower(actor, 1);
                FinalizeResult(ref result, outcome, roll, total, actor.Willpower, "Успех. +1 воля.");
                break;
            case CheckOutcome.CriticalSuccess:
                AddWillpower(actor, 1);
                AddIntellect(actor, 1);
                FinalizeResult(ref result, outcome, roll, total, actor.Willpower, "Крит. +1 воля и +1 интеллект.");
                break;
        }

        LogResult(nameof(LiveByCode), actor, null, result);
        return result;
    }

    public static void ResolvePassiveTurnEffects(Candidate actor)
    {
        if (actor == null)
            return;

        TickPrison(actor);

        bool hadUnpopular = actor.HasAbility(UnpopularAbilityName);
        if (hadUnpopular)
        {
            int unpopularRoll = UnityEngine.Random.Range(0, 100);
            bool loseInfluence = unpopularRoll < 50;
            if (loseInfluence)
                AddInfluence(actor, -1);
            RemoveAbilityIfPresent(actor, UnpopularAbilityName);
            Debug.Log($"[CandidateActions:ResolvePassiveTurnEffects] actor={actor.Name} status={UnpopularAbilityName} roll={unpopularRoll} triggered={loseInfluence} effect={(loseInfluence ? "-1_influence" : "none")}");
        }

        bool hadCorrupt = actor.HasAbility(CorruptAbilityName);
        if (hadCorrupt)
        {
            int corruptRoll = UnityEngine.Random.Range(0, 100);
            bool jailed = corruptRoll < 50;
            if (jailed)
                SendToPrison(actor, 1);
            RemoveAbilityIfPresent(actor, CorruptAbilityName);
            Debug.Log($"[CandidateActions:ResolvePassiveTurnEffects] actor={actor.Name} status={CorruptAbilityName} roll={corruptRoll} triggered={jailed} effect={(jailed ? "prison" : "none")}");
        }

        int investments = GetInvestmentCount(actor);
        if (investments > 0)
        {
            int chance = Mathf.Clamp(GetCrisisCount() * 10, 0, 100);
            int roll = UnityEngine.Random.Range(0, 100);
            bool burned = roll < chance;
            if (burned)
                SetInvestmentCount(actor, 0);

            Debug.Log($"[CandidateActions:ResolvePassiveTurnEffects] actor={actor.Name} investments={investments} burnChance={chance} roll={roll} burned={burned}");
        }
    }
}
