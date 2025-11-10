using UnityEngine;
using UnityEngine.UI;

public class StageCell : MonoBehaviour
{
    public bool isBuildable = true; // 바닥 가능한 구역인지
    public Image floorImage;        // 흰 바닥
    public Image buffOverlayImage;  // 버프/강조 효과(나중에)

    public void Refresh(bool floorOn)
    {
        floorImage.enabled = floorOn;
    }
}