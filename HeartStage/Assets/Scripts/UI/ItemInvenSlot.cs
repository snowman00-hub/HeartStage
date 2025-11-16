using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInvenSlot : MonoBehaviour
{
	public Image itemImage;
    public TextMeshProUGUI amountText;

    private int itemId;

    public void Init(int id, int amount)
    {
        // 스프라이트 세팅
        var texture = ResourceManager.Instance.Get<Texture2D>(DataTableManager.ItemTable.Get(id).prefab);
        itemImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        // 수량 세팅
        amountText.text = $"X{amount}";
        itemImage.enabled = true;
        amountText.enabled = true;
        itemId = id;
    }

    private void OnDisable()
    {
        itemImage.enabled = false;
        amountText.enabled = false;
    }

    // 아이템 슬롯 클릭시 아이템 설명창 띄우기
    public void OnItemImageClicked()
    {
        if (!itemImage.enabled)
            return;

        ItemInventoryUI.Instance.OpenItemInfoPanel(itemId);
    }
}
