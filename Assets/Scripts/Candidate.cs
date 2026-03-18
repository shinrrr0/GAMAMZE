using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Candidate
{
    private string name;
    private int age;
    private int influence;
    private int intellect;
    private int money;
    private int willpower;
    private List<string> abilities;
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

    public List<string> Abilities
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
    public Candidate(string name, int age, int influence, int intellect, int money, int willpower, List<string> abilities, string background)
    {
        Name = name;
        Age = age;
        Influence = influence;
        Intellect = intellect;
        Money = money;
        Willpower = willpower;
        Abilities = abilities ?? new List<string>();
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

    private static List<string> GenerateRandomAbilities()
    {
        var allAbilities = new[] 
        { 
            "Переговоры", "Лидерство", "Стратегия", "Экономика",
            "Дипломатия", "Интриги", "Медицина", "Техника",
            "Разведка", "Риторика", "Благосостояние", "Наука"
        };
        
        var abilities = new List<string>();
        
        // Берем 2 случайных способности
        for (int i = 0; i < 2; i++)
        {
            int randomIndex = Random.Range(0, allAbilities.Length);
            abilities.Add(allAbilities[randomIndex]);
        }
        
        return abilities;
    }

    private static string GenerateRandomBackground()
    {
        return "TODO";
    }

    public override string ToString()
    {
        return $"{Name}, {Age} лет — Влияние {Influence}, Интеллект {Intellect}, Деньги {Money}, Воля {Willpower}\n" +
               $"Умения: {string.Join(", ", Abilities)}\n" +
               $"Бэкграунд: {Background}";
    }
}
