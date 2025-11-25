using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Gacha5TryResultPrefabUI : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI characterNameText;

    public void Init(GachaData gachaData)
    {
        if (gachaData == null)
            return;

        // 캐릭터 데이터 가챠 아이템이 캐릭터 아이디임 
        var characterData = DataTableManager.CharacterTable.Get(gachaData.Gacha_item);
        if (characterData == null)
        {
            return;
        }
        // 캐릭터 이미지 설정
        SetCharacterImage(characterData);

        // 캐릭터 이름 설정
        if (characterNameText != null)
        {
            var sb = new StringBuilder();
            sb.Clear();
            sb.Append(characterData.char_name);
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
}
