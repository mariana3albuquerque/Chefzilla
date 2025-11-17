using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager I { get; private set; }
    public int Score { get; private set; }

    public event Action<int> OnScoreChanged;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Add(int pts)
    {
        if (pts < 0) pts = 0;
        Score += pts;
        OnScoreChanged?.Invoke(Score);
    }

    public void ResetScore()
    {
        Score = 0;
        OnScoreChanged?.Invoke(Score);
    }
}
