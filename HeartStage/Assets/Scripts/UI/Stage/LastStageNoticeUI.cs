using UnityEngine;
using UnityEngine.UI;

public class LastStageNoticeUI : GenericWindow
{
    [SerializeField] private Button lobbyButton;

    private void Awake()
    {
        lobbyButton.onClick.AddListener(OnLobbyButtonClicked);
    }

    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
    }

    private void OnLobbyButtonClicked()
    {
        Close();
        WindowManager.currentWindow = WindowType.LobbyHome;
        GameSceneManager.ChangeScene(SceneType.LobbyScene);
    }
}
