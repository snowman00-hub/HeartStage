using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraAutoScaler : MonoBehaviour
{
    [Header("기준 비율 (예: 9:16 = 0.5625)")]
    public float referenceAspect = 9f / 16f;
    [Header("기준 orthographicSize")]
    public float referenceOrthoSize = 9.6f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        UpdateCameraSize();
    }

    private void Update()
    {
#if UNITY_EDITOR
        UpdateCameraSize(); // 에디터에서 실시간 반영
#endif
    }

    // 카메라 OrthoGraphic Size 자동 조정
    private void UpdateCameraSize()
    {
        float currentAspect = (float)Screen.width / Screen.height;

        // 기준보다 가로가 넓은 경우 → 더 많이 보이게
        if (currentAspect > referenceAspect)
        {
            cam.orthographicSize = referenceOrthoSize;
        }
        else // 기준보다 세로가 긴 경우 → 더 넓게 보이게
        {
            float scale = referenceAspect / currentAspect;
            cam.orthographicSize = referenceOrthoSize * scale;
        }
    }
}
