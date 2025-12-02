using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NicknameWindow : MonoBehaviour
{
    public static NicknameWindow Instance;

    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button okButton;
    [SerializeField] private Button cancelButton;

    private const int MinNickLength = 2;
    private const int MaxNickLength = 8;

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
            // 이미 닉이 있으면 기본값으로 보여주고, 없으면 빈칸
            inputField.text = data.nickname;
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
        string raw = inputField.text.Trim();

        // 길이 체크 (간단)
        int len = raw.Length;
        if (len < MinNickLength || len > MaxNickLength)
        {
            messageText.text = $"닉네임은 {MinNickLength}~{MaxNickLength}글자까지 가능합니다.";
            return;
        }

        // TODO: 욕설/금지어 필터
        // TODO: 닉네임 중복 체크 (나중 단계에서 NicknameService 붙이면 됨)

        if (SaveLoadManager.Data is not SaveDataV1 data)
        {
            messageText.text = "세이브 데이터를 찾을 수 없습니다.";
            return;
        }

        // 닉네임 저장
        data.nickname = raw;

        // 세이브 + publicProfiles 동기화
        await SaveLoadManager.SaveToServer();

        int achievementCount = AchievementUtil.GetCompletedAchievementCount(data);
        await PublicProfileService.UpdateMyPublicProfileAsync(data, achievementCount);

        Close();
    }
}
