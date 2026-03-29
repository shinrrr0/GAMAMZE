using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Linq;

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
    private string exclusiveCrisisThisTurn;
    private bool forceHungerNextTurn;
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
        bool isInRedFlash = Time.time < redFlashEndTime;

        if (isInRedFlash)
        {
            actionSelectionStatusText.color = Color.red;
            return;
        }

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

    public void RequestExclusiveCrisisThisTurn(string crisisName)
    {
        if (string.IsNullOrWhiteSpace(crisisName))
            return;

        exclusiveCrisisThisTurn = crisisName;
    }

    public Crisis GetWorstCrisis(CrisisCategory category)
    {
        return activeCrises
            .Where(c => c != null && c.category == category)
            .OrderByDescending(c => c.percentPenalty > 0 ? 1 : 0)
            .ThenByDescending(c => c.percentPenalty)
            .ThenByDescending(c => c.hpPenalty)
            .ThenByDescending(c => c.otherPenalty)
            .ThenByDescending(c => c.turnsActive)
            .FirstOrDefault();
    }

    public bool RemoveActiveCrisis(Crisis crisis)
    {
        if (crisis == null)
            return false;

        bool removed = activeCrises.Remove(crisis);
        if (removed)
            LogToText($"Кризис устранён: {crisis.name}");
        return removed;
    }

    public void NextTurn()
    {
        if (finalCrisisTriggered)
        {
            LogToText("[President] Финальный кризис уже запущен. Следующий ход недоступен.");
            return;
        }

        if (candidateCardsController != null && !candidateCardsController.AreAllActionsSelected())
        {
            LogToText("[President] Ошибка: не все действия выбраны!");
            redFlashEndTime = Time.time + 0.5f;
            return;
        }

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

        ProcessExistingCrises();

        Crisis newCrisis = null;
        bool shouldTriggerFinalCrisis = hp <= 0;

        if (shouldTriggerFinalCrisis)
        {
            TriggerFinalCrisis();
            LastTurnHadNewCrisis = true;
        }
        else
        {
            bool noCrisisForced = ConsumeNoCrisisFlags();
            newCrisis = TryGenerateCrisis(noCrisisForced);
            LastTurnHadNewCrisis = newCrisis != null;
        }

        UpdateForcedHungerFlag();
        exclusiveCrisisThisTurn = null;

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

    private bool ConsumeNoCrisisFlags()
    {
        bool noCrisisForced = false;
        if (candidateCardsController == null)
            return false;

        foreach (Candidate candidate in candidateCardsController.GetCandidates())
        {
            if (candidate != null && candidate.NoCrisisNextTurn)
            {
                noCrisisForced = true;
                candidate.NoCrisisNextTurn = false;
            }
        }

        return noCrisisForced;
    }

    private void ProcessExistingCrises()
    {
        for (int i = activeCrises.Count - 1; i >= 0; i--)
        {
            Crisis crisis = activeCrises[i];
            if (crisis == null)
            {
                activeCrises.RemoveAt(i);
                continue;
            }

            crisis.AdvanceTurn();
            if (crisis.IsExpired())
            {
                LogToText($"Кризис завершился: {crisis.name}");
                activeCrises.RemoveAt(i);
            }
        }
    }

    private void UpdateForcedHungerFlag()
    {
        Crisis poorHarvest = activeCrises.FirstOrDefault(c => c != null && c.name == "Плохой урожай");
        forceHungerNextTurn = poorHarvest != null && poorHarvest.turnsActive > 2;
    }

    private Crisis TryGenerateCrisis(bool noCrisisForced)
    {
        if (!string.IsNullOrWhiteSpace(exclusiveCrisisThisTurn))
            return AddCrisisByName(exclusiveCrisisThisTurn);

        if (forceHungerNextTurn)
        {
            forceHungerNextTurn = false;
            return AddCrisisByName("Голод");
        }

        if (noCrisisForced)
            return null;

        int activeCount = activeCrises.Count;
        if (activeCount >= 5)
            return null;

        int crisisChance = 15 + (insanity * 2);
        if (activeCount >= 4)
            crisisChance -= 15;

        crisisChance = Mathf.Clamp(crisisChance, 0, 100);
        if (Random.Range(0, 101) > crisisChance)
            return null;

        return AddRandomCrisis();
    }

    private Crisis AddRandomCrisis()
    {
        HashSet<string> activeNames = new HashSet<string>(activeCrises.Where(c => c != null).Select(c => c.name));
        Crisis randomCrisis = CrisisDatabase.GetRandomCrisisExcluding(activeNames);

        if (randomCrisis == null)
        {
            Debug.LogWarning("[President] Нет доступных кризисов для случайного появления.");
            return null;
        }

        activeCrises.Add(randomCrisis);
        LogToText($"Новый кризис: {randomCrisis.name}");

        return randomCrisis;
    }

    private Crisis AddCrisisByName(string crisisName)
    {
        if (string.IsNullOrWhiteSpace(crisisName))
            return null;

        if (activeCrises.Any(c => c != null && c.name == crisisName))
        {
            LogToText($"Кризис '{crisisName}' уже активен, новый не появляется.");
            return null;
        }

        Crisis crisis = CrisisDatabase.CreateRuntimeCopyByName(crisisName);
        if (crisis == null)
        {
            Debug.LogWarning($"[President] Не найден кризис '{crisisName}'.");
            return null;
        }

        activeCrises.Add(crisis);
        LogToText($"Новый кризис: {crisis.name}");

        return crisis;
    }

    private void TriggerFinalCrisis()
    {
        if (finalCrisisTriggered)
            return;

        finalCrisisTriggered = true;

        Crisis finalCrisis = CrisisDatabase.GetRandomCrisisExcluding(new HashSet<string>());
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