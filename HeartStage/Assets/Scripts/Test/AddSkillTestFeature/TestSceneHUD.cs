using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 테스트 전용 HUD
/// - StageManager / MonsterSpawner 코드는 건드리지 않고
/// - TestStageManager(브리지)에서 SetWaveCount / SetRemainMonsterCount만 호출해주면 됨
/// </summary>
public class TestSceneHUD : MonoBehaviour
{
    [Header("Label")]
    [SerializeField] private TextMeshProUGUI stageLabel;
    [SerializeField] private TextMeshProUGUI timeScaleLabel;

    [Header("Wave / Monster")]
    [SerializeField] private TextMeshProUGUI waveCountText;          // 웨이브 표시
    [SerializeField] private TextMeshProUGUI remainMonsterCountText; // 남은 몬스터 표시

    [Header("TimeScale Buttons")]
    [SerializeField] private Button pauseButton;   // 0x
    [SerializeField] private Button slowButton;    // 0.5x

    [Header("Optional Buttons")]
    [SerializeField] private Button resetStageButton;    // 현재 스테이지 리로드

    [Header("Message (선택)")]
    [SerializeField] private TextMeshProUGUI messageLabel;

    private void Awake()
    {
        // TimeScale 컨트롤
        if (pauseButton != null)
            pauseButton.onClick.AddListener(() => SetTimeScale(0f));
        if (slowButton != null)
            slowButton.onClick.AddListener(() => SetTimeScale(0.5f));

        // 현재 스테이지 리로드
        if (resetStageButton != null)
            resetStageButton.onClick.AddListener(OnClickResetStage);
    }

    private void Start()
    {
        // 첫 진입 시 스테이지 라벨/타임스케일 표시
        int stageId = GetCurrentStageId();
        if (stageLabel != null)
            stageLabel.text = $"Stage : {stageId}";

        UpdateTimeScaleLabel();
    }

    private void OnEnable()
    {
        UpdateTimeScaleLabel();
    }

    // ───────────────────────────────── Stage / Wave / Monster 표시 ────────────────────────────────

    /// <summary>
    /// TestStageManager(or 기타 매니저)에서 호출해주는 웨이브 표시용
    /// </summary>
    public void SetWaveCount(int stageNumber, int waveOrder)
    {
        if (waveCountText == null)
            return;

        if (stageNumber == 0)
        {
            // 튜토리얼 같은 특수 케이스
            waveCountText.text = $"Tutorial\nWave {waveOrder}";
        }
        else
        {
            waveCountText.text = $"Stage {stageNumber}\nWave {waveOrder}";
        }
    }

    /// <summary>
    /// 남은 몬스터 수 표시
    /// </summary>
    public void SetRemainMonsterCount(int remainMonsterCount)
    {
        if (remainMonsterCountText != null)
            remainMonsterCountText.text = $"Mons: {remainMonsterCount.ToString()}";
    }

    // ───────────────────────────────── TimeScale ────────────────────────────────────────────────

    private void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        UpdateTimeScaleLabel();

        LogMessage($"TimeScale = {scale:0.##}");
    }

    private void UpdateTimeScaleLabel()
    {
        if (timeScaleLabel == null)
            return;

        float s = Time.timeScale;
        string label;

        if (Mathf.Approximately(s, 0f))
            label = "Pause";
        else if (Mathf.Approximately(s, 1f))
            label = "x1";
        else if (Mathf.Approximately(s, 0.2f))
            label = "x0.2";
        else if (Mathf.Approximately(s, 2f))
            label = "x2";
        else
            label = $"x{s:0.##}";

        timeScaleLabel.text = label;
    }

    // ───────────────────────────────── Stage / Scene 제어 ────────────────────────────────────────

    private int GetCurrentStageId()
    {
        // StageManager가 있으면 우선 사용,
        // 없으면 PlayerPrefs(SelectedStageID) → 기본값 601
        int id = StageManager.Instance?.GetCurrentStageData()?.stage_ID
                 ?? PlayerPrefs.GetInt("SelectedStageID", 601);
        if (id <= 0)
            id = 601;
        return id;
    }

    private void OnClickResetStage()
    {
        int targetId = GetCurrentStageId();
        LogMessage($"리셋(씬 리로드) : {targetId}");

        LoadSceneManager.Instance.GoTestStage(targetId, 1);
    }

    // ───────────────────────────────── Helper ────────────────────────────────────────────────────

    private void LogMessage(string msg)
    {
        if (messageLabel != null)
            messageLabel.text = msg;

        Debug.Log($"[TestSceneHUD] {msg}");
    }
}
