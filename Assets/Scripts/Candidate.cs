using System.Collections.Generic;
using UnityEngine;

// Делегат для функции механики способности
public delegate void AbilityFunction();

[System.Serializable]
public class Ability
{
    public string name;
    public Sprite icon;
    
    [TextArea(2, 4)]
    public string description;
    
    // Ссылка на функцию, которая исполняет механику способности
    // Хранится как строка имени метода для сериализации
    [SerializeField]
    private string functionName;
    
    // Кэшированная ссылка на функцию
    private AbilityFunction cachedFunction;
    
    public Ability()
    {
        name = "New Ability";
        icon = null;
        description = "";
        functionName = "";
        cachedFunction = null;
    }
    
    public Ability(string name, Sprite icon, string description)
    {
        this.name = name;
        this.icon = icon;
        this.description = description;
        this.functionName = "";
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
        var abilityNames = new[] 
        { 
            "Переговоры", "Лидерство", "Стратегия", "Экономика",
            "Дипломатия", "Интриги", "Медицина", "Техника",
            "Разведка", "Риторика", "Благосостояние", "Наука"
        };
        
        var abilities = new List<Ability>();
        
        // Берем 2 случайных способности
        for (int i = 0; i < 2; i++)
        {
            int randomIndex = Random.Range(0, abilityNames.Length);
            Ability newAbility = new Ability(
                abilityNames[randomIndex],
                null, // иконка будет заполнена позже или через инспектор
                "Описание способности будет добавлено" // описание
            );
            abilities.Add(newAbility);
        }
        
        return abilities;
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
