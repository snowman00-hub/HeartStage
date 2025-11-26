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

    private void Awake()
    {
        stageUiButton.onClick.AddListener(OnStageUiButtonClicked);
        homeUiButton.onClick.AddListener(OnLobbyHomeUiButtonClicked);
        gachaButton.onClick.AddListener(OnGachaButtonClicked);
    }

    private void OnStageUiButtonClicked()
    {
        windowManager.Open(WindowType.StageSelect);
        SoundManager.Instance.PlaySFX("Ui_click_01");
    }

    private void OnLobbyHomeUiButtonClicked()
    {
        windowManager.Open(WindowType.LobbyHome);
        SoundManager.Instance.PlaySFX("Ui_click_01");
    }

    private void OnGachaButtonClicked()
    {
        windowManager.OpenOverlay(WindowType.Gacha);
        SoundManager.Instance.PlaySFX("Ui_click_01");
    }
}