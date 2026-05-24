using UnityEngine;
using System;

public class AudioManager : MonoBehaviour, IAudioService
{
    // Масив звуків, який ти заповнюєш в Unity Editor
    public Sound[] sounds;

    // Створюємо AudioSource для музики при старті
    void Awake()
    {
        foreach (Sound s in sounds)
        {
            // Для музики ми створюємо джерела заздалегідь
            if (s.loop)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
            }
        }
    }

    // Реалізація методу для SFX (Кроки, удари)
    public void PlaySFX(string name, Vector3 position)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning($"Звук: {name} не знайдено!");
            return;
        }

        // --- ЛОГІКА 3D ЗВУКУ ---
        // PlayClipAtPoint створює тимчасовий об'єкт у точці 'position', грає звук і знищується.
        // Це ідеально для кроків, бо звук лишається там, де був зроблений крок.

        // Але PlayClipAtPoint не дає контролю пітчу, тому зробимо трохи хитріше:
        GameObject soundObj = new GameObject("TempSFX_" + name);
        soundObj.transform.position = position;

        AudioSource audioSource = soundObj.AddComponent<AudioSource>();
        audioSource.clip = s.clip;
        audioSource.volume = s.volume;
        audioSource.pitch = s.pitch;
        audioSource.spatialBlend = 1f; // 1.0 = Повністю 3D звук
        audioSource.minDistance = 2f;  // Дистанція, де звук найгучніший
        audioSource.maxDistance = 50f; // Дистанція, де звук зникає

        audioSource.Play();

        // Знищуємо об'єкт після того, як звук закінчиться
        Destroy(soundObj, s.clip.length / s.pitch);
    }

    public void PlayMusic(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) return;
        if (!s.source.isPlaying) s.source.Play();
    }

    public void StopMusic(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) return;
        s.source.Stop();
    }
}