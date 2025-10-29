using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class FixAgent2D : MonoBehaviour
{
    void Awake()
    {
        var agent = GetComponent<NavMeshAgent>();
        // Impede o agente de inclinar no eixo X/Z (mantém ele “em pé” no 2D)
        agent.updateUpAxis = false;
        agent.updateRotation = false; // mantém rotação no plano 2D
    }

    void LateUpdate()
    {
        // Garante que o personagem não “amasse”
        Vector3 e = transform.eulerAngles;
        transform.eulerAngles = new Vector3(0f, e.y, 0f);
    }
}
