using UnityEngine;
using UnityEngine.UI;

// 얘는 항상 Open 이니 GenericWindow X
public class LobbyUI : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private WindowManager windowManager;

    [Header("Button")]
    [SerializeField] private Button stageUiButton;
    [SerializeField] private Button homeUiButton;
    [SerializeField] private Button gachaButton;
    [SerializeField] private Button QuestButton;

    private void Awake()
    {
        stageUiButton.onClick.AddListener(OnStageUiButtonClicked);
        homeUiButton.onClick.AddListener(OnLobbyHomeUiButtonClicked);
        gachaButton.onClick.AddListener(OnGachaButtonClicked);
        QuestButton.onClick.AddListener(OnQuestButtonClicked);
    }

    private void OnStageUiButtonClicked()
    {
        windowManager.Open(WindowType.StageSelect);
        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Button_Click);
    }

    private void OnLobbyHomeUiButtonClicked()
    {
        windowManager.Open(WindowType.LobbyHome);
        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Button_Click);
    }

    private void OnGachaButtonClicked()
    {
        windowManager.OpenOverlay(WindowType.Gacha);
        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Button_Click);
    }
    private void OnQuestButtonClicked()
    { 
        windowManager.OpenOverlay(WindowType.Quest);
        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Button_Click);
    }
}