using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaPercentageItemPrefab : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI percentageText;
    [SerializeField] private TextMeshProUGUI countText;

    public void Init(GachaData gachaData, int count)
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
        if(characterNameText != null)
        {
            var sb = new StringBuilder();
            sb.Clear();
            sb.Append(characterData.char_name);
            characterNameText.text = sb.ToString();
        }

        // 확률 설정
        if(percentageText != null)
        {
            var sb = new StringBuilder();
            sb.Clear();
            sb.Append(gachaData.Gacha_per).Append("%");
            percentageText.text = sb.ToString();
        }

        // 획득 개수 설정
        if(countText != null)
        {
            var sb = new StringBuilder();
            sb.Clear();
            sb.Append(1); 
            countText.text = sb.ToString();
        }
    }

    private void SetCharacterImage(CharacterCSVData characterData)
    {
        if(characterImage == null || string.IsNullOrEmpty(characterData.card_imageName))
        {
            return;
        }

        var texture = ResourceManager.Instance.Get<Texture2D>(characterData.card_imageName);
        if(texture != null)
        {
            characterImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            Debug.LogWarning($"캐릭터 이미지를 로드할 수 없습니다: {characterData.card_imageName}");
        }
    }

}
