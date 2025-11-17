using UnityEngine;

public class TutorialEndPanel : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] NPCSpawner npcSpawner;   // arrasta o SpawnSystem aqui no Inspector

    [Header("Comportamento")]
    [SerializeField] bool pauseGame = true;
    [SerializeField] bool clearTablesOnStart = true; // limpar itens das mesas ao começar o jogo

    CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        // Garante que começa escondido
        HideImmediate();
    }

    void HideImmediate()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        gameObject.SetActive(false);
    }

    // Chamado quando o tutorial termina
    public void Show()
    {
        Debug.Log("[TutorialEndPanel] Show chamado");

        gameObject.SetActive(true);

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;      // <<< garante que fica visível

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
                Destroy(obj); // some com o prato/garrafa/etc.
        }
    }
}
