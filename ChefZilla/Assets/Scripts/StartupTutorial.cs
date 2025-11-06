using UnityEngine;
using UnityEngine.Events; // << precisa disso

public class StartupTutorial : MonoBehaviour
{
    [SerializeField] GameObject panelTutorial;
    [SerializeField] GameObject hamburgerButton;
    [SerializeField] bool pauseAudio = false;
    [SerializeField] float fade = 0.25f;

    public UnityEvent onClosed;   // << isto cria o campo "On Closed" no Inspector

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

        showing = true;
        if (cg && fade > 0f) { cg.alpha = 0f; StartCoroutine(FadeTo(1f)); }
        else if (cg) cg.alpha = 1f;
    }

    void Update()
    {
        if (!showing) return;
        // se quiser manter ENTER também, deixe:
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            Close();
    }

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

        onClosed?.Invoke();   // << dispara os métodos ligados no Inspector
    }

    System.Collections.IEnumerator FadeTo(float target)
    {
        float start = cg ? cg.alpha : 1f, t = 0f;
        while (t < fade) { t += Time.unscaledDeltaTime; if (cg) cg.alpha = Mathf.Lerp(start, target, t / fade); yield return null; }
        if (cg) cg.alpha = target;
    }
}
