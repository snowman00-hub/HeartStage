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
    private Sprite currentMailSprite;

    [SerializeField] private MailUI mailUI;

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

        // 메일을 읽음으로 표시
        if (!mailData.isRead)
        {
            mailData.isRead = true;
            MarkMailAsRead();
        }

        SetMailIconState(true);
        SetupRewardItems();
        InitializeRewardButton();
    }

    private void SetMailIconState(bool isRead)
    {
        if (currentMailSprite != null)
        {
            DestroyImmediate(currentMailSprite);
            currentMailSprite = null;
        }

        string imageName = isRead ? "Mail-Open-100" : "Mail-Heart";
        var texture = ResourceManager.Instance.Get<Texture2D>(imageName);

        if (texture != null)
        {
            currentMailSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            mailIcon.sprite = currentMailSprite;
        }
        else
        {
            Debug.LogWarning($"메일 아이콘 이미지를 로드할 수 없습니다: {imageName}");
        }
    }

    private void SetupRewardItems()
    {
        // 기존 보상 아이템 제거
        foreach (Transform child in rewardItemParent)
        {
            Destroy(child.gameObject);
        }

        // 보상 아이템 생성
        if (currentMailData.itemList != null && currentMailData.itemList.Count > 0)
        {
            foreach (var item in currentMailData.itemList)
            {
                GameObject itemObj = Instantiate(rewardItemPrefab, rewardItemParent);
                var itemScript = itemObj.GetComponent<MailItemPrefab>();
                itemScript?.Setup(item);
            }
        }
    }

    private void InitializeRewardButton()
    {
        var buttonText = receiveRewardButton.GetComponentInChildren<TextMeshProUGUI>();

        // 보상이 있는 메일인지 확인
        if (currentMailData.itemList != null && currentMailData.itemList.Count > 0)
        {
            if (currentMailData.isRewarded)
            {
                // 이미 보상 받은 경우
                receiveRewardButton.interactable = false;

                if (buttonText != null)
                    buttonText.text = "수령 완료";
            }
            else
            {
                // 보상 받을 수 있는 경우
                receiveRewardButton.interactable = true;

                if (buttonText != null)
                    buttonText.text = "보상 수령";
            }
        }
        else
        {
            // 보상이 없는 경우
            receiveRewardButton.interactable = false;
        }
    }

    private void OnReceiveRewardClicked()
    {
        OnReceiveRewardClickedAsync().Forget();
    }

    private async UniTaskVoid OnReceiveRewardClickedAsync()
    {
        if (currentMailData.isRewarded) return;

        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Button_Click);

        // 아이템 지급 처리
        foreach (var item in currentMailData.itemList)
        {
            if (int.TryParse(item.itemId, out int itemId))
            {
                ItemInvenHelper.AddItem(itemId, item.count);
            }
        }

        currentMailData.isRewarded = true;

        // 서버에 보상 상태 업데이트
        string userId = AuthManager.Instance.UserId;
        await MailManager.Instance.UpdateRewardStatusAsync(userId, currentMailData.mailId, true);

        if (mailUI != null)
        {
            mailUI.UpdateMailRewardStatus(currentMailData.mailId, true);
        }

        // 버튼 상태 업데이트 (다시 InitializeRewardButton 호출)
        InitializeRewardButton();
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
        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Exit_Button_Click);
        Close();
    }

    private void OnDestroy()
    {
        if (currentMailSprite != null)
        {
            DestroyImmediate(currentMailSprite);
            currentMailSprite = null;
        }
    }
}