using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VictoryDefeatPanel : MonoBehaviour
{
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

    private void Start()
    {
        goStageChoiceButton.onClick.AddListener(StageManager.Instance.GoLobby); // 일단 로비로 가게 설정
    }

    private void OnEnable()
    {
        nextStageOrRetryButton.onClick.RemoveAllListeners();
        // UI 갱신 스테이지 매니저 수정후 고치기
        // 보상 저장해 뒀다가 UI 적용하기
        //currentStageText.text = $"스테이지 {1}-{StageManager.Instance.stageNumber}"; //
        //clearWaveText.text = $"{StageManager.Instance.WaveCount}";//
        //addFansText.text = 

        if (isClear)
        {
            clearOrFailText.text = "Clear";
            rightButtonText.text = "다음\n스테이지";
            //nextStageOrRetryButton.onClick.AddListener(); 다음 스테이지 시작하는 함수 만들기
        }
        else
        {
            clearOrFailText.text = "Fail";
            rightButtonText.text = "재도전";
            nextStageOrRetryButton.onClick.AddListener(LoadSceneManager.Instance.GoStage);
        }
    }
}
