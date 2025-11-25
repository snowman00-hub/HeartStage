using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EncyclopediaWindow : MonoBehaviour
{
    [Header("버튼 슬롯들")]
    public CharacterButtonView[] CharacterButtons;

    [Header("정렬 드롭다운 1개")]
    public TMP_Dropdown sortDropdown;

    [Header("상세 패널(공용 1개)")]
    public CharacterDetailPanel detailPanel;

    private bool _initialized;

    // 현재 보여줄 후보들/해금여부
    private List<CharacterData> _candidates = new List<CharacterData>();
    private List<bool> _unlockedList = new List<bool>();

    private void Start()
    {
        InitDropdown();
        RebuildAndRender();
        _initialized = true;
    }

    private void OnEnable()
    {
        if (_initialized)
            RebuildAndRender();
    }

    private void InitDropdown()
    {
        if (sortDropdown == null) return;

        sortDropdown.ClearOptions();
        sortDropdown.AddOptions(new List<string>
        {
            "등급순",
            "레벨순",
            "이름순",
            "속성순"
        });

        sortDropdown.onValueChanged.RemoveAllListeners();
        sortDropdown.onValueChanged.AddListener(_ =>
        {
            RebuildAndRender();
        });
    }

    private void RebuildAndRender()
    {
        BuildCandidates();
        ApplySort(_candidates, _unlockedList, sortDropdown != null ? sortDropdown.value : 0);
        RenderButtons();
    }

    // ✅ SaveLoadManager.unlockedByName 키만 후보로
    private void BuildCandidates()
    {
        _candidates.Clear();
        _unlockedList.Clear();

        var saveData = SaveLoadManager.Data;
        var unlockedByName = saveData != null ? saveData.unlockedByName : null;
        if (unlockedByName == null) return;

        foreach (var kvp in unlockedByName)
        {
            string name = kvp.Key;
            bool unlocked = kvp.Value;

            // 이름으로 데이터 찾기 (CharacterTable에 GetByName 있어야 함)
            var data = DataTableManager.CharacterTable.GetByName(name);
            if (data == null) continue;

            _candidates.Add(data);
            _unlockedList.Add(unlocked);
        }
    }

    private void RenderButtons()
    {
        for (int i = 0; i < CharacterButtons.Length; i++)
        {
            if (i < _candidates.Count)
            {
                var data = _candidates[i];
                bool unlocked = _unlockedList[i];
                var btnView = CharacterButtons[i];

                btnView.gameObject.SetActive(true);
                btnView.SetButton(data.char_id);
                btnView.SetLocked(!unlocked);

                // ✅ 클릭 연결
                BindClick(btnView, data.char_id);
            }
            else
            {
                CharacterButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void BindClick(CharacterButtonView btnView, int charId)
    {
        // CharacterButtonView 안에 Button이 없을 수도 있으니 GetComponent로 안전하게
        var uiButton = btnView.GetComponent<Button>();
        if (uiButton == null) return;

        uiButton.onClick.RemoveAllListeners();
        uiButton.onClick.AddListener(() => OnCharacterSelected(charId));
    }

    private void OnCharacterSelected(int charId)
    {
        if (detailPanel == null)
        {
            Debug.LogWarning("[EncyclopediaWindow] detailPanel null");
            return;
        }

        var csvdata = DataTableManager.CharacterTable.Get(charId);
        if (csvdata == null)
        {
            Debug.LogWarning($"[EncyclopediaWindow] CharacterData null: {charId}");
            return;
        }
        
        detailPanel.SetCharacter(csvdata);
        detailPanel.OpenPanel();
    }

    // true 먼저 / false 뒤로 + 내부는 드롭다운 정렬
    private void ApplySort(List<CharacterData> list, List<bool> unlockedList, int sortIndex)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            for (int j = i + 1; j < list.Count; j++)
            {
                var A = list[i];
                var B = list[j];
                bool unlockedA = unlockedList[i];
                bool unlockedB = unlockedList[j];

                int cmp = CompareWithUnlocked(A, unlockedA, B, unlockedB, sortIndex);
                if (cmp > 0)
                {
                    // data swap
                    list[i] = B;
                    list[j] = A;

                    // unlocked swap
                    bool tmp = unlockedList[i];
                    unlockedList[i] = unlockedList[j];
                    unlockedList[j] = tmp;
                }
            }
        }
    }

    private int CompareWithUnlocked(CharacterData A, bool unlockedA,
                                    CharacterData B, bool unlockedB,
                                    int sortIndex)
    {
        // 1순위: unlocked true 먼저
        if (unlockedA != unlockedB)
            return unlockedB.CompareTo(unlockedA);

        // 2순위: 드롭다운 기준
        return CompareData(A, B, sortIndex);
    }

    private int CompareData(CharacterData A, CharacterData B, int sortIndex)
    {
        string nameA = A.char_name ?? "";
        string nameB = B.char_name ?? "";

        switch (sortIndex)
        {
            case 0: // 등급순
                {
                    int c = A.char_rank.CompareTo(B.char_rank);
                    if (c != 0) return c;
                    c = A.char_lv.CompareTo(B.char_lv);
                    if (c != 0) return c;
                    return string.CompareOrdinal(nameA, nameB);
                }
            case 1: // 레벨순
                {
                    int c = A.char_lv.CompareTo(B.char_lv);
                    if (c != 0) return c;
                    c = A.char_rank.CompareTo(B.char_rank);
                    if (c != 0) return c;
                    return string.CompareOrdinal(nameA, nameB);
                }
            case 2: // 이름순
                return string.CompareOrdinal(nameA, nameB);

            case 3: // 속성순
                {
                    int c = A.char_type.CompareTo(B.char_type);
                    if (c != 0) return c;
                    c = A.char_rank.CompareTo(B.char_rank);
                    if (c != 0) return c;
                    c = A.char_lv.CompareTo(B.char_lv);
                    if (c != 0) return c;
                    return string.CompareOrdinal(nameA, nameB);
                }
        }
        return 0;
    }
}
