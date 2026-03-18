using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuBuilder : MonoBehaviour
{
    [ContextMenu("Build Pause Menu UI")]
    public void BuildPauseMenuUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (FindObjectOfType<PauseMenu>() != null)
        {
            Debug.LogWarning("PauseMenu уже существует на сцене.");
            return;
        }

        GameObject root = new GameObject("PauseMenuController");
        PauseMenu pauseMenu = root.AddComponent<PauseMenu>();

        GameObject overlay = CreateUIObject("PauseMenuRoot", canvas.transform);
        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.35f);

        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        StretchFullScreen(overlayRect);

        GameObject panel = CreateUIObject("Panel", overlay.transform);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(480f, 360f);
        panelRect.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 12f;
        layout.padding = new RectOffset(30, 30, 30, 30);
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = panel.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        GameObject title = CreateText("Title", panel.transform, "Пауза", 32);
        LayoutElement titleLayout = title.AddComponent<LayoutElement>();
        titleLayout.minHeight = 50f;

        CreateButton(panel.transform, "Продолжить", pauseMenu.ContinueGame);
        CreateButton(panel.transform, "Настройки", pauseMenu.OpenSettings);
        CreateButton(panel.transform, "Начать заново", pauseMenu.RestartGame);
        CreateButton(panel.transform, "Выйти в главное меню", pauseMenu.QuitToMainMenu);
        CreateButton(panel.transform, "Выйти на рабочий стол", pauseMenu.QuitToDesktop);

        GameObject settingsWindow = CreateUIObject("SettingsWindow", overlay.transform);
        Image settingsImage = settingsWindow.AddComponent<Image>();
        settingsImage.color = new Color(0.08f, 0.08f, 0.08f, 0.98f);

        RectTransform settingsRect = settingsWindow.GetComponent<RectTransform>();
        settingsRect.anchorMin = new Vector2(0.5f, 0.5f);
        settingsRect.anchorMax = new Vector2(0.5f, 0.5f);
        settingsRect.pivot = new Vector2(0.5f, 0.5f);
        settingsRect.sizeDelta = new Vector2(560f, 420f);
        settingsRect.anchoredPosition = Vector2.zero;

        CreateText("SettingsTitle", settingsWindow.transform, "Настройки", 30).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 160);
        CreateText("SettingsStub", settingsWindow.transform, "Окно настроек-заглушка", 22).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 40);

        GameObject closeSettingsButton = CreateButton(settingsWindow.transform, "Закрыть", pauseMenu.CloseSettings);
        RectTransform closeButtonRect = closeSettingsButton.GetComponent<RectTransform>();
        closeButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
        closeButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
        closeButtonRect.pivot = new Vector2(0.5f, 0.5f);
        closeButtonRect.sizeDelta = new Vector2(260f, 50f);
        closeButtonRect.anchoredPosition = new Vector2(0, -140);

        settingsWindow.SetActive(false);
        overlay.SetActive(false);

        SetPrivateField(pauseMenu, "pauseMenuRoot", overlay);
        SetPrivateField(pauseMenu, "settingsWindow", settingsWindow);

        Debug.Log("Pause Menu UI создан.");
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void StretchFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    private static GameObject CreateText(string name, Transform parent, string textValue, float fontSize)
    {
        GameObject textGO = CreateUIObject(name, parent);

        RectTransform rect = textGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(420f, 50f);

        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = textValue;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        return textGO;
    }

    private static GameObject CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonGO = CreateUIObject(label + "Button", parent);

        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.22f, 0.22f, 0.22f, 1f);

        Button button = buttonGO.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        RectTransform rect = buttonGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(380f, 50f);

        GameObject textGO = CreateUIObject("Text", buttonGO.transform);
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        StretchFullScreen(textRect);

        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        LayoutElement layout = buttonGO.AddComponent<LayoutElement>();
        layout.minHeight = 50f;
        layout.preferredHeight = 50f;

        return buttonGO;
    }

    private static void SetPrivateField(PauseMenu menu, string fieldName, GameObject value)
    {
        var field = typeof(PauseMenu).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            field.SetValue(menu, value);
    }
}