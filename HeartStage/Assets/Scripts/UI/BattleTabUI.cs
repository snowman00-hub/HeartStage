using UnityEngine;
using TMPro;

public class BattleTabUI : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform contentParent;  
    public GameObject stagePrefab; // 피벗 (0.5,0), 앵커 (0.5,0)    

    [Header("Layout Settings")]
    public float verticalSpacing = 400f; // 세로 간격
    public float horizontalOffset = 300f; // 좌우 번갈이 거리
    public int totalChapters = 5;         // 총 챕터 수
    public int stagesPerChapter = 3;      // 챕터당 스테이지 수
    public float verticalPadding = 100f;

    // 자식 오브젝트 삭제
    [ContextMenu("DeleteChildren")]
    public void DeleteChildren()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(contentParent.GetChild(i).gameObject);
        }
    }

    // 스테이지 이미지 간격에 맞게 생성
    [ContextMenu("GenerateStages()")]
    public void GenerateStages()
    {
        DeleteChildren();

        int index = 0;
        int totalStages = totalChapters * stagesPerChapter;

        // 스테이지 생성
        for (int group = 1; group <= totalChapters; group++)
        {
            for (int stage = 1; stage <= stagesPerChapter; stage++)
            {
                string name = $"{group} - {stage}";

                GameObject stageObj = Instantiate(stagePrefab, contentParent);
                RectTransform rect = stageObj.GetComponent<RectTransform>();

                // 아래에서 위로 쌓이게
                float y = index * verticalSpacing + verticalPadding;
                // 좌우 번갈아 배치
                float x = (index % 2 == 0) ? -horizontalOffset : horizontalOffset;

                rect.anchoredPosition = new Vector2(x, y);
                stageObj.name = $"{name}";

                // Text 표시
                var text = stageObj.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.text = name;

                index++;
            }
        }

        // Content의 높이 자동 조정
        float contentHeight = (totalStages - 1) * verticalSpacing + 500f;
        Vector2 size = contentParent.sizeDelta;
        size.y = contentHeight;
        contentParent.sizeDelta = size;
    }
}
