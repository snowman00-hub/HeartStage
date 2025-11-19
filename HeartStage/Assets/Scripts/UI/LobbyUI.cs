using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : GenericWindow
{
    [Header("Reference")]
    [SerializeField] private WindowManager windowManager;

    [Header("Button")]
    [SerializeField] private Button stageUiButton;
    [SerializeField] private Button gameStartButton;

    private void Awake()
    {
        stageUiButton.onClick.AddListener(OnStageUiButtonClicked);
        gameStartButton.onClick.AddListener(() => OnGameStartButtonClicked());
    }

    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
    }

    private void OnStageUiButtonClicked()
    {
        windowManager.OpenOverlay(WindowType.StageSelect);
        SoundManager.Instance.PlaySFX("Ui_click_01");
    }

    private void OnGameStartButtonClicked()
    {
        // 테스트용: 강제로 튜토리얼로 리셋
        PlayerPrefs.SetInt("SelectedStageID", 601);
        PlayerPrefs.Save();

        LoadSceneManager.Instance.GoStage();
    }
}
