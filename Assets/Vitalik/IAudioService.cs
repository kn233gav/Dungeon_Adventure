using UnityEngine;

public interface IAudioService
{
    // Метод для звукових ефектів (кроки, удари), які мають позицію у просторі
    void PlaySFX(string name, Vector3 position);

    // Метод для музики або 2D звуків (інтерфейс, меню)
    void PlayMusic(string name);

    // Метод для зупинки музики (опціонально)
    void StopMusic(string name);
}