using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [SerializeField] private WindowManager windowManager;
    [SerializeField] private SpriteRenderer backGroundSprite;

    public StageUI StageUI;
    public LevelUpPanel LevelUpPanel;
    public Slider expSlider;
    public VictoryDefeatPanel VictoryDefeatPanel;
    [SerializeField] public StageCSVData currentStageCSVData;
    [SerializeField] public StageData currentStageData;

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
            StageUI.SetWaveCount(stageNumber, waveOrder); 
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
        //LoadSelectedStageData();
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

                SetBackgroundByStageData(stageData);

                // 현재 웨이브 설정
                int startingWave = PlayerPrefs.GetInt("StartingWave", 1);
                SetWaveInfo(stageData.stage_step1, startingWave);
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

        SoundManager.Instance.PlaySFX("Ui_click_01"); // test
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
    }

    public void SetCurrentStageData(StageCSVData stageData)
    {
        currentStageCSVData = stageData;
        if (stageData != null)
        {
            stageNumber = stageData.stage_step1;
            waveOrder = 1; // 스테이지 시작시 첫 번째 웨이브
            waveCount = 1;

            if (StageUI != null)
                StageUI.SetWaveCount(stageNumber, waveOrder);
        }
    }

    // 현재 스테이지 데이터 가져오기
    public StageCSVData GetCurrentStageData()
    {
        return currentStageCSVData;
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

        SoundManager.Instance.PlaySFX("Ui_reward_01"); // test
    }

    // 원래 타임스케일 복원
    public void RestoreTimeScale()
    {
        Time.timeScale = currentTimeScale;
    }

    public void CompleteStage()
    {
        Clear();
        SoundManager.Instance.PlaySFX("stageClearReward");
    }

    // 승리시 
    public void Clear() 
    {
        VictoryDefeatPanel.isClear = true;

        if (windowManager != null)
        {
            windowManager.OpenOverlay(WindowType.VictoryDefeat);
        }

        Time.timeScale = 0f;
    }

    // 패배시
    public void Defeat()
    {
        VictoryDefeatPanel.isClear = false;

        if (windowManager != null)
        {
            windowManager.OpenOverlay(WindowType.VictoryDefeat);
        }
        else
        {
            if (VictoryDefeatPanel != null)
            {
                VictoryDefeatPanel.gameObject.SetActive(true);
            }
        }

        Time.timeScale = 0f;
    }

    public void SetBackgroundByStageData(StageCSVData stageData)
    {

        if (stageData == null || string.IsNullOrEmpty(stageData.prefab) || stageData.prefab == "nan")
        {
            return;
        }

        if (backGroundSprite == null)
        {
            return;
        }

        // 먼저 Sprite로 시도
        var backgroundSprite = ResourceManager.Instance.Get<Sprite>(stageData.prefab);
        if (backgroundSprite != null)
        {
            backGroundSprite.sprite = backgroundSprite;
            return;
        }

        // Sprite가 없으면 Texture2D로 시도하고 Sprite로 변환
        var backgroundTexture = ResourceManager.Instance.Get<Texture2D>(stageData.prefab);
        if (backgroundTexture != null)
        {
            // Texture2D를 Sprite로 변환
            var sprite = Sprite.Create(
                backgroundTexture,
                new Rect(0, 0, backgroundTexture.width, backgroundTexture.height),
                new Vector2(0.5f, 0.5f)
            );
            backGroundSprite.sprite = sprite;
            return;
        }
    }
}