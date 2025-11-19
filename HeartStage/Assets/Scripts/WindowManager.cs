using UnityEngine;
using System.Collections.Generic;

public class WindowManager : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private List<GenericWindow> windows;

    public WindowType currentWindow { get; private set; }

    private void Start()
    {
        currentWindow = WindowType.None;

        // null 체크를 포함한 초기화
        foreach (var window in windows)
        {
            if (window != null)
            {
                window.Init(this);
                // 로비 씬에서는 Lobby 윈도우만 활성화 상태 유지
                bool isLobbyWindow = window.GetComponent<LobbyUI>() != null;
                bool isLobbyScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Lobby";

                if (isLobbyScene && isLobbyWindow)
                {
                    // 로비 윈도우는 활성화 상태 유지
                    continue;
                }
                else
                {
                    window.gameObject.SetActive(false);
                }
            }
        }
    }

    public void OpenOverlay(WindowType id)
    {
        // 안전한 배열 접근
        if (!IsValidWindow(id)) return;

        // 이미 같은 타입의 오버레이가 열려있으면 열지 않음
        if (windows[(int)id].gameObject.activeSelf)
            return;

        windows[(int)id].Open();
    }

    public void Open(WindowType id)
    {
        if (!IsValidWindow(id)) return;

        // 현재 윈도우 닫기
        if (IsValidWindow(currentWindow))
        {
            windows[(int)currentWindow].Close();
        }

        currentWindow = id;
        windows[(int)currentWindow].gameObject.SetActive(true);
        windows[(int)currentWindow].Open();
    }

    private bool IsValidWindow(WindowType windowType)
    {
        int index = (int)windowType;
        return index >= 0 && index < windows.Count && windows[index] != null;
    }
}