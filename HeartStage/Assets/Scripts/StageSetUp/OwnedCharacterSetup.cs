using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class OwnedCharacterSetup : MonoBehaviour
{
    [Header("UI 부모 (Scroll View의 Content)")]
    public RectTransform content;
    public DragMe characterPrefab;

    private readonly List<CharacterData> _ownedCharacters = new List<CharacterData>();

    public bool IsReady { get; private set; }

    private async void Start()
    {
        IsReady = false;

        // 1) 세이브 데이터 / 캐릭터 테이블 준비까지 기다리기
        await UniTask.WaitUntil(() =>
            SaveLoadManager.Data != null &&
            DataTableManager.CharacterTable != null
        );

        // 2) 리스트 + 프리팹 생성
        BuildOwnedCharacterList();
        InstantiateCharacters();

        // 3) 레이아웃 강제 재계산 (한 프레임 기다리지 않고 지금 프레임에 자리잡게)
        await UniTask.Yield(); // LayoutRebuilder 전에 한 프레임 넘기고 싶으면 유지, 아니면 빼도 됨
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        Canvas.ForceUpdateCanvases();

        IsReady = true;
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
        // 1차: 등급, 2차: 레벨, 3차: 이름 내림차순 정렬
        _ownedCharacters.Sort((a, b) =>
        {
            int cmp = b.char_rank.CompareTo(a.char_rank);
            if (cmp != 0) return cmp;

            cmp = b.char_lv.CompareTo(a.char_lv);
            if (cmp != 0) return cmp;

            return b.char_name.CompareTo(a.char_name);
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
            var dragMeInstance = Instantiate(characterPrefab, content);
            dragMeInstance.name = characterData.char_name;

            // 1) DragMe에 데이터 꽂고
            dragMeInstance.characterData = characterData;

            // 2) CharacterSelectPanel도 바로 초기화
            var panel = dragMeInstance.GetComponent<CharacterSelectPanel>();
            if (panel != null)
                panel.Init(characterData);

            // 3) RectTransform 정리
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

