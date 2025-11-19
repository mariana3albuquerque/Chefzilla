using UnityEngine;

public class UpgradeMenu : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] GameObject panelRoot;  // arraste aqui o UpgradePanel

    [Header("Comportamento")]
    [SerializeField] bool pauseTime = true;
    [SerializeField] bool pauseAudio = false; // queremos que a música continue

    bool isOpen = false;
    float previousTimeScale = 1f;

    void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (pauseTime)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        if (pauseAudio)
            AudioListener.pause = true;

        // opcional: impedir pause enquanto o menu de upgrade está aberto
        PauseMenu.AllowPause = false;
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (pauseTime)
            Time.timeScale = previousTimeScale;

        if (pauseAudio)
            AudioListener.pause = false;

        PauseMenu.AllowPause = true;
    }

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    void Update()
    {
        if (!isOpen) return;

        // permitir fechar com ESC ou Enter
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
        {
            Close();
        }
    }
}

