using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInvenSlot : MonoBehaviour
{
	public Image itemImage;
    public TextMeshProUGUI amountText;

    private int itemId;

    //public void Init(int id, int amount)
    //{
    //    // 스프라이트 세팅
    //    var texture = ResourceManager.Instance.Get<Texture2D>(DataTableManager.ItemTable.Get(id).prefab);
    //    itemImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    //    // 수량 세팅
    //    amountText.text = $"X{amount}";
    //    itemImage.enabled = true;
    //    amountText.enabled = true;
    //    itemId = id;
    //}

    public void Init(int id, int amount)
    {
        Debug.Log($"Init Slot: id={id}, amount={amount}");

        if (itemImage == null) Debug.LogError("itemImage is NULL!");
        if (amountText == null) Debug.LogError("amountText is NULL!");

        var itemData = DataTableManager.ItemTable.Get(id);
        if (itemData == null)
        {
            Debug.LogError($"ItemTable에 id {id} 없음!");
            return;
        }

        var texture = ResourceManager.Instance.Get<Texture2D>(itemData.prefab);
        if (texture == null)
        {
            Debug.LogError($"텍스처 로드 실패: {itemData.prefab}");
            return;
        }

        itemImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
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
