using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResetButton : MonoBehaviour
{
    [SerializeField] private Button resetButton;
    [SerializeField] private int resetStageId = -1; // -1이면 현재 스테이지
    [SerializeField] private TextMeshProUGUI resetMassage;

    private bool _isResetting;

    private void Start()
    {
        resetButton.onClick.AddListener(OnResetButtonClicked);
    }

    private void OnDestroy()
    {
        resetButton.onClick.RemoveListener(OnResetButtonClicked);
    }

    private void OnResetButtonClicked()
    {
        if (_isResetting) return;
        _isResetting = true;
        resetButton.interactable = false;

        int targetId = resetStageId > 0 ? resetStageId :
        (StageManager.Instance?.GetCurrentStageData()?.stage_ID
        ?? PlayerPrefs.GetInt("SelectedStageID", 601));

        if (resetMassage != null)
            resetMassage.text = $"리셋(씬 리로드) : {targetId}";

        // prefs 저장 + Stage 씬 다시 로드
        LoadSceneManager.Instance.GoTestStage(targetId, 1);
    }
}
