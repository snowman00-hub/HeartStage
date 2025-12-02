using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusMessageWindow : MonoBehaviour
{
    public static StatusMessageWindow Instance;

    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button okButton;
    [SerializeField] private Button cancelButton;

    public bool IsOpen => gameObject.activeSelf;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);

        okButton.onClick.AddListener(() => OnClickOk().Forget());
        cancelButton.onClick.AddListener(CloseInternal);
    }

    public void Open()
    {
        gameObject.SetActive(true);

        if (SaveLoadManager.Data is SaveDataV1 data)
            inputField.text = data.statusMessage;
        else
            inputField.text = "";

        messageText.text = "상태 메시지를 입력해 주세요";
        inputField.ActivateInputField();
    }

    public void CloseInternal()
    {
        gameObject.SetActive(false);
    }

    private void CloseWithModal()
    {
        CloseInternal();
        ProfileWindow.Instance?.HideModalPanel();
    }

    private async UniTaskVoid OnClickOk()
    {
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
        // 프로필 UI 즉시 갱신
        ProfileWindow.Instance?.RefreshAll();

        CloseWithModal();
    }
}
