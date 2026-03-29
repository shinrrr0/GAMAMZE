using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pauseMenuRoot;
    [SerializeField] private GameObject settingsMenuRoot;

    [Header("Optional")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused;

    public bool IsOpen => isPaused;

    private void Start()
    {
        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);

        if (settingsMenuRoot != null)
            settingsMenuRoot.SetActive(false);

        ResumeGameState();
    }

    private void Update()
    {
        if (!WasEscapePressedThisFrame())
            return;

        if (settingsMenuRoot != null && settingsMenuRoot.activeSelf)
        {
            CloseSettings();
            return;
        }

        TogglePauseMenu();
    }

    private bool WasEscapePressedThisFrame()
    {
        return Keyboard.current != null &&
               Keyboard.current.escapeKey.wasPressedThisFrame;
    }

    public void TogglePauseMenu()
    {
        if (isPaused)
            ContinueGame();
        else
            OpenPauseMenu();
    }

    public void OpenPauseMenu()
    {
        isPaused = true;

        if (settingsMenuRoot != null)
            settingsMenuRoot.SetActive(false);

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(true);

        PauseGameState();
    }

    public void ContinueGame()
    {
        isPaused = false;

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);

        if (settingsMenuRoot != null)
            settingsMenuRoot.SetActive(false);

        ResumeGameState();
    }

    public void OpenSettings()
    {
        isPaused = true;

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);

        if (settingsMenuRoot != null)
            settingsMenuRoot.SetActive(true);

        PauseGameState();
    }


    public void CloseSettings()
    {
        isPaused = true;

        if (settingsMenuRoot != null)
            settingsMenuRoot.SetActive(false);

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(true);

        PauseGameState();
    }

    public void RestartGame()
    {
        ResumeGameState();
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void QuitToMainMenu()
    {
        ResumeGameState();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitToDesktop()
    {
        ResumeGameState();
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void PauseGameState()
    {
        Time.timeScale = 0f;
        AudioListener.pause = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void ResumeGameState()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public AudioMixer mixer;

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            mixer.SetFloat("LowpassCutoff", 800f); // ����������
        }
        else
        {
            Time.timeScale = 1f;
            mixer.SetFloat("LowpassCutoff", 22000f); // ���� ����
        }
    }

}
