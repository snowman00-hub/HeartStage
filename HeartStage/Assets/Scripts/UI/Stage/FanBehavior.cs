using UnityEngine;

public class FanBehavior : MonoBehaviour
{
    private readonly string Walk = "Walk";
    private float walkSpeed = 2f;
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
                fanPosition = new Vector3(0f, 6f, 0f);
                break;
            case 2:
                fanPosition = new Vector3(0f, 1f, 0f); // 중앙 스테이지용 위치
                break;
            case 3:
                fanPosition = new Vector3(0f, -3f, 0f);
                break;
            default:
                fanPosition = new Vector3(0f, -3f, 0f);
                break;
        }

        targetPosition = fanPosition;
    }

    private void DownPosition(int fanIndex, float spacing)
    {
        // 짝수는 왼쪽, 홀수는 오른쪽에서 시작
        bool startFromLeft = (fanIndex % 2) == 0;

        // Y 위치는 목표 위치와 같게, X는 화면 밖에서 시작
        float startX = startFromLeft ? -10f : 10f;

        // 팬들이 겹치지 않도록 Y 위치에 약간의 랜덤 오프셋 추가
        float yOffset = Random.Range(-spacing, spacing);

        startPosition = new Vector3(startX, targetPosition.y + yOffset, 0f);
        transform.position = startPosition;
    }

    public void SetupFan(int fanIndex, float speed, float spacing)
    {
        walkSpeed = speed;

        // 스테이지 위치에 따른 목표 지점 설정
        SpawnFanPosition();

        // 팬 인덱스에 따라 시작 위치 설정 (양쪽에서 번갈아가며)
        DownPosition(fanIndex, spacing);

        // 걷기 시작
        StartWalking();
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
        }
    }
}