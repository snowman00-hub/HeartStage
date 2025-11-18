using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    public StageUI StageUI;
    public LevelUpPanel LevelUpPanel;
    public Slider expSlider;
    public VictoryDefeatPanel VictoryDefeatPanel;
    private StageCsvData currentStageData;

    private float currentTimeScale = 1f;

    // 스테이지 관련 추가 한 것
    [HideInInspector]
    public int stageNumber = 1;
    [HideInInspector]
    public int waveOrder = 1;

    private int waveCount = 1;
    public int WaveCount
    {
        get { return waveCount; }
        set
        {
            waveCount = value;
            StageUI.SetWaveCount(waveCount);
        }
    }

    private int remainMonsterCount;
    public int RemainMonsterCount
    {
        get { return remainMonsterCount; }
        set
        {
            remainMonsterCount = value;
            StageUI.SetReaminMonsterCount(remainMonsterCount);
        }
    }

    private void Start()
    {
        // 저장된 스테이지 데이터 로드
        LoadSelectedStageData();
    }

    private void LoadSelectedStageData()
    {
        int stageID = PlayerPrefs.GetInt("SelectedStageID", -1);
        if (stageID != -1)
        {
            // DataTableManager를 통해 스테이지 데이터 로드
            var stageData = DataTableManager.StageTable.GetStage(stageID);
            if (stageData != null)
            {
                SetCurrentStageData(stageData);

                // 현재 웨이브 설정
                int startingWave = PlayerPrefs.GetInt("StartingWave", 1);
                SetWaveInfo(stageData.stage_step1, startingWave);
            }
            else
            {
                Debug.LogError($"스테이지 ID {stageID}에 해당하는 데이터를 찾을 수 없습니다.");
            }
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    public void GoLobby()
    {
        LoadSceneManager.Instance.GoLobby();
    }

    public void SetTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
        currentTimeScale = timeScale;
    }

    private void OnDestroy()
    {
        SetTimeScale(1f);
    }

    // 스테이지 관련 추가 한 것
    public void SetWaveInfo(int stage, int wave)
    {
        stageNumber = stage;
        waveOrder = wave;
        waveCount = wave; // 기존 호환성을 위해 유지

        if (StageUI != null)
        {
            StageUI.SetWaveCount(stageNumber, waveOrder);
        }
        else
        {
            Debug.LogWarning("StageUI가 null입니다!");
        }
    }

    public void SetCurrentStageData(StageCsvData stageData)
    {
        currentStageData = stageData;
        if (stageData != null)
        {
            stageNumber = stageData.stage_step1;
            waveOrder = 1; // 스테이지 시작시 첫 번째 웨이브
        }
    }

    // 현재 스테이지 데이터 가져오기
    public StageCsvData GetCurrentStageData()
    {
        return currentStageData;
    }

    // 경험치 얻기
    public void ExpGet(int value)
    {
        expSlider.value += value;
        if (expSlider.maxValue == expSlider.value)
        {
            expSlider.value = 0f;
            LevelUp();
        }
    }

    // 레벨업
    public void LevelUp()
    {
        Time.timeScale = 0f;
        LevelUpPanel.gameObject.SetActive(true);
    }

    // 원래 타임스케일 복원
    public void RestoreTimeScale()
    {
        Time.timeScale = currentTimeScale;
    }

    // 승리시 
    public void Clear()
    {
        VictoryDefeatPanel.isClear = true;
        VictoryDefeatPanel.gameObject.SetActive(true);
    }

    // 패배시
    public void Defeat()
    {
        VictoryDefeatPanel.isClear = false;
        VictoryDefeatPanel.gameObject.SetActive(true);
    }
}