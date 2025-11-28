using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonitoringCharacterPrefab : MonoBehaviour
{
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dipatchCount;

    public void Init(CharacterData characterData)
    {
        if (characterData == null)
        {
            return;
        }

        // 캐릭터 이름 설정
        if (characterNameText != null)
        {
            characterNameText.text = characterData.char_name;
        }

        // 캐릭터 이미지 설정
        if (characterImage != null && !string.IsNullOrEmpty(characterData.card_imageName))
        {
            var texture2D = ResourceManager.Instance.Get<Texture2D>(characterData.card_imageName);
            if (texture2D != null)
            {
                characterImage.sprite = Sprite.Create
                (
                    texture2D,
                    new Rect(0, 0, texture2D.width, texture2D.height),
                    new Vector2(0.5f, 0.5f)
                );
            }
        }

        // 파견 횟수
        if (dipatchCount != null)
        {
            dipatchCount.text = "2"; 
        }
    }
}