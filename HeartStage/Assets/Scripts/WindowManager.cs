using UnityEngine;
using System.Collections.Generic;

public class WindowManager : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private List<GenericWindow> windows;

    private WindowType defaultWindow = WindowType.TestWindow; // test
    public WindowType currentWindow { get; private set; }

    private void Start()
    {
        currentWindow = defaultWindow;

        foreach (var window in windows)
        {
            window.Init(this);
            window.gameObject.SetActive(false);
        }

        windows[(int)currentWindow].Open();
    }

    public void OpenOverlay(WindowType id)
    {
        // 이미 같은 타입의 오버레이가 열려있으면 열지 않음
        if (windows[(int)id].gameObject.activeSelf)
            return;

        // 현재 창은 닫지 않고 새 창만 열기
        windows[(int)id].Open();
    }
    public void Open(WindowType id)
    {
        windows[(int)currentWindow].Close();
        currentWindow = id;
        windows[(int)currentWindow].gameObject.SetActive(true);
        windows[(int)currentWindow].Open();
    }
}
