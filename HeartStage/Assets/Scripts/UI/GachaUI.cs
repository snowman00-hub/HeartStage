using UnityEngine;
using UnityEngine.UI;

public class GachaUI : GenericWindow
{
    [Header("Button")]
    [SerializeField] private Button percentageInfoButton;
    [SerializeField] private Button gachaButton;
    [SerializeField] private Button gachaFiveButton;

    public override void Open()
    {
        base.Open();     
    }
    public override void Close()
    {
        base.Close();
    }


}

