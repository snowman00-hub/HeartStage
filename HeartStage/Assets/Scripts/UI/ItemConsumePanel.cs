using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemConsumePanel : MonoBehaviour
{
    [HideInInspector]
    public int itemId;

    public Image itemIcon;
    public TextMeshProUGUI countText;

    private int itemCount;
    public int ItemCount
    {
        get { return itemCount; }
        set
        {
            itemCount = value;
            countText.text = $"{itemCount}";
        }
    }

    public void Open(int itemId)
    {
        gameObject.SetActive(true);
        this.itemId = itemId;
        var itemData = DataTableManager.ItemTable.Get(itemId);
        var texture = ResourceManager.Instance.Get<Texture2D>(itemData.prefab);
        itemIcon.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        ItemCount = 0;
    }

    public void PlusCount()
    {
        ItemCount++;
    }

    public void MinusCount()
    {
        ItemCount--;
    }

    public void MaxCount()
    {
        ItemCount = SaveLoadManager.Data.itemList[itemId];
    }

    public void MinCount()
    {
        ItemCount = 1;
    }
}
