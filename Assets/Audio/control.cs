using UnityEngine;

public class AudioVolumeControl : MonoBehaviour
{
    public AudioSource mainSource;   // тот, который нужно приглушать
    public AudioSource triggerSource; // тот, при запуске которого глушим

    private float normalVolume;

    void Start()
    {
        // запоминаем исходную громкость
        normalVolume = mainSource.volume;
    }

    void Update()
    {
        if (triggerSource.isPlaying)
        {
            mainSource.volume = 0.05f;
        }
        else
        {
            mainSource.volume = normalVolume;
        }
    }
}