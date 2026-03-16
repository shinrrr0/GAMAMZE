using UnityEngine;
using UnityEngine.UI;

public class CustomGridQuarters : MonoBehaviour
{
    // Этот метод появится в меню при нажатии правой кнопкой на скрипт
    [ContextMenu("Arrange with Percentages")]
    public void Arrange()
    {
        // 1. УДАЛЯЕМ старые сетки, если они остались и мешают
        var oldLayout = GetComponent<LayoutGroup>();
        if (oldLayout != null) {
            Debug.Log("Удалите компонент " + oldLayout.GetType().Name + " вручную, он мешает!");
        }

        int childCount = transform.childCount;
        if (childCount == 0) return;

        // Настройки отступов в % (0.1 = 10%)
        float padTop = 0.10f;    
        float padBottom = 0.05f; 
        float padSide = 0.05f;   
        float spacing = 0.05f;   

        // Расчет размеров одной карточки
        float totalWidth = 1f - (padSide * 2);
        float totalHeight = 1f - padTop - padBottom;
        
        float childW = (totalWidth - spacing) / 2f;
        float childH = (totalHeight - spacing) / 2f;

        for (int i = 0; i < childCount; i++)
        {
            // Берем только первые 4 объекта
            if (i >= 4) break; 

            RectTransform rect = transform.GetChild(i) as RectTransform;
            if (rect == null) continue;

            int col = i % 2;      // 0 или 1
            int row = 1 - (i / 2); // 1 или 0 (верхний ряд первый)

            // Математика якорей
            float xMin = padSide + col * (childW + spacing);
            float xMax = xMin + childW;
            float yMin = padBottom + row * (childH + spacing);
            float yMax = yMin + childH;

            // ПРИНУДИТЕЛЬНАЯ УСТАНОВКА
            rect.anchorMin = new Vector2(xMin, yMin);
            rect.anchorMax = new Vector2(xMax, yMax);
            rect.offsetMin = Vector2.zero; // Сброс отступов в 0
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);

            Debug.Log($"Объект {rect.name} расставлен!");
        }
    }

    // Чтобы срабатывало сразу при запуске игры
    void Start() { Arrange(); }
}
