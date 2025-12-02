using UnityEngine;
using UnityEngine.EventSystems;

public class ProfileModalPanel : MonoBehaviour, IPointerClickHandler
{
    [Header("모달 안의 팝업들")]
    [SerializeField] private NicknameWindow nicknameWindow;
    [SerializeField] private StatusMessageWindow statusMessageWindow;
    [SerializeField] private IconChangeWindow iconChangeWindow;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 배경 눌렀을 때만 작동 (자식 버튼/Image가 RaycastTarget이면 거기서 먼저 먹음)
        CloseAllPopups();
        Hide();
    }

    public void CloseAllPopups()
    {
        if (nicknameWindow != null && nicknameWindow.IsOpen)
            nicknameWindow.CloseInternal();   // public으로 하나 만들어 둘 거임

        if (statusMessageWindow != null && statusMessageWindow.IsOpen)
            statusMessageWindow.CloseInternal();

        if (iconChangeWindow != null && iconChangeWindow.IsOpen)
            iconChangeWindow.CloseInternal();
    }
}
