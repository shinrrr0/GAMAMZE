using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class CustomGridQuarters : MonoBehaviour
{
    [Header("Настройка сетки (в процентах 0..1)")]
    public float padTop = 0.10f;    // Высота панели президента
    public float padBottom = 0.05f; // Отступ снизу
    public float padSide = 0.05f;   // Отступ СЛЕВА и СПРАВА (общий для всех)
    public float spacing = 0.02f;   // Расстояние между картами

    [ContextMenu("Arrange All")]
    public void Arrange()
    {
        ArrangePresident();
        ArrangeCards();
    }

    private void ArrangePresident()
    {
        Transform presTrans = transform.Find("President");
        if (presTrans == null) return;

        RectTransform rect = presTrans as RectTransform;
        
        // Теперь xMin = padSide, а xMax = 1 - padSide
        // Это создаст такие же "поля" по бокам, как у карточек
        rect.anchorMin = new Vector2(padSide, 1f - padTop);
        rect.anchorMax = new Vector2(1f - padSide, 1f);
        
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void ArrangeCards()
    {
        // Расчет доступной высоты для карт (с учетом отступа сверху под президента)
        float totalWidth = 1f - (padSide * 2);
        float totalHeight = 1f - padTop - padBottom - spacing; // spacing отделяет президента от карт

        float childW = (totalWidth - spacing) / 2f;
        float childH = (totalHeight - spacing) / 2f;

        int cardIndex = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform rect = transform.GetChild(i) as RectTransform;
            if (rect == null || rect.name == "President") continue;
            if (cardIndex >= 4) break;

            int col = cardIndex % 2;
            int row = 1 - (cardIndex / 2); // 1 - верхний ряд, 0 - нижний

            float xMin = padSide + col * (childW + spacing);
            float xMax = xMin + childW;
            
            // y рассчитывается от низа, поэтому ряд 1 будет под президентом
            float yMin = padBottom + row * (childH + spacing);
            float yMax = yMin + childH;

            rect.anchorMin = new Vector2(xMin, yMin);
            rect.anchorMax = new Vector2(xMax, yMax);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            cardIndex++;
        }
    }

    void Start() { Arrange(); }
}
