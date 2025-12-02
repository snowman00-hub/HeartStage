using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class MailUI : GenericWindow
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button receiveAllButton;
    [SerializeField] private GameObject mailPrefab;
    [SerializeField] private Transform mailContentParent;
    [SerializeField] private MailInfoUI mailInfoUI;

    private List<MailData> currentMails = new List<MailData>();

    private void Awake()
    {
        closeButton.onClick.AddListener(OnExitButtonClicked);
        deleteButton.onClick.AddListener(OnDeleteReadMailsClicked);
        receiveAllButton.onClick.AddListener(OnReceiveAllItemsClicked);
    }

    private void OnEnable()
    {
        // 실시간 메일 수신 이벤트 구독
        if (MailManager.Instance != null)
        {
            MailManager.Instance.OnMailReceived += OnNewMailReceived;
        }
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (MailManager.Instance != null)
        {
            MailManager.Instance.OnMailReceived -= OnNewMailReceived;
        }
    }

    public override void Open()
    {
        base.Open();
        LoadUserMails().Forget();
    }

    public override void Close()
    {
        base.Close();
    }

    // 유저의 모든 메일 로드
    public async UniTaskVoid LoadUserMails()
    {
        if (AuthManager.Instance?.UserId == null) return;

        string userId = AuthManager.Instance.UserId;
        currentMails = await MailManager.Instance.GetUserMailsAsync(userId);
        RefreshMailList();
    }

    // 메일 목록 UI 새로고침
    public void RefreshMailList()
    {
        // 기존 프리팹 제거
        foreach (Transform child in mailContentParent)
        {
            Destroy(child.gameObject);
        }

        // 새로운 메일 프리팹 생성
        foreach (var mailData in currentMails)
        {
            CreateMailPrefab(mailData);
        }
    }

    // 개별 메일 프리팹 생성
    private void CreateMailPrefab(MailData mailData)
    {
        GameObject mailObj = Instantiate(mailPrefab, mailContentParent);
        MailPrefab mailPrefabScript = mailObj.GetComponent<MailPrefab>();

        if (mailPrefabScript != null)
        {
            mailPrefabScript.Setup(mailData);

            // 이벤트 중복 등록 방지
            mailPrefabScript.OnMailClicked -= OnMailPrefabClicked;
            mailPrefabScript.OnMailClicked += OnMailPrefabClicked;
        }
    }

    // 메일 클릭 시 상세 정보 표시
    private void OnMailPrefabClicked(MailData mailData)
    {
        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Button_Click);

        if (mailInfoUI != null)
        {
            mailInfoUI.SetMailData(mailData);
        }

        WindowManager.Instance.OpenOverlay(WindowType.MailInfoUI);
    }

    // 읽은 메일 삭제
    private async void OnDeleteReadMailsClicked()
    {
        string userId = AuthManager.Instance.UserId;

        for (int i = currentMails.Count - 1; i >= 0; i--)
        {
            var mail = currentMails[i];
            if (CanDeleteMail(mail))
            {
                await MailManager.Instance.DeleteMailAsync(userId, mail.mailId);
                currentMails.RemoveAt(i);
            }
        }

        RefreshMailList();
    }

    // 메일 삭제 가능 여부 확인
    private bool CanDeleteMail(MailData mail)
    {
        if (!mail.isRead) return false;
        bool hasRewards = mail.itemList?.Count > 0;
        return !hasRewards || mail.isRewarded;
    }

    private void OnReceiveAllItemsClicked()
    {
        OnReceiveAllItemsClickedAsync().Forget();
    }

    // 모든 메일의 보상 일괄 수령
    private async UniTaskVoid OnReceiveAllItemsClickedAsync()
    {
        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Button_Click);

        string userId = AuthManager.Instance.UserId;
        List<string> rewardedMailIds = new List<string>();

        foreach (var mail in currentMails)
        {
            if (mail.isRewarded) continue;

            if (mail.itemList?.Count > 0)
            {
                foreach (var item in mail.itemList)
                {
                    if (int.TryParse(item.itemId, out int itemId))
                    {
                        ItemInvenHelper.AddItem(itemId, item.count);
                    }
                }

                mail.isRewarded = true;
                rewardedMailIds.Add(mail.mailId);
            }
        }

        // 보상 수령 상태 업데이트
        if (rewardedMailIds.Count > 0)
        {
            await MailManager.Instance.UpdateMultipleRewardStatusAsync(userId, rewardedMailIds);
            RefreshMailList();
        }
    }

    // 메일 보상 수령 상태 업데이트
    public void UpdateMailRewardStatus(string mailId, bool isRewarded)
    {
        var mail = currentMails.Find(m => m.mailId == mailId);
        if (mail != null)
        {
            mail.isRewarded = isRewarded;
            if (gameObject.activeInHierarchy)
            {
                RefreshMailList();
            }
        }
    }

    // 새로운 메일이 실시간으로 수신되었을 때 호출
    private void OnNewMailReceived(MailData newMail)
    {
        if (AuthManager.Instance?.UserId != newMail.receiverId) return;

        // 중복 메일 체크
        bool alreadyExists = currentMails.Exists(m => m.mailId == newMail.mailId);
        if (!alreadyExists)
        {
            // 새 메일을 목록 맨 앞에 추가 (최신순)
            currentMails.Insert(0, newMail);

            // UI가 활성화되어 있을 때만 즉시 새로고침
            if (gameObject.activeInHierarchy)
            {
                RefreshMailList();
            }
        }
    }

    private void OnExitButtonClicked()
    {
        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Exit_Button_Click);
        Close();
    }
}