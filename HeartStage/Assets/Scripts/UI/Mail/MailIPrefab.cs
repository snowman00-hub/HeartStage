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
        // 읽음/안읽음에 따른 이미지 색상 또는 투명도 변경
        if (mailIcon != null)
        {
            mailIcon.color = isRead ? Color.gray : Color.white;
        }
    }

    private void OnButtonClicked()
    {
        OnMailClicked?.Invoke(mailData);
    }

    private string GetTimeString(long timestamp)
    {
        var dateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
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
    }
}