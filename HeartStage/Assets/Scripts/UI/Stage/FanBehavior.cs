using UnityEngine;

public class FanBehavior : MonoBehaviour
{
    private readonly string Walk = "Walk";
    private readonly string Idle = "Idle"; 

    private float walkSpeed = 1f;
    private Vector3 targetPosition;
    private Vector3 startPosition;
    private bool isWalking = false;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void StartFan()
    {
        animator.SetTrigger(Walk);
    }

    public void SpawnFanPosition()
    {
        var stagePosition = StageManager.Instance.GetCurrentStageData().stage_position;

        Vector3 fanPosition = Vector3.zero;

        switch (stagePosition)
        {
            case 1:
                break;
            case 2:
                break;
            case 3:
                fanPosition = new Vector3(0f, -4f, 0f);
                break;
            default:
                fanPosition = new Vector3(0f, -4f, 0f);
                break;
        }

        targetPosition = fanPosition;
    }

    public void SetupFan(int fanIndex, float speed, float spacing, int currentTotalFans)
    {
        walkSpeed = speed;

        // 스테이지 위치에 따른 기본 목표 지점 설정
        SpawnFanPosition();

        // 팬 인덱스와 기존 팬 수를 고려해서 최종 위치 설정
        SetFinalPosition(fanIndex, spacing, currentTotalFans);

        // 시작 위치 설정 (최종 위치 계산 후에 실행)
        SetStartPosition(fanIndex);

        SetFanSpriteDirection(fanIndex);

        // 걷기 시작
        StartWalking();
    }

    private void SetFinalPosition(int fanIndex, float spacing, int currentTotalFans)
    {
        // 웨이브 단계 계산 (4명씩 한 웨이브)
        int waveLevel = currentTotalFans / 4;

        // 각 웨이브마다 기존 팬들의 바깥쪽에 배치하기 위한 오프셋
        float baseOffset = waveLevel * 2f * spacing;

        float xOffset = 0f;

        if (fanIndex == 0) // 왼쪽 첫번째
        {
            xOffset = -(spacing * 0.5f + baseOffset);
        }
        else if (fanIndex == 1) // 왼쪽 두번째
        {
            xOffset = -(spacing * 1.5f + baseOffset);
        }
        else if (fanIndex == 2) // 오른쪽 첫번째  
        {
            xOffset = (spacing * 0.5f + baseOffset);
        }
        else if (fanIndex == 3) // 오른쪽 두번째
        {
            xOffset = (spacing * 1.5f + baseOffset);
        }

        targetPosition = new Vector3(targetPosition.x + xOffset, targetPosition.y, targetPosition.z);
    }

    private void SetStartPosition(int fanIndex)
    {
        // fanIndex 0, 1: 왼쪽에서 시작
        // fanIndex 2, 3: 오른쪽에서 시작
        bool startFromLeft = (fanIndex == 0 || fanIndex == 1);
        float startX = startFromLeft ? -10f : 10f;

        startPosition = new Vector3(startX, targetPosition.y, 0f);
        transform.position = startPosition;
    }

    private void StartWalking()
    {
        isWalking = true;
        animator.SetTrigger(Walk);
    }

    private void Update()
    {
        if (!isWalking) return;

        // 목표 지점으로 이동
        float step = walkSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isWalking = false;
            animator.SetTrigger("Idle");
        }
    }

    private void SetFanSpriteDirection(int fanIndex)
    {
        bool comingRight = (fanIndex == 2 || fanIndex == 3);

        if (comingRight)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }
}