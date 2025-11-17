using UnityEngine;

public class TutorialEndPanel : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] NPCSpawner npcSpawner;            // SpawnSystem (NPCSpawner)
    [SerializeField] ScoreAndCountdownTMP countdown;   // objeto de timer (ScoreAndCountdownTMP)

    [Header("Comportamento")]
    [SerializeField] bool pauseGame = true;
    [SerializeField] bool clearTablesOnStart = true;   // limpar itens das mesas ao começar o jogo

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

        // volta o tempo
        if (pauseGame)
            Time.timeScale = 1f;

        // limpa mesas do tutorial
        if (clearTablesOnStart)
            ClearAllTables();

        // libera spawn de clientes
        if (npcSpawner != null)
            npcSpawner.EnableSpawning();
        else
            Debug.LogWarning("[TutorialEndPanel] NpcSpawner não atribuído.");

        // inicia o timer do jogo
        if (countdown != null)
        {
            countdown.ResetTimer();   // volta para 05:00
            countdown.StartTimer();   // começa a contar
        }

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
