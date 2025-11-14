using System.Collections.Generic;
using UnityEngine;

public class MonsterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    private float moveSpeed = 1f;
    private Vector3 direction = Vector3.down;
    private MonsterData monsterData;
    private bool isInitialized = false;

    [Header("Slot System")]
    [SerializeField] private float slotSize = 1f; // 슬롯 간격
    [SerializeField] private int maxMonsterPerSlot = 1; // 슬롯당 1마리만

    // 슬롯 관리 (전역 공유)
    private static Dictionary<Vector3, List<MonsterMovement>> slotDictionary = new Dictionary<Vector3, List<MonsterMovement>>();
    private Vector3 assignedSlot;
    private Vector3 targetPosition;
    private bool hasAssignedSlot = false;
    private float repositionCooldown = 0f;

    // 벽 감지 관련
    private bool isNearWall = false;
    private bool shouldStop = false;

    private void Start()
    {
        FindAndAssignSlot();
    }

    private void Update()
    {
        if (!isInitialized) return;

        repositionCooldown -= Time.deltaTime;

        // 벽 근처인지 체크
        CheckWallProximity();

        // 이동 로직
        if (!shouldStop)
        {
            MoveToTarget();
        }

        // 주기적으로 더 좋은 슬롯 찾기
        if (repositionCooldown <= 0f)
        {
            CheckForBetterSlot();
            repositionCooldown = 1f;
        }
    }

    public void Init(MonsterData data, Vector3 direction)
    {
        monsterData = data;
        moveSpeed = data.moveSpeed;
        this.direction = Vector3.down; // 강제로 아래 방향
        isInitialized = true;
    }

    private void CheckWallProximity()
    {
        // 벽과의 거리 체크
        Collider2D wallCollider = Physics2D.OverlapCircle(transform.position, monsterData.attackRange, LayerMask.GetMask(Tag.Wall));

        if (wallCollider != null)
        {
            // 벽 근처에 도달 - 정지
            isNearWall = true;
            shouldStop = true;
        }
        else
        {
            // 앞에 다른 몬스터가 있는지 체크
            Vector3 frontCheckPos = transform.position + Vector3.down * slotSize;
            Collider2D frontMonster = Physics2D.OverlapCircle(frontCheckPos, slotSize * 0.4f, LayerMask.GetMask(Tag.Monster));

            if (frontMonster != null && frontMonster.gameObject != gameObject)
            {
                // 앞에 몬스터가 있으면 정지
                shouldStop = true;
            }
            else
            {
                // 앞이 비어있으면 계속 이동
                shouldStop = false;
                isNearWall = false;
            }
        }
    }

    private void FindAndAssignSlot()
    {
        Vector3 currentPos = transform.position;
        Vector3 slot = FindNearSlot(currentPos);
        AssignToSlot(slot);
    }

    private Vector3 FindNearSlot(Vector3 worldPosition)
    {
        Vector3 baseSlot = new Vector3(
            Mathf.Round(worldPosition.x / slotSize) * slotSize,
            Mathf.Round(worldPosition.y / slotSize) * slotSize,
            0f
        );

        // y축으로만 3칸 체크 (x축 고정)
        var slotsToCheck = new List<Vector3>();
        for (int y = -1; y <= 1; y++)  // y축으로만 3개
        {
            var checkSlot = baseSlot + new Vector3(0, y * slotSize, 0f);
            slotsToCheck.Add(checkSlot);
        }

        // 거리순으로 정렬
        slotsToCheck.Sort((a, b) =>
            Vector3.Distance(worldPosition, a).CompareTo(Vector3.Distance(worldPosition, b)));

        // 사용 가능한 슬롯 찾기
        foreach (var slot in slotsToCheck)
        {
            if (!slotDictionary.ContainsKey(slot))
            {
                slotDictionary[slot] = new List<MonsterMovement>();
            }

            if (slotDictionary[slot].Count < maxMonsterPerSlot)
            {
                return slot;
            }
        }

        return slotsToCheck[0];
    }

    private void AssignToSlot(Vector3 slot)
    {
        // 이전 슬롯에서 제거
        if (hasAssignedSlot)
        {
            UnregisterFromSlot();
        }

        // 새 슬롯에 등록
        assignedSlot = slot;

        if (!slotDictionary.ContainsKey(assignedSlot))
        {
            slotDictionary[assignedSlot] = new List<MonsterMovement>();
        }

        slotDictionary[assignedSlot].Add(this);
        hasAssignedSlot = true;
        targetPosition = assignedSlot;
    }

    private void UnregisterFromSlot()
    {
        if (!hasAssignedSlot) return;

        if (slotDictionary.ContainsKey(assignedSlot))
        {
            slotDictionary[assignedSlot].Remove(this);

            // 슬롯이 비어있으면 딕셔너리에서 제거
            if (slotDictionary[assignedSlot].Count == 0)
            {
                slotDictionary.Remove(assignedSlot);
            }
        }

        hasAssignedSlot = false;
    }

    private void MoveToTarget()
    {
        if (!hasAssignedSlot) return;

        // 1. y축 아래 방향 이동 (기본 이동)
        Vector3 forwardMovement = Vector3.down * moveSpeed * Time.deltaTime;

        // 2. 슬롯 위치 조정 (x축으로만, y축은 건드리지 않음)
        Vector3 directionToSlot = (targetPosition - transform.position);
        directionToSlot.y = 0f; // y축 이동 제거
        directionToSlot = directionToSlot.normalized;

        Vector3 slotAdjustment = directionToSlot * moveSpeed * 0.3f * Time.deltaTime;

        // 3. 최종 이동
        Vector3 finalMovement = forwardMovement + slotAdjustment;
        transform.position += finalMovement;

        // 4. 목표 슬롯도 아래로 함께 이동 (대형 유지)
        targetPosition += Vector3.down * moveSpeed * Time.deltaTime;
        assignedSlot += Vector3.down * moveSpeed * Time.deltaTime;
    }

    private void CheckForBetterSlot()
    {
        // 더 좋은 슬롯이 있는지 확인
        Vector3 idealSlot = FindNearSlot(transform.position);

        float currentDistance = Vector3.Distance(transform.position, assignedSlot);
        float idealDistance = Vector3.Distance(transform.position, idealSlot);

        // 현재 슬롯보다 0.5f 이상 가까운 슬롯이 있으면 이동
        if (idealDistance < currentDistance - 0.5f)
        {
            AssignToSlot(idealSlot);
        }
    }

    // 몬스터가 죽거나 제거될 때 슬롯에서 해제
    private void OnDestroy()
    {
        UnregisterFromSlot();
    }

    // 디버그용 시각화
    private void OnDrawGizmosSelected()
    {
        if (!hasAssignedSlot) return;

        // 할당된 슬롯 표시
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(assignedSlot, Vector3.one * slotSize);

        // 목표 위치 표시
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(targetPosition, 0.1f);

        // 벽 감지 범위 표시
        Gizmos.color = isNearWall ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, monsterData.attackRange);
    }
}