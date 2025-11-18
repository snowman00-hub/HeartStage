using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeletStageWindow : MonoBehaviour
{
    //드래그 슬롯들
    public DraggableSlot[] DraggableSlots;
    //스테이지 자리 : (인덱스, 캐릭터ID)
    public Dictionary<int, int> StageIndexs;
    //스폰 자리
    public GameObject[] SpawnPos;
    public Button StartButton;
    public GameObject basePrefab;

    // 캐릭터 펜스
    public CharacterFence fence;

    //패시브 타입 자리
    public Dictionary<int, (PassiveType, int, float, int, float, int, float)> PassiveIndexs;
    //패시브 타입 보여줄 이미지 바닥 색변경
    public Image[] PassiveImages;

    //DataTableManager.SkillTable.Get(DraggableSlots[i].characterData.ID).skillvalue~~ > 0 ; 작업시 사용할 스킬 데이터\
    //스킬 데이터에서 패시브타입 1~8가지의 경우 값 확인 

    private void OnEnable()
    {
        StageIndexs = new Dictionary<int, int>();
        PassiveIndexs = new Dictionary<int, (PassiveType, int, float, int, float, int, float)>();
        Time.timeScale = 0f;
        StartButton.onClick.AddListener(StartButtonClick);
    }
    private void OnDisable()
    {
        StartButton.onClick.RemoveListener(StartButtonClick);
    }

    private Dictionary<int, int> GetStagePos()
    {
        for (int i = 0; i < DraggableSlots.Length; i++)
        {
            if (DraggableSlots[i].characterData != null)
            {
                StageIndexs.Add(i, DraggableSlots[i].characterData.char_id);
                var data = DataTableManager.SkillTable.Get(DraggableSlots[i].characterData.skill_id1);
                PassiveIndexs.Add(i, (data.passive_type,
                    data.skill_eff1, data.skill_eff1_val,
                    data.skill_eff2, data.skill_eff2_val,
                    data.skill_eff3, data.skill_eff3_val));
            }
        }
        return StageIndexs;
    }

    private void StartButtonClick()
    {
        GetStagePos();
        PlaceAll();
        SoundManager.Instance.PlaySFX("Ui_click_01");
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    private void PlaceAll()
    {
        foreach (var kvp in StageIndexs)
        {
            int slotIndex = kvp.Key;
            int characterId = kvp.Value;

            Vector3 spawnPosition = SpawnPos[slotIndex].transform.position;
            PlaceCharacter(characterId, spawnPosition, slotIndex);
        }
    }

    private void PlaceCharacter(int characterId, Vector3 worldPos, int slotIndex)
    {
        GameObject obj = Instantiate(basePrefab, worldPos, Quaternion.identity);
        var attack = obj.GetComponent<CharacterAttack>();

        AddPassiveEffects(obj, slotIndex);

        attack.id = characterId;

        // 체력 적용
        CharacterFence.Instance.Init();
    }

    private void AddPassiveEffects(GameObject obj, int slotIndex)
    {
        // 이 슬롯에 패시브 정보가 없다면 패시브 없음
        if (!PassiveIndexs.TryGetValue(slotIndex, out var passiveData))
            return;
        // passiveData 구조:
        // (PassiveType, int eff1Id, float eff1Val, int eff2Id, float eff2Val, int eff3Id, float eff3Val)
        // 1번 효과
        if (passiveData.Item2 != 0)
            EffectRegistry.Apply(obj, passiveData.Item2, passiveData.Item3, 99999);

        // 2번 효과
        if (passiveData.Item4 != 0)
            EffectRegistry.Apply(obj, passiveData.Item4, passiveData.Item5, 99999);

        // 3번 효과
        if (passiveData.Item6 != 0)
            EffectRegistry.Apply(obj, passiveData.Item6, passiveData.Item7, 99999);
    }
}