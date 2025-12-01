using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

public class CharacterDropdownFilter : MonoBehaviour
{
    [Header("Dropdowns")]
    [SerializeField] private TMP_Dropdown typeDropdown;
    [SerializeField] private TMP_Dropdown rankDropdown;
    [SerializeField] private TMP_Dropdown levelDropdown;

    [Header("Character UI")]
    [SerializeField] private RectTransform content;   // ScrollView → Content
    [SerializeField] private DragMe characterPrefab;  // DragMe 달려있는 프리팹

    // Addressables에서 불러온 전체 캐릭터 SO
    private readonly List<CharacterData> allCharacters = new List<CharacterData>();

    // 드롭다운 값 매핑용 (index -> 실제 값)
    private readonly List<int> _typeOptionValues = new List<int>();
    private readonly List<int> _rankOptionValues = new List<int>();
    private readonly List<int> _levelOptionValues = new List<int>();

    private async void Start()
    {
        // 1) CharacterData 라벨로 SO 전부 로드 (기존 CharacterDataLoad와 동일) 
        AsyncOperationHandle<IList<CharacterData>> handle =
            Addressables.LoadAssetsAsync<CharacterData>(
                "CharacterData",
                null
            );

        IList<CharacterData> loadedList = await handle.Task;

        allCharacters.Clear();
        allCharacters.AddRange(loadedList);

        Debug.Log($"[CharacterDropdownFilter] Loaded {allCharacters.Count} CharacterData SOs.");

        // 2) 정렬: 이름 → 타입 → 랭크
        allCharacters.Sort((a, b) =>
        {
            int cmp = a.char_name.CompareTo(b.char_name);
            if (cmp != 0) return cmp;

            cmp = a.char_type.CompareTo(b.char_type);
            if (cmp != 0) return cmp;

            return a.char_rank.CompareTo(b.char_rank);
        });

        // 3) 드롭다운 초기화 + 리스트 그리기
        InitDropdowns();
        RefreshList();

        // 필요하면 나중에 Release 할 수도 있음
        // Addressables.Release(handle);
    }

    private void InitDropdowns()
    {
        InitTypeDropdown();
        InitRankDropdown();
        InitLevelDropdown();

        typeDropdown.onValueChanged.AddListener(_ => RefreshList());
        rankDropdown.onValueChanged.AddListener(_ => RefreshList());
        levelDropdown.onValueChanged.AddListener(_ => RefreshList());
    }

    #region Dropdown Init

    private void InitTypeDropdown()
    {
        typeDropdown.ClearOptions();
        _typeOptionValues.Clear();

        var options = new List<TMP_Dropdown.OptionData>();

        // 0번: All
        options.Add(new TMP_Dropdown.OptionData("TypeAll"));
        _typeOptionValues.Add(-1);

        // 실제 타입 값들
        var distinctTypes = allCharacters
            .Select(c => c.char_type)
            .Distinct()
            .OrderBy(t => t);

        foreach (var t in distinctTypes)
        {
            string label = GetTypeName(t);
            options.Add(new TMP_Dropdown.OptionData(label));
            _typeOptionValues.Add(t);
        }

        typeDropdown.AddOptions(options);
        typeDropdown.value = 0;
        typeDropdown.RefreshShownValue();
    }

    private void InitRankDropdown()
    {
        rankDropdown.ClearOptions();
        _rankOptionValues.Clear();

        var options = new List<TMP_Dropdown.OptionData>();

        options.Add(new TMP_Dropdown.OptionData("RankAll"));
        _rankOptionValues.Add(-1);

        var distinctRanks = allCharacters
            .Select(c => c.char_rank)
            .Distinct()
            .OrderBy(r => r);

        foreach (var r in distinctRanks)
        {
            options.Add(new TMP_Dropdown.OptionData($"R{r}"));
            _rankOptionValues.Add(r);
        }

        rankDropdown.AddOptions(options);
        rankDropdown.value = 0;
        rankDropdown.RefreshShownValue();
    }

    private void InitLevelDropdown()
    {
        levelDropdown.ClearOptions();
        _levelOptionValues.Clear();

        var options = new List<TMP_Dropdown.OptionData>();

        options.Add(new TMP_Dropdown.OptionData("LevelAll"));
        _levelOptionValues.Add(-1);

        var distinctLevels = allCharacters
            .Select(c => c.char_lv)
            .Distinct()
            .OrderBy(lv => lv);

        foreach (var lv in distinctLevels)
        {
            options.Add(new TMP_Dropdown.OptionData($"Lv.{lv}"));
            _levelOptionValues.Add(lv);
        }

        levelDropdown.AddOptions(options);
        levelDropdown.value = 0;
        levelDropdown.RefreshShownValue();
    }

    #endregion

    // 타입 숫자를 문자열로 바꿔주는 함수
    private string GetTypeName(int type)
    {
        // enum 쓸 거면 이렇게:
        // return ((CharacterType)type).ToString();

        // 일단 테스트용
        return ((CharacterType)type).ToString();
    }

    // 현재 드롭다운 선택값 → 필터 조건
    private int? GetSelectedType()
    {
        int idx = typeDropdown.value;
        int raw = _typeOptionValues[idx];
        return raw == -1 ? (int?)null : raw;
    }

    private int? GetSelectedRank()
    {
        int idx = rankDropdown.value;
        int raw = _rankOptionValues[idx];
        return raw == -1 ? (int?)null : raw;
    }

    private int? GetSelectedLevel()
    {
        int idx = levelDropdown.value;
        int raw = _levelOptionValues[idx];
        return raw == -1 ? (int?)null : raw;
    }

    // 드롭다운 바뀔 때마다 호출
    private void RefreshList()
    {
        int? typeFilter = GetSelectedType();
        int? rankFilter = GetSelectedRank();
        int? levelFilter = GetSelectedLevel();

        IEnumerable<CharacterData> query = allCharacters;

        if (typeFilter.HasValue)
            query = query.Where(c => c.char_type == typeFilter.Value);

        if (rankFilter.HasValue)
            query = query.Where(c => c.char_rank == rankFilter.Value);

        if (levelFilter.HasValue)
            query = query.Where(c => c.char_lv == levelFilter.Value);

        // 정렬: 이름 → 타입 → 랭크 (필터된 것만)
        var result = query
            .OrderBy(c => c.char_name)
            .ThenBy(c => c.char_type)
            .ThenBy(c => c.char_rank)
            .ToList();

        RebuildCharacterUI(result);
    }

    private void RebuildCharacterUI(List<CharacterData> list)
    {
        // 기존 슬롯 제거
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        // 새로 생성
        foreach (var characterData in list)
        {
            var dragMeInstance = Instantiate(characterPrefab, content);
            dragMeInstance.name = characterData.char_name; // 이게 더 직관적

            // DragMe 프리팹에 CharacterData 꽂기
            dragMeInstance.characterData = characterData;

            // 여기 타입을 CharacterSelectTestPanel 로 변경
            var panel = dragMeInstance.GetComponent<CharacterSelectTestPanel>();
            if (panel != null)
            {
                // 래핑된 Init 함수: 내부에서 InitAsync().Forget() 호출
                panel.Init(characterData);
                // 또는 async로 하고 싶으면:
                // panel.InitAsync(characterData).Forget();
            }

            if (dragMeInstance.transform is RectTransform rect)
            {
                rect.localScale = Vector3.one;
                rect.anchoredPosition3D = Vector3.zero;
            }
        }
    }

}
