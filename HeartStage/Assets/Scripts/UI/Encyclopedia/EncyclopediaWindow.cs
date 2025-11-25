using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EncyclopediaWindow : MonoBehaviour
{
    public CharacterButtonView[] CharacterButtons;
    public TMP_Dropdown sortDropdown;

    private bool _initialized;

    private void Start()
    {
        InitDropdown();
        RefreshButtons();
        _initialized = true;
    }

    private void OnEnable()
    {
        if (_initialized)
            RefreshButtons();
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

        sortDropdown.onValueChanged.AddListener(_ => RefreshButtons());
    }

    private void RefreshButtons()
    {
        var saveData = SaveLoadManager.Data;
        var unlockedByName = saveData != null ? saveData.unlockedByName : null;
        if (unlockedByName == null)
        {
            // 세이브 아직 안 올라온 상태면 전부 숨김
            for (int i = 0; i < CharacterButtons.Length; i++)
                CharacterButtons[i].gameObject.SetActive(false);
            return;
        }

        // unlockedByName에 있는 이름만 후보로 만든다
        List<CharacterData> candidates = new List<CharacterData>();
        List<bool> unlockedList = new List<bool>();

        foreach (var kvp in unlockedByName)
        {
            string name = kvp.Key;
            bool unlocked = kvp.Value;

            var data = DataTableManager.CharacterTable.GetByName(name);
            if (data == null)
            {
                Debug.LogWarning($"[EncyclopediaWindow] CharacterTable에 없는 이름: {name}");
                continue;
            }

            candidates.Add(data);
            unlockedList.Add(unlocked);
        }

        // 정렬 (두 리스트 같이 스왑)
        int sortIndex = sortDropdown != null ? sortDropdown.value : 0;
        ApplySort(candidates, unlockedList, sortIndex);

        // 버튼 갱신
        for (int i = 0; i < CharacterButtons.Length; i++)
        {
            if (i < candidates.Count)
            {
                var data = candidates[i];
                bool unlocked = unlockedList[i];

                var btn = CharacterButtons[i];
                btn.gameObject.SetActive(true);
                btn.SetButton(data.char_id);
                btn.SetLocked(!unlocked); // false면 회색
            }
            else
            {
                CharacterButtons[i].gameObject.SetActive(false);
            }
        }
    }

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
        // 1순위: unlocked true가 먼저 오게
        if (unlockedA != unlockedB)
        {
            // unlockedA=false, unlockedB=true면 A가 뒤로 가야 하니까 +1 반환
            return unlockedB.CompareTo(unlockedA); // true > false
        }

        //  2순위: 드롭다운 기준 정렬
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
