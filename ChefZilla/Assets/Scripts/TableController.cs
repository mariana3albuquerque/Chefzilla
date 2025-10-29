using UnityEngine;

public class TableController : MonoBehaviour
{
    // Estado da mesa
    private bool ocupada = false;

    // Posição do assento (posicione um Empty como filho ao lado da mesa)
    public Transform pontoDeAssento;

    // Indica se a mesa está disponível para NPC ocupar
    public bool EstaDisponivel()
    {
        return !ocupada;
    }

    // Chame este método quando um NPC sentar-se
    public void Ocupar()
    {
        ocupada = true;
        // Aqui você pode ativar alguma animação ou cor
        // Exemplo: GetComponent<Renderer>().material.color = Color.red;
    }

    // Chame este método quando o NPC sair da mesa
    public void Liberar()
    {
        ocupada = false;
        // Volta para cor padrão
        // GetComponent<Renderer>().material.color = Color.white;
    }
}
