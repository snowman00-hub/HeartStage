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

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);

        okButton.onClick.AddListener(() => OnClickOk().Forget());
        cancelButton.onClick.AddListener(Close);
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

    private void Close()
    {
        gameObject.SetActive(false);
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
        Close();
    }
}
