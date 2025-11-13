using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeletStageWindow : MonoBehaviour
{
    public DraggableSlot[] DraggableSlots;
    public Dictionary<int, int> StageIndexs;
    public GameObject[] SpawnPos;
    public Button StartButton;

    public GameObject basePrefab;

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

    public Dictionary<int, int> GetStagePos()
    {
        for (int i = 0; i < DraggableSlots.Length; i++)
        {
            if (DraggableSlots[i].characterData != null)
            {
                StageIndexs.Add(i, DraggableSlots[i].GetCharacterID());
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
        attack.id = characterId;
    }
}
