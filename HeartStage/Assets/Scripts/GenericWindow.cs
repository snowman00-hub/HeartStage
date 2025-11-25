using UnityEngine;
using UnityEngine.EventSystems;

public class GenericWindow : MonoBehaviour
{
    protected WindowManager manager;
    protected bool isOverlayWindow = false; // 오버레이 창인지 구분  

    public void Init(WindowManager mgr)
    {
        manager = mgr;
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
    }
}