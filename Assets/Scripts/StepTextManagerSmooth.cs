using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StepTextManagerSimple : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject textsParent;

    [Header("Step setup")]
    [Tooltip("Сколько TMP-объектов составляют один логический шаг. В твоей сцене это 2: основной текст + его дубликат-тень.")]
    [SerializeField] private int textsPerStep = 2;

    [Tooltip("Если true — при показе текста всегда возвращается его исходный цвет из сцены.")]
    [SerializeField] private bool preserveOriginalColors = true;

    [Tooltip("Если true — менеджер только показывает/скрывает шаги и не трогает цвета вообще.")]
    [SerializeField] private bool neverTintTexts = true;

    [Header("Optional tint mode")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    [Header("Scene flow")]
    [SerializeField] private string nextSceneName = "NextScene";
    [SerializeField] private string startButtonText = "НАЧАТЬ ИГРУ";

    private readonly List<TextMeshProUGUI> textComponents = new List<TextMeshProUGUI>();
    private readonly List<Color> originalColors = new List<Color>();
    private readonly List<bool> originalActiveStates = new List<bool>();

    private int currentStep = 0;
    private int totalSteps = 0;
    private bool isCompleted = false;
    private bool isInitialized = false;

    private void Awake()
    {
        CacheReferences();
        CaptureOriginalState();
        InitializeVisibleState();
    }

    private void Start()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(OnNextButtonClick);
            nextButton.onClick.AddListener(OnNextButtonClick);
        }

        // Повторно закрепляем цвета после полного старта сцены.
        ReapplyVisibleState();
    }

    private void OnEnable()
    {
        if (isInitialized)
            ReapplyVisibleState();
    }

    private void OnDestroy()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnNextButtonClick);
    }

    private void Update()
    {
        if (!isCompleted && Input.GetKeyDown(KeyCode.Space))
            AdvanceStep();
    }

    private void CacheReferences()
    {
        if (nextButton == null)
            nextButton = GetComponentInChildren<Button>(true);

        if (textsParent == null)
        {
            Transform found = transform.Find("Texts");
            if (found != null)
                textsParent = found.gameObject;
        }

        textComponents.Clear();

        if (textsParent == null)
        {
            Debug.LogError("[StepTextManagerSimple] textsParent не назначен.", this);
            return;
        }

        foreach (Transform child in textsParent.transform)
        {
            TextMeshProUGUI tmp = child.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
                textComponents.Add(tmp);
        }

        if (textComponents.Count == 0)
            Debug.LogError("[StepTextManagerSimple] В объекте Texts не найдены дочерние TextMeshProUGUI.", this);
    }

    private void CaptureOriginalState()
    {
        originalColors.Clear();
        originalActiveStates.Clear();

        for (int i = 0; i < textComponents.Count; i++)
        {
            TextMeshProUGUI tmp = textComponents[i];
            originalColors.Add(tmp != null ? tmp.color : Color.white);
            originalActiveStates.Add(tmp != null && tmp.gameObject.activeSelf);
        }

        totalSteps = textsPerStep > 0
            ? Mathf.CeilToInt(textComponents.Count / (float)textsPerStep)
            : textComponents.Count;
    }

    private void InitializeVisibleState()
    {
        if (textComponents.Count == 0)
            return;

        currentStep = 0;
        isCompleted = false;
        isInitialized = true;

        for (int i = 0; i < textComponents.Count; i++)
        {
            bool shouldShow = IsIndexInStep(i, 0);
            SetTextVisible(i, shouldShow, shouldShowAsActive: shouldShow);
        }
    }

    private void ReapplyVisibleState()
    {
        if (!isInitialized)
            return;

        if (isCompleted)
        {
            for (int i = 0; i < textComponents.Count; i++)
                SetTextVisible(i, true, shouldShowAsActive: true);
            return;
        }

        for (int i = 0; i < textComponents.Count; i++)
        {
            bool shouldShow = IsIndexVisibleAtCurrentProgress(i);
            SetTextVisible(i, shouldShow, shouldShowAsActive: shouldShow);
        }
    }

    private void OnNextButtonClick()
    {
        if (!isCompleted)
            AdvanceStep();
        else
            SceneManager.LoadScene(nextSceneName);
    }

    private void AdvanceStep()
    {
        if (isCompleted)
            return;

        int nextStep = currentStep + 1;
        if (nextStep >= totalSteps)
        {
            ShowFinalState();
            return;
        }

        currentStep = nextStep;

        for (int i = 0; i < textComponents.Count; i++)
        {
            bool shouldShow = IsIndexVisibleAtCurrentProgress(i);
            SetTextVisible(i, shouldShow, shouldShowAsActive: shouldShow);
        }

        if (currentStep >= totalSteps - 1)
            ShowFinalState();
    }

    private void ShowFinalState()
    {
        for (int i = 0; i < textComponents.Count; i++)
            SetTextVisible(i, true, shouldShowAsActive: true);

        isCompleted = true;

        if (nextButton != null)
        {
            TextMeshProUGUI btnTxt = nextButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (btnTxt != null)
                btnTxt.text = startButtonText;
        }
    }

    private void SetTextVisible(int index, bool visible, bool shouldShowAsActive)
    {
        if (index < 0 || index >= textComponents.Count)
            return;

        TextMeshProUGUI tmp = textComponents[index];
        if (tmp == null)
            return;

        tmp.gameObject.SetActive(visible);

        if (!visible)
            return;

        if (neverTintTexts)
        {
            RestoreOriginalColor(index);
            return;
        }

        if (preserveOriginalColors && shouldShowAsActive)
        {
            RestoreOriginalColor(index);
            return;
        }

        tmp.color = shouldShowAsActive ? activeColor : inactiveColor;
    }

    private void RestoreOriginalColor(int index)
    {
        if (index < 0 || index >= textComponents.Count)
            return;

        TextMeshProUGUI tmp = textComponents[index];
        if (tmp == null)
            return;

        if (index < originalColors.Count)
            tmp.color = originalColors[index];

        tmp.ForceMeshUpdate();
    }

    private bool IsIndexInStep(int textIndex, int stepIndex)
    {
        if (textsPerStep <= 0)
            return textIndex == stepIndex;

        int start = stepIndex * textsPerStep;
        int endExclusive = start + textsPerStep;
        return textIndex >= start && textIndex < endExclusive;
    }

    private bool IsIndexVisibleAtCurrentProgress(int textIndex)
    {
        if (textsPerStep <= 0)
            return textIndex <= currentStep;

        int maxVisibleIndexExclusive = (currentStep + 1) * textsPerStep;
        return textIndex < maxVisibleIndexExclusive;
    }

    [ContextMenu("Rebuild Text Cache")]
    private void RebuildTextCache()
    {
        CacheReferences();
        CaptureOriginalState();
        InitializeVisibleState();
    }
}
