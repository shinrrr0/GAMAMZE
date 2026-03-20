using UnityEngine;
using System.Collections.Generic;
using TMPro; // Добавьте это, если используете TextMeshPro
using UnityEngine.UI; // А это, если используете обычный UI Text

public class President : MonoBehaviour
{
    public int hp = 30;
    public int insanity = 80;
    public int age = 40;
    public int turnCount = 1;

    // Ссылка на текстовое поле в Unity
    public TextMeshProUGUI turnCountText; 
    // Ссылка на кнопку, чтобы её выключить в конце игры
    public Button nextTurnButton; 

    // Было: public List<string> activeCrises;
    // Нужно:
    public List<Crisis> activeCrises = new List<Crisis>(); 


    void Start()
    {
        UpdateUI(); // Обновляем текст при старте игры
    }

    public void NextTurn()
    {
        turnCount++;
        age += 5;

        if (Random.Range(0, 101) <= age)
        {
            hp -= 1;
            LogToText("Здоровье упало из-за возраста.");
        }

        insanity += 1;
        
        // 25% шанс добавить новый кризис
        if (Random.Range(0, 101) <= 25)
        {
            AddRandomCrisis();
        }

        LogToText($"Ход {turnCount}: Возраст {age}, HP {hp}, Безумие {insanity}, Кризисов {activeCrises.Count}");
        
        UpdateUI(); // Обновляем цифры на экране
        CheckGameOver(); // Проверяем, не умер ли президент
    }
    
    private void AddRandomCrisis()
    {
        // Используем CrisisDatabase для получения случайного кризиса
        Crisis randomCrisis = CrisisDatabase.GetRandomCrisis();
        
        if (randomCrisis != null)
        {
            activeCrises.Add(randomCrisis);
            LogToText($"Новый кризис: {randomCrisis.name}");
        }
        else
        {
            Debug.LogError("[President] CrisisDatabase вернул null!");
        }
    }

    void UpdateUI()
    {
        if (turnCountText != null)
            turnCountText.text = "Ход: " + turnCount;
    }

    void CheckGameOver()
    {
        if (hp <= 0)
        {
            LogToText("Игра окончена: Президент скончался.");
            if (nextTurnButton != null)
                nextTurnButton.interactable = false; // Блокируем кнопку
            
            // Здесь можно вызвать загрузку сцены меню или показать панель Game Over
        }
    }

    private void LogToText(string message)
    {
        Debug.Log(message);
        System.IO.File.AppendAllText(Application.dataPath + "/game_log.txt", message + "\n");
    }
}
