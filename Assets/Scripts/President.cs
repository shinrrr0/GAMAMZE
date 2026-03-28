using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class President : MonoBehaviour
{
    public static President Current { get; private set; }

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

    private TextMeshProUGUI actionSelectionStatusText;
    private bool finalCrisisTriggered = false;
    private float redFlashEndTime = 0f;
    public bool LastTurnHadNewCrisis { get; private set; }

    public CandidateCardsController CandidateController => candidateCardsController;

    private void Awake()
    {
        Current = this;
    }

    private void Start()
    {
        CrisisDatabase.Initialize();
        UpdateUI();
        
        // Find Status Text element (should be in the same Grid container as CandidateCardsController)
        if (candidateCardsController != null)
        {
            Transform gridContainer = candidateCardsController.transform.parent;
            if (gridContainer != null)
            {
                Transform statusTextTrans = gridContainer.Find("Status Text");
                if (statusTextTrans != null)
                    actionSelectionStatusText = statusTextTrans.GetComponent<TextMeshProUGUI>();
            }
        }
        
        if (actionSelectionStatusText == null)
        {
            // Fallback: try to find it in the entire scene
            actionSelectionStatusText = FindObjectOfType<TextMeshProUGUI>();
            foreach (var txt in FindObjectsOfType<TextMeshProUGUI>())
            {
                if (txt.gameObject.name == "Status Text")
                {
                    actionSelectionStatusText = txt;
                    break;
                }
            }
        }
        
        UpdateActionSelectionStatus();
    }

    private void Update()
    {
        UpdateActionSelectionStatus();
    }

    private void UpdateActionSelectionStatus()
    {
        if (actionSelectionStatusText == null || candidateCardsController == null)
            return;

        bool allSelected = candidateCardsController.AreAllActionsSelected();
        
        // Проверяем, находимся ли мы в красной вспышке
        bool isInRedFlash = Time.time < redFlashEndTime;
        
        if (isInRedFlash)
        {
            // Красный свет - не выполняем обновление
            actionSelectionStatusText.color = Color.red;
            return;
        }
        
        // Обычное отображение
        if (allSelected)
        {
            actionSelectionStatusText.text = "";
            actionSelectionStatusText.color = Color.white;
        }
        else
        {
            actionSelectionStatusText.text = "выберите действия для всех кандидатов";
            actionSelectionStatusText.color = Color.white;
        }
    }

    public void NextTurn()
    {
        if (finalCrisisTriggered)
        {
            LogToText("[President] Финальный кризис уже запущен. Следующий ход недоступен.");
            return;
        }

        // Check if all actions are selected
        if (candidateCardsController != null && !candidateCardsController.AreAllActionsSelected())
        {
            LogToText("[President] Ошибка: не все действия выбраны!");
            
            // Activate red flash for 0.5 seconds
            redFlashEndTime = Time.time + 0.5f;
            
            return;
        }

        // Reset red flash timer if we got here
        redFlashEndTime = 0f;

        int hpBefore = hp;
        int insanityBefore = insanity;
        LastTurnHadNewCrisis = false;

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
            LastTurnHadNewCrisis = true;
        }
        else
        {
            bool noCrisisForced = false;
            if (candidateCardsController != null)
            {
                foreach (Candidate candidate in candidateCardsController.GetCandidates())
                {
                    if (candidate != null && candidate.NoCrisisNextTurn)
                    {
                        noCrisisForced = true;
                        candidate.NoCrisisNextTurn = false;
                    }
                }
            }

            if (!noCrisisForced && Random.Range(0, 101) <= 15 + (insanity * 2))
            {
                newCrisis = AddRandomCrisis();
                LastTurnHadNewCrisis = newCrisis != null;
            }
        }

        if (candidateCardsController != null)
            candidateCardsController.ResolveEndOfTurnEffects(LastTurnHadNewCrisis);

        PresidentTurnChange presidentChange = new PresidentTurnChange
        {
            hpBefore = hpBefore,
            hpAfter = hp,
            insanityBefore = insanityBefore,
            insanityAfter = insanity
        };

        LogToText($"Ход {turnCount}: Возраст {age}, HP {hp}, Безумие {insanity}, Кризисов {activeCrises.Count}");

        UpdateUI();
        UpdateActionSelectionStatus();

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
            return;

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
