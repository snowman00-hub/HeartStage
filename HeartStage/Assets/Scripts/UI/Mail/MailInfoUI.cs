using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class MailInfoUI : GenericWindow
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button receiveRewardButton;

    [SerializeField] private Image mailIcon;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;

    [SerializeField] private Transform rewardItemParent;
    [SerializeField] private GameObject rewardItemPrefab;

    private MailData currentMailData;

    private void Awake()
    {
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        receiveRewardButton.onClick.AddListener(OnReceiveRewardClicked);
    }

    public void SetMailData(MailData mailData)
    {
        currentMailData = mailData;

        // 메일 기본 정보 설정
        titleText.text = mailData.title;
        contentText.text = mailData.content;

        // 메일 아이콘 상태 설정 (읽음/안읽음)
        SetMailIconState(mailData.isRead);

        // 메일을 읽음으로 표시
        if (!mailData.isRead)
        {
            mailData.isRead = true;
            MarkMailAsRead();
        }

        SetupRewardItems();
    }

    private void SetMailIconState(bool isRead)
    {
        if (mailIcon != null)
        {
            mailIcon.color = isRead ? Color.gray : Color.white;
        }
    }

    private void SetupRewardItems()
    {
        // 기존 보상 아이템 제거
        foreach (Transform child in rewardItemParent)
        {
            Destroy(child.gameObject);
        }

        // 보상 아이템 생성 (horizontal)
        if (currentMailData.itemList != null && currentMailData.itemList.Count > 0)
        {
            foreach (var item in currentMailData.itemList)
            {
                GameObject itemObj = Instantiate(rewardItemPrefab, rewardItemParent);
                var itemScript = itemObj.GetComponent<MailItemPrefab>();
                itemScript?.Setup(item);
            }

            receiveRewardButton.gameObject.SetActive(!currentMailData.isRewarded);
        }
        else
        {
            receiveRewardButton.gameObject.SetActive(false);
        }
    }

    private void OnReceiveRewardClicked()
    {
        OnReceiveRewardClickedAsync().Forget();
    }

    private async UniTaskVoid OnReceiveRewardClickedAsync()
    {
        if (currentMailData.isRewarded) return;

        // 아이템 지급 처리
        foreach (var item in currentMailData.itemList)
        {
            if (int.TryParse(item.itemId, out int itemId))
            {
                ItemInvenHelper.AddItem(itemId, item.count);
            }
        }

        currentMailData.isRewarded = true;
        receiveRewardButton.gameObject.SetActive(false);
    }

    private void MarkMailAsRead()
    {
        MarkMailAsReadAsync().Forget();
    }

    private async UniTaskVoid MarkMailAsReadAsync()
    {
        string userId = AuthManager.Instance.UserId;
        await MailManager.Instance.MarkAsReadAsync(userId, currentMailData.mailId);
    }

    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
    }

    private void OnCloseButtonClicked()
    {
        Close();
    }
}