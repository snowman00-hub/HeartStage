using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using System.Threading;

public class MonsterNavMeshAgent : MonoBehaviour
{
    [Header("Reference")]
    public List<Transform> targetPoints;

    [Header("Field")]
    private float stopDistance = 5f;
    private float checkInterval = 0.3f;
    private float raycastDistance = 5f; // 레이캐스트 거리

    private Transform target;
    private NavMeshAgent navMeshAgent;
    public bool isChasingPlayer = false;
    private bool isMonsterBlocked = false;
    private bool isExternalStopped = false;
    private CancellationTokenSource cancellationTokenSource;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;

        isChasingPlayer = true;

        cancellationTokenSource = new CancellationTokenSource();
        CheckForwardMonsterAsync(cancellationTokenSource.Token).Forget();
    }

    private void OnDestroy()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
    }

    public void SetUp()
    {
        if (targetPoints != null && targetPoints.Count > 0)
        {
            var randomIndex = Random.Range(0, targetPoints.Count);
            this.target = targetPoints[randomIndex];
        }
    }

    public void ClearTarget()
    {
        target = null;
        isExternalStopped = true;
    }

    public void RestoreTarget()
    {
        SetUp();
        isExternalStopped = false;
    }

    public void ApplyMoveSpeed(float moveSpeed)
    {
        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = moveSpeed;
    }

    private async UniTaskVoid CheckForwardMonsterAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (!isExternalStopped && isChasingPlayer)
                {
                    CheckForwardMonster();
                }
                await UniTask.Delay((int)(checkInterval * 1000), cancellationToken: cancellationToken);
            }
            catch (System.OperationCanceledException)
            {
                break;
            }
        }
    }

    private void CheckForwardMonster()
    {
        if (navMeshAgent == null || target == null)
        {
            isMonsterBlocked = false;
            return;
        }

        // 타겟과의 거리 체크
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // 타겟에 충분히 가까우면 멈춤
        if (distanceToTarget <= stopDistance)
        {
            isMonsterBlocked = true;
            return;
        }

        // 타겟 방향 계산
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector2 rayOrigin = new Vector2(transform.position.x, transform.position.y);
        Vector2 rayDirection = new Vector2(directionToTarget.x, directionToTarget.y);

        // 타겟 방향으로 레이캐스트 발사 (앞에 다른 몬스터가 있는지만 체크)
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, raycastDistance);

        bool foundBlockingMonster = false;

        // 앞에 다른 몬스터가 있으면 멈춤 (거리 상관없이)
        if (hit.collider != null && hit.collider.gameObject != gameObject && hit.collider.CompareTag(Tag.Monster))
        {
            foundBlockingMonster = true;
        }

        isMonsterBlocked = foundBlockingMonster;

        // 디버그용 레이 그리기
        Debug.DrawRay(transform.position, directionToTarget * raycastDistance,
                     foundBlockingMonster ? Color.red : Color.green, checkInterval);
    }

    private void Update()
    {
        // 외부에서 정지된 경우 (MonsterBehavior)
        if (isExternalStopped)
        {
            navMeshAgent.isStopped = true;
            return;
        }

        // 몬스터 차단으로 인한 정지
        navMeshAgent.isStopped = isMonsterBlocked;

        if (target != null && isChasingPlayer && !navMeshAgent.isStopped)
        {
            navMeshAgent.SetDestination(target.position);
        }
    }

    // 디버그용 기즈모
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && target != null)
        {
            // 타겟 라인
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, target.position);

            // 레이캐스트 방향과 거리 표시
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            Gizmos.color = isMonsterBlocked ? Color.red : Color.green;
            Gizmos.DrawRay(transform.position, directionToTarget * raycastDistance);

            // 정지 거리 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + directionToTarget * stopDistance, 0.2f);
        }
    }
}