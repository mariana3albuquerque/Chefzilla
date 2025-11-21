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
        if (AllowPause && Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    // Conecte este método no OnClick do BtnHamburger
    public void TogglePause()
    {
        if (paused) Resume();
        else Pause();
    }

    void Pause()
    {
        paused = true;

        Time.timeScale = 0f;
        AudioListener.pause = true;             // opcional

        if (panelPause) panelPause.SetActive(true);
        if (hamburgerButton) hamburgerButton.SetActive(false);  // esconde o ícone

        if (firstSelected)
            EventSystem.current?.SetSelectedGameObject(firstSelected.gameObject);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        paused = false;

        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (panelPause) panelPause.SetActive(false);
        if (hamburgerButton) hamburgerButton.SetActive(true);   // mostra o ícone
    }

    // Chamado pelo botão "Exit" do menu de pause
    public void ExitToMenu()
    {
        // Garante que o jogo não fique travado
        paused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (panelPause) panelPause.SetActive(false);
        if (hamburgerButton) hamburgerButton.SetActive(true);

        // 🔁 Zera score, moedas e upgrades antes de sair
        if (ScoreManager.I != null)
            ScoreManager.I.ResetScore();

        if (CurrencyManager.I != null)
            CurrencyManager.I.ResetCoins();

        if (KitchenUpgradeManager.I != null)
            KitchenUpgradeManager.I.ResetAllUpgrades();

        // (Opcional) libera pause de novo ao entrar no menu
        AllowPause = true;

        // Carrega cena de menu principal
        if (!string.IsNullOrEmpty(mainMenuScene))
            SceneManager.LoadScene(mainMenuScene);
        else
            Debug.LogWarning("[PauseMenu] mainMenuScene não definido.");
    }

    // Sair completamente do jogo
    public void QuitGame()
    {
        // Garante que nada fique pausado
        paused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Também limpa o estado global
        if (ScoreManager.I != null)
            ScoreManager.I.ResetScore();

        if (CurrencyManager.I != null)
            CurrencyManager.I.ResetCoins();

        if (KitchenUpgradeManager.I != null)
            KitchenUpgradeManager.I.ResetAllUpgrades();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
