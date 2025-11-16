using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoPanel : MonoBehaviour
{
	public Image itemImage;
	public TextMeshProUGUI itemName;
	public TextMeshProUGUI itemDescription;

    [HideInInspector]
	public int itemId = 7101;

    // 아이템 설명창 띄우기
    private void OnEnable()
    {
        var data = DataTableManager.ItemTable.Get(itemId);
        var texture = ResourceManager.Instance.Get<Texture2D>(data.prefab);
        itemImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        itemName.text = data.item_name;
        itemDescription.text = data.item_desc;
    }

    // 터치하면 닫기
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            gameObject.SetActive(false);
        }
    }
}
