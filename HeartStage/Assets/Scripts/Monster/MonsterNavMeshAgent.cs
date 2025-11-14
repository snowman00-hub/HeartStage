using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterNavMeshAgent : MonoBehaviour
{
    [Header("Field")]
    private Vector3 targetPosition;
    private NavMeshAgent navMeshAgent;
    private bool isExternalStopped = false;
    private float originalSpeed;

    private Vector3[] targetPoints;
    public Vector3[] TargetPoints => targetPoints;
    private void Awake()
    {
        InitializeNavMeshAgent();
        InitializeTargetPoints();
    }

    public void InitializeTargetPoints()
    {
        float targetY = -3.5f;

        targetPoints = new Vector3[]
        {
        new Vector3(-6, targetY, 0),
        new Vector3(-4, targetY, 0),
        new Vector3(-2, targetY, 0),
        new Vector3(0, targetY, 0),
        new Vector3(2, targetY, 0),
        new Vector3(4, targetY, 0),
        new Vector3(6, targetY, 0),
        new Vector3(8, targetY, 0)
        };
    }
    private void InitializeNavMeshAgent()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        navMeshAgent.radius = 0.4f;
        navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    }

    public void SetUp()
    {
        float monsterX = transform.position.x;

        // 첫 번째 포인트로 시작하되 반복문에서 모든 포인트를 비교
        Vector3 closestTarget = targetPoints[0];

        float closestDistance = Mathf.Abs(monsterX - targetPoints[0].x);

        // 1번 인덱스부터 시작 (0번은 이미 초기값으로 설정했으므로)
        for (int i = 1; i < targetPoints.Length; i++)
        {
            float distance = Mathf.Abs(monsterX - targetPoints[i].x);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = targetPoints[i];
            }
        }

        targetPosition = closestTarget;
        SetDestination();
    }

    public void ClearTarget()
    {       
        isExternalStopped = true;
        if(navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.ResetPath();
        }
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
        if (navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(targetPosition);
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