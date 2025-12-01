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

    public override void Open()
    {
        base.Open();
        LoadUserMails();
    }

    public override void Close()
    {
        base.Close();
    }

    private async void LoadUserMails()
    {
        string userId = AuthManager.Instance.UserId;
        currentMails = await MailManager.Instance.GetUserMailsAsync(userId);
        RefreshMailList();
    }

    private void RefreshMailList()
    {
        // 기존 프리팹 제거
        foreach (Transform child in mailContentParent)
        {
            Destroy(child.gameObject);
        }

        // 새로운 메일 프리팹 생성
        foreach (var mailData in currentMails)
        {
            GameObject mailObj = Instantiate(mailPrefab, mailContentParent);
            MailPrefab mailPrefabScript = mailObj.GetComponent<MailPrefab>();

            mailPrefabScript.Setup(mailData);

            // 이벤트 중복 등록 방지
            mailPrefabScript.OnMailClicked -= OnMailPrefabClicked; // 먼저 제거
            mailPrefabScript.OnMailClicked += OnMailPrefabClicked;
        }
    }

    private void OnMailPrefabClicked(MailData mailData)
    {
        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Button_Click);

        if (mailInfoUI != null)
        {
            mailInfoUI.SetMailData(mailData);
        }

        WindowManager.Instance.OpenOverlay(WindowType.MailInfoUI);
    }

    private async void OnDeleteReadMailsClicked()
    {
        string userId = AuthManager.Instance.UserId;

        for (int i = currentMails.Count - 1; i >= 0; i--)
        {
            if (currentMails[i].isRead)
            {
                await MailManager.Instance.DeleteMailAsync(userId, currentMails[i].mailId);
                currentMails.RemoveAt(i);
            }
        }

        RefreshMailList();
    }

    private void OnReceiveAllItemsClicked()
    {
        OnReceiveAllItemsClickedAsync().Forget();
    }

    private async UniTaskVoid OnReceiveAllItemsClickedAsync()
    {
        string userId = AuthManager.Instance.UserId;

        foreach (var mail in currentMails)
        {
            if (!mail.isRewarded && mail.itemList.Count > 0)
            {
                foreach (var item in mail.itemList)
                {
                    if (int.TryParse(item.itemId, out int itemId))
                    {
                        ItemInvenHelper.AddItem(itemId, item.count);
                    }
                }

                mail.isRewarded = true;
            }
        }

        RefreshMailList();
    }

    private void OnExitButtonClicked()
    {
        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Exit_Button_Click);
        Close();
    }
}