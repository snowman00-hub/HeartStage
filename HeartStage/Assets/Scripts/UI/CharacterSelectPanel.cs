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

    private void Start()
    {
		Init();
    }

	private void Init()
    {
        var dragMe = GetComponent<DragMe>();
        var characterData = dragMe.characterData;
		rankText.text = $"{characterData.char_rank}";
		// attributeIcon 변경하기
		characterName.text = characterData.name;
		idolPowerCount.text = $"{characterData.GetTotalPower()}";
		levelText.text = $"{characterData.char_lv}";
		// expSlider 세팅하기
    }
}