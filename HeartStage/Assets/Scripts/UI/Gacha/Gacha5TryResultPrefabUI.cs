using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Gacha5TryResultPrefabUI : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI characterNameText;

    private Sprite currentSprite; // 현재 스프라이트 참조 저장

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

        // 중복일 때 아이템 표시
        if (gachaResult.isDuplicate && gachaData.Gacha_have > 0)
        {
            var itemData = DataTableManager.ItemTable.Get(gachaData.Gacha_have);
            if (itemData != null)
            {
                SetImage(itemData.prefab);
                SetCharacterNameText(itemData.item_name);
                return;
            }
        }

        // 새로운 캐릭터일 때
        SetImage(characterData.card_imageName);
        SetCharacterNameText(characterData.char_name);
    }

    private void SetCharacterNameText(string name)
    {
        if (characterNameText != null)
        {
            characterNameText.text = name;
        }
    }

    private void SetImage(string imageName)
    {
        if (characterImage == null || string.IsNullOrEmpty(imageName))
        {
            return;
        }

        // 기존 스프라이트 정리
        if (currentSprite != null)
        {
            DestroyImmediate(currentSprite);
            currentSprite = null;
        }

        var texture = ResourceManager.Instance.Get<Texture2D>(imageName);
        if (texture != null)
        {
            currentSprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            characterImage.sprite = currentSprite;
        }
        else
        {
            Debug.LogWarning($"이미지를 로드할 수 없습니다: {imageName}");
        }
    }

    private void OnDestroy()
    {
        // 컴포넌트 파괴시 스프라이트 정리
        if (currentSprite != null)
        {
            DestroyImmediate(currentSprite);
            currentSprite = null;
        }
    }
}