using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VictoryDefeatPanel : GenericWindow
{
    [SerializeField] private MonsterSpawner monsterSpawner;

    public GameObject powerInfoWindow; 

    public TextMeshProUGUI clearOrFailText;
    public TextMeshProUGUI currentStageText;
    public TextMeshProUGUI clearWaveText;
    public TextMeshProUGUI addFansText;
    public TextMeshProUGUI lightStickCount;
    public TextMeshProUGUI heartStickCount;
    public TextMeshProUGUI trainingPoint;
    public TextMeshProUGUI rightButtonText;

    public Button goStageChoiceButton;
    public Button nextStageOrRetryButton;

    public bool isClear = false;

    public override void Open()
    {
        base.Open();
        powerInfoWindow.SetActive(false);
    }

    public override void Close()
    {
        base.Close();
    }

    private void Start()
    {
        goStageChoiceButton.onClick.AddListener(OnGoStageChoiceButtonClicked);
    }

    private void OnEnable()
    {
        Init();
    }

    private void Init()
    {
        nextStageOrRetryButton.onClick.RemoveAllListeners();
        int stageID = PlayerPrefs.GetInt("SelectedStageID", -1);

        if(StageManager.Instance != null && StageManager.Instance.GetCurrentStageData() != null)
        {
            var currentStage = StageManager.Instance.GetCurrentStageData();
            currentStageText.text = $"스테이지 {currentStage.stage_step1}-{currentStage.stage_step2}";
            clearWaveText.text = $"{StageManager.Instance.WaveCount}";
        }

        if (isClear)
        {
            clearOrFailText.text = "Clear";
            rightButtonText.text = "다음\n스테이지";
            nextStageOrRetryButton.onClick.AddListener(() => OnNextStageButtonClicked());

            clearWaveText.text = $"{StageManager.Instance.WaveCount}";
        }
        else
        {
            clearOrFailText.text = "Fail";
            rightButtonText.text = "재도전";
            nextStageOrRetryButton.onClick.AddListener(LoadSceneManager.Instance.GoStage);

            clearWaveText.text = $"{StageManager.Instance.WaveCount - 1}";
        }

        var stageData = StageManager.Instance.currentStageCSVData;
        currentStageText.text = $"스테이지 {stageData.stage_step1}-{stageData.stage_step2}";
        addFansText.text = $"{StageManager.Instance.fanReward}";

        // 획득 아이템 표시
        lightStickCount.text = $"{ItemManager.Instance.lightStickCount}";
        if (ItemManager.Instance.acquireItemList.ContainsKey(ItemID.HeartStick))
        {
            heartStickCount.text = $"{ItemManager.Instance.acquireItemList[ItemID.HeartStick]}";
        }
        else
        {
            heartStickCount.text = "0";
        }
        if (ItemManager.Instance.acquireItemList.ContainsKey(ItemID.TrainingPoint))
        {
            trainingPoint.text = $"{ItemManager.Instance.acquireItemList[ItemID.TrainingPoint]}";
        }
        else
        {
            trainingPoint.text = "0";
        }
        //
    }

    private void OnNextStageButtonClicked()
    {
        if (monsterSpawner == null)
            return;

        var nextStage = monsterSpawner.GetNextStage();
        if(nextStage != null)
        {
            // 다음 스테이지 ID를 저장
            PlayerPrefs.SetInt("SelectedStageID", nextStage.stage_ID);
            PlayerPrefs.Save();

            // 스테이지 변경
            LoadSceneManager.Instance.GoStage();

            Time.timeScale = 1f; 
            Close();
        }
    }

    private void OnGoStageChoiceButtonClicked()
    {
        WindowManager.currentWindow = WindowType.StageSelect;
        LoadSceneManager.Instance.GoLobby();
    }
}