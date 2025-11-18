using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class ScoreAndCountdownTMP : MonoBehaviour
{
    [Header("Refs")]
    public TMP_Text label;                // Detecta automaticamente se vazio

    [Header("Timer")]
    [Min(1)] public int startSeconds = 300;   // 5:00
    public bool autoStart = true;
    public bool useUnscaledTime = true;

    float remaining;      // segundos (float)
    bool running;

    public bool IsRunning => running;
    public float Remaining => remaining;
    public int RemainingSeconds => Mathf.CeilToInt(remaining);

    void Reset()
    {
        label = GetComponent<TMP_Text>();
        if (startSeconds < 1) startSeconds = 300;
    }

    void Awake()
    {
        if (!label) label = GetComponent<TMP_Text>();
        remaining = Mathf.Max(1, startSeconds);
        Render();
    }

    void OnEnable()
    {
        if (autoStart) StartTimer();
        else Render();
    }

    public void StartTimer()   { running = true; }
    public void PauseTimer()   { running = false; }
    public void ResetTimer()   { remaining = Mathf.Max(1, startSeconds); running = false; Render(); }
    public void RestartTimer() { remaining = Mathf.Max(1, startSeconds); running = true;  Render(); }

    void Update()
    {
        TickTimer();
        Render();
    }

    void TickTimer()
    {
        if (!running) return;
        if (remaining <= 0f) { running = false; return; }

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        remaining -= dt;
        if (remaining < 0f) remaining = 0f;
        if (remaining <= 0f) running = false;
    }

    void Render()
    {
        int secs = Mathf.CeilToInt(remaining);
        int m = secs / 60, s = secs % 60;

        int score = ScoreManager.I    ? ScoreManager.I.Score    : 0;
        int coins = CurrencyManager.I ? CurrencyManager.I.Coins : 0;

        if (label)
            label.text = $"Score: {score}   |   Moedas: {coins}   |   Tempo: {m:00}:{s:00}";
    }
}
