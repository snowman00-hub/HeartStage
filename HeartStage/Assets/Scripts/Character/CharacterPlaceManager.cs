using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct CharacterPlaceInfo
{
    public int id;
    public Vector3 position;
}

public class CharacterPlaceManager : MonoBehaviour
{
    public GameObject basePrefab;

    [SerializeField]
    private List<CharacterPlaceInfo> placeList = new List<CharacterPlaceInfo>();

    private void Awake()
    {
        PlaceAll();
    }

    [ContextMenu("Place All")]
    private void PlaceAll()
    {
        foreach (var info in placeList)
        {
            PlaceCharacter(info.id, info.position);
        }
    }

    public void PlaceCharacter(int characterId, Vector3 worldPos)
    {
        GameObject obj = Instantiate(basePrefab, worldPos, Quaternion.identity);
        var attack = obj.GetComponent<CharacterAttack>();
        attack.id = characterId;
    }
}