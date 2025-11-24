using System.Collections.Generic;
using UnityEngine;

public class MonsterMovement : MonoBehaviour
{
    private MonsterData monsterData;
    private bool isInitialized = false;

    [SerializeField] private float separationRadius = 1.2f;   // 분리 반경
    [SerializeField] private float separationForce = 3f;      // 분리 힘
    [SerializeField] private float minDistance = 0.6f;        // 최소 거리 유지
    [SerializeField] private float frontCheckDistance = 0.8f; // 앞줄 체크 거리

    [SerializeField] private float screenMargin = 0.5f;

    private bool isNearWall = false;      // 벽 근접 상태
    private bool isFrontBlocked = false;  // 앞줄 막힘 상태

    private float leftBound;
    private float rightBound;
    private bool boundsInitialized = false;

    private static List<MonsterMovement> allActiveMonsters = new List<MonsterMovement>();

    private Collider2D selfCollider;  // 혼란 전용 셀프 콜라이더
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
    }

    private void Update()
    {
        // 상태 체크
        if (!CanMove()) return;

        if (!boundsInitialized)
        {
            InitializeScreenBounds();
        }

        // 벽, 앞줄 체크
        CheckWallProximity();
        CheckFrontBlocked();

        // 이동 처리
        if (!isNearWall && !isFrontBlocked)
        {
            MoveWithSeparation(Vector3.down * monsterData.moveSpeed * Time.deltaTime);
        }

        else if (!isNearWall && isFrontBlocked)
        {
            MoveWithSeparation(Vector3.zero);
        }
    }

    // 이동 가능 상태 체크
    private bool CanMove()
    {
        if (EffectBase.Has<KnockbackEffect>(gameObject))
        {
            return false;
        }

        if (EffectBase.Has<ConfuseEffect>(gameObject))
        {
            ConfuseMove(); 
            return false;
        }

        if (!isInitialized || monsterData == null)
        {
            return false;
        }

        if (EffectBase.Has<StunEffect>(gameObject) || EffectBase.Has<ParalyzeEffect>(gameObject))
        {
            return false;
        }

        return true;
    }

    public void Init(MonsterData data, Vector3 direction)
    {
        monsterData = data;
        isInitialized = true;
    }

    // 화면 경계 초기화
    private void InitializeScreenBounds()
    {
        leftBound = -4f + screenMargin;
        rightBound = 4f - screenMargin;
        boundsInitialized = true;
    }

    // 벽 근접 확인
    private void CheckWallProximity()
    {
        isNearWall = Physics2D.OverlapCircle
        (
            transform.position,
            monsterData.attackRange,
            LayerMask.GetMask(Tag.Wall)
        )

        != null;
    }

    // 앞줄 막힘 확인
    private void CheckFrontBlocked()
    {
        isFrontBlocked = false;

        foreach (var other in allActiveMonsters)
        {
            if (other == this || other == null || !other.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (other.transform.position.y < transform.position.y)
            {
                float dx = Mathf.Abs(other.transform.position.x - transform.position.x);
                float dy = transform.position.y - other.transform.position.y;

                if (dx < minDistance && dy < frontCheckDistance)
                {
                    isFrontBlocked = true;
                    break;
                }
            }
        }
    }

    // 이동 + 분리 힘 적용
    private void MoveWithSeparation(Vector3 move)
    {
        Vector3 separation = GetHorizontalSeparationForce();
        Vector3 newPosition = transform.position + move + separation;
        newPosition = ClampToScreenBounds(newPosition);
        transform.position = newPosition;
    }

    // 좌우 분리 힘 계산
    private Vector3 GetHorizontalSeparationForce()
    {
        Vector3 forceSum = Vector3.zero;
        foreach (var other in allActiveMonsters)
        {
            if (other == this || other == null || !other.gameObject.activeInHierarchy)
            { 
                continue;
            }

            float distance = Vector3.Distance(transform.position, other.transform.position);

            if (distance < separationRadius && distance > 0.01f)
            {
                Vector3 away = transform.position - other.transform.position;
                away.y = 0f;

                // x축이 거의 같으면 랜덤하게 좌/우로 분리
                if (Mathf.Abs(away.x) < 0.01f)
                {
                    away.x = (Random.value > 0.5f) ? 1f : -1f;
                }

                if (away.magnitude > 0.01f)
                {
                    float norm = distance / separationRadius;
                    float strength = separationForce * (1f - norm) * Time.deltaTime;

                    if (distance < minDistance)
                    {
                        strength *= 1.5f;
                    }

                    Vector3 force = away.normalized * strength;

                    // 경계에 붙었을 때 바깥 방향 힘 무시
                    if ((transform.position.x <= leftBound && force.x < 0) || (transform.position.x >= rightBound && force.x > 0))
                    {
                        force.x = 0;
                    }

                    forceSum += force;
                }
            }
        }

        float maxSpeed = monsterData.moveSpeed * 0.5f;

        if (forceSum.magnitude > maxSpeed)
        {
            forceSum = forceSum.normalized * maxSpeed;
        }

        return forceSum;
    }

    // 화면 경계 제한
    private Vector3 ClampToScreenBounds(Vector3 pos)
    {
        if (!boundsInitialized)
        {
            return pos;
        }

        if (pos.x < leftBound)
        {
            pos.x = leftBound + 0.01f;
        }

        else if (pos.x > rightBound)
        {
            pos.x = rightBound - 0.01f;
        }

        return pos;
    }

    // 몬스터 리스트 정리
    private void LateUpdate()
    {
        if (Time.frameCount % 60 == 0)
        {
            allActiveMonsters.RemoveAll(m => m == null || !m.gameObject.activeInHierarchy);
        }
    }

    // 혼란 상태 이동
    private void ConfuseMove()
    {
        if (!isInitialized || monsterData == null)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll
        (
            transform.position,
            confuseSearchRadius,
            LayerMask.GetMask(Tag.Monster)
        );

        Collider2D closest = null;
        float bestDistSq = float.MaxValue;

        foreach (var col in hits)
        {
            if (col == null || col == selfCollider)
                continue;

            float dSq = (col.transform.position - transform.position).sqrMagnitude;

            if (dSq < bestDistSq)
            {
                bestDistSq = dSq;
                closest = col;
            }
        }

        if (closest == null) 
            return;

        float attackRangeSq = monsterData.attackRange * monsterData.attackRange;

        if (bestDistSq > attackRangeSq)
        {
            Vector3 dir = (closest.transform.position - transform.position).normalized;
            transform.position += dir * monsterData.moveSpeed * Time.deltaTime;
        }
    }
}