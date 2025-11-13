using Cysharp.Threading.Tasks;
using UnityEngine;

public class UIToWorldExample : MonoBehaviour
{
    public RectTransform[] uiRect;  // 변환할 UI RectTransform
    public GameObject[] objects;
    public Camera mainCam;         // 메인 카메라 (2D 카메라)

    async UniTaskVoid Start()
    {
        if (mainCam == null)
            mainCam = Camera.main;

        // 레이아웃 완료까지 한 프레임 양보
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

        Canvas canvas = uiRect.Length > 0 ? uiRect[0].GetComponentInParent<Canvas>() : null;
        if (canvas == null)
        {
            Debug.LogError("Canvas 없음");
            return;
        }

        for (int i = 0; i < uiRect.Length && i < objects.Length; i++)
        {
            if (uiRect[i] == null || objects[i] == null) continue;

            // UI 월드 위치
            Vector3 uiWorldPos = uiRect[i].position;

            Camera uiCam = null;
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera ||
                canvas.renderMode == RenderMode.WorldSpace)
            {
                uiCam = canvas.worldCamera;
            }

            Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(uiCam, uiWorldPos);

            float zDist = Mathf.Abs(mainCam.transform.position.z);
            Vector3 worldPos = mainCam.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, zDist)
            );
            worldPos.z = 0f;

            objects[i].transform.position = worldPos;
        }
    }
}