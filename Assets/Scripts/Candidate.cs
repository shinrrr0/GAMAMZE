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
    private Sprite faceSprite;
    private Sprite clothesSprite;
    private Sprite headSprite;

    // Статические массивы спрайтов для генерации
    private static Sprite[] faces;
    private static Sprite[] clothesSprites;
    private static Sprite[] heads;
    private static bool loaded = false;

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

    public Sprite Face
    {
        get { return faceSprite; }
        set { faceSprite = value; }
    }

    public Sprite Clothes
    {
        get { return clothesSprite; }
        set { clothesSprite = value; }
    }

    public Sprite Head
    {
        get { return headSprite; }
        set { headSprite = value; }
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

        // Генерируем аватар
        GenerateAvatar();
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

        // Для заданных данных аватар не генерируется, оставляем null
        Face = null;
        Clothes = null;
        Head = null;
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
        return new List<string> { "TODO: способность 1", "TODO: способность 2" };
    }

    private static void LoadSprites()
    {
        if (loaded) return;

        faces = Resources.LoadAll<Sprite>("Sprites/faces");
        clothesSprites = Resources.LoadAll<Sprite>("Sprites/clothes");
        heads = Resources.LoadAll<Sprite>("Sprites/head");

        loaded = true;
    }

    private static string GenerateRandomBackground()
    {
        return "TODO: предыстория кандидата";
    }
    private void GenerateAvatar()
    {
        LoadSprites();

        if (faces != null && faces.Length > 0)
            face = faces[Random.Range(0, faces.Length)];

        if (clothesSprites != null && clothesSprites.Length > 0)
            clothes = clothesSprites[Random.Range(0, clothesSprites.Length)];

        if (heads != null && heads.Length > 0)
            head = heads[Random.Range(0, heads.Length)];
    }

    public override string ToString()
    {
        return $"{Name}, {Age} лет — Влияние {Influence}, Интеллект {Intellect}, Деньги {Money}, Воля {Willpower}\n" +
               $"Умения: {string.Join(", ", Abilities)}\n" +
               $"Бэкграунд: {Background}";
    }
}
