using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class StepTextManagerSimple : MonoBehaviour
{
    public Button nextButton;
    public GameObject textsParent; // Перетащи сюда объект "Texts" из иерархии
    
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    
    public string nextSceneName = "NextScene";
    public string startButtonText = "НАЧАТЬ ИГРУ";
    
    private List<TextMeshProUGUI> textComponents = new List<TextMeshProUGUI>();
    private int currentIndex = 0;
    private bool isCompleted = false;

    void Start()
    {
        if (nextButton == null) nextButton = GetComponentInChildren<Button>();
        nextButton.onClick.AddListener(OnNextButtonClick);

        // Собираем все текстовые компоненты из дочерних объектов
        if (textsParent != null)
        {
            foreach (Transform child in textsParent.transform)
            {
                TextMeshProUGUI t = child.GetComponent<TextMeshProUGUI>();
                if (t != null) textComponents.Add(t);
            }
        }

        if (textComponents.Count == 0)
        {
            Debug.LogError("Тексты не найдены! Проверь, что в объекте Texts есть дочерние объекты с TextMeshPro.");
            return;
        }

        // Инициализация
        for (int i = 0; i < textComponents.Count; i++)
        {
            if (i == 0)
            {
                textComponents[i].gameObject.SetActive(true);
                textComponents[i].color = activeColor;
            }
            else
            {
                textComponents[i].gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isCompleted)
        {
            AdvanceStep();
        }
    }

    void OnNextButtonClick()
    {
        if (!isCompleted) AdvanceStep();
        else SceneManager.LoadScene(nextSceneName);
    }

    void AdvanceStep()
    {
        if (currentIndex < textComponents.Count - 1)
        {
            textComponents[currentIndex].color = inactiveColor;
            currentIndex++;
            textComponents[currentIndex].gameObject.SetActive(true);
            textComponents[currentIndex].color = activeColor;
        }
        else if (!isCompleted)
        {
            ShowFinalState();
        }
    }

    void ShowFinalState()
    {
        foreach (var txt in textComponents)
        {
            txt.gameObject.SetActive(true);
            txt.color = activeColor;
        }
        
        isCompleted = true;

        TextMeshProUGUI btnTxt = nextButton.GetComponentInChildren<TextMeshProUGUI>();
        if (btnTxt != null) btnTxt.text = startButtonText;
    }
}