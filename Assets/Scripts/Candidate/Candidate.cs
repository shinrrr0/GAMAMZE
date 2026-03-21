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
    public Color color = Color.red; // Цвет для отображения спрайта
    
    [TextArea(2, 4)]
    public string description;
    
    // Кэшированная ссылка на функцию
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
    
    /// <summary>
    /// Устанавливает функцию, которая будет вызываться при использовании способности
    /// </summary>
    public void SetFunction(AbilityFunction function)
    {
        cachedFunction = function;
    }
    
    /// <summary>
    /// Исполняет механику способности
    /// </summary>
    public void Execute()
    {
        if (cachedFunction != null)
        {
            cachedFunction.Invoke();
        }
        else
        {
            Debug.LogWarning($"[Ability] Функция для способности '{name}' не установлена!");
        }
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
    // Свойства
    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    public int Age
    {
        get { return age; }
        set { age = value; }
    }

    public int Influence
    {
        get { return influence; }
        set { influence = value; }
    }

    public int Intellect
    {
        get { return intellect; }
        set { intellect = value; }
    }

    public int Money
    {
        get { return money; }
        set { money = value; }
    }

    public int Willpower
    {
        get { return willpower; }
        set { willpower = value; }
    }

    public List<Ability> Abilities
    {
        get { return abilities; }
        set { abilities = value; }
    }

    public string Background
    {
        get { return background; }
        set { background = value; }
    }

    // конструктор
    public Candidate()
    {
        Name = GenerateRandomName();
        Age = Random.Range(35, 71);

        Influence = Random.Range(30, 101);
        Intellect = Random.Range(30, 101);
        Money = Random.Range(30, 101);
        Willpower = Random.Range(30, 101);

        Abilities = GenerateRandomAbilities();
        Background = GenerateRandomBackground();
    }

    // тут заданные данные
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
    }

    private static string GenerateRandomName()
    {
        var firstNames = new[] { "Алексей", "Мария", "Иван", "Ольга", "Дмитрий", "Наталья", "Сергей", "Екатерина", "Павел", "Анна" };
        var lastNames = new[] { "Иванов", "Петров", "Смирнов", "Кузнецова", "Соколов", "Попова", "Лебедев", "Козлова", "Новиков", "Морозова" };

        var first = firstNames[Random.Range(0, firstNames.Length)];
        var last = lastNames[Random.Range(0, lastNames.Length)];
        return first + " " + last;
    }

    private static List<Ability> GenerateRandomAbilities()
    {
        // Инициализируем базу данных способностей
        AbilityDatabase.Initialize();
        
        // Берем от 1 до 3 случайных способностей из базы данных
        int abilityCount = Random.Range(1, 4); // 1, 2 или 3
        
        return AbilityDatabase.GetRandomAbilities(abilityCount);
    }

    public bool NoCrisisNextTurn { get; set; }

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
        {
            Abilities.Add(new Ability(fromDb.name, fromDb.icon, fromDb.description, fromDb.color));
        }
        else
        {
            Abilities.Add(new Ability(abilityName, null, $"Автоматически добавлена способность '{abilityName}'", Color.white));
        }

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

    public void ExecuteAbility(string abilityName)
    {
        if (Abilities == null || string.IsNullOrWhiteSpace(abilityName))
            return;

        Ability ab = Abilities.FirstOrDefault(a => a != null && a.name != null && a.name.Equals(abilityName, System.StringComparison.OrdinalIgnoreCase));
        if (ab != null)
        {
            ab.Execute();
        }
        else
        {
            Debug.LogWarning($"[Candidate] {Name} не найдено действие '{abilityName}' для исполнения");
        }
    }

    public void BindAbilitiesToActions()
    {
        if (Abilities == null)
            return;

        foreach (var ability in Abilities)
        {
            if (ability == null || string.IsNullOrEmpty(ability.name))
                continue;

            switch (ability.name)
            {
                case "Воровство":
                    ability.SetFunction(() => CandidateActions.Steal(this));
                    break;
                case "Лобирование":
                    ability.SetFunction(() => CandidateActions.Lobby(this));
                    break;
                case "Обращение важное":
                    ability.SetFunction(() => CandidateActions.MajorAppeal(this, 0));
                    break;
                case "Интриги":
                    ability.SetFunction(() => Debug.LogWarning("[Candidate] Интриги требуют кандидата-цели: используйте CandidateActions.Intrigue(actor, target)."));
                    break;
                case "Дебаты":
                    ability.SetFunction(() => Debug.LogWarning("[Candidate] Дебаты требуют оппонента: используйте CandidateActions.Debate(actor, opponent)."));
                    break;
                default:
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
        {
            abilityNames.Add(ability.name);
        }
        
        return $"{Name}, {Age} лет — Влияние {Influence}, Интеллект {Intellect}, Деньги {Money}, Воля {Willpower}\n" +
               $"Умения: {string.Join(", ", abilityNames)}\n" +
               $"Бэкграунд: {Background}";
    }
}
