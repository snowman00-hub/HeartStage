using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoWindow : MonoBehaviour
{
    public Image characterImage;
    public TextMeshProUGUI characterName;
    public Image attributeIcon;
    public TextMeshProUGUI rankText; // 나중에 별로 변경
    public TextMeshProUGUI passiveDescText;
    public TextMeshProUGUI activeDescText;
    // 스탯 Value
    public TextMeshProUGUI vocal;
    public TextMeshProUGUI lab;
    public TextMeshProUGUI dance;
    public TextMeshProUGUI visual;
    public TextMeshProUGUI sexy;
    public TextMeshProUGUI cuty;
    public TextMeshProUGUI charisma;

    public void Init(CharacterData data)
    {
        // 캐릭터이미지 바꾸기 characterImage
        characterName.text = data.char_name;
        // 속성 아이콘 변경하기 attributeIcon
        rankText.text = $"{data.char_rank}";
        // 패시브 스킬 정보 세팅하기, 하나만, 범위도 표시하기
        var skillIds = data.GetSkillIds();
        var passiveSkills = skillIds.Where(x => x > 32000).ToList();
        if(passiveSkills.Count > 0)
        {
            var skillData = DataTableManager.SkillTable.Get(passiveSkills[0]);
            passiveDescText.text = skillData.info;
        }
        // 스탯 표시
        vocal.text = $"{data.atk_dmg}";
        lab.text = $"{Mathf.FloorToInt(data.atk_speed)}";
        dance.text = $"{data.char_hp}";
        visual.text = $"{Mathf.FloorToInt(data.crt_chance)}";
        sexy.text = $"{Mathf.FloorToInt(data.crt_dmg)}";
        cuty.text = $"{Mathf.FloorToInt(data.atk_addcount)}";
        charisma.text = $"{Mathf.FloorToInt(data.atk_range)}";
        // 액티브 스킬 아이콘 표시하기
        var activeSkills = skillIds.Where(x => x > 31000 && x < 32000).ToList();
        if(activeSkills.Count > 0)
        {
            var skillData = DataTableManager.SkillTable.Get(activeSkills[0]);
            activeDescText.text = skillData.info;
        }
    }
}