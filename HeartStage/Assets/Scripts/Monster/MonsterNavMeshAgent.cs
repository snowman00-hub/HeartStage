using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;

public class MonsterNavMeshAgent : MonoBehaviour
{
    [Header("Reference")]
    public List<Transform> targetPoints;

    [Header("Field")]
    private Transform target;
    private NavMeshAgent navMeshAgent;
    public bool isChasingPlayer = false;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        isChasingPlayer = true;
    }

    public void SetUp()
    {
        if(targetPoints != null && targetPoints.Count > 0)
        {
            var randomIndex = Random.Range(0, targetPoints.Count);            
            this.target = targetPoints[randomIndex];
        }     
    }
    public void ClearTarget()
    {
        target = null;
    }

    public void RestoreTarget()
    {
        SetUp(); 
    }

    private void Update()
    {
        if (target != null && isChasingPlayer && !navMeshAgent.isStopped)
        {
            navMeshAgent.SetDestination(target.position);
        }
    }
}