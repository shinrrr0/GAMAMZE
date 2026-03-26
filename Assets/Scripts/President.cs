using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class President : MonoBehaviour
{
    public int hp = 30;
    public int insanity = 80;
    public int age = 40;
    public int turnCount = 1;

    public TextMeshProUGUI turnCountText;
    public Button nextTurnButton;

    public List<Crisis> activeCrises = new List<Crisis>();

    [Header("UI")]
    [SerializeField] private CrisisTooltip crisisTooltip;
    [SerializeField] private TurnSummaryPopup turnSummaryPopup;

    [Header("Candidates")]
    [SerializeField] private CandidateCardsController candidateCardsController;

    private void Start()
    {
        CrisisDatabase.Initialize();
        UpdateUI();
    }

    public void NextTurn()
    {
        int hpBefore = hp;
        int insanityBefore = insanity;

        List<CandidateTurnChange> candidateChanges = new List<CandidateTurnChange>();

        if (candidateCardsController != null)
            candidateChanges = candidateCardsController.ExecuteAllActions();

        turnCount++;
        age += 5;

        if (Random.Range(0, 101) <= age)
        {
            hp -= 1;
            LogToText("Здоровье упало из-за возраста.");
        }

        insanity += 1;

        Crisis newCrisis = null;
        if (Random.Range(0, 101) <= 15 + (insanity * 2))
            newCrisis = AddRandomCrisis();

        PresidentTurnChange presidentChange = new PresidentTurnChange
        {
            hpBefore = hpBefore,
            hpAfter = hp,
            insanityBefore = insanityBefore,
            insanityAfter = insanity
        };

        LogToText($"Ход {turnCount}: Возраст {age}, HP {hp}, Безумие {insanity}, Кризисов {activeCrises.Count}");

        UpdateUI();

        if (turnSummaryPopup != null)
            turnSummaryPopup.ShowSummary(turnCount, presidentChange, candidateChanges, newCrisis);

        CheckGameOver();
    }

    private Crisis AddRandomCrisis()
    {
        Crisis randomCrisis = CrisisDatabase.GetRandomCrisis();

        if (randomCrisis == null)
        {
            Debug.LogError("[President] CrisisDatabase вернул null!");
            return null;
        }

        activeCrises.Add(randomCrisis);
        LogToText($"Новый кризис: {randomCrisis.name}");

        return randomCrisis;
    }

    private void UpdateUI()
    {
        if (turnCountText != null)
            turnCountText.text = "Ход: " + turnCount;
    }

    private void CheckGameOver()
    {
        if (hp <= 0)
        {
            LogToText("Игра окончена: Президент скончался.");

            if (nextTurnButton != null)
                nextTurnButton.interactable = false;
        }
    }

    private void LogToText(string message)
    {
        Debug.Log(message);
        System.IO.File.AppendAllText(Application.dataPath + "/game_log.txt", message + "\n");
    }
}