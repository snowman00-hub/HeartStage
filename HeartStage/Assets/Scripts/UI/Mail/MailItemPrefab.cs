using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailItemPrefab : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemCountText;

    public void Setup(ItemAttachment itemAttachment)
    {
        if (itemAttachment == null)
            return;

        // itemId를 int로 변환 (ItemAttachment에서는 string으로 저장됨)
        if (int.TryParse(itemAttachment.itemId, out int itemId))
        {
            SetItemData(itemId, itemAttachment.count);
        }
        else
        {
            Debug.LogWarning($"Invalid itemId format: {itemAttachment.itemId}");
        }
    }

    private void SetItemData(int itemId, int count)
    {
        // ItemTable에서 아이템 데이터 가져오기
        var itemData = DataTableManager.ItemTable.Get(itemId);
        if (itemData == null)
        {
            Debug.LogWarning($"Item data not found for ID: {itemId}");
            return;
        }

        // 아이템 이름 설정
        if (itemNameText != null)
            itemNameText.text = itemData.item_name;

        // 아이템 개수 설정
        if (itemCountText != null)
            itemCountText.text = count.ToString();

        // 아이템 아이콘 설정
        SetItemIcon(itemData.prefab);
    }

    private void SetItemIcon(string prefabName)
    {
        if (itemIcon == null || string.IsNullOrEmpty(prefabName))
            return;

        // ResourceManager를 통해 텍스처 로드
        var texture = ResourceManager.Instance.Get<Texture2D>(prefabName);
        if (texture != null)
        {
            // 텍스처를 스프라이트로 변환하여 설정
            itemIcon.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
        }
        else
        {
            Debug.LogWarning($"Texture not found for prefab: {prefabName}");
        }
    }
}