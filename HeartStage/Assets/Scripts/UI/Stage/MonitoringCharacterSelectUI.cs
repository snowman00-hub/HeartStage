using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonitoringCharacterSelectUI : GenericWindow
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject characterPrefab;

    [SerializeField] private Button closeButton;
    [SerializeField] private Button startButton;

    public override void Open()
    {
        base.Open();
        Display(); // 창이 열릴 때 캐릭터들을 표시
    }

    public override void Close()
    {
        base.Close();
    }

    private void Display()
    {
        var allCharacters = GetOwnedCharacters();

        // 기존 프리팹들 제거
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        // 소유한 캐릭터들로 프리팹 생성
        foreach (var characterData in allCharacters)
        {
            var prefabInstance = Instantiate(characterPrefab, content);
            prefabInstance.name = characterData.char_name;

            // MonitoringCharacterPrefab 컴포넌트 초기화
            var monitoringPrefab = prefabInstance.GetComponent<MonitoringCharacterPrefab>();
            if (monitoringPrefab != null)
            {
                monitoringPrefab.Init(characterData);
            }

            // RectTransform 정리
            if (prefabInstance.transform is RectTransform rect)
            {
                rect.localScale = Vector3.one;
                rect.anchoredPosition3D = Vector3.zero;
            }
        }
    }

    private List<CharacterData> GetOwnedCharacters()
    {
        List<CharacterData> ownedCharacters = new List<CharacterData>();

        var saveData = SaveLoadManager.Data;
        if (saveData == null)
        {
            return ownedCharacters;
        }

        var charTable = DataTableManager.CharacterTable;
        if (charTable == null)
        {
            return ownedCharacters;
        }

        // SaveLoadManager.Data.ownedIds에서 소유한 캐릭터 ID들을 가져와서 CharacterData로 변환
        foreach (int id in saveData.ownedIds)
        {
            var csvData = charTable.Get(id);
            if (csvData == null)
            {
                continue;
            }

            // CSV의 data_AssetName을 사용해 ScriptableObject 로드
            var characterData = ResourceManager.Instance.Get<CharacterData>(csvData.data_AssetName);
            if (characterData == null)
            {
                continue;
            }

            ownedCharacters.Add(characterData);
        }

        // 캐릭터 정렬 (등급 내림차순 -> 레벨 내림차순 -> 이름 내림차순)
        ownedCharacters.Sort((a, b) =>
        {
            int cmp = b.char_rank.CompareTo(a.char_rank);
            if (cmp != 0) return cmp;

            cmp = b.char_lv.CompareTo(a.char_lv);
            if (cmp != 0) return cmp;

            return b.char_name.CompareTo(a.char_name);
        });

        return ownedCharacters;
    }
}