using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeletStageWindow : MonoBehaviour
{
    //드래그 슬롯들
    public DraggableSlot[] DraggableSlots;
    //스테이지 자리
    public Dictionary<int, int> StageIndexs;
    //스폰 자리
    public GameObject[] SpawnPos;
    public Button StartButton;
    public GameObject basePrefab;


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
                StageIndexs.Add(i, DraggableSlots[i].characterData.ID);
                var data = DataTableManager.SkillTable.Get(DraggableSlots[i].characterData.skill_id);
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
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    private void PlaceAll()
    {
        Debug.Log("PlaceAll");
        Debug.Log(StageIndexs.Count);
        foreach (var kvp in StageIndexs)
        {
            Vector3 spawnPosition = SpawnPos[kvp.Key].transform.position;
            PlaceCharacter(kvp.Value, spawnPosition);
        }
    }

    private void PlaceCharacter(int characterId, Vector3 worldPos)
    {
        GameObject obj = Instantiate(basePrefab, worldPos, Quaternion.identity);
        var attack = obj.GetComponent<CharacterAttack>();

        AddPassiveEffects(obj);

        attack.id = characterId;
    }

    private void AddPassiveEffects(GameObject obj)
    {
        //패시브 타입에 따른 이펙트 추가
        //passiveData.Item1 : PassiveType
        //passiveData.Item2~7 : 효과값들
        //PassiveType은 바닥 타일 색상변경이다.
        //PassiveIndexs 의 Key값은 인덱스 값이다.
        for(int i = 0; i < PassiveImages.Length; i++)
        {
            if(PassiveIndexs.ContainsKey(i))
            {
                EffectRegistry.Apply(obj, PassiveIndexs[i].Item2, PassiveIndexs[i].Item3, 99999);
                EffectRegistry.Apply(obj, PassiveIndexs[i].Item4, PassiveIndexs[i].Item5, 99999);
                EffectRegistry.Apply(obj, PassiveIndexs[i].Item6, PassiveIndexs[i].Item7, 99999);
            }
        }

    }

}