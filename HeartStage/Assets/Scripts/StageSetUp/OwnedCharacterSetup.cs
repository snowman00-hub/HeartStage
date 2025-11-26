using System.Collections.Generic;
using UnityEngine;

public class OwnedCharacterSetup : MonoBehaviour
{
    [Header("UI 부모 (Scroll View의 Content)")]
    public RectTransform content;      // ScrollView → Viewport → Content
    public DragMe characterPrefab;     // DragMe 달려있는 공통 프리팹

    // 보유 캐릭터들의 CharacterData SO 캐싱
    private readonly List<CharacterData> _ownedCharacters = new List<CharacterData>();

    private void Start()
    {
        BuildOwnedCharacterList();
        InstantiateCharacters();
    }

    /// Save 데이터 기준으로 '보유 중인 캐릭터'만 SO 리스트로 만든다.
    private void BuildOwnedCharacterList()
    {
        _ownedCharacters.Clear();

        var saveData = SaveLoadManager.Data;
        if (saveData == null)
        {
            Debug.LogWarning("[OwnedCharacterSetup] SaveLoadManager.Data == null");
            return;
        }

        var charTable = DataTableManager.CharacterTable;
        if (charTable == null)
        {
            Debug.LogWarning("[OwnedCharacterSetup] CharacterTable == null");
            return;
        }

        // ownedIds 안에 현재 보유 캐릭터 ID가 들어있음
        foreach (int id in saveData.ownedIds)
        {
            var row = charTable.Get(id);   // CharacterCSVData
            if (row == null)
            {
                Debug.LogWarning($"[OwnedCharacterSetup] CharacterTable row null: id={id}");
                continue;
            }

            // CSV에 있는 data_AssetName 기준으로 SO 로드 (너가 쓰는 패턴)
            // 예) "C001_Data" 같은 애들
            var so = ResourceManager.Instance.Get<CharacterData>(row.data_AssetName);
            if (so == null)
            {
                Debug.LogWarning($"[OwnedCharacterSetup] CharacterData SO null: {row.data_AssetName}");
                continue;
            }

            _ownedCharacters.Add(so);
        }

        // 정렬 규칙 한 번 잡아주자 (원하면 바꿔도 됨)
        // 1차: 등급 오름차순, 2차: 레벨, 3차: 이름
        _ownedCharacters.Sort((a, b) =>
        {
            int cmp = a.char_rank.CompareTo(b.char_rank);
            if (cmp != 0) return cmp;

            cmp = a.char_lv.CompareTo(b.char_lv);
            if (cmp != 0) return cmp;

            return a.char_name.CompareTo(b.char_name);
        });

        Debug.Log($"[OwnedCharacterSetup] OwnedCharacters = {_ownedCharacters.Count}");
    }

    /// ScrollView Content 밑에 DragMe 프리팹들 생성
    private void InstantiateCharacters()
    {
        if (content == null || characterPrefab == null)
        {
            Debug.LogWarning("[OwnedCharacterSetup] content 또는 characterPrefab 미할당");
            return;
        }

        // 기존 자식들 정리 (리프레시 가능하게)
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        foreach (var characterData in _ownedCharacters)
        {
            // 공통 DragMe 프리팹을 Content 밑에 생성
            DragMe dragMeInstance = Instantiate(characterPrefab, content);
            dragMeInstance.name = characterData.char_name;  // 보기 좋게 이름

            // DragMe 쪽에 SO 꽂아주기 (TestCharacterDataLaod에서 하던 그대로) :contentReference[oaicite:2]{index=2}
            if (dragMeInstance != null)
            {
                // 네 DragMe에 RebindCharacter 같은 함수 있으면 그걸 써도 됨
                dragMeInstance.characterData = characterData;
            }

            // RectTransform 초기화 (LayoutGroup이 위치 정리하게)
            if (dragMeInstance.transform is RectTransform rect)
            {
                rect.localScale = Vector3.one;
                rect.anchoredPosition3D = Vector3.zero;
            }
        }
    }

    /// 나중에 새 캐릭 획득 후 리스트 갱신하고 싶을 때 호출용
    public void Refresh()
    {
        BuildOwnedCharacterList();
        InstantiateCharacters();
    }
}

