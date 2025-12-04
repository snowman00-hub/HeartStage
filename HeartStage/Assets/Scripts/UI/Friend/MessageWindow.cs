using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageWindow : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Button")]
    [SerializeField] private Button closeButton;

    [Header("색상 대상")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private TextMeshProUGUI buttonLabel;
    [SerializeField] private Image frameImage;

    [Header("색상 설정")]
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color failColor = Color.red;
    [SerializeField] private Color neutralColor = Color.yellow;

    [SerializeField, Range(0f, 1f)]
    private float frameDarkenFactor = 0.75f;

    private void Awake()
    {
        if (root != null)
            root.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        // buttonImage를 안 넣어줬다면 closeButton.targetGraphic을 기본으로 사용
        if (buttonImage == null && closeButton != null)
        {
            buttonImage = closeButton.targetGraphic as Image;
        }
    }

    /// <summary>
    /// 중립(노란색) 알림
    /// </summary>
    public void Open(string title, string message)
    {
        SetTitle(title);
        SetMessage(message);
        ApplyColorMode(neutralColor);

        if (root != null)
            root.SetActive(true);
    }

    /// <summary>
    /// 성공/실패 알림
    /// </summary>
    public void Open(string title, string message, bool isSuccess)
    {
        SetTitle(title);
        SetMessage(message);
        ApplyColorMode(isSuccess ? successColor : failColor);

        if (root != null)
            root.SetActive(true);
    }

    /// <summary>
    /// 실패 전용
    /// </summary>
    public void OpenFail(string title, string message)
    {
        Open(title, message, false);
    }

    /// <summary>
    /// 성공 전용
    /// </summary>
    public void OpenSuccess(string title, string message)
    {
        Open(title, message, true);
    }

    public void SetTitle(string title)
    {
        if (titleText != null)
            titleText.text = title;
    }

    public void SetMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }

    private void ApplyColorMode(Color baseColor)
    {
        // 버튼은 전달된 색 그대로
        if (buttonImage != null)
            buttonImage.color = baseColor;

        // 프레임은 같은 색 계열에서 조금 더 어둡게
        if (frameImage != null)
        {
            Color dark = new Color(
                baseColor.r * frameDarkenFactor,
                baseColor.g * frameDarkenFactor,
                baseColor.b * frameDarkenFactor,
                frameImage.color.a
            );
            frameImage.color = dark;
        }

        if (buttonLabel != null)
        {
            buttonLabel.color = Color.white;
        }
    }

    public void Close()
    {
        if (root != null)
            root.SetActive(false);
    }
}