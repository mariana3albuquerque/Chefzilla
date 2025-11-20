using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class ScoreAndCountdownTMP : MonoBehaviour
{
    [Header("Refs")]
    public TMP_Text label; // Detecta automaticamente se vazio

    // último valor que o GameTimer avisou
    int lastSeconds = -1;

    void Reset()
    {
        label = GetComponent<TMP_Text>();
    }

    void Awake()
    {
        if (!label) label = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        // liga no GameTimer para receber os ticks
        if (GameTimer.I != null)
        {
            HandleTick(GameTimer.I.RemainingSeconds); // valor inicial
            GameTimer.I.OnTick += HandleTick;
        }
        else
        {
            HandleTick(0);
        }
    }

    void OnDisable()
    {
        if (GameTimer.I != null)
            GameTimer.I.OnTick -= HandleTick;
    }

    void HandleTick(int seconds)
    {
        lastSeconds = seconds;
        Render();
    }

    void Update()
    {
        // atualiza score/moedas mesmo que o segundo não tenha mudado
        Render();
    }

    void Render()
    {
        if (!label) return;

        int secs = lastSeconds;
        if (secs < 0)
            secs = GameTimer.I != null ? GameTimer.I.RemainingSeconds : 0;

        secs = Mathf.Max(0, secs);

        int m = secs / 60;
        int s = secs % 60;

        int score = ScoreManager.I ? ScoreManager.I.Score : 0;
        int coins = CurrencyManager.I ? CurrencyManager.I.Coins : 0;

        label.text = $"Score: {score}   |   Moedas: {coins}   |   Tempo: {m:00}:{s:00}";
    }
}
