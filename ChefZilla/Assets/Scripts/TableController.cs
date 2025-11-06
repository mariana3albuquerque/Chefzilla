using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class TableController : MonoBehaviour
{
    [Header("Seat (Empty posicionado em frente à mesa)")]
    public Transform pontoDeAssento;

    [Header("Estado")]
    [SerializeField] bool ocupada = false;
    [SerializeField] bool reservada = false;
    public GameObject ocupante; // quem está/irá sentar (NPC)

    [Header("Eventos")]
    public UnityEvent onReservada;
    public UnityEvent onOcupada;
    public UnityEvent onLiberada;

    public bool EstaDisponivel() => !ocupada && !reservada && pontoDeAssento != null;

    // tenta reservar (retorna false se não puder)
    public bool TentarReservar(GameObject quem)
    {
        if (!EstaDisponivel()) return false;
        reservada = true;
        ocupante = quem;
        onReservada?.Invoke();
        return true;
    }

    // chama quando chegou
    public void Ocupar()
    {
        reservada = false;
        ocupada = true;
        onOcupada?.Invoke();
    }

    public void Liberar()
    {
        ocupada = false;
        reservada = false;
        ocupante = null;
        onLiberada?.Invoke();
    }
}
