using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemAcquirePanel : MonoBehaviour
{
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemCount;
    public Image itemImage;

    public ItemConsumePanel itemConsumePanel;
    public ItemInfoPanel itemInfoPanel;

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        itemConsumePanel.gameObject.SetActive(false);
        itemInfoPanel.gameObject.SetActive(false);
    }

    public void Open(int itemID, int amount)
    {
        gameObject.SetActive(true);
        var itemData = DataTableManager.ItemTable.Get(itemID);
        itemName.text = itemData.item_name;
        itemCount.text = $"X{amount}";
        var texture = ResourceManager.Instance.Get<Texture2D>(itemData.prefab);
        itemImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    public void AcquireCharacter(int characterID)
    {
        gameObject.SetActive(true);
        var characterData = DataTableManager.CharacterTable.Get(characterID);
        itemName.text = characterData.char_name;
        itemCount.text = "X1";
        var texture = ResourceManager.Instance.Get<Texture2D>(characterData.card_imageName);
        itemImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        // 캐릭터 얻기
        CharacterHelper.AcquireCharacter(characterID, DataTableManager.CharacterTable);
        //
        ItemInventoryUI.Instance.ShowInventoryWithSorting();
    }
}