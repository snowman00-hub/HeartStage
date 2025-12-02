using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NicknameWindow : MonoBehaviour
{
    public static NicknameWindow Instance;

    [Header("UI")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button okButton;
    [SerializeField] private Button cancelButton;

    public bool IsOpen => gameObject.activeSelf;

    private void Awake()
    {
        Instance = this;

        // 시작은 꺼진 상태
        gameObject.SetActive(false);

        if (okButton != null)
            okButton.onClick.AddListener(() => OnClickOk().Forget());

        if (cancelButton != null)
            cancelButton.onClick.AddListener(Close);
    }

    public void Open()
    {
        if (SaveLoadManager.Data is SaveDataV1 data && inputField != null)
        {
            inputField.text = data.nickname;
        }

        if (messageText != null)
            messageText.text = "사용할 닉네임을 입력해 주세요.";

        gameObject.SetActive(true);
        inputField?.ActivateInputField();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        ProfileWindow.Instance?.OnPopupClosed();
    }

    // 로딩에서 예열용
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

        messageText.text = "확인 중입니다...";

        var (ok, error) = await NicknameService.TryChangeNicknameAsync(raw);

        if (!ok)
        {
            messageText.text = error;
            return;
        }

        messageText.text = "닉네임이 변경되었습니다.";

        // 프로필 UI / 로비 재화 UI 갱신
        ProfileWindow.Instance?.RefreshAll();
        LobbyManager.Instance?.MoneyUISet();

        Close();
    }
}
