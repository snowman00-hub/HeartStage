using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ResetButton : MonoBehaviour
{
    [SerializeField] private Button resetButton;
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private StageSetupWindow stageSetupWindow;
    [SerializeField] private int resetStageId = 601;

    private void Start()
    {
        resetButton.onClick.AddListener(OnResetButtonClicked);
    }

    private async void OnResetButtonClicked()
    {
        if (resetButton == null || monsterSpawner == null || stageSetupWindow == null || StageManager.Instance == null)
            return;

        resetButton.interactable = false;

        try
        {
            // 0) 배치창 먼저 띄워서 "바로 보이게"
            stageSetupWindow.gameObject.SetActive(true);

            // 1) OnEnable이 timeScale=0 해버렸으니
            //    ChangeStage 로딩이 멈추지 않게 잠깐 1로 복구
            StageManager.Instance.SetTimeScale(1f);

            // 2) 몬스터/아군 정리
            monsterSpawner.DespawnAllMonsters();
            stageSetupWindow.DespawnAllAllies();

            // 3) 스테이지 갈아끼우기 (timeScale 1 상태에서)
            await monsterSpawner.ChangeStage(resetStageId);

            // 4) ChangeStage 끝났으니 배치창 기준으로 다시 멈춤
            StageManager.Instance.SetTimeScale(0f);

            // 5) 배치창에 새 스테이지 내용 반영(안 하면 이전 스테이지가 보일 수 있음)
            var stageCsv = DataTableManager.StageTable.GetStage(resetStageId);
            stageSetupWindow.ApplyStage(stageCsv);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            resetButton.interactable = true;
        }
    }
}
