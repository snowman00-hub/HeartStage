using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ProfileModalPanel : MonoBehaviour, IPointerClickHandler
{
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
        // 배경 클릭 시 열려 있는 창들 닫기
        bool anyClosed = false;

        if (nicknameWindow != null && nicknameWindow.IsOpen)
        {
            nicknameWindow.Close();
            anyClosed = true;
        }

        if (statusMessageWindow != null && statusMessageWindow.IsOpen)
        {
            statusMessageWindow.Close();
            anyClosed = true;
        }

        if (iconChangeWindow != null && iconChangeWindow.IsOpen)
        {
            iconChangeWindow.Close();
            anyClosed = true;
        }

        if (anyClosed)
        {
            Hide();
        }
    }
}
