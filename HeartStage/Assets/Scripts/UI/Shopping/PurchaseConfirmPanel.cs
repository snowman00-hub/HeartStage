using TMPro;
using UnityEngine;

public class PurchaseConfirmPanel : MonoBehaviour
{
    public static PurchaseConfirmPanel Instance;

    [SerializeField] private GameObject wholePanel;
    [SerializeField] private TextMeshProUGUI confirmText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI currentAmountText;

    private void Awake()
    {
        Instance = this;
    }

    public void Open(int shopTableID)
    {
        var shopTableData = DataTableManager.ShopTable.Get(shopTableID);
        confirmText.text = $"{shopTableData.Shop_item_name}\n을 구매하시겠습니까?";
        wholePanel.gameObject.SetActive(true);
    }
}