using UnityEngine;

public class MusicPlayerManual : MonoBehaviour
{
    public AudioSource introSource;
    public AudioSource loopSource;

    public AudioClip introClip;
    public AudioClip loopClip;

    [Tooltip("Длина интро в секундах (вручную)")]
    public float introDuration = 5f;

    void Start()
    {
        double startTime = AudioSettings.dspTime;

        // Запускаем интро
        introSource.clip = introClip;
        introSource.loop = false;
        introSource.PlayScheduled(startTime);

        // Запускаем луп после заданного времени
        loopSource.clip = loopClip;
        loopSource.loop = true;
        loopSource.PlayScheduled(startTime + introDuration);
    }
}