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
        goStageChoiceButton.onClick.AddListener(StageManager.Instance.GoLobby); // 일단 로비로 가게 설정
    }

    private void OnEnable()
    {
        Init();
    }

    private void Init()
    {
        nextStageOrRetryButton.onClick.RemoveAllListeners();
        int stageID = PlayerPrefs.GetInt("SelectedStageID", -1);
        // UI 갱신 스테이지 매니저 수정후 고치기
        // 보상 저장해 뒀다가 UI 적용하기
        //currentStageText.text = $"스테이지 {1}-{StageManager.Instance.stageNumber}"; //
        //clearWaveText.text = $"{StageManager.Instance.WaveCount}";//
        //addFansText.text = 

        if (isClear)
        {
            clearOrFailText.text = "Clear";
            rightButtonText.text = "다음\n스테이지";
            nextStageOrRetryButton.onClick.AddListener(() => OnNextStageButtonClicked());
        }
        else
        {
            clearOrFailText.text = "Fail";
            rightButtonText.text = "재도전";
            nextStageOrRetryButton.onClick.AddListener(LoadSceneManager.Instance.GoStage);
        }
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
            //monsterSpawner.ChangeStage(nextStage.stage_ID).Forget();

            Time.timeScale = 1f; 
            Close();
        }
    }
}