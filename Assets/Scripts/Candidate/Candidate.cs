using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Делегат для функции механики способности
public delegate void AbilityFunction();

[System.Serializable]
public class Ability
{
    public string name;
    public Sprite icon;
    public Color color = Color.red;
    
    [TextArea(2, 4)]
    public string description;
    
    private AbilityFunction cachedFunction;
    
    public Ability()
    {
        name = "New Ability";
        icon = null;
        color = Color.red;
        description = "";
        cachedFunction = null;
    }
    
    public Ability(string name, Sprite icon, string description, Color color = default)
    {
        this.name = name;
        this.icon = icon;
        this.color = color != default ? color : Color.red;
        this.description = description;
        this.cachedFunction = null;
    }
    
    public void SetFunction(AbilityFunction function)
    {
        cachedFunction = function;
    }
    
    public void Execute()
    {
        if (cachedFunction != null)
            cachedFunction.Invoke();
        else
            Debug.LogWarning($"[Ability] Функция для способности '{name}' не установлена!");
    }
}

[System.Serializable]
public class Candidate
{
    private string name;
    private int age;
    private int influence;
    private int intellect;
    private int money;
    private int willpower;
    private List<Ability> abilities;
    private string background;

    public string Name { get => name; set => name = value; }
    public int Age { get => age; set => age = value; }
    public int Influence { get => influence; set => influence = value; }
    public int Intellect { get => intellect; set => intellect = value; }
    public int Money { get => money; set => money = value; }
    public int Willpower { get => willpower; set => willpower = value; }
    public List<Ability> Abilities { get => abilities; set => abilities = value; }
    public string Background { get => background; set => background = value; }

    public bool NoCrisisNextTurn { get; set; }
    public int InvestmentCount { get; set; }
    public bool PredictedCrisisNextTurn { get; set; }
    public int PredictionTurnIssued { get; set; } = -1;
    public int PrisonTurnsLeft { get; set; }

    public bool IsInPrison => PrisonTurnsLeft > 0;

    public string ClassName
    {
        get
        {
            if (Abilities == null || Abilities.Count == 0 || Abilities[0] == null)
                return string.Empty;

            return Abilities[0].name ?? string.Empty;
        }
    }

    public Candidate()
    {
        Name = GenerateRandomName();
        Age = Random.Range(35, 71);

        Influence = Random.Range(3, 7);
        Intellect = Random.Range(3, 7);
        Money = Random.Range(3, 7);
        Willpower = Random.Range(3, 7);

        Abilities = GenerateRandomAbilities();
        Background = GenerateRandomBackground();
        InitializeClassState();
    }

    public Candidate(HashSet<string> usedClasses)
    {
        Name = GenerateRandomName();
        Age = Random.Range(35, 71);

        Influence = Random.Range(3, 7);
        Intellect = Random.Range(3, 7);
        Money = Random.Range(3, 7);
        Willpower = Random.Range(3, 7);

        Abilities = GenerateRandomAbilities(usedClasses);
        Background = GenerateRandomBackground();
        InitializeClassState();
    }

    public Candidate(string name, int age, int influence, int intellect, int money, int willpower, List<Ability> abilities, string background)
    {
        Name = name;
        Age = age;
        Influence = influence;
        Intellect = intellect;
        Money = money;
        Willpower = willpower;
        Abilities = abilities ?? new List<Ability>();
        Background = background;
        InitializeClassState();
    }

    private void InitializeClassState()
    {
        InvestmentCount = ClassName.Equals("Моральный богач", System.StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        PrisonTurnsLeft = 0;
        PredictedCrisisNextTurn = false;
        PredictionTurnIssued = -1;
    }

    private static string GenerateRandomName()
    {
        bool isFemale = Random.value > 0.5f;
        return GenerateRandomName(isFemale);
    }

    private static string GenerateRandomName(bool isFemale)
    {
        var maleFirstNames = new[] { "Алексей", "Иван", "Дмитрий", "Сергей", "Павел", "Николай", "Петр", "Михаил" };
        var femaleFirstNames = new[] { "Мария", "Ольга", "Наталья", "Екатерина", "Анна", "Елена", "Людмила", "Виктория" };

        var maleLastNames = new[] { "Иванов", "Петров", "Смирнов", "Соколов", "Лебедев", "Новиков", "Морозов", "Сидоров" };
        var femaleLastNames = new[] { "Иванова", "Петрова", "Смирнова", "Соколова", "Лебедева", "Новикова", "Морозова", "Сидорова" };

        string firstName;
        string lastName;

        if (isFemale)
        {
            firstName = femaleFirstNames[Random.Range(0, femaleFirstNames.Length)];
            lastName = femaleLastNames[Random.Range(0, femaleLastNames.Length)];
        }
        else
        {
            firstName = maleFirstNames[Random.Range(0, maleFirstNames.Length)];
            lastName = maleLastNames[Random.Range(0, maleLastNames.Length)];
        }

        return firstName + " " + lastName;
    }

    private static List<Ability> GenerateRandomAbilities()
    {
        ClassDatabase.Initialize();
        AbilityDatabase.Initialize();
        
        List<Ability> abilities = new List<Ability>();
        Ability playerClass = ClassDatabase.GetRandomClass();
        abilities.Add(new Ability(playerClass.name, playerClass.icon, playerClass.description, playerClass.color));
        
        int additionalAbilityCount = Random.Range(0, 3);
        List<Ability> randomAbilities = AbilityDatabase.GetRandomAbilities(additionalAbilityCount);
        abilities.AddRange(randomAbilities);
        
        return abilities;
    }

    private static List<Ability> GenerateRandomAbilities(HashSet<string> usedClasses)
    {
        ClassDatabase.Initialize();
        AbilityDatabase.Initialize();
        
        List<Ability> abilities = new List<Ability>();
        Ability playerClass = ClassDatabase.GetRandomClassExcluding(usedClasses);
        abilities.Add(new Ability(playerClass.name, playerClass.icon, playerClass.description, playerClass.color));
        
        int additionalAbilityCount = Random.Range(0, 3);
        List<Ability> randomAbilities = AbilityDatabase.GetRandomAbilities(additionalAbilityCount);
        abilities.AddRange(randomAbilities);
        
        return abilities;
    }

    public bool HasAbility(string abilityName)
    {
        if (Abilities == null || string.IsNullOrWhiteSpace(abilityName))
            return false;

        return Abilities.Any(a => a != null && a.name != null && a.name.Equals(abilityName, System.StringComparison.OrdinalIgnoreCase));
    }

    public void AddAbility(string abilityName)
    {
        if (string.IsNullOrWhiteSpace(abilityName))
            return;

        if (Abilities == null)
            Abilities = new List<Ability>();

        if (HasAbility(abilityName))
            return;

        Ability fromDb = AbilityDatabase.GetAllAbilities().FirstOrDefault(a => a.name.Equals(abilityName, System.StringComparison.OrdinalIgnoreCase));
        if (fromDb != null)
            Abilities.Add(new Ability(fromDb.name, fromDb.icon, fromDb.description, fromDb.color));
        else
            Abilities.Add(new Ability(abilityName, null, $"Автоматически добавлена способность '{abilityName}'", Color.white));

        Debug.Log($"[Candidate] {Name} получил способность '{abilityName}'");
    }

    public bool RemoveAbility(string abilityName)
    {
        if (Abilities == null || string.IsNullOrWhiteSpace(abilityName))
            return false;

        Ability ab = Abilities.FirstOrDefault(a => a != null && a.name != null && a.name.Equals(abilityName, System.StringComparison.OrdinalIgnoreCase));
        if (ab != null)
        {
            Abilities.Remove(ab);
            Debug.Log($"[Candidate] {Name} потерял способность '{abilityName}'");
            return true;
        }

        return false;
    }

    public void SendToPrison(int turns = 1)
    {
        PrisonTurnsLeft = Mathf.Max(PrisonTurnsLeft, turns);
        AddAbility("В тюрьме");
    }

    public void TickPrison()
    {
        if (PrisonTurnsLeft <= 0)
            return;

        PrisonTurnsLeft--;
        if (PrisonTurnsLeft <= 0)
        {
            PrisonTurnsLeft = 0;
            RemoveAbility("В тюрьме");
        }
    }

    public void ExecuteAbility(string abilityName)
    {
        if (Abilities == null || string.IsNullOrWhiteSpace(abilityName))
            return;

        Ability ab = Abilities.FirstOrDefault(a => a != null && a.name != null && a.name.Equals(abilityName, System.StringComparison.OrdinalIgnoreCase));
        if (ab != null)
            ab.Execute();
        else
            Debug.LogWarning($"[Candidate] {Name} не найдено действие '{abilityName}' для исполнения");
    }

    public void BindAbilitiesToActions()
    {
        if (Abilities == null)
            return;

        foreach (var ability in Abilities)
        {
            if (ability == null || string.IsNullOrEmpty(ability.name))
                continue;

            if (ClassDatabase.IsClass(ability.name))
            {
                ability.SetFunction(() => Debug.Log($"[Candidate] {Name} использует класс '{ability.name}'"));
                continue;
            }

            switch (ability.name)
            {
                case "Воровство":
                    ability.SetFunction(() => CandidateActions.Steal(this));
                    break;
                case "Лоббирование":
                    ability.SetFunction(() => CandidateActions.Lobby(this));
                    break;
                case "Образование":
                    ability.SetFunction(() => CandidateActions.MajorAppeal(this, 0));
                    break;
                case "Интриги":
                    ability.SetFunction(() => Debug.LogWarning("[Candidate] Интриги требуют кандидата-цели: используйте CandidateActions.Intrigue(actor, target)."));
                    break;
                case "Дебаты":
                    ability.SetFunction(() => Debug.LogWarning("[Candidate] Дебаты требуют оппонента: используйте CandidateActions.Debate(actor, opponent)."));
                    break;
            }
        }
    }

    private static string GenerateRandomBackground()
    {
        return "TODO";
    }

    public override string ToString()
    {
        var abilityNames = new List<string>();
        foreach (var ability in Abilities)
            abilityNames.Add(ability.name);
        
        return $"{Name}, {Age} лет — Влияние {Influence}, Интеллект {Intellect}, Деньги {Money}, Воля {Willpower}\n" +
               $"Умения: {string.Join(", ", abilityNames)}\n" +
               $"Бэкграунд: {Background}";
    }
}
