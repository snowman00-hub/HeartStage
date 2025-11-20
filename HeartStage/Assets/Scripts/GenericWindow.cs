using UnityEngine;
using UnityEngine.EventSystems;

public class GenericWindow : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GameObject firstSelected;

    protected WindowManager manager;
    protected bool isOverlayWindow = false; // 오버레이 창인지 구분  

    public void Init(WindowManager mgr)
    {
        manager = mgr;
    }

    public void OnFocus()
    {
        EventSystem.current.SetSelectedGameObject(firstSelected);
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
        OnFocus();
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
    }
}