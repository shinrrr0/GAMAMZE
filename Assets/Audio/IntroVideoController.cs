using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroVideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName = "MainMenu";

    void Start()
    {
        videoPlayer.Play();
    }

    void Update()
    {
        // Если видео закончилось
        if (!videoPlayer.isPlaying && videoPlayer.frame > 0)
        {
            LoadNext();
        }

        // Пропуск
        if (Input.anyKeyDown)
        {
            LoadNext();
        }
    }

    void LoadNext()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}