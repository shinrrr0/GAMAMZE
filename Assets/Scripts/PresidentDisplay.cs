using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PresidentDisplay : MonoBehaviour
{
    public President presidentData; 

    [Header("UI Text")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI insanityText;
    public TextMeshProUGUI ageText;
    public TextMeshProUGUI turnText; 

    [Header("UI Crisises")]
    public Image[] crisisImages;

    void Update()
    {
        if (presidentData == null) return;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (hpText == null || insanityText == null || ageText == null || turnText == null) 
        {
            Debug.LogWarning("hp, insanity, age or turn not specified");
            return;
        }

    hpText.text = $"HP: {presidentData.hp}";

        hpText.text = $"HP: {presidentData.hp}";
        insanityText.text = $"Insanity: {presidentData.insanity}";
        ageText.text = $"Age: {presidentData.age}";
        turnText.text = $"Turn: {presidentData.turnCount}";

        // Цвета (лучше задать один раз в Start, но для теста пойдет)
        hpText.color = Color.green;
        insanityText.color = Color.magenta;
        ageText.color = Color.cyan;
        turnText.color = Color.yellow; 

        for (int i = 0; i < crisisImages.Length; i++)
        {
            // Проверяем, есть ли кризис для этой иконки
            if (i < presidentData.activeCrises.Count)
            {
                Crisis currentCrisis = presidentData.activeCrises[i];
                
                // Проверяем, не пустой ли кризис и есть ли у него иконка
                if (currentCrisis != null && currentCrisis.icon != null)
                {
                    crisisImages[i].gameObject.SetActive(true);
                    crisisImages[i].sprite = currentCrisis.icon;
                }
                else
                {
                    // Если данных нет, скрываем иконку
                    crisisImages[i].gameObject.SetActive(false);
                }
            }
            else
            {
                crisisImages[i].gameObject.SetActive(false);
            }
        }
    }
}
