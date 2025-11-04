using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Nav2DTopDown : MonoBehaviour
{
    public string targetName = "Point";
    private Transform target;
    private NavMeshAgent agent;

    void Start()
    {
        target = GameObject.Find(targetName)?.transform;
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    void Update()
    {
        if (target == null) return;

        Vector3 dest = new Vector3(target.position.x, transform.position.y, target.position.z);
        agent.SetDestination(dest);

        // Aplica posição convertida para XY
        Vector3 np = agent.nextPosition;
        transform.position = new Vector3(np.x, np.z, transform.position.z);

        // Rotaciona sprite para olhar na direção do movimento (opcional)
        Vector3 v = agent.velocity;
        if (v.sqrMagnitude > 0.01f)
        {
            float ang = Mathf.Atan2(v.z, v.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, ang);
        }
    }
}
