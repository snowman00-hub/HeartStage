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
        gameObject.SetActive(false);

        okButton.onClick.AddListener(() => OnClickOk().Forget());
        cancelButton.onClick.AddListener(CloseInternal);
    }

    public void Open()
    {
        gameObject.SetActive(true);

        if (SaveLoadManager.Data is SaveDataV1 data)
        {
            inputField.text = data.nickname;  // 기존 닉 있으면 보여주기
        }
        else
        {
            inputField.text = "";
        }

        messageText.text = "사용할 닉네임을 입력해 주세요";
        inputField.ActivateInputField();
    }

    // 모달 패널에서 직접 부를 내부용 Close
    public void CloseInternal()
    {
        gameObject.SetActive(false);
    }

    // 프로필에서 "취소/확인"으로 닫을 때는 모달까지 같이 닫아야 함
    private void CloseWithModal()
    {
        CloseInternal();
        ProfileWindow.Instance?.HideModalPanel();
    }

    private async UniTaskVoid OnClickOk()
    {
        string raw = inputField.text;

        messageText.text = "확인 중입니다...";

        var (ok, error) = await NicknameService.TryChangeNicknameAsync(raw);

        if (!ok)
        {
            messageText.text = error;
            return;
        }

        messageText.text = "닉네임이 변경되었습니다.";

        // ✅ 프로필 텍스트 즉시 갱신
        if (ProfileWindow.Instance != null)
        {
            ProfileWindow.Instance.RefreshAll();
        }

        // ✅ 라이트 스틱 / 자원 UI 즉시 갱신
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.MoneyUISet();
        }

        CloseWithModal();
    }
}
