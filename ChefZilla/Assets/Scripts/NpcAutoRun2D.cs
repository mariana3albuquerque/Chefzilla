using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NpcAutoRun2D : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;   // Satisfied1_00_0 (NavMeshAgent)
    [SerializeField] Transform spawn;      // SpawnPoint (porta)
    [SerializeField] Transform seat;       // SeatTarget (em frente à mesa)
    [SerializeField] float speed = 2.5f;
    [SerializeField] float stopping = 0.25f;
    [SerializeField] float sampleRange = 1.5f; // raio p/ “colar” na malha

    // Start pode ser coroutine; uso 1 frame de atraso p/ garantir o NavMesh após o load aditivo
    IEnumerator Start()
    {
        if (!agent || !spawn || !seat) yield break;
        yield return null; // espera 1 frame

        // Setup 2D
        agent.updateRotation = false;
        agent.updateUpAxis   = false;
        agent.autoBraking    = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        agent.radius = Mathf.Min(agent.radius, 0.20f); // ajuste conforme a largura dos corredores
        agent.speed  = speed;
        agent.stoppingDistance = stopping;

        // SPAWN: cola na malha
        if (NavMesh.SamplePosition(spawn.position, out var sHit, sampleRange, NavMesh.AllAreas))
            agent.Warp(sHit.position);
        else
            agent.Warp(spawn.position);

        // DESTINO: cola na malha
        var dest = seat.position;
        if (NavMesh.SamplePosition(seat.position, out var dHit, sampleRange, NavMesh.AllAreas))
            dest = dHit.position;

        // Valida caminho
        var path = new NavMeshPath();
        if (agent.CalculatePath(dest, path) && path.status == NavMeshPathStatus.PathComplete)
            agent.SetPath(path);
        else
        {
            // fallback: tenta um ponto vizinho
            if (NavMesh.SamplePosition(dest + new Vector3(0.2f, 0f, 0f), out var alt, sampleRange, NavMesh.AllAreas))
                agent.SetDestination(alt.position);
        }
    }
}
