using UnityEngine;

public class TutorialEndPanel : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] NPCSpawner npcSpawner;              // SpawnSystem (NPCSpawner)
    [SerializeField] ScoreAndCountdownTMP countdown;     // HUD de score/tempo (opcional)
    [SerializeField] bool pauseGame = true;
    [SerializeField] bool clearTablesOnStart = true;     // limpar itens das mesas ao começar o jogo
    [SerializeField] AudioSource tutorialMusic;

    CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        HideImmediate();
    }

    void HideImmediate()
    {
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    // Chamado quando o tutorial termina
    public void Show()
    {
        Debug.Log("[TutorialEndPanel] Show chamado");

        if (canvasGroup)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (pauseGame)
            Time.timeScale = 0f;
    }

    // OnClick do botão Play desse painel
    public void OnPlayPressed()
    {
        Debug.Log("[TutorialEndPanel] Play pressionado");

        // volta o tempo de jogo
        if (pauseGame)
            Time.timeScale = 1f;

        // para música do tutorial, se tiver
        if (tutorialMusic && tutorialMusic.isPlaying)
            tutorialMusic.Stop();

        // limpa mesas do tutorial
        if (clearTablesOnStart)
            ClearAllTables();

        // libera spawn de clientes
        if (npcSpawner != null)
            npcSpawner.EnableSpawning();
        else
            Debug.LogWarning("[TutorialEndPanel] NpcSpawner não atribuído.");

        // 🔹 controla o tempo pelo GameTimer
        if (GameTimer.I != null)
        {
            GameTimer.I.ResetTimer();   // volta pro tempo inicial (ex: 5:00)
            GameTimer.I.StartTimer();   // começa a contar AGORA
        }

        // OBS: não chamamos mais countdown.ResetTimer / StartTimer
        // O ScoreAndCountdownTMP (se estiver na cena) deve apenas ler e exibir GameTimer.I.RemainingSeconds.

        // esconde o painel
        HideImmediate();
    }

    void ClearAllTables()
    {
        var spots = FindObjectsByType<TableSpot>(FindObjectsSortMode.None);
        foreach (var spot in spots)
        {
            GameObject obj = spot.Remove();
            if (obj != null)
                Destroy(obj); // some com prato/garrafa/etc.
        }
    }
}
