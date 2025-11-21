using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;   // se for Text normal, trocar para using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [Header("Referências de UI")]
    [SerializeField] CanvasGroup panel;       // CanvasGroup do painel de fim de jogo
    [SerializeField] TMP_Text titleText;      // "Fim de Jogo"
    [SerializeField] TMP_Text scoreText;      // "Sua pontuação:"
    [SerializeField] TMP_Text coinsText;      // "Moedas:"

    [Header("Cena de Menu")]
    [SerializeField] string menuSceneName = "Menu";   // troque pelo nome exato da cena de menu

    [Header("Áudio")]
    [SerializeField] AudioSource sfxSource;           // AudioSource para o som de fim de jogo
    [SerializeField] AudioClip gameOverSFX;           // clip que toca quando o painel aparece
    [SerializeField, Range(0f, 1f)] float gameOverVolume = 1f;

    bool isShowing = false;

    GameTimer cachedTimer;
    bool subscribed = false;

    void Awake()
    {
        if (!panel)
            panel = GetComponent<CanvasGroup>();

        HideImmediate();
    }

    void OnEnable()
    {
        // toda vez que o painel é habilitado, tenta assinar o GameTimer
        TrySubscribeToTimer();
    }

    void Update()
    {
        // se ainda não conseguiu assinar e o timer já existir, assina agora
        if (!subscribed)
            TrySubscribeToTimer();
    }

    void OnDisable()
    {
        UnsubscribeFromTimer();
    }

    void TrySubscribeToTimer()
    {
        if (subscribed) return;
        if (GameTimer.I == null) return;   // ainda não existe, tenta de novo no próximo frame

        cachedTimer = GameTimer.I;
        cachedTimer.OnTimeUp += HandleTimeUp;
        subscribed = true;
    }

    void UnsubscribeFromTimer()
    {
        if (!subscribed) return;

        if (cachedTimer != null)
            cachedTimer.OnTimeUp -= HandleTimeUp;

        subscribed = false;
        cachedTimer = null;
    }

    // ===================================================================
    // Chamado pelo GameTimer quando o tempo chegar a zero
    // ===================================================================
    public void HandleTimeUp()
    {
        if (isShowing) return;  // garante que só executa uma vez

        Debug.Log("[GameOverPanel] HandleTimeUp chamado, mostrando painel de fim de jogo.");
        Show();
    }

    void Show()
    {
        isShowing = true;

        // Congela o jogo
        Time.timeScale = 0f;
        AudioListener.pause = true;
        PauseMenu.AllowPause = false;

        // Atualiza textos
        int score = ScoreManager.I ? ScoreManager.I.Score : 0;
        int coins = CurrencyManager.I ? CurrencyManager.I.Coins : 0;

        if (titleText)
            titleText.text = "Fim de Jogo";

        if (scoreText)
            scoreText.text = $"Sua pontuação: {score}";

        if (coinsText)
            coinsText.text = $"Moedas: {coins}";

        // 🔊 Toca som de fim de jogo com volume controlado
        if (sfxSource && gameOverSFX)
        {
            sfxSource.ignoreListenerPause = true; // toca mesmo com AudioListener.pause = true
            sfxSource.clip = gameOverSFX;
            sfxSource.volume = gameOverVolume;    // volume exato que você define no inspector
            sfxSource.Play();
        }

        // Mostra painel
        if (panel)
        {
            panel.alpha = 1f;
            panel.interactable = true;
            panel.blocksRaycasts = true;
        }
    }

    void HideImmediate()
    {
        if (panel)
        {
            panel.alpha = 0f;
            panel.interactable = false;
            panel.blocksRaycasts = false;
        }
        isShowing = false;
    }

    // ===================================================================
    // Botões do painel
    // ===================================================================

    // Ligado no botão "Jogar novamente"
    public void OnClickPlayAgain()
    {
        Debug.Log("[GameOverPanel] Jogar novamente.");

        // Descongela o jogo
        Time.timeScale = 1f;
        AudioListener.pause = false;
        PauseMenu.AllowPause = true;

        // Zera score e moedas
        ScoreManager.I?.ResetScore();
        CurrencyManager.I?.ResetCoins();

        // 🔁 Zera TODOS os upgrades (fogão rápido, chef rápido, etc.)
        if (KitchenUpgradeManager.I != null)
            KitchenUpgradeManager.I.ResetAllUpgrades();

        // Recarrega a cena atual (voltando pro fluxo normal: tela de tutorial etc.)
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    // Ligado no botão "Voltar ao menu"
    public void OnClickBackToMenu()
    {
        Debug.Log("[GameOverPanel] Voltar ao menu.");

        Time.timeScale = 1f;
        AudioListener.pause = false;
        PauseMenu.AllowPause = true;

        // Se quiser que ao voltar pro menu também tudo esteja zerado:
        ScoreManager.I?.ResetScore();
        CurrencyManager.I?.ResetCoins();
        KitchenUpgradeManager.I?.ResetAllUpgrades();

        if (!string.IsNullOrEmpty(menuSceneName))
            SceneManager.LoadScene(menuSceneName);
        else
            Debug.LogWarning("[GameOverPanel] menuSceneName não definido.");
    }
}
