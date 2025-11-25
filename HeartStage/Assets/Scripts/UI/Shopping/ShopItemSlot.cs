using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemSlot : MonoBehaviour
{
    public int shopTableID = 0;

    [SerializeField] private Image backgroundImage; // 나중에 변경할 수도?
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image currencyIcon;

    public void Init(int id)
    {
        shopTableID = id;
        var shopTableData = DataTableManager.ShopTable.Get(id);
        var itemData = DataTableManager.ItemTable.Get(shopTableData.Shop_item_type1); // 일단 1번 아이템만

        itemNameText.text = shopTableData.Shop_item_name;

        var texture = ResourceManager.Instance.Get<Texture2D>(itemData.prefab);
        itemImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        
        priceText.text = $"{shopTableData.Shop_price}";

        CurrencyIcon.CurrencyIconChange(currencyIcon, shopTableData.Shop_currency);
    }
}
