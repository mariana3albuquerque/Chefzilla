using UnityEngine;
using UnityEngine.Events; // << precisa disso

public class StartupTutorial : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] GameObject panelTutorial;
    [SerializeField] GameObject hamburgerButton;
    [SerializeField] bool pauseAudio = false;
    [SerializeField] float fade = 0.25f;
    [SerializeField] AudioSource tutorialMusic;

    [Header("Fluxo")]
    public UnityEvent onClosed;                         // chamado quando o player VAI fazer o tutorial
    [SerializeField] TutorialEndPanel tutorialEndPanel; // usado quando o player PULA o tutorial
    [SerializeField] MonoBehaviour tutorialManager;     // script do tutorial (opcional, pode deixar vazio)

    CanvasGroup cg;
    bool showing;

    void Start()
    {
        panelTutorial.SetActive(true);
        cg = panelTutorial.GetComponent<CanvasGroup>();

        if (hamburgerButton) hamburgerButton.SetActive(false);
        PauseMenu.AllowPause = false;

        Time.timeScale = 0f;
        if (pauseAudio) AudioListener.pause = true;

        if (tutorialMusic && !tutorialMusic.isPlaying)
            tutorialMusic.Play();

        showing = true;
        if (cg && fade > 0f) { cg.alpha = 0f; StartCoroutine(FadeTo(1f)); }
        else if (cg) cg.alpha = 1f;
    }

    void Update()
    {
        if (!showing) return;

        // ENTER ainda começa o tutorial normalmente
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            Close();
    }

    // ================= CAMINHO 1: Jogador QUER fazer o tutorial =================
    public void Close()
    {
        if (!showing) return;
        showing = false;
        StartCoroutine(CloseRoutine());
    }

    System.Collections.IEnumerator CloseRoutine()
    {
        if (cg && fade > 0f) yield return FadeTo(0f);

        panelTutorial.SetActive(false);
        Time.timeScale = 1f;
        if (pauseAudio) AudioListener.pause = false;

        if (hamburgerButton) hamburgerButton.SetActive(true);
        PauseMenu.AllowPause = true;

        // segue fluxo normal do tutorial
        onClosed?.Invoke();
    }

    // ================= CAMINHO 2: Jogador QUER PULAR o tutorial =================
    // este método será ligado no botão "Pular tutorial"
    public void SkipTutorial()
    {
        if (!showing) return;
        showing = false;
        StartCoroutine(SkipRoutine());
    }

    System.Collections.IEnumerator SkipRoutine()
    {
        if (cg && fade > 0f) yield return FadeTo(0f);

        panelTutorial.SetActive(false);
        Time.timeScale = 1f;
        if (pauseAudio) AudioListener.pause = false;

        if (hamburgerButton) hamburgerButton.SetActive(true);
        PauseMenu.AllowPause = true;

        // desliga lógica de tutorial, se tiver
        if (tutorialManager != null)
            tutorialManager.enabled = false;

        // 🔹 DESLIGA HINTS / OBJETIVOS DO PLAYER
#if UNITY_2023_1_OR_NEWER
        var pz = FindFirstObjectByType<PlayerInteractionZone>();
#else
        var pz = FindObjectOfType<PlayerInteractionZone>();
#endif
        if (pz != null)
            pz.DisableTutorialMode();

        // para a música do tutorial
        if (tutorialMusic && tutorialMusic.isPlaying)
            tutorialMusic.Stop();

        // pula direto pro "começar o jogo" (fim do tutorial)
        if (tutorialEndPanel != null)
        {
            tutorialEndPanel.OnPlayPressed();
        }
        else
        {
            Debug.LogWarning("[StartupTutorial] TutorialEndPanel não atribuído para SkipTutorial.");
        }
    }

    // ================= Fade genérico =================
    System.Collections.IEnumerator FadeTo(float target)
    {
        float start = cg ? cg.alpha : 1f, t = 0f;
        while (t < fade)
        {
            t += Time.unscaledDeltaTime;
            if (cg) cg.alpha = Mathf.Lerp(start, target, t / fade);
            yield return null;
        }
        if (cg) cg.alpha = target;
    }
}
