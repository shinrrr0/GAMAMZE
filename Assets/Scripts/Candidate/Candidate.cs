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

    /// <summary>
    /// Конструктор с поддержкой уникальных классов
    /// </summary>
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
        // Определяем пол случайно (true = женский, false = мужской)
        bool isFemale = Random.value > 0.5f;
        return GenerateRandomName(isFemale);
    }

    /// <summary>
    /// Генерирует имя и фамилию для персонажа с синхронизацией по полу
    /// </summary>
    private static string GenerateRandomName(bool isFemale)
    {
        // Мужские имена
        var maleFirstNames = new[] { "Алексей", "Иван", "Дмитрий", "Сергей", "Павел", "Николай", "Петр", "Михаил" };
        // Женские имена
        var femaleFirstNames = new[] { "Мария", "Ольга", "Наталья", "Екатерина", "Анна", "Елена", "Людмила", "Виктория" };

        // Мужские фамилии
        var maleLastNames = new[] { "Иванов", "Петров", "Смирнов", "Соколов", "Лебедев", "Новиков", "Морозов", "Сидоров" };
        // Женские фамилии (окончание -а/-ова/-ева вместо -о/-ов/-ев)
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
        // Инициализируем базы данных
        ClassDatabase.Initialize();
        AbilityDatabase.Initialize();
        
        List<Ability> abilities = new List<Ability>();
        
        // Первая способность - это класс персонажа (ровно 1)
        Ability playerClass = ClassDatabase.GetRandomClass();
        abilities.Add(new Ability(playerClass.name, playerClass.icon, playerClass.description, playerClass.color));
        
        // Остальные способности генерируются от 0 до 2 (на 1 меньше, чем было)
        int additionalAbilityCount = Random.Range(0, 3); // 0, 1 или 2
        
        List<Ability> randomAbilities = AbilityDatabase.GetRandomAbilities(additionalAbilityCount);
        abilities.AddRange(randomAbilities);
        
        return abilities;
    }

    /// <summary>
    /// Генерирует способности с учётом уже использованных классов (для уникальности)
    /// </summary>
    private static List<Ability> GenerateRandomAbilities(HashSet<string> usedClasses)
    {
        // Инициализируем базы данных
        ClassDatabase.Initialize();
        AbilityDatabase.Initialize();
        
        List<Ability> abilities = new List<Ability>();
        
        // Первая способность - уникальный класс персонажа
        Ability playerClass = ClassDatabase.GetRandomClassExcluding(usedClasses);
        abilities.Add(new Ability(playerClass.name, playerClass.icon, playerClass.description, playerClass.color));
        
        // Остальные способности генерируются от 0 до 2 (на 1 меньше, чем было)
        int additionalAbilityCount = Random.Range(0, 3); // 0, 1 или 2
        
        List<Ability> randomAbilities = AbilityDatabase.GetRandomAbilities(additionalAbilityCount);
        abilities.AddRange(randomAbilities);
        
        return abilities;
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

            // Классы персонажей не имеют прямых действий - это профессии/роли
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
