using UnityEngine;

public enum CrisisCategory
{
    None,
    Social,
    Economic
}

[System.Serializable]
public class Crisis
{
    public string name;
    public Sprite icon;
    public Color color = new Color(0.8f, 0.2f, 0.8f, 1f);

    [TextArea(2, 4)]
    public string description;

    [Header("Meta")]
    public CrisisCategory category = CrisisCategory.None;
    public bool canAppearRandomly = true;
    public int durationTurns = 0; // 0 = бессрочно

    [Header("Severity")]
    public int percentPenalty;
    public int hpPenalty;
    public int otherPenalty;

    [Header("Runtime")]
    public int remainingTurns;
    public int turnsActive;

    public bool IsTemporary => durationTurns > 0;

    public Crisis()
    {
        name = "New Crisis";
        icon = null;
        color = new Color(0.8f, 0.2f, 0.8f, 1f);
        description = "";
        category = CrisisCategory.None;
        canAppearRandomly = true;
        durationTurns = 0;
        percentPenalty = 0;
        hpPenalty = 0;
        otherPenalty = 0;
        remainingTurns = 0;
        turnsActive = 0;
    }

    public Crisis(
        string name,
        string description,
        Color color = default,
        CrisisCategory category = CrisisCategory.None,
        bool canAppearRandomly = true,
        int durationTurns = 0,
        int percentPenalty = 0,
        int hpPenalty = 0,
        int otherPenalty = 0)
    {
        this.name = name;
        this.description = description;
        this.color = color != default ? color : new Color(0.8f, 0.2f, 0.8f, 1f);
        this.icon = null;
        this.category = category;
        this.canAppearRandomly = canAppearRandomly;
        this.durationTurns = Mathf.Max(0, durationTurns);
        this.percentPenalty = Mathf.Max(0, percentPenalty);
        this.hpPenalty = Mathf.Max(0, hpPenalty);
        this.otherPenalty = Mathf.Max(0, otherPenalty);
        this.remainingTurns = this.durationTurns;
        this.turnsActive = 0;
    }

    public Crisis CreateRuntimeCopy()
    {
        return new Crisis(name, description, color, category, canAppearRandomly, durationTurns, percentPenalty, hpPenalty, otherPenalty)
        {
            icon = icon,
            remainingTurns = durationTurns,
            turnsActive = 0
        };
    }

    public void AdvanceTurn()
    {
        turnsActive++;

        if (IsTemporary && remainingTurns > 0)
            remainingTurns--;
    }

    public bool IsExpired()
    {
        return IsTemporary && remainingTurns <= 0;
    }
}
