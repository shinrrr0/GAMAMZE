using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    private const string VolumePrefKey = "settings_volume";
    private const string ScreenModePrefKey = "settings_screen_mode";

    [Header("UI")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Dropdown screenModeDropdown;

    [Header("Defaults")]
    [SerializeField] [Range(0f, 1f)] private float defaultVolume = 1f;
    [SerializeField] private int defaultScreenModeIndex = 0;

    private bool isInitializing;

    // 0 = Fullscreen, 1 = Windowed
    private enum ScreenModeOption
    {
        Fullscreen = 0,
        Windowed = 1
    }

    private void Awake()
    {
        SetupUI();
        LoadSettings();
        ApplyLoadedValuesToUI();
        ApplyCurrentSettings();
        BindEvents();
    }

    private void SetupUI()
    {
        isInitializing = true;

        if (screenModeDropdown != null)
        {
            screenModeDropdown.ClearOptions();
            screenModeDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Полноэкранный",
                "Оконный"
            });
        }

        isInitializing = false;
    }

    private void BindEvents()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        if (screenModeDropdown != null)
            screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);

        if (screenModeDropdown != null)
            screenModeDropdown.onValueChanged.RemoveListener(OnScreenModeChanged);
    }

    private void LoadSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, defaultVolume);
        int savedScreenMode = PlayerPrefs.GetInt(ScreenModePrefKey, defaultScreenModeIndex);

        savedVolume = Mathf.Clamp01(savedVolume);
        savedScreenMode = Mathf.Clamp(savedScreenMode, 0, 1);

        if (volumeSlider != null)
            volumeSlider.value = savedVolume;

        if (screenModeDropdown != null)
            screenModeDropdown.value = savedScreenMode;
    }

    private void ApplyLoadedValuesToUI()
    {
        if (screenModeDropdown != null)
            screenModeDropdown.RefreshShownValue();
    }

    private void ApplyCurrentSettings()
    {
        float volume = volumeSlider != null ? volumeSlider.value : defaultVolume;
        int mode = screenModeDropdown != null ? screenModeDropdown.value : defaultScreenModeIndex;

        ApplyVolume(volume);
        ApplyScreenMode(mode);
    }

    private void OnVolumeChanged(float value)
    {
        if (isInitializing)
            return;

        ApplyVolume(value);
        PlayerPrefs.SetFloat(VolumePrefKey, value);
        PlayerPrefs.Save();
    }

    private void OnScreenModeChanged(int index)
    {
        if (isInitializing)
            return;

        ApplyScreenMode(index);
        PlayerPrefs.SetInt(ScreenModePrefKey, index);
        PlayerPrefs.Save();
    }

    private void ApplyVolume(float value)
    {
        AudioListener.volume = Mathf.Clamp01(value);
    }

    private void ApplyScreenMode(int index)
    {
        ScreenModeOption mode = (ScreenModeOption)Mathf.Clamp(index, 0, 1);

        switch (mode)
        {
            case ScreenModeOption.Fullscreen:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                Screen.fullScreen = true;
                break;

            case ScreenModeOption.Windowed:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                Screen.fullScreen = false;
                break;
        }
    }
}