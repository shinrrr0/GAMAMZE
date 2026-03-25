using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject settingsMenuRoot;

    [Header("Scenes")]
    [SerializeField] private string firstGameSceneName = "GameScene";
    [SerializeField] private string entrySceneName = "Entry";

    private const string HasLaunchedBeforeKey = "HasLaunchedBefore";

    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (root != null)
            root.SetActive(true);

        if (settingsMenuRoot != null)
            settingsMenuRoot.SetActive(false);
    }

    private void Update()
    {
        if (settingsMenuRoot != null &&
            settingsMenuRoot.activeSelf &&
            Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseSettings();
        }
    }

    public void Entry()
    {
        MarkGameAsLaunched();
        SceneManager.LoadScene(entrySceneName);
    }

    public void StartNewGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (IsFirstLaunch())
        {
            MarkGameAsLaunched();
            SceneManager.LoadScene(entrySceneName);
            return;
        }

        SceneManager.LoadScene(firstGameSceneName);
    }

    public void OpenSettings()
    {
        if (root != null)
            root.SetActive(false);

        if (settingsMenuRoot != null)
            settingsMenuRoot.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsMenuRoot != null)
            settingsMenuRoot.SetActive(false);

        if (root != null)
            root.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private bool IsFirstLaunch()
    {
        return PlayerPrefs.GetInt(HasLaunchedBeforeKey, 0) == 0;
    }

    private void MarkGameAsLaunched()
    {
        PlayerPrefs.SetInt(HasLaunchedBeforeKey, 1);
        PlayerPrefs.Save();
    }
}