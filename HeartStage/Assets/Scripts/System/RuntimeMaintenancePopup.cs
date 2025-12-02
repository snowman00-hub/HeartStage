using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
public class RuntimeMaintenancePopup : MonoBehaviour
{
    private Canvas _canvas;

    [Header("루트 패널 (전체 배경 Panel)")]
    [SerializeField] private GameObject root;               // 전체 화면을 덮는 Background Panel

    [Header("메시지 텍스트")]
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("확인 버튼")]
    [SerializeField] private Button okButton;

    private Action _onOk;

    private void Awake()
    {
        // 자기 Canvas 세팅 (로딩UI처럼 맨 위에 깔리게)
        _canvas = GetComponent<Canvas>();
        if (_canvas == null)
            _canvas = gameObject.AddComponent<Canvas>();

        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.overrideSorting = true;
        _canvas.sortingOrder = 2000;   // 로딩창(1000)보다 위에 오게

        // 프리팹 Instantiate 되면 바로 보이도록
        if (root != null)
            root.SetActive(true);
    }

    /// <summary>
    /// 콜백 등록 + 버튼 세팅 (MaintenanceWatcher에서 호출)
    /// </summary>
    public void Init(Action onOk)
    {
        _onOk = onOk;

        if (okButton != null)
        {
            okButton.onClick.RemoveAllListeners();
            okButton.onClick.AddListener(OnClickOkInternal);
        }
    }

    public void SetMessage(string msg)
    {
        if (messageText != null)
        {
            messageText.text = msg;
        }
    }

    private void OnClickOkInternal()
    {
        _onOk?.Invoke();
    }
}
