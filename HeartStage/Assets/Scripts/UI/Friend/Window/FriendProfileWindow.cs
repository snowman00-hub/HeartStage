using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendProfileWindow : MonoBehaviour
{
    public static FriendProfileWindow Instance;

    [SerializeField] private GameObject root;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text fanText;
    [SerializeField] private TMP_Text mainStageText;
    [SerializeField] private TMP_Text achievementText;
    [SerializeField] private TMP_Text fanMeetingTimeText;
    //[SerializeField] private TMP_Text specialStageTimeText;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        Instance = this;
        root.SetActive(false);
        closeButton.onClick.AddListener(() => root.SetActive(false));
    }

    public void Open(string uid)
    {
        root.SetActive(true);
        LoadAsync(uid).Forget();
    }

    private async UniTaskVoid LoadAsync(string uid)
    {
        var data = await PublicProfileService.GetPublicProfileAsync(uid);
        if (data == null)
        {
            nicknameText.text = "알 수 없는 플레이어";
            return;
        }

        nicknameText.text = data.nickname;
        fanText.text = data.fanAmount.ToString("♥ 팬: 없음");

        if (DataTableManager.TitleTable != null && data.equippedTitleId != 0)
        {
            var t = DataTableManager.TitleTable.Get(data.equippedTitleId);
            titleText.text = t != null ? t.Title_name : $"달성한 업적: {data.equippedTitleId}개";
        }
        else
        {
            titleText.text = "달성한 업적: 0개";
        }

        if (data.mainStageStep1 <= 0)
            mainStageText.text = "메인 스테이지 진행도: 없음";
        else
            mainStageText.text = $"{data.mainStageStep1}-{data.mainStageStep2}";

        achievementText.text = $"{data.achievementCompletedCount}개";
        fanMeetingTimeText.text = FormatMMSS(data.bestFanMeetingSeconds);

        //추후 추가 예정
        //specialStageTimeText.text = FormatMMSS(data.specialStageBestSeconds);

        var sprite = ResourceManager.Instance.Get<Sprite>(data.profileIconKey);
        iconImage.sprite = sprite;
    }

    private string FormatMMSS(int sec)
    {
        if (sec <= 0) return "팬미팅 진행시간: 없음";
        return $"팬미팅 진행시간: {sec / 60:00}:{sec % 60:00}";
    }
}
