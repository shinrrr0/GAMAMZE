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

    public void StartNewGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
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
}