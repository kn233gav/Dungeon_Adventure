using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;           // Ім'я, яке ти пишеш у PlayerController (напр. "Step_Human")
    public AudioClip clip;        // Сам аудіофайл

    [Range(0f, 1f)]
    public float volume = 0.7f;

    [Range(0.1f, 3f)]
    public float pitch = 1f;

    public bool loop;

    [HideInInspector]
    public AudioSource source;    // Джерело (використовується переважно для музики)
}