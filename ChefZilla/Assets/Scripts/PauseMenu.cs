using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool AllowPause = true;

    [Header("Refs")]
    [SerializeField] GameObject panelPause;
    [SerializeField] Selectable firstSelected;
    [SerializeField] GameObject hamburgerButton;   // <<< arraste o BtnHamburger aqui
    [SerializeField] string mainMenuScene = "MainMenu";

    bool paused;

    void Update()
    {
        // Se quiser manter ESC também, deixe esta linha:
        if (AllowPause && Input.GetKeyDown(KeyCode.Escape)) TogglePause();
    }

    // Conecte este método no OnClick do BtnHamburger
    public void TogglePause()
    {
        if (paused) Resume(); else Pause();
    }

    void Pause()
    {
        paused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;             // opcional
        panelPause.SetActive(true);
        if (hamburgerButton) hamburgerButton.SetActive(false);  // esconde o ícone

        if (firstSelected) EventSystem.current?.SetSelectedGameObject(firstSelected.gameObject);
        Cursor.visible = true; Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        paused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        panelPause.SetActive(false);
        if (hamburgerButton) hamburgerButton.SetActive(true);   // mostra o ícone
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f; AudioListener.pause = false;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f; AudioListener.pause = false;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
