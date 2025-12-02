using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusMessageWindow : MonoBehaviour
{
    public static StatusMessageWindow Instance;

    [Header("UI")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button okButton;
    [SerializeField] private Button cancelButton;

    public bool IsOpen => gameObject.activeSelf;

    private void Awake()
    {
        Instance = this;

        gameObject.SetActive(false);

        if (okButton != null)
            okButton.onClick.AddListener(() => OnClickOk().Forget());

        if (cancelButton != null)
            cancelButton.onClick.AddListener(Close);
    }

    public void Open()
    {
        if (SaveLoadManager.Data is SaveDataV1 data && inputField != null)
            inputField.text = data.statusMessage;
        else if (inputField != null)
            inputField.text = "";

        if (messageText != null)
            messageText.text = "상태 메시지를 입력해 주세요.";

        gameObject.SetActive(true);
        inputField?.ActivateInputField();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        ProfileWindow.Instance?.OnPopupClosed();
    }

    public void Prewarm()
    {
        bool wasActive = gameObject.activeSelf;
        Open();
        gameObject.SetActive(wasActive);
    }

    private async UniTaskVoid OnClickOk()
    {
        if (inputField == null || messageText == null)
            return;

        string raw = inputField.text;

        if (!NicknameValidator.ValidateStatus(raw, out string error))
        {
            messageText.text = error;
            return;
        }

        if (SaveLoadManager.Data is not SaveDataV1 data)
        {
            messageText.text = "세이브 데이터를 찾을 수 없습니다.";
            return;
        }

        data.statusMessage = raw.Trim();

        await SaveLoadManager.SaveToServer();

        int achievementCount = AchievementUtil.GetCompletedAchievementCount(data);
        await PublicProfileService.UpdateMyPublicProfileAsync(data, achievementCount);

        ProfileWindow.Instance?.RefreshAll();

        Close();
    }
}
