using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterNavMeshAgent : MonoBehaviour
{
    [Header("Reference")]
    public List<Transform> targetPoints;

    private Transform target;
    private NavMeshAgent navMeshAgent;
    private bool isExternalStopped = false;
    private float originalSpeed;

    private void Awake()
    {
        InitializeNavMeshAgent();
    }

    private void InitializeNavMeshAgent()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        navMeshAgent.radius = 0.6f;
        navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    }

    public void SetUp()
    {
        if (targetPoints?.Count > 0)
        {
            target = targetPoints[Random.Range(0, targetPoints.Count)];
            SetDestination();
        }
    }

    public void ClearTarget()
    {
        target = null;
        isExternalStopped = true;
        navMeshAgent.ResetPath();
    }

    public void RestoreTarget()
    {
        SetUp();
        isExternalStopped = false;
    }

    public void ApplyMoveSpeed(float moveSpeed)
    {
        originalSpeed = moveSpeed;
        navMeshAgent.speed = moveSpeed;
    }

    private bool CheckMonsterInFront()
    {
        Collider[] nearbyMonsters = Physics.OverlapSphere(transform.position, 1.5f, LayerMask.GetMask("Monster"));

        foreach (var monster in nearbyMonsters)
        {
            if (monster.transform != transform && monster.gameObject.activeInHierarchy)
            {
                return true;
            }
        }
        return false;
    }

    private void SetDestination()
    {
        if (target != null && navMeshAgent.enabled)
        {
            navMeshAgent.SetDestination(target.position);
        }
    }

    private void Update()
    {
        if (isExternalStopped)
        {
            navMeshAgent.isStopped = true;
            return;
        }

        // 앞에 몬스터 체크 및 이동 제어
        bool monsterInFront = CheckMonsterInFront();
        navMeshAgent.isStopped = monsterInFront;

        if (!monsterInFront)
        {
            navMeshAgent.speed = originalSpeed;
        }
    }
}