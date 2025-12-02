using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailItemPrefab : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemCountText;

    private Sprite dynamicSprite; // 메모리 관리용

    public void Setup(ItemAttachment itemAttachment)
    {
        if (itemAttachment == null) return;

        if (int.TryParse(itemAttachment.itemId, out int itemId))
        {
            SetItemData(itemId, itemAttachment.count);
        }
    }

    private void SetItemData(int itemId, int count)
    {
        var itemData = DataTableManager.ItemTable.Get(itemId);
        if (itemData == null) return;

        if (itemNameText != null)
            itemNameText.text = itemData.item_name;

        if (itemCountText != null)
            itemCountText.text = count.ToString();

        SetItemIcon(itemData.prefab);
    }

    private void SetItemIcon(string prefabName)
    {
        if (itemIcon == null || string.IsNullOrEmpty(prefabName)) return;

        var texture = ResourceManager.Instance.Get<Texture2D>(prefabName);
        if (texture != null)
        {
            // 기존 동적 스프라이트 정리
            if (dynamicSprite != null)
            {
                DestroyImmediate(dynamicSprite);
            }

            dynamicSprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
            itemIcon.sprite = dynamicSprite;
        }
    }

    // 메모리 정리
    private void OnDestroy()
    {
        if (dynamicSprite != null)
        {
            DestroyImmediate(dynamicSprite);
            dynamicSprite = null;
        }
    }
}