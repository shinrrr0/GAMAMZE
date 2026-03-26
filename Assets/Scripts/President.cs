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

    private bool finalCrisisTriggered = false;

    private void Start()
    {
        CrisisDatabase.Initialize();
        UpdateUI();
    }

    public void NextTurn()
    {
        if (finalCrisisTriggered)
        {
            LogToText("[President] Финальный кризис уже запущен. Следующий ход недоступен.");
            return;
        }

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
        bool shouldTriggerFinalCrisis = hp <= 0;
        
        if (shouldTriggerFinalCrisis)
        {
            TriggerFinalCrisis();
        }
        else if (Random.Range(0, 101) <= 15 + (insanity * 2))
        {
            newCrisis = AddRandomCrisis();
        }

        PresidentTurnChange presidentChange = new PresidentTurnChange
        {
            hpBefore = hpBefore,
            hpAfter = hp,
            insanityBefore = insanityBefore,
            insanityAfter = insanity
        };

        LogToText($"Ход {turnCount}: Возраст {age}, HP {hp}, Безумие {insanity}, Кризисов {activeCrises.Count}");

        UpdateUI();

        if (turnSummaryPopup != null && !shouldTriggerFinalCrisis)
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

        if (crisisTooltip != null)
            crisisTooltip.ShowCrisis(randomCrisis);
        else
            Debug.LogWarning("[President] CrisisTooltip не привязан в инспекторе.");

        return randomCrisis;
    }

    private void TriggerFinalCrisis()
    {
        if (finalCrisisTriggered)
            return;

        finalCrisisTriggered = true;

        Crisis finalCrisis = CrisisDatabase.GetRandomCrisis();
        if (finalCrisis == null)
        {
            Debug.LogError("[President] Не удалось получить кризис для финального события.");
            return;
        }

        activeCrises.Add(finalCrisis);
        LogToText($"ФИНАЛЬНЫЙ КРИЗИС: {finalCrisis.name}");

        if (nextTurnButton != null)
            nextTurnButton.interactable = false;

        if (crisisTooltip == null)
        {
            Debug.LogWarning("[President] CrisisTooltip не привязан в инспекторе. Финальный кризис показан не будет.");
            return;
        }

        List<Candidate> candidates = candidateCardsController != null
            ? candidateCardsController.GetCandidates()
            : null;

        crisisTooltip.ShowCrisis(finalCrisis);
        crisisTooltip.ShowFinalCandidateMenu(candidates, OnFinalCandidateSelected);
    }

    private void OnFinalCandidateSelected(Candidate selectedCandidate)
    {
        if (selectedCandidate == null)
        {
            LogToText("[President] Финальный кризис завершился без выбранного кандидата.");
            FinishGame(false);
            return;
        }

        SkillCheckResult result = CandidateActions.ResolveFinalCrisisSkillCheck(selectedCandidate);
        bool playerWon = result.outcome != CheckOutcome.Fail;

        LogToText($"[President] Финальный скиллчек: {selectedCandidate.Name}, стат={result.statName}, бросок={result.selectedRoll}, сумма={result.totalValue}, исход={result.outcome}");

        if (crisisTooltip != null)
            crisisTooltip.ShowFinalSkillCheckResult(result, playerWon);

        FinishGame(playerWon);
    }

    private void FinishGame(bool playerWon)
    {
        string message = playerWon
            ? "Игра окончена: кандидат прошел финальный кризис. Игрок выиграл."
            : "Игра окончена: кандидат не прошел финальный кризис. Игрок проиграл.";

        LogToText(message);

        if (nextTurnButton != null)
            nextTurnButton.interactable = false;
    }

    private void UpdateUI()
    {
        if (turnCountText != null)
            turnCountText.text = "Ход: " + turnCount;
    }

    private void CheckGameOver()
    {
        if (hp <= 0 && !finalCrisisTriggered)
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