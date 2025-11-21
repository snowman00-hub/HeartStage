using System.Collections.Generic;
using UnityEngine;

public class MonsterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    private MonsterData monsterData;
    private bool isInitialized = false;

    [Header("Anti-Overlap Settings")]
    [SerializeField] private float separationRadius = 1.0f;  // 분리 반경
    [SerializeField] private float separationForce = 2f;     // 분리 힘 (줄임)
    [SerializeField] private float minDistance = 0.6f;       // 최소 거리 유지
    [SerializeField] private float frontCheckDistance = 0.8f; // 앞줄 체크 거리

    [Header("Smooth Movement")]
    [SerializeField] private float movementSmoothing = 5f;    // 이동 부드럽기 조절
    [SerializeField] private float maxSeparationSpeed = 1.5f; // 최대 분리 속도

    [Header("Screen Bounds")]
    [SerializeField] private float screenMargin = 0.3f; // 마진을 줄임

    // 벽 감지 관련
    private bool isNearWall = false;
    private bool isFrontBlocked = false; // 앞줄 막힘 상태

    // 화면 경계 캐싱
    private float leftBound, rightBound;
    private bool boundsInitialized = false;

    // 부드러운 이동을 위한 변수들
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 targetSeparationForce = Vector3.zero;
    private Vector3 smoothedSeparationForce = Vector3.zero;

    // 모든 활성 몬스터 추적
    private static List<MonsterMovement> allActiveMonsters = new List<MonsterMovement>();

    private Collider2D selfCollider;  //혼란 전용 셀프 콜라이더
    [SerializeField] private float confuseSearchRadius = 5f; // 혼란 상태에서 타겟 탐색 반경

    public void Awake()
    {
        selfCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        if (!allActiveMonsters.Contains(this))
        {
            allActiveMonsters.Add(this);
        }
    }

    private void OnDisable()
    {
        allActiveMonsters.Remove(this);
        // 비활성화 시 부드러운 이동 변수들 초기화
        currentVelocity = Vector3.zero;
        targetSeparationForce = Vector3.zero;
        smoothedSeparationForce = Vector3.zero;
    }

    private void Update()
    {
        if (EffectBase.Has<KnockbackEffect>(gameObject))
        {
            return;
        }

        if (EffectBase.Has<ConfuseEffect>(gameObject))
        {
            ConfuseMove();
            return;
        }

        if (!isInitialized ||
            monsterData == null ||
            EffectBase.Has<StunEffect>(gameObject) ||
            EffectBase.Has<ParalyzeEffect>(gameObject))
        {
            return;
        }

        if (!boundsInitialized)
        {
            InitializeScreenBounds();
        }

        // 벽과 앞줄 체크
        CheckWallProximity();
        CheckFrontBlocked();

        // 경계 근처 확인
        bool isNearLeftBound = transform.position.x <= leftBound + 0.5f;
        bool isNearRightBound = transform.position.x >= rightBound - 0.5f;

        // 목표 분리 힘 계산
        targetSeparationForce = CalculateTargetSeparationForce(isNearLeftBound, isNearRightBound);

        // 분리 힘을 부드럽게 보간
        smoothedSeparationForce = Vector3.Lerp(
            smoothedSeparationForce,
            targetSeparationForce,
            movementSmoothing * Time.deltaTime
        );

        // 막혀있지 않으면 이동
        if (!isNearWall && !isFrontBlocked)
        {
            MoveDown(isNearLeftBound, isNearRightBound);
        }
        else if (!isNearWall && isFrontBlocked)
        {
            // 앞이 막혀있으면 좌우 분리만 적용
            ApplyOnlyHorizontalSeparation(isNearLeftBound, isNearRightBound);
        }
    }

    public void Init(MonsterData data, Vector3 direction)
    {
        monsterData = data;
        isInitialized = true;
    }

    // 화면 경계 초기화 - 범위를 더 넓게 설정
    private void InitializeScreenBounds()
    {
        // 스폰 범위와 일치하되 더 넓은 이동 범위 제공
        leftBound = -4.0f + screenMargin;  // -3.7f
        rightBound = 4.0f - screenMargin;  // 3.7f
        boundsInitialized = true;
    }

    // 벽 근접 확인 (SO의 최신 attackRange 사용)
    private void CheckWallProximity()
    {
        Collider2D wallCollider = Physics2D.OverlapCircle
        (
            transform.position,
            monsterData.attackRange, // SO의 최신 값 직접 사용
            LayerMask.GetMask(Tag.Wall)
        );

        isNearWall = (wallCollider != null);
    }

    // 앞줄 막힘 확인
    private void CheckFrontBlocked()
    {
        isFrontBlocked = false;

        // 앞쪽(아래쪽) 영역에 다른 몬스터가 있는지 체크
        Vector3 frontCheckPosition = transform.position + Vector3.down * frontCheckDistance;

        foreach (var otherMonster in allActiveMonsters)
        {
            if (otherMonster == this || otherMonster == null) continue;
            if (!otherMonster.gameObject.activeInHierarchy) continue;

            // 다른 몬스터가 내 앞(아래쪽)에 있는지 체크
            if (otherMonster.transform.position.y < transform.position.y)
            {
                float horizontalDistance = Mathf.Abs(otherMonster.transform.position.x - transform.position.x);
                float verticalDistance = transform.position.y - otherMonster.transform.position.y;

                // 앞쪽 근처에 있으면 막힌 것으로 판정
                if (horizontalDistance < minDistance && verticalDistance < frontCheckDistance)
                {
                    isFrontBlocked = true;
                    break;
                }
            }
        }
    }

    private void MoveDown(bool isNearLeftBound, bool isNearRightBound)
    {
        // 1. 기본 아래쪽 이동 (SO의 최신 moveSpeed 직접 사용)
        Vector3 downwardMovement = Vector3.down * monsterData.moveSpeed * Time.deltaTime;

        // 2. 부드럽게 보간된 좌우 분리 적용
        Vector3 finalMovement = downwardMovement + smoothedSeparationForce * Time.deltaTime;
        Vector3 newPosition = transform.position + finalMovement;

        // 3. 화면 경계 제한 적용
        newPosition = ClampToScreenBounds(newPosition);

        transform.position = newPosition;
    }

    // 좌우 분리만 적용 (경계 고려)
    private void ApplyOnlyHorizontalSeparation(bool isNearLeftBound, bool isNearRightBound)
    {
        // 앞이 막혔을 때는 좌우 분리만 적용 (부드럽게 보간된 힘 사용)
        Vector3 separationMovement = smoothedSeparationForce * Time.deltaTime;
        Vector3 newPosition = transform.position + separationMovement;

        // 화면 경계 제한
        newPosition = ClampToScreenBounds(newPosition);

        transform.position = newPosition;
    }

    // 목표 분리 힘 계산 (부드러운 이동을 위한 별도 메서드)
    private Vector3 CalculateTargetSeparationForce(bool isNearLeftBound, bool isNearRightBound)
    {
        Vector3 separationForceVector = Vector3.zero;

        foreach (var otherMonster in allActiveMonsters)
        {
            if (otherMonster == this || otherMonster == null) continue;
            if (!otherMonster.gameObject.activeInHierarchy) continue;

            float distance = Vector3.Distance(transform.position, otherMonster.transform.position);

            // 분리 반경 내에 있으면 좌우로만 밀어내기
            if (distance < separationRadius && distance > 0.01f)
            {
                Vector3 directionAway = transform.position - otherMonster.transform.position;
                directionAway.y = 0f; // y축 제거 - 좌우로만 밀어내기

                if (directionAway.magnitude > 0.01f)
                {
                    // 거리 기반 강도 계산 (더 부드럽게)
                    float normalizedDistance = distance / separationRadius;
                    float strength = separationForce * (1f - normalizedDistance);

                    // 너무 가까우면 조금 더 강한 힘 (기존보다 약하게)
                    if (distance < minDistance)
                    {
                        strength *= 1.2f; // 1.5f에서 1.2f로 줄임
                    }

                    Vector3 forceDirection = directionAway.normalized;

                    // 경계 근처에서는 분리 방향 조정 (부드럽게)
                    if (isNearLeftBound && forceDirection.x < 0)
                    {
                        // 왼쪽 경계 근처에서 더 왼쪽으로 가려고 하면 오른쪽으로 유도
                        forceDirection.x = Mathf.Abs(forceDirection.x);
                        strength *= 1.2f; // 1.5f에서 1.2f로 줄임
                    }
                    else if (isNearRightBound && forceDirection.x > 0)
                    {
                        // 오른쪽 경계 근처에서 더 오른쪽으로 가려고 하면 왼쪽으로 유도
                        forceDirection.x = -Mathf.Abs(forceDirection.x);
                        strength *= 1.2f; // 1.5f에서 1.2f로 줄임
                    }

                    separationForceVector += forceDirection * strength;
                }
            }
        }

        // 분리 힘 제한 (더 낮은 최대값)
        if (separationForceVector.magnitude > maxSeparationSpeed)
        {
            separationForceVector = separationForceVector.normalized * maxSeparationSpeed;
        }

        return separationForceVector;
    }

    // 화면 경계 제한
    private Vector3 ClampToScreenBounds(Vector3 position)
    {
        if (!boundsInitialized) return position;

        position.x = Mathf.Clamp(position.x, leftBound, rightBound);
        return position;
    }

    // 몬스터 리스트 정리 (성능 최적화)
    private void LateUpdate()
    {
        if (Time.frameCount % 60 == 0)
        {
            CleanupMonsterList();
        }
    }

    // 비활성 몬스터 정리
    private static void CleanupMonsterList()
    {
        allActiveMonsters.RemoveAll(monster => monster == null || !monster.gameObject.activeInHierarchy);
    }

    // 디버그용 시각화
    private void OnDrawGizmosSelected()
    {
        // 분리 반경 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationRadius);

        // 최소 거리 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minDistance);

        // 앞줄 체크 영역 표시
        Gizmos.color = isFrontBlocked ? Color.red : Color.blue;
        Vector3 frontCheckPos = transform.position + Vector3.down * frontCheckDistance;
        Gizmos.DrawWireCube(frontCheckPos, new Vector3(minDistance * 2, frontCheckDistance * 0.5f, 0));

        // 벽 감지 범위 표시 (SO의 최신 값 사용)
        if (monsterData != null)
        {
            Gizmos.color = isNearWall ? Color.red : Color.green;
            Gizmos.DrawWireSphere(transform.position, monsterData.attackRange);
        }

        // 현재 분리 힘 방향 표시 (디버깅용)
        if (smoothedSeparationForce.magnitude > 0.1f)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position, smoothedSeparationForce);
        }
    }

    private void ConfuseMove()
    {
        if (!isInitialized || monsterData == null)
            return;

        // 1) 주변에서 자기 제외하고 가장 가까운 몬스터 찾기
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            confuseSearchRadius,
            LayerMask.GetMask(Tag.Monster)
        );

        Collider2D closest = null;
        float bestDistSq = float.MaxValue;

        foreach (var col in hits)
        {
            if (col == null || col == selfCollider)   // 자기 자신 제외
                continue;

            float dSq = (col.transform.position - transform.position).sqrMagnitude;
            if (dSq < bestDistSq)
            {
                bestDistSq = dSq;
                closest = col;
            }
        }

        //근처 타겟이없으면 그냥 가만히 서있기
        if (closest == null)
            return;

        // 2) 사거리 체크
        float attackRangeSq = monsterData.attackRange * monsterData.attackRange;

        if (bestDistSq > attackRangeSq)
        {
            // 2-1) 사거리 밖이면 → 타겟 쪽으로 이동 (부드럽게)
            Vector3 dir = (closest.transform.position - transform.position).normalized;
            Vector3 targetPosition = transform.position + dir * monsterData.moveSpeed * Time.deltaTime;

            // 부드럽게 이동
            Vector3 newPosition = Vector3.Lerp(
                transform.position,
                targetPosition,
                movementSmoothing * Time.deltaTime
            );

            // 화면 경계 제한 적용
            newPosition = ClampToScreenBounds(newPosition);
            transform.position = newPosition;
            return;
        }
    }
}