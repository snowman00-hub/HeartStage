using UnityEngine;
using UnityEngine.UI;

public class StageCell : MonoBehaviour
{
    public bool isBuildable = true; // 바닥 가능한 구역인지
    public Image floorImage;        // 파란색 바닥
    public Image buffOverlayImage;  // 흰색 바닥 버프구역
    public void Refresh(bool floorOn)
    {
        floorImage.enabled = floorOn;
    }
}