using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailPrefab : MonoBehaviour
{
    [SerializeField] private Image mailIcon;
    [SerializeField] private TextMeshProUGUI mailNameText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Button mailButton;

    public event Action<MailData> OnMailClicked;
    private MailData mailData;
    private Sprite currentMailSprite;

    private void Awake()
    {
        mailButton.onClick.AddListener(OnButtonClicked);
    }

    public void Setup(MailData data)
    {
        mailData = data;

        mailNameText.text = data.title;
        timeText.text = GetTimeString(data.timestamp);

        // 메일 이미지 상태 변경 (읽음/안읽음)
        SetMailIconState(data.isRead);
    }

    private void SetMailIconState(bool isRead)
    {
        if (mailIcon == null) return;

        // 기존 스프라이트 정리
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
    }

    private void OnButtonClicked()
    {
        OnMailClicked?.Invoke(mailData);
    }

    private string GetTimeString(long timestamp)
    {
        var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp); 
        var now = DateTimeOffset.UtcNow;
        var diff = now - dateTime;

        if (diff.TotalDays >= 1)
            return $"{(int)diff.TotalDays}일 전";
        else if (diff.TotalHours >= 1)
            return $"{(int)diff.TotalHours}시간 전";
        else
            return $"{(int)diff.TotalMinutes}분 전";
    }

    private void OnDestroy()
    {
        OnMailClicked = null; // 이벤트 정리

        if (currentMailSprite != null)
        {
            DestroyImmediate(currentMailSprite);
            currentMailSprite = null;
        }
    }

    public MailData GetMailData()
    {
        return mailData;
    }
}