using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

public class MonsterNavMeshAgent : MonoBehaviour
{
    [Header("Reference")]
    public List<Transform> targetPoints;

    [Header("Field")]
    private Transform target;
    private NavMeshAgent navMeshAgent;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
    }

    public void SetUp()
    {
        if(targetPoints != null && targetPoints.Count > 0)
        {
            var randomIndex = Random.Range(0, targetPoints.Count);            
            this.target = targetPoints[randomIndex];
        }     
    }

    private void Update()
    {
        if (target != null)
        {
            navMeshAgent.SetDestination(target.position);
        }
    }
}

