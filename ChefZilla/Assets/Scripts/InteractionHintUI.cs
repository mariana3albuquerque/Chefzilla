using UnityEngine;
using TMPro;

public class InteractionHintUI : MonoBehaviour
{
    [Header("Referência")]
    public TextMeshProUGUI hintText;

    [Header("Mensagem padrão")]
    [TextArea] public string defaultMessage = "Use as setas do teclado para mover o chef";

    CanvasGroup group;

    void Awake()
    {
        if (hintText == null)
        {
            Debug.LogError("[InteractionHintUI] HintText não atribuído no Inspector.");
            return;
        }

        // Garante que temos um CanvasGroup e que está visível
        group = hintText.GetComponent<CanvasGroup>();
        if (!group) group = hintText.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 1f;

        // Garante um tamanho e alinhamento razoáveis (caso tenha vindo minúsculo)
        if (!hintText.enableAutoSizing && hintText.fontSize < 10f)
            hintText.fontSize = 36f;
        hintText.alignment = TextAlignmentOptions.Center;

        // Põe a mensagem inicial
        SetText(null);
    }

    /// <summary>Define o texto. Se message for nulo/vazio, usa a defaultMessage.</summary>
    public void SetText(string message)
    {
        if (hintText == null) return;

        hintText.text = string.IsNullOrEmpty(message) ? defaultMessage : message;
        if (group != null) group.alpha = 1f; // sempre visível
    }

    public void HideHint()
	{
	    if (!hintText) return;
	    var cg = hintText.GetComponent<CanvasGroup>() ?? hintText.gameObject.AddComponent<CanvasGroup>();
	    cg.alpha = 0f; // invisível
	}

	public void ShowHint(string message)
	{
	    if (!hintText) return;
	    hintText.text = message;
	    var cg = hintText.GetComponent<CanvasGroup>() ?? hintText.gameObject.AddComponent<CanvasGroup>();
	    cg.alpha = 1f; // visível
	}

}

