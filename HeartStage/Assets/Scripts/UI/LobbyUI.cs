using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private WindowManager windowManager;

    [Header("Button")]
    [SerializeField] private Button stageUiButton;
    [SerializeField] private Button homeUiButton;
    [SerializeField] private Button gachaButton;
    [SerializeField] private Button QuestButton;

    [Header("ImageIcon")]
    [SerializeField] private Image playerProfileIcon;

    private void Awake()
    {
        stageUiButton.onClick.AddListener(OnStageUiButtonClicked);
        homeUiButton.onClick.AddListener(OnLobbyHomeUiButtonClicked);
        gachaButton.onClick.AddListener(OnGachaButtonClicked);
        QuestButton.onClick.AddListener(OnQuestButtonClicked);
    }

    private void Start()
    {
        // 로비 처음 들어왔을 때 현재 프로필 아이콘으로 세팅
        RefreshProfileIcon();
    }

    private void OnStageUiButtonClicked()
    {
        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Button_Click);

        windowManager.Open(WindowType.StageSelect);
    }

    private void OnLobbyHomeUiButtonClicked()
    {
        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Button_Click);

        windowManager.OpenOverlay(WindowType.LobbyHome);
    }

    private void OnGachaButtonClicked()
    {
        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Button_Click);

        windowManager.OpenOverlay(WindowType.Gacha);
    }

    private void OnQuestButtonClicked()
    {
        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Button_Click);

        windowManager.OpenOverlay(WindowType.Quest);
    }

    /// SaveData의 profileIconKey 기준으로 로비 프로필 아이콘 갱신
    public void RefreshProfileIcon()
    {
        if (playerProfileIcon == null)
            return;

        var data = SaveLoadManager.Data as SaveDataV1;
        if (data == null)
            return;

        string key = data.profileIconKey;

        // 혹시 비어있으면 기본 아이콘 하나 지정 (기존에 쓰던 키로 맞춰줘)
        if (string.IsNullOrEmpty(key))
            key = "hanaicon";

        var sprite = ResourceManager.Instance.GetSprite(key);
        if (sprite != null)
        {
            playerProfileIcon.sprite = sprite;
        }
    }
}
