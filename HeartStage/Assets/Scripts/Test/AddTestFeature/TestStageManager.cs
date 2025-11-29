using UnityEngine;

/// <summary>
/// TestScene 전용 어댑터 매니저.
/// - MonsterSpawner / StageManager 코드는 전혀 수정하지 않고,
///   StageManager가 들고 있는 정보를 TestSceneHUD로 전달만 해준다.
/// </summary>
public class TestStageManager : MonoBehaviour
{
    [Header("테스트 HUD (비워두면 자동으로 찾음)")]
    [SerializeField] private TestSceneHUD testHud;

    private int _lastStageNumber = -1;
    private int _lastWaveOrder = -1;
    private int _lastRemainCount = -1;

    private StageManager _stageManager;

    private void Update()
    {
        if (testHud == null)
            return;

        // 아직 StageManager를 못 잡았으면 계속 시도
        if (_stageManager == null)
        {
            if (StageManager.Instance == null)
                return;

            _stageManager = StageManager.Instance;
            ForceUpdateHud();
            return;
        }

        // StageManager에서 현재 값 읽기
        int currentStageNumber = _stageManager.stageNumber;
        int currentWaveOrder = _stageManager.waveOrder;
        int currentRemain = _stageManager.RemainMonsterCount;

        // 스테이지/웨이브 변경 감지
        if (currentStageNumber != _lastStageNumber ||
            currentWaveOrder != _lastWaveOrder)
        {
            _lastStageNumber = currentStageNumber;
            _lastWaveOrder = currentWaveOrder;

            testHud.SetWaveCount(currentStageNumber, currentWaveOrder);
        }

        // 남은 몬스터 수 변경 감지
        if (currentRemain != _lastRemainCount)
        {
            _lastRemainCount = currentRemain;
            testHud.SetRemainMonsterCount(currentRemain);
        }
    }

    private void ForceUpdateHud()
    {
        if (_stageManager == null || testHud == null)
            return;

        _lastStageNumber = _stageManager.stageNumber;
        _lastWaveOrder = _stageManager.waveOrder;
        _lastRemainCount = _stageManager.RemainMonsterCount;

        testHud.SetWaveCount(_lastStageNumber, _lastWaveOrder);
        testHud.SetRemainMonsterCount(_lastRemainCount);
    }
}
