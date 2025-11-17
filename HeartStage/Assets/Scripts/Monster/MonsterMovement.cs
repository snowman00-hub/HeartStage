using System.Collections.Generic;
using UnityEngine;

public class MonsterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    private MonsterData monsterData; 
    private bool isInitialized = false;

    [Header("Anti-Overlap Settings")]
    [SerializeField] private float separationRadius = 1.0f;  // 분리 반경
    [SerializeField] private float separationForce = 3f;     // 분리 힘
    [SerializeField] private float minDistance = 0.6f;       // 최소 거리 유지
    [SerializeField] private float frontCheckDistance = 0.8f; // 앞줄 체크 거리

    [Header("Screen Bounds")]
    [SerializeField] private float screenMargin = 0.5f;

    // 벽 감지 관련
    private bool isNearWall = false;
    private bool isFrontBlocked = false; // 앞줄 막힘 상태

    // 화면 경계 캐싱
    private float leftBound, rightBound;
    private bool boundsInitialized = false;

    // 모든 활성 몬스터 추적
    private static List<MonsterMovement> allActiveMonsters = new List<MonsterMovement>();

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
    }

    private void Update()
    {
        if (!isInitialized || monsterData == null) return;

        if (!boundsInitialized)
        {
            InitializeScreenBounds();
        }

        // 벽과 앞줄 체크
        CheckWallProximity();
        CheckFrontBlocked();

        // 막혀있지 않으면 이동
        if (!isNearWall && !isFrontBlocked)
        {
            MoveDown();
        }
        else if (!isNearWall && isFrontBlocked)
        {
            // 앞이 막혀있으면 좌우 분리만 적용
            ApplyOnlyHorizontalSeparation();
        }
    }

    public void Init(MonsterData data, Vector3 direction)
    {
        monsterData = data; 
        isInitialized = true;
    }

    // 화면 경계 초기화
    private void InitializeScreenBounds()
    {
        if (Camera.main != null)
        {
            Vector3 leftScreen = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
            Vector3 rightScreen = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, Camera.main.nearClipPlane));

            leftBound = leftScreen.x + screenMargin;
            rightBound = rightScreen.x - screenMargin;
            boundsInitialized = true;
        }
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

  
    private void MoveDown()
    {
        // 1. 기본 아래쪽 이동 (SO의 최신 moveSpeed 직접 사용)
        Vector3 downwardMovement = Vector3.down * monsterData.moveSpeed * Time.deltaTime;

        // 2. 좌우 분리만 적용 (y축 이동 방해 안함)
        Vector3 separationMovement = GetHorizontalSeparationForce();

        // 3. 최종 이동
        Vector3 finalMovement = downwardMovement + separationMovement;
        Vector3 newPosition = transform.position + finalMovement;

        // 4. 화면 경계 제한 적용
        newPosition = ClampToScreenBounds(newPosition);

        transform.position = newPosition;
    }

    // 좌우 분리만 적용
    private void ApplyOnlyHorizontalSeparation()
    {
        // 앞이 막혔을 때는 좌우 분리만 적용
        Vector3 separationMovement = GetHorizontalSeparationForce();
        Vector3 newPosition = transform.position + separationMovement;

        // 화면 경계 제한
        newPosition = ClampToScreenBounds(newPosition);

        transform.position = newPosition;
    }

    // 좌우 분리 힘 계산 (SO의 최신 moveSpeed 사용)
    private Vector3 GetHorizontalSeparationForce()
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
                    float normalizedDistance = distance / separationRadius;
                    float strength = separationForce * (1f - normalizedDistance) * Time.deltaTime;

                    // 너무 가까우면 더 강한 힘
                    if (distance < minDistance)
                    {
                        strength *= 1.5f;
                    }

                    separationForceVector += directionAway.normalized * strength;
                }
            }
        }

        // 분리 힘 제한 (좌우로만) - SO의 최신 moveSpeed 직접 사용
        float maxSeparationSpeed = monsterData.moveSpeed * 0.5f;
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

        // 화면 경계 표시
        if (boundsInitialized)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector3(leftBound, transform.position.y - 2f, 0),
                          new Vector3(leftBound, transform.position.y + 2f, 0));
            Gizmos.DrawLine(new Vector3(rightBound, transform.position.y - 2f, 0),
                          new Vector3(rightBound, transform.position.y + 2f, 0));
        }
    }
}