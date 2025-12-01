using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 🔹 using UnityEngine.UI; 만 쓰면 됨

public class NoticeItemUI : MonoBehaviour
{
    [Header("헤더 영역 (항상 보이는 부분)")]
    [SerializeField] private Button headerButton;          // 카드 전체 클릭용
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI summaryText;
    [SerializeField] private TextMeshProUGUI dateText;

    [Header("펼쳐지는 본문 영역")]
    [SerializeField] private GameObject bodyRoot;          // 펼쳐질 컨테이너
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private Button cafeButton;            // "카페에서 자세히 보기"

    [Header("레이아웃 갱신용 (상위 Content 등)")]
    [SerializeField] private RectTransform layoutRoot;     // 보통 Content의 RectTransform

    private NoticeData _data;
    private bool _expanded = false;

    public void Init(NoticeData data)
    {
        _data = data;

        if (titleText != null)
            titleText.text = data.title;

        if (summaryText != null)
        {
            if (!string.IsNullOrEmpty(data.summary))
            {
                summaryText.text = data.summary;
            }
            else
            {
                // summary가 비어 있으면 body 첫 줄만 가져다 씀
                var lines = (data.body ?? "").Split(new[] { '\n' }, StringSplitOptions.None);
                summaryText.text = lines.Length > 0 ? lines[0] : "";
            }
        }

        if (dateText != null)
        {
            // createdAt이 "2025-12-01T12:00:00+09:00" 형식이면 앞부분만 잘라 써도 됨
            if (!string.IsNullOrEmpty(data.createdAt))
            {
                // 날짜만 보여주고 싶으면:
                int idx = data.createdAt.IndexOf('T');
                dateText.text = (idx > 0) ? data.createdAt.Substring(0, idx) : data.createdAt;
            }
            else
            {
                dateText.text = "";
            }
        }

        if (bodyText != null)
            bodyText.text = data.body ?? "";

        // 처음에는 접힌 상태
        _expanded = false;
        if (bodyRoot != null)
            bodyRoot.SetActive(false);

        // 헤더 클릭 → 펼치기/접기 토글
        if (headerButton != null)
        {
            headerButton.onClick.RemoveAllListeners();
            headerButton.onClick.AddListener(ToggleExpanded);
        }

        // 카페 버튼 세팅
        if (cafeButton != null)
        {
            bool hasUrl = !string.IsNullOrEmpty(data.externalUrl);
            cafeButton.gameObject.SetActive(hasUrl);
            cafeButton.onClick.RemoveAllListeners();

            if (hasUrl)
            {
                cafeButton.onClick.AddListener(() =>
                {
                    Application.OpenURL(data.externalUrl);
                });
            }
        }
    }

    private void ToggleExpanded()
    {
        _expanded = !_expanded;

        if (bodyRoot != null)
            bodyRoot.SetActive(_expanded);

        // 레이아웃 강제로 다시 계산 (세로로 쫙 늘어나게)
        if (layoutRoot != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
        }
        else
        {
            // 혹시 안 넣었으면 자기 부모 기준으로라도 한 번
            var parent = transform.parent as RectTransform;
            if (parent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
            }
        }
    }
}
