using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] GameObject panelPause;     // arraste o PanelPause
    [SerializeField] Selectable firstSelected;  // arraste o BtnResume
    [SerializeField] string mainMenuScene = "MainMenu";

    bool paused;

    void Update()
    {
        // ESC para abrir/fechar
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        if (paused) Resume();
        else Pause();
    }

    void Pause()
    {
        paused = true;
        Time.timeScale = 0f;            // pausa física/tempo
        //AudioListener.pause = true;     // pausa áudio (se não usar Mixer)
        panelPause.SetActive(true);

        // Seleciona o primeiro botão para teclado/controle
        if (firstSelected) EventSystem.current?.SetSelectedGameObject(firstSelected.gameObject);

        // Mostra cursor (útil se você escondia no gameplay)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        paused = false;
        Time.timeScale = 1f;
        //AudioListener.pause = false;
        panelPause.SetActive(false);
    }

    public void ExitToMenu()
    {
        // Garante estado normal ao trocar de cena
        Time.timeScale = 1f;
        //AudioListener.pause = false;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
