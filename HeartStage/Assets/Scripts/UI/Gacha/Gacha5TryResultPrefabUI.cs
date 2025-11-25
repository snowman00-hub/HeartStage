using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Gacha5TryResultPrefabUI : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI characterNameText;

    public void Init(GachaResult gachaResult)
    {
        if (gachaResult.gachaData == null)
            return;

        var gachaData = gachaResult.gachaData;
        var characterData = gachaResult.characterData;

        if (characterData == null)
        {
            return;
        }

        // 중복
        if (gachaResult.isDuplicate && gachaData.Gacha_have > 0)
        {
            var itemData = DataTableManager.ItemTable.Get(gachaData.Gacha_have);
            if (itemData != null)
            {
                SetCharacterImageToItem(itemData);
                SetCharacterNameText(itemData.item_name);
                return;
            }
        }

        // 중복 아닐때 
        SetCharacterImage(characterData);
        SetCharacterNameText(characterData.char_name);
    }

    private void SetCharacterNameText(string name)
    {
        if (characterNameText != null)
        {
            var sb = new StringBuilder();
            sb.Clear();
            sb.Append(name);
            characterNameText.text = sb.ToString();
        }
    }

    private void SetCharacterImage(CharacterCSVData characterData)
    {
        if (characterImage == null || string.IsNullOrEmpty(characterData.card_imageName))
        {
            return;
        }

        var texture = ResourceManager.Instance.Get<Texture2D>(characterData.card_imageName);
        if (texture != null)
        {
            characterImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            Debug.LogWarning($"캐릭터 이미지를 로드할 수 없습니다: {characterData.card_imageName}");
        }
    }

    private void SetCharacterImageToItem(ItemCSVData itemCsvData)
    {
        if (characterImage == null || string.IsNullOrEmpty(itemCsvData.prefab))
        {
            return;
        }

        var texture = ResourceManager.Instance.Get<Texture2D>(itemCsvData.prefab);
        if (texture != null)
        {
            characterImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            Debug.LogWarning($"아이템 이미지를 로드할 수 없습니다: {itemCsvData.prefab}");
        }
    }
}