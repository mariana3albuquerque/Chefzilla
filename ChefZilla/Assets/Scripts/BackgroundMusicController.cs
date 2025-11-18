using System.Collections;
using UnityEngine;
using TMPro;   // <<< pra usar TextMeshProUGUI

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicController : MonoBehaviour
{
    [Header("Músicas")]
    [SerializeField] AudioClip normalLoop;   // música padrão
    [SerializeField] AudioClip tenseLoop;    // música tensa do final
    [SerializeField] float switchAtSeconds = 60f; // quando faltar <= isso, troca
    [SerializeField] float fadeDuration = 1.0f;   // tempo de fade entre músicas

    [Header("Mensagem de aviso")]
    [SerializeField] TextMeshProUGUI warningText;      // texto na tela (TMP)
    [TextArea][SerializeField] string warningMessage = "Seu tempo está acabando!";
    [SerializeField] float warningDuration = 3f;       // quanto tempo fica visível
    [SerializeField] float warningFadeTime = 0.25f;    // tempo de fade in/out

    [SerializeField] float blinkFrequency = 4f;
    AudioSource source;
    bool isTense = false;
    bool started = false;

    float targetVolume = 1f;   // volume desejado (vem do AudioSource)
    CanvasGroup warningGroup;
    Coroutine warningRoutine;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;

        // guarda o volume que você colocar no AudioSource no Inspector
        targetVolume = source.volume;

        SetupWarningUI();
    }

    void SetupWarningUI()
    {
        if (!warningText) return;

        warningGroup = warningText.GetComponent<CanvasGroup>();
        if (!warningGroup)
            warningGroup = warningText.gameObject.AddComponent<CanvasGroup>();

        warningGroup.alpha = 0f;
        warningText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Chame isso quando o JOGO começar de verdade (depois do tutorial).
    /// </summary>
    public void StartNormalMusic()
    {
        if (normalLoop == null)
        {
            Debug.LogWarning("[BackgroundMusicController] normalLoop não definido.");
            return;
        }

        source.clip = normalLoop;
        source.volume = targetVolume;  // usa o volume do Inspector
        source.loop = true;
        source.Play();
        started = true;
        isTense = false;
    }

    /// <summary>
    /// Seu script de timer deve chamar isso passando o tempo restante.
    /// </summary>
    public void UpdateTimeRemaining(float secondsRemaining)
    {
        if (!started) return;
        if (isTense) return;
        if (tenseLoop == null) return;

        if (secondsRemaining <= switchAtSeconds)
        {
            StartCoroutine(SwitchToTenseRoutine());
        }
    }

    IEnumerator SwitchToTenseRoutine()
    {
        isTense = true;

        // fade out da música atual
        float t = 0f;
        float startVol = source.volume;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
            yield return null;
        }

        source.clip = tenseLoop;
        source.Play();

        // dispara a mensagem de aviso junto com a troca
        ShowWarning();

        // fade in da nova música até o volume alvo (o do Inspector)
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, t / fadeDuration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    // ====================== AVISO VISUAL ======================

    void ShowWarning()
    {
        if (!warningText) return;

        if (warningRoutine != null)
            StopCoroutine(warningRoutine);

        warningRoutine = StartCoroutine(WarningRoutine());
    }

    IEnumerator WarningRoutine()
    {
        if (!warningText) yield break;

        if (warningGroup == null)
            SetupWarningUI();

        warningText.text = warningMessage;
        warningText.gameObject.SetActive(true);

        // começa invisível
        warningGroup.alpha = 0f;

        float timer = 0f;

        // enquanto durar o aviso, pisca o texto
        while (timer < warningDuration)
        {
            timer += Time.unscaledDeltaTime;

            // calcula em qual "fase" do pisca estamos
            // blinkFrequency = quantas vezes por segundo troca de estado
            int phase = Mathf.FloorToInt(timer * blinkFrequency);

            // par = visível, ímpar = invisível
            warningGroup.alpha = (phase % 2 == 0) ? 1f : 0f;

            yield return null;
        }

        // garante que some no final
        warningGroup.alpha = 0f;
        warningText.gameObject.SetActive(false);
        warningRoutine = null;
    }
}
