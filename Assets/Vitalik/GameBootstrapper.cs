using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    public AudioManager audioManagerPrefab;
    void Awake()
    {
        var audioManager = FindAnyObjectByType<AudioManager>();
        ServiceLocator.Register<IAudioService>(audioManager);
        // –еЇструЇмо серв≥с ан≥мац≥й
        ServiceLocator.Register<IAnimationService>(new AnimationManager());
        // –еЇстрац≥€ шини под≥й
        ServiceLocator.Register<IEventService>(new EventBus());
        Debug.Log("Animation Service Registered via Locator");
    }
}
