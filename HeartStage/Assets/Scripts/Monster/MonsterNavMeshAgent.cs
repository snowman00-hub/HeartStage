using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class MonsterNavMeshAgent : MonoBehaviour
{
    [Header("Field")]
    private Transform target;
    private NavMeshAgent navMeshAgent;

    public void SetUp(Transform target)
    { 
        this.target = target;

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        navMeshAgent.updatePosition = false;

        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
    }

    private void Update()
    {
        if (target != null)
        {
            navMeshAgent.SetDestination(target.position);
        }

        Vector3 pos = transform.position;
        if (pos.z != 0f)
        {
            pos.z = 0f;
            transform.position = pos;
        }
    }
}

