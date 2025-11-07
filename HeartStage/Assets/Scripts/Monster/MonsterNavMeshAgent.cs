using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class MonsterNavMeshAgent : MonoBehaviour
{
    [Header("Field")]
    private Transform target;
    private NavMeshAgent navMeshAgent;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
    }

    public void SetUp(Transform target)
    {
        this.target = target;
    }

    private void Update()
    {
        if (target != null)
        {
            navMeshAgent.SetDestination(target.position);
        }
    }
}

