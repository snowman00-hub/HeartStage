using UnityEngine;
using TMPro;

public class BattleTabUI : MonoBehaviour
{
    public RectTransform contentParent;  // Scroll View의 Content
    public GameObject stagePrefab;       // 무대(이미지) 프리팹
    public float verticalSpacing = 400f; // 세로 간격
    public float horizontalOffset = 300f; // 좌우 이동 거리
    public int totalChapters = 5;
    public int stagesPerChapter = 3;

    [ContextMenu("GenerateStages()")]
    public void GenerateStages()
    {
        // 기존에 생성된 오브젝트들 삭제 (중복 방지)
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(contentParent.GetChild(i).gameObject);
        }

        int index = 0;
        int totalStages = totalChapters * stagesPerChapter;

        for (int group = 1; group <= totalChapters; group++)
        {
            for (int stage = 1; stage <= stagesPerChapter; stage++)
            {
                string name = $"{group}-{stage}";

                GameObject stageObj = Instantiate(stagePrefab, contentParent);
                RectTransform rect = stageObj.GetComponent<RectTransform>();
                           
                float y = -index * verticalSpacing;
                float x = (index % 2 == 0) ? -horizontalOffset : horizontalOffset;

                rect.anchoredPosition = new Vector2(x, y);
                stageObj.name = $"Stage_{name}";

                var text = stageObj.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.text = name;

                index++;
            }
        }

        // Content의 높이를 자동으로 조정
        float contentHeight = totalStages * verticalSpacing;
        Vector2 size = contentParent.sizeDelta;
        size.y = contentHeight;
        contentParent.sizeDelta = size;
    }
}
