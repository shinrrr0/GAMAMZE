using UnityEngine;

[System.Serializable] // Позволяет видеть поля класса в инспекторе Unity
public class Crisis
{
    public string name;
    public Sprite icon;
    public Color color = new Color(0.8f, 0.2f, 0.8f, 1f); // Фиолетовый по умолчанию
    
    [TextArea(2, 4)]
    public string description;

    public Crisis()
    {
        name = "New Crisis";
        icon = null;
        color = new Color(0.8f, 0.2f, 0.8f, 1f);
        description = "";
    }

    public Crisis(string name, string description, Color color = default)
    {
        this.name = name;
        this.description = description;
        this.color = color != default ? color : new Color(0.8f, 0.2f, 0.8f, 1f);
        this.icon = null;
    }
}
