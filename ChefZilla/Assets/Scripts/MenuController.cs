using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] string playScene = "Tilemap";   // nome exato da sua cena de jogo
    [SerializeField] GameObject optionsPanel;        // arraste um painel de opções (opcional)

    public void Play()
    {
        SceneManager.LoadScene(playScene);
    }

    public void ToggleOptions(bool show)
    {
        if (optionsPanel) optionsPanel.SetActive(show);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
