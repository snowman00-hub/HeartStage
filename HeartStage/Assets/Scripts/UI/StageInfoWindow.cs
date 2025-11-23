using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageInfoWindow : GenericWindow
{
    [Header("Reference")]
    [SerializeField] private WindowManager windowManager;    

    [Header("Button")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button stageStartButton;
    //[SerializeField] private Button nextButton;
    //[SerializeField] private Button backButton;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI stageStepText;
    [SerializeField] private TextMeshProUGUI stageNameText;

    [Header("Field")]
    private StageTable stageTable;
    private StageCSVData currentStageData; // 현재 선택된 스테이지 데이터

    private void Awake()
    {
        isOverlayWindow = true; // 오버레이 창으로 설정
        closeButton.onClick.AddListener(() => OnCloseButtonClicked());
        stageStartButton.onClick.AddListener(() => OnStageStartButtonClicked());
    }
    public override void Open()
    {
        base.Open();
        UpdateText();
    }

    public override void Close()
    {
        base.Close();
    }

    private void OnCloseButtonClicked()
    {
        Close();
    }

    public void SetStageData(StageCSVData stageData)
    {
        currentStageData = stageData;
        if(gameObject.activeInHierarchy)
        {
            UpdateText();
        }
    }
    private void UpdateText()
    {
        var sb = new StringBuilder();
        sb.Clear();
        sb.Append($"스테이지 번호 ({currentStageData.stage_step1} - {currentStageData.stage_step2})");
        stageStepText.text = sb.ToString();

        sb.Clear();
        sb.Append($"{currentStageData.stage_name}");
        stageNameText.text = sb.ToString();
    }


    private void OnStageStartButtonClicked()
    {
        // 드림 에너지 소모

        if (currentStageData == null)
        {
            return;
        }

        // 스테이지 데이터를 저장
        SaveSelectedStageData();

        // 게임 씬으로 전환
        StartStage();
    }
    private void SaveSelectedStageData()
    {
        // 선택된 스테이지 정보를 저장
        PlayerPrefs.SetInt("SelectedStageID", currentStageData.stage_ID);
        PlayerPrefs.SetInt("SelectedStageStep1", currentStageData.stage_step1);
        PlayerPrefs.SetInt("SelectedStageStep2", currentStageData.stage_step2);
        PlayerPrefs.SetInt("StartingWave", 1); // 첫 번째 웨이브부터 시작
        PlayerPrefs.Save();
    }
    private void StartStage()
    {
        // LoadSceneManager를 사용하여 게임 씬으로 전환
        if (LoadSceneManager.Instance != null)
        {
            LoadSceneManager.Instance.GoStage();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Stage");
        }
    }
    private StageCSVData GetNextStage()
    {
        if(currentStageData == null || stageTable == null)
            return null;

        var allStages = stageTable.GetOrderedStages();

        // 1. 현재 스테이지와 같은 챕터의 다음 스테이지를 먼저 찾기
        for (int i = 0; i < allStages.Count; i++)
        {
            var stage = allStages[i];   
            if (stage.stage_step1 == currentStageData.stage_step1 &&
                stage.stage_step2 == currentStageData.stage_step2 + 1)
            {
                return stage;
            }
        }

        // 2. 같은 챕터에 다음 스테이지가 없으면 다음 챕터의 첫 번째 스테이지 찾기
        for (int i = 0; i < allStages.Count; i++)
        {
            var stage = allStages[i];   
            if (stage.stage_step1 == currentStageData.stage_step1 + 1 &&
                stage.stage_step2 == 1)
            {
                return stage;
            }
        }
        return null;
    }
    private StageCSVData GetPreviousStage()
    {
        if (currentStageData == null || stageTable == null) return null;

        var allStages = stageTable.GetOrderedStages();

        // 1. 현재 스테이지와 같은 챕터의 이전 스테이지를 먼저 찾기
        if (currentStageData.stage_step2 > 1)
        {
            for (int i = 0; i < allStages.Count; i++)
            {
                var stage = allStages[i];
                if (stage.stage_step1 == currentStageData.stage_step1 &&
                    stage.stage_step2 == currentStageData.stage_step2 - 1)
                {
                    return stage;
                }
            }
        }

        return null;
    }

    private void OnNextButtonClicked()
    {
        var nextStage = GetNextStage();
        if (nextStage != null)
        {
            SetStageData(nextStage);
        }
    }

    private void OnBackButtonClicked()
    {
        var previousStage = GetPreviousStage();
        if (previousStage != null)
        {
            SetStageData(previousStage);
        }
    }
}
