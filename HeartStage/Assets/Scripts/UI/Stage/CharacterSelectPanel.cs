using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPanel : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public Image attributeIcon; // 속성(큐티, 보컬, 댄스 등) 이미지 
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI idolPowerCount;
    public TextMeshProUGUI levelText;
    public Slider expSlider;
    public Image cardImage;

    private void Start()
    {
        InitAsync().Forget();
    }

    private async UniTask InitAsync()
    {
        var dragMe = GetComponent<DragMe>();
        await UniTask.WaitUntil(() => dragMe.characterData != null);
        var characterData = dragMe.characterData;
        rankText.text = $"{characterData.char_rank}";
        // attributeIcon 변경하기
        characterName.text = characterData.char_name;
        idolPowerCount.text = $"{characterData.GetTotalPower()}";
        levelText.text = $"LV {characterData.char_lv}";
        // expSlider 세팅하기
        var texture2D = ResourceManager.Instance.Get<Texture2D>(characterData.card_imageName);
        cardImage.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
    }
}