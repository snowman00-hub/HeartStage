using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResetButton : MonoBehaviour
{
    [SerializeField] private Button resetButton;
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private StageSetupWindow stageSetupWindow;
    [SerializeField] private int resetStageId = -1; // -1이면 현재 스테이지
    [SerializeField] private TextMeshProUGUI resetMassage;

    private bool _isResetting;

    private void Start()
    {
        resetButton.onClick.AddListener(OnResetButtonClicked);
    }

    private async void OnResetButtonClicked()
    {
        if (_isResetting || monsterSpawner == null) return;
        _isResetting = true;
        resetButton.interactable = false;

        try
        {
            // 1) 몬스터/아군 정리
            monsterSpawner.DespawnAllMonsters();
            stageSetupWindow?.DespawnAllAllies();

            int targetId = resetStageId > 0 ? resetStageId : monsterSpawner.currentStageId;

            resetMassage.text = $"리셋중... {targetId}";

            await monsterSpawner.ChangeStage(targetId);

            // 3) 배치창 켜고 멈춤
            if (stageSetupWindow != null)
                stageSetupWindow.gameObject.SetActive(true);

            resetMassage.text = $"리셋완료 {targetId}";

            StageManager.Instance.SetTimeScale(0f);
        }
        finally
        {
            resetButton.interactable = true;
            _isResetting = false;
        }
    }
}
