using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NpcOverlayLoader : MonoBehaviour
{
    [SerializeField] string navScene = "TilemapM"; // nome EXATO da cena do NPC
    [SerializeField] string keepRoot = "Overlay_NPC"; // root que deve ficar ativo
    [SerializeField] Vector3 offset; // use se precisar alinhar posição

    public void LoadNpcScene() => StartCoroutine(LoadRoutine());

    IEnumerator LoadRoutine()
    {
        var op = SceneManager.LoadSceneAsync(navScene, LoadSceneMode.Additive);
        yield return op;

        var s = SceneManager.GetSceneByName(navScene);
        if (!s.IsValid()) yield break;

        foreach (var go in s.GetRootGameObjects())
        {
            if (go.name != keepRoot) go.SetActive(false); // desliga Environment, Camera etc.
            else
            {
                go.SetActive(true);
                if (offset != Vector3.zero) go.transform.position += offset;
            }
        }
    }
}
