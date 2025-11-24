using UnityEngine;
using UnityEngine.UI;

public class GachaPercentageUI : GenericWindow
{
    [Header("Reference")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject percentageInfoPrefab;

    public override void Open()
    {
        base.Open();
    }
    public override void Close()
    {
        base.Close();
    }
    
}
