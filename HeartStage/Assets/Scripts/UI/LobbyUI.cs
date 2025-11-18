using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : GenericWindow
{
    [Header("Reference")]
    [SerializeField] private WindowManager windowManager;

    [Header("Button")]
    [SerializeField] private Button stageUiButton;

    private void Awake()
    {
        stageUiButton.onClick.AddListener(OnStageUiButtonClicked);
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
    }
}
