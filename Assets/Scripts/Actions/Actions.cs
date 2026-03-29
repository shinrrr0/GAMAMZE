using System;
using UnityEngine;

[Serializable]
public class GameAction
{
    public string name;

    [TextArea(2, 4)]
    public string description;

    // Ссылка на функцию: первый параметр — тот кто действует, второй — цель
    public Action<Candidate, Candidate> execute;

    public GameAction() { }

    public GameAction(string name, string description, Action<Candidate, Candidate> execute)
    {
        this.name = name;
        this.description = description;
        this.execute = execute;
    }

    public void Execute(Candidate actor, Candidate target)
    {
        if (execute != null)
            execute.Invoke(actor, target);
        else
            Debug.LogWarning($"[GameAction] Функция для действия '{name}' не установлена!");
    }
}
