using UnityEngine;

public class SeamlessMusicPlayer : MonoBehaviour
{
    public AudioSource introSource;
    public AudioSource loopSource;

    public AudioClip introClip;
    public AudioClip loopClip;

    public float startDelay = 0.2f; // твоя задержка

    void Start()
    {
        introSource.clip = introClip;
        loopSource.clip = loopClip;
        loopSource.loop = true;

        double dspTime = AudioSettings.dspTime;


        double startTime = dspTime + startDelay;


        introSource.PlayScheduled(startTime);


        double introDuration = (double)introClip.samples / introClip.frequency;

        double loopStartTime = startTime + introDuration;

        loopSource.PlayScheduled(loopStartTime);
    }
}