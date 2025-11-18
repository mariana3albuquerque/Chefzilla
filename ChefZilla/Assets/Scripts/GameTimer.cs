using System;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public static GameTimer I { get; private set; }

    [Min(1)] public int initialSeconds = 300;   // 5:00
    public bool startOnEnable = true;
    public bool useUnscaledTime = true;         // <- pode deixar true por padrão

    [Header("Música de fundo")]
    [SerializeField] BackgroundMusicController backgroundMusic;

    public bool Running { get; private set; }
    public float Remaining { get; private set; }
    public int RemainingSeconds => Mathf.CeilToInt(Mathf.Max(0f, Remaining));

    public event Action<int> OnTick;
    public event Action OnTimeUp;

    int lastBroadcast = -1;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;

        if (initialSeconds < 1) initialSeconds = 300;
        Remaining = initialSeconds;   // << já deixa 05:00 carregado
        lastBroadcast = -1;
    }

    void OnEnable()
    {
        if (startOnEnable)
        {
            ResetTimer();
            StartTimer();
        }
        else
        {
            // notifica a UI do valor atual mesmo sem iniciar
            NotifyTick();
        }
    }

    public void ResetTimer()
    {
        Remaining = Mathf.Max(1, initialSeconds);
        lastBroadcast = -1;
        NotifyTick();                 // << atualiza UI para 05:00
    }

    public void StartTimer() { Running = true; }
    public void Pause() { Running = false; }
    public void Resume() { Running = true; }

    void Update()
    {
        if (!Running || Remaining <= 0f) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        Remaining -= dt;
        if (Remaining < 0f) Remaining = 0f;

        // 🔊 avisa o controlador de música quanto tempo falta
        if (backgroundMusic != null)
            backgroundMusic.UpdateTimeRemaining(Remaining);

        int sec = RemainingSeconds;
        if (sec != lastBroadcast)
        {
            lastBroadcast = sec;
            OnTick?.Invoke(sec);
        }

        if (Remaining <= 0f)
        {
            Running = false;
            OnTimeUp?.Invoke();
        }
    }

    void NotifyTick() => OnTick?.Invoke(RemainingSeconds);

    public static string FormatMMSS(int seconds)
    {
        int m = Mathf.Max(0, seconds) / 60;
        int s = Mathf.Max(0, seconds) % 60;
        return $"{m:00}:{s:00}";
    }
}
