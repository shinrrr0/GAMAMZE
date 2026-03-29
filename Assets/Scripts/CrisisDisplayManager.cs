using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  // Add this line for Button
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class CrisisDisplayManager : MonoBehaviour
{
    public TextMeshProUGUI crisisName;
    public TextMeshProUGUI crisisDescription;
    public Button nextCrisisButton;

    private int currentCrisisIndex = 0;

    public static CrisisDisplayManager Instance { get; private set; }
    private List<Crisis> allCrises = new List<Crisis>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        // Устанавливаем текущий кризис
        if (allCrises.Count > 0 && currentCrisisIndex < allCrises.Count)
        {
            UpdateDisplay(allCrises[currentCrisisIndex]);
            CheckForNext();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "CrisisOverviewScene")
        {
            President president = FindObjectOfType<President>();
            if (president != null)
            {
                // Храним все кризисы для последовательного отображения
                allCrises = president.activeCrises.ToList();
                currentCrisisIndex = 0;
                UpdateDisplay(allCrises[currentCrisisIndex]);
                CheckForNext();
            }
        }
    }

    void UpdateDisplay(Crisis crisis)
    {
        if (crisis != null && crisisName != null && crisisDescription != null)
        {
            crisisName.text = crisis.name;
            crisisDescription.text = crisis.description;
        }
    }

    public void ShowNextCrisis()
    {
        if (currentCrisisIndex < allCrises.Count - 1)
        {
            currentCrisisIndex++;
            UpdateDisplay(allCrises[currentCrisisIndex]);
            CheckForNext();
        }
        else
        {
            // Кризисы закончились, возвращаемся в главную сцену
            SceneManager.LoadScene("MainScene");
        }
    }

    void CheckForNext()
    {
        if (nextCrisisButton != null)
        {
            nextCrisisButton.interactable = (currentCrisisIndex < allCrises.Count - 1);
        }
    }
}