using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreAndTimerUI : MonoBehaviour
{
    [Header("Defina um dos dois (ou deixe auto)")]
    public Text uText;          // UI Text legado
    public TMP_Text tmpText;    // TextMeshProUGUI

    void Reset()
    {
        uText   = GetComponent<Text>();
        tmpText = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        if (!uText)   uText   = GetComponent<Text>();
        if (!tmpText) tmpText = GetComponent<TMP_Text>();

        if (ScoreManager.I)   ScoreManager.I.OnScoreChanged   += HandleScoreChanged;
        if (GameTimer.I)      GameTimer.I.OnTick              += HandleTick;
        if (CurrencyManager.I) CurrencyManager.I.OnCoinsChanged += HandleCoinsChanged;

        Refresh();
    }

    void OnDisable()
    {
        if (ScoreManager.I)   ScoreManager.I.OnScoreChanged   -= HandleScoreChanged;
        if (GameTimer.I)      GameTimer.I.OnTick              -= HandleTick;
        if (CurrencyManager.I) CurrencyManager.I.OnCoinsChanged -= HandleCoinsChanged;
    }

    void HandleScoreChanged(int _)  => Refresh();
    void HandleTick(int _)          => Refresh();
    void HandleCoinsChanged(int _)  => Refresh();

    void Refresh()
    {
        int score = ScoreManager.I   ? ScoreManager.I.Score              : 0;
        int secs  = GameTimer.I      ? GameTimer.I.RemainingSeconds      : 0;
        int coins = CurrencyManager.I ? CurrencyManager.I.Coins           : 0;

        string mmss = GameTimer.FormatMMSS(secs);
        string line = $"Score: {score} | Moedas: {coins} | Tempo: {mmss}";

        if (tmpText) tmpText.text = line;
        if (uText)   uText.text   = line;
    }
}
