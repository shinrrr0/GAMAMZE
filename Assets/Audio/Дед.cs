using UnityEngine;


public class RandomButtonSound : MonoBehaviour
{
    public AudioSource audioSource;   // Источник звука
    public AudioClip[] sounds;        // Массив звуков

    public void PlayRandomSound()
    {
        if (sounds.Length == 0 || audioSource == null)
            return;

        // Останавливаем текущий звук
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Выбираем случайный звук
        int randomIndex = Random.Range(0, sounds.Length);

        // Назначаем и проигрываем
        audioSource.clip = sounds[randomIndex];
        audioSource.Play();


    }
}