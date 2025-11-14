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
    public Dictionary<int, (PassiveType,int)> PassiveIndex;
    //패시브 타입 보여줄 이미지 바닥 색변경
    public Image[] PassiveImages;


    private void OnEnable()
    {
        StageIndexs = new Dictionary<int, int>();
        Time.timeScale = 0f;
        StartButton.onClick.AddListener(StartButtonClick);
    }
    private void OnDisable()
    {
        StartButton.onClick.RemoveListener(StartButtonClick);
    }

    public SkillCSVData GetSkillData(int id)
    {
        var data = DataTableManager.SkillTable.Get(id);
        return data;
    }


    public Dictionary<int, int> GetStagePos()
    {
        for (int i = 0; i < DraggableSlots.Length; i++)
        {
            if (DraggableSlots[i].characterData != null)
            {
                StageIndexs.Add(i, DraggableSlots[i].characterData.ID);
            }
        }
        return StageIndexs;
    }

    public void StartButtonClick()
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

    public void PlaceCharacter(int characterId, Vector3 worldPos)
    {
        GameObject obj = Instantiate(basePrefab, worldPos, Quaternion.identity);
        var attack = obj.GetComponent<CharacterAttack>();

        EffectRegistry.Apply(obj, 3001, 0.15f, 10f);

        attack.id = characterId;
    }
}

public enum PassiveType
{
    None = 0,
    Type1 = 1,
    Type2 = 2,
    Type3 = 3,
    Type4 = 4,
    Type5 = 5,
    Type6 = 6,
    Type7 = 7,
    Type8 = 8,
}