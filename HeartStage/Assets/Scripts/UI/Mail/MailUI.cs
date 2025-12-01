using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MailUI : GenericWindow
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button receiveAllButton;

    [SerializeField] private GameObject mailPrefab;
    [SerializeField] private Transform mailContentParent;
    [SerializeField] private ScrollRect mailScrollRect;

    [SerializeField] private MailInfoUI mailInfoUI;

    private List<MailData> currentMails = new List<MailData>(); // 현재 로드된 메일 데이터 리스트
    private List<MailPrefab> mailPrefabs = new List<MailPrefab>(); // 현재 생성된 메일 프리팹 리스트

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
    }

    public override void Open()
    {
        base.Open();
    }
    public override void Close()
    {
        base.Close();
    }

    private void OnExitButtonClicked()
    {
        SoundManager.Instance.PlayBGM(SoundName.SFX_UI_Exit_Button_Click);
        Close();
    }

    private void OnMailPrefabButtonClicked()
    {
        SoundManager.Instance.PlaySFX(SoundName.SFX_UI_Button_Click);
        WindowManager.Instance.OpenOverlay(WindowType.MailInfoUI);
    }
}
