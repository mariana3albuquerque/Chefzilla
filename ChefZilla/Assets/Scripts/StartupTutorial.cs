using UnityEngine;

public class StartupTutorial : MonoBehaviour
{
    [SerializeField] GameObject panelTutorial;     // Panel que cobre a tela
    [SerializeField] GameObject hamburgerButton;   // <<< arraste o BtnHamburger aqui
    [SerializeField] bool pauseAudio = false;
    [SerializeField] float fade = 0.25f;

    CanvasGroup cg;
    bool showing;

    void Start()
    {
        panelTutorial.SetActive(true);
        cg = panelTutorial.GetComponent<CanvasGroup>();

        // esconde o botão de menu enquanto o tutorial está aberto
        if (hamburgerButton) hamburgerButton.SetActive(false);

        // desabilita pause por ESC enquanto o tutorial está aberto
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
        // se você decidiu usar só o botão "Jogar" no popup, pode remover este bloco
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

        // mostra o botão de menu novamente
        if (hamburgerButton) hamburgerButton.SetActive(true);

        // reabilita o pause por ESC
        PauseMenu.AllowPause = true;
    }

    System.Collections.IEnumerator FadeTo(float target)
    {
        float start = cg.alpha, t = 0f;
        while (t < fade) { t += Time.unscaledDeltaTime; cg.alpha = Mathf.Lerp(start, target, t / fade); yield return null; }
        cg.alpha = target;
    }
}
