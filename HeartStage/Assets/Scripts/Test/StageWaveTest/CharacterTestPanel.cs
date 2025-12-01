using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterTestPanel : MonoBehaviour
{
    [Header("루트 & 타이틀")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text titleText;

    [Header("캐릭터 선택")]
    [SerializeField] private TMP_Dropdown characterDropdown;            // 캐릭터 선택용
    [SerializeField] private TMP_InputField characterIdSearchInput;     // ID로 직접 검색

    [Header("필터 (타입 / 레벨 / 랭크)")]
    [SerializeField] private TMP_Dropdown filterTypeDropdown;
    [SerializeField] private TMP_Dropdown filterLevelDropdown;
    [SerializeField] private TMP_Dropdown filterRankDropdown;

    [Header("페이지 버튼")]
    [SerializeField] private Button page1Button;
    [SerializeField] private Button page2Button;

    [SerializeField] private GameObject page1Root;
    [SerializeField] private GameObject page2Root;

    [Header("공통 버튼")]
    [SerializeField] private Button closeButton;

    // ==========================
    // Page1 - 기본 정보 / 전투 스탯
    // ==========================
    [Header("Page1 - 기본 / 전투 스탯")]
    [SerializeField] private TMP_InputField charIdInput;      // 읽기 전용 추천
    [SerializeField] private TMP_InputField charNameInput;
    [SerializeField] private TMP_InputField charLevelInput;
    [SerializeField] private TMP_InputField charRankInput;
    [SerializeField] private TMP_InputField charTypeInput;    // CharacterType(int 값)

    [SerializeField] private TMP_InputField atkDmgInput;
    [SerializeField] private TMP_InputField atkSpeedInput;
    [SerializeField] private TMP_InputField atkRangeInput;
    [SerializeField] private TMP_InputField atkAddCountInput;

    [SerializeField] private TMP_InputField bulletCountInput;
    [SerializeField] private TMP_InputField bulletSpeedInput;
    [SerializeField] private TMP_InputField charHpInput;

    [SerializeField] private TMP_InputField crtChanceInput;
    [SerializeField] private TMP_InputField crtDmgInput;

    // ==========================
    // Page2 - 스킬 / Info / 에셋 이름
    // ==========================
    [Header("Page2 - 스킬 / Info / 에셋")]
    [SerializeField] private TMP_InputField skillId1Input;
    [SerializeField] private TMP_InputField skillId2Input;
    [SerializeField] private TMP_InputField skillId3Input;
    [SerializeField] private TMP_InputField skillId4Input;
    [SerializeField] private TMP_InputField skillId5Input;
    [SerializeField] private TMP_InputField skillId6Input;

    [SerializeField] private TMP_InputField infoInput;

    [SerializeField] private TMP_InputField imagePrefabNameInput;
    [SerializeField] private TMP_InputField dataAssetNameInput;
    [SerializeField] private TMP_InputField bulletPrefabNameInput;
    [SerializeField] private TMP_InputField projectileAssetNameInput;
    [SerializeField] private TMP_InputField hitEffectAssetNameInput;
    [SerializeField] private TMP_InputField cardImageNameInput;

    // ==========================
    // 내부 상태
    // ==========================
    private CharacterData currentCharacter;
    private int currentPage = 1;
    private System.Action onClosed;

    private Dictionary<int, CharacterData> characterDB;
    private List<CharacterData> allCharacters;   // 전체 목록
    private List<CharacterData> characterList;   // 필터 적용 후 목록 (드롭다운에서 보는 애들)

    private bool filtersInitialized = false;
    private bool isUpdatingUI = false;           // 드롭다운 갱신 중 이벤트 무시용

    private void Awake()
    {
        if (page1Button != null) page1Button.onClick.AddListener(() => ShowPage(1));
        if (page2Button != null) page2Button.onClick.AddListener(() => ShowPage(2));
        if (closeButton != null) closeButton.onClick.AddListener(Close);

        HookInputEvents();
        HookCharacterSelectorEvents();
        HookFilterEvents();

        if (root == null)
            root = gameObject;

        Hide();
    }

    // ==========================
    // 공개 API
    // ==========================

    /// <summary>
    /// 외부에서 단순히 "캐릭터 패널 열기"용.
    /// 기본 캐릭터는 내부에서 자동 선택.
    /// </summary>
    public void Open(System.Action onClosed = null)
    {
        TryInitCharacterDB();
        this.onClosed = onClosed;

        if (characterList == null || characterList.Count == 0)
        {
            Debug.LogWarning("[CharacterTestPanel] characterList 가 비어있습니다.");
            return;
        }

        // 처음 여는 경우: 첫 캐릭터를 기본으로
        if (currentCharacter == null)
        {
            currentCharacter = characterList[0];
        }

        isUpdatingUI = true;
        SyncCharacterSelectorUI();
        isUpdatingUI = false;

        SyncCharacterToUI();
        ShowPage(1);

        if (root != null)
            root.SetActive(true);

        UpdateTitle();
    }

    /// <summary>
    /// 특정 캐릭터로 바로 열고 싶을 때 쓸 수 있는 오버로드 (안 써도 됨).
    /// </summary>
    public void Open(CharacterData character, System.Action onClosed = null)
    {
        TryInitCharacterDB();
        this.onClosed = onClosed;

        if (character == null)
        {
            Debug.LogWarning("[CharacterTestPanel] Open(character) 에 null 이 들어옴. 기본 Open() 사용 권장.");
            Open(onClosed);
            return;
        }

        // DB 기반 인스턴스로 정규화
        if (characterDB != null &&
            characterDB.TryGetValue(character.char_id, out var fromDb) &&
            fromDb != null)
        {
            currentCharacter = fromDb;
        }
        else
        {
            currentCharacter = character;
        }

        // 필터 반영 후에도 currentCharacter 가 보이도록 필터 초기화/조정
        ApplyFiltersAndRebuildDropdowns(forceShowCharacter: true);

        isUpdatingUI = true;
        SyncCharacterSelectorUI();
        isUpdatingUI = false;

        SyncCharacterToUI();
        ShowPage(1);

        if (root != null)
            root.SetActive(true);

        UpdateTitle();
    }

    /// <summary>
    /// 외부에서 패널 끄고 싶을 때 호출용.
    /// </summary>
    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
        // currentCharacter 는 유지 (다음에 열 때 마지막 선택 기억용)
    }

    private void Close()
    {
        Hide();
        onClosed?.Invoke();
    }

    // ==========================
    // 캐릭터 DB / 드롭다운 & 필터
    // ==========================

    private void TryInitCharacterDB()
    {
        if (characterDB != null) return;

        if (DataTableManager.CharacterTable == null)
        {
            Debug.LogWarning("[CharacterTestPanel] DataTableManager.CharacterTable 이 없습니다.");
            return;
        }

        characterDB = DataTableManager.CharacterTable.GetAllCharacterData(); // Dictionary<int, CharacterData>
        allCharacters = new List<CharacterData>(characterDB.Values);
        allCharacters.Sort((a, b) => a.char_id.CompareTo(b.char_id));

        // 처음에는 필터 없이 전체
        characterList = new List<CharacterData>(allCharacters);

        BuildFilterDropdowns();
        BuildCharacterDropdown();
    }

    private void BuildCharacterDropdown()
    {
        if (characterDropdown == null) return;

        characterDropdown.ClearOptions();

        if (characterList == null || characterList.Count == 0)
        {
            var emptyOptions = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData("(캐릭터 없음)")
            };
            characterDropdown.AddOptions(emptyOptions);
            characterDropdown.value = 0;
            characterDropdown.RefreshShownValue();
            return;
        }

        var options = new List<TMP_Dropdown.OptionData>();
        foreach (var ch in characterList)
        {
            string label = $"{ch.char_id} - {ch.char_name}";
            options.Add(new TMP_Dropdown.OptionData(label));
        }

        characterDropdown.AddOptions(options);
        characterDropdown.value = 0;
        characterDropdown.RefreshShownValue();
    }

    private void BuildFilterDropdowns()
    {
        if (filtersInitialized) return;
        if (allCharacters == null) return;

        filtersInitialized = true;

        HashSet<int> typeSet = new HashSet<int>();
        HashSet<int> levelSet = new HashSet<int>();
        HashSet<int> rankSet = new HashSet<int>();

        foreach (var ch in allCharacters)
        {
            typeSet.Add(ch.char_type);
            levelSet.Add(ch.char_lv);
            rankSet.Add(ch.char_rank);
        }

        var typeList = new List<int>(typeSet);
        var levelList = new List<int>(levelSet);
        var rankList = new List<int>(rankSet);

        typeList.Sort();
        levelList.Sort();
        rankList.Sort();

        isUpdatingUI = true;

        if (filterTypeDropdown != null)
        {
            var opts = new List<TMP_Dropdown.OptionData>();
            opts.Add(new TMP_Dropdown.OptionData("AllType"));
            foreach (var v in typeList)
                opts.Add(new TMP_Dropdown.OptionData($"{(CharacterType)v}"));

            filterTypeDropdown.ClearOptions();
            filterTypeDropdown.AddOptions(opts);
            filterTypeDropdown.value = 0;
            filterTypeDropdown.RefreshShownValue();
        }

        if (filterLevelDropdown != null)
        {
            var opts = new List<TMP_Dropdown.OptionData>();
            opts.Add(new TMP_Dropdown.OptionData("AllLv"));
            foreach (var v in levelList)
                opts.Add(new TMP_Dropdown.OptionData($"Lv.{v.ToString()}"));

            filterLevelDropdown.ClearOptions();
            filterLevelDropdown.AddOptions(opts);
            filterLevelDropdown.value = 0;
            filterLevelDropdown.RefreshShownValue();
        }

        if (filterRankDropdown != null)
        {
            var opts = new List<TMP_Dropdown.OptionData>();
            opts.Add(new TMP_Dropdown.OptionData("AllRank"));
            foreach (var v in rankList)
                opts.Add(new TMP_Dropdown.OptionData($"Rank.{v.ToString()}"));

            filterRankDropdown.ClearOptions();
            filterRankDropdown.AddOptions(opts);
            filterRankDropdown.value = 0;
            filterRankDropdown.RefreshShownValue();
        }

        isUpdatingUI = false;
    }

    private void HookFilterEvents()
    {
        if (filterTypeDropdown != null)
            filterTypeDropdown.onValueChanged.AddListener(OnFilterDropdownChanged);

        if (filterLevelDropdown != null)
            filterLevelDropdown.onValueChanged.AddListener(OnFilterDropdownChanged);

        if (filterRankDropdown != null)
            filterRankDropdown.onValueChanged.AddListener(OnFilterDropdownChanged);
    }

    private void OnFilterDropdownChanged(int _)
    {
        if (isUpdatingUI) return;
        ApplyFiltersAndRebuildDropdowns(forceShowCharacter: false);
    }

    /// <summary>
    /// 현재 필터 상태를 기준으로 characterList 재구성 + 캐릭터 드롭다운 갱신
    /// </summary>
    private void ApplyFiltersAndRebuildDropdowns(bool forceShowCharacter)
    {
        if (allCharacters == null) return;

        int? typeFilter = GetFilterIntFromDropdown(filterTypeDropdown);
        int? levelFilter = GetFilterIntFromDropdown(filterLevelDropdown);
        int? rankFilter = GetFilterIntFromDropdown(filterRankDropdown);

        var filtered = new List<CharacterData>();

        foreach (var ch in allCharacters)
        {
            if (typeFilter.HasValue && ch.char_type != typeFilter.Value) continue;
            if (levelFilter.HasValue && ch.char_lv != levelFilter.Value) continue;
            if (rankFilter.HasValue && ch.char_rank != rankFilter.Value) continue;

            filtered.Add(ch);
        }

        // 필터 결과가 없으면 전체로 롤백
        if (filtered.Count == 0)
        {
            Debug.LogWarning("[CharacterTestPanel] 필터 결과 없음. 전체 목록으로 복원합니다.");
            filtered.AddRange(allCharacters);

            // 필터도 전체로 리셋
            isUpdatingUI = true;
            if (filterTypeDropdown != null) { filterTypeDropdown.value = 0; filterTypeDropdown.RefreshShownValue(); }
            if (filterLevelDropdown != null) { filterLevelDropdown.value = 0; filterLevelDropdown.RefreshShownValue(); }
            if (filterRankDropdown != null) { filterRankDropdown.value = 0; filterRankDropdown.RefreshShownValue(); }
            isUpdatingUI = false;
        }

        characterList = filtered;

        // forceShowCharacter == true 이면 currentCharacter 가 무조건 보이도록 필터 재조정하는 용도
        if (forceShowCharacter && currentCharacter != null)
        {
            // currentCharacter 가 필터 결과에 없다면 → 전체 목록에서 currentCharacter 기준으로 필터를 재조정하진 않고,
            // 일단 characterList 전체를 full 목록으로 복구
            bool contains = characterList.Exists(c => c.char_id == currentCharacter.char_id);
            if (!contains)
            {
                characterList = new List<CharacterData>(allCharacters);

                isUpdatingUI = true;
                if (filterTypeDropdown != null) { filterTypeDropdown.value = 0; filterTypeDropdown.RefreshShownValue(); }
                if (filterLevelDropdown != null) { filterLevelDropdown.value = 0; filterLevelDropdown.RefreshShownValue(); }
                if (filterRankDropdown != null) { filterRankDropdown.value = 0; filterRankDropdown.RefreshShownValue(); }
                isUpdatingUI = false;
            }
        }

        BuildCharacterDropdown();

        // currentCharacter 가 필터 후 목록에 없으면 첫 번째로 교체
        if (currentCharacter == null ||
            !characterList.Exists(c => c.char_id == currentCharacter.char_id))
        {
            currentCharacter = characterList[0];
        }

        isUpdatingUI = true;
        SyncCharacterSelectorUI();
        isUpdatingUI = false;

        SyncCharacterToUI();
        UpdateTitle();
    }

    private int? GetFilterIntFromDropdown(TMP_Dropdown dd)
    {
        if (dd == null) return null;
        if (dd.options == null || dd.options.Count == 0) return null;

        // 0번 인덱스는 항상 "전체"
        if (dd.value <= 0) return null;

        string text = dd.options[dd.value].text;

        // 타입 필터: "Vocal", "Rap" 이런 Enum 이름
        if (dd == filterTypeDropdown)
        {
            if (System.Enum.TryParse<CharacterType>(text, out var typeEnum))
                return (int)typeEnum;
            return null;
        }

        // 레벨 필터: "Lv.10" 형식
        if (dd == filterLevelDropdown)
        {
            const string prefix = "Lv.";
            if (text.StartsWith(prefix))
            {
                string numPart = text.Substring(prefix.Length);
                if (int.TryParse(numPart, out int lv))
                    return lv;
            }
            return null;
        }

        // 랭크 필터: "Rank.2" 형식
        if (dd == filterRankDropdown)
        {
            const string prefix = "Rank.";
            if (text.StartsWith(prefix))
            {
                string numPart = text.Substring(prefix.Length);
                if (int.TryParse(numPart, out int rank))
                    return rank;
            }
            return null;
        }

        if (int.TryParse(text, out int v2)) return v2;
        return null;
    }

    private void HookCharacterSelectorEvents()
    {
        if (characterDropdown != null)
            characterDropdown.onValueChanged.AddListener(OnCharacterDropdownChanged);

        if (characterIdSearchInput != null)
            characterIdSearchInput.onEndEdit.AddListener(OnCharacterIdInputChanged);
    }

    private void OnCharacterDropdownChanged(int index)
    {
        if (isUpdatingUI) return;
        if (characterList == null) return;
        if (index < 0 || index >= characterList.Count) return;

        var ch = characterList[index];
        if (ch == null) return;

        currentCharacter = ch;

        isUpdatingUI = true;
        SyncCharacterSelectorUI();
        isUpdatingUI = false;

        SyncCharacterToUI();
        UpdateTitle();
    }

    private void OnCharacterIdInputChanged(string newText)
    {
        if (characterDB == null)
        {
            Debug.LogWarning("[CharacterTestPanel] characterDB 가 아직 초기화되지 않았습니다.");
            TryInitCharacterDB();
            if (characterDB == null) return;
        }

        int fallback = currentCharacter != null ? currentCharacter.char_id : 0;
        int id = ParseInt(newText, fallback);

        if (!characterDB.TryGetValue(id, out var ch) || ch == null)
        {
            Debug.LogWarning($"[CharacterTestPanel] char_id {id} 에 해당하는 CharacterData 를 찾지 못했습니다.");
            // 잘못된 ID면 기존 캐릭터 ID로 되돌리기
            if (currentCharacter != null && characterIdSearchInput != null)
                characterIdSearchInput.SetTextWithoutNotify(currentCharacter.char_id.ToString());
            return;
        }

        currentCharacter = ch;

        // ID 검색으로 찾을 때는 필터를 전체로 리셋해서 무조건 보여주게
        isUpdatingUI = true;
        if (filterTypeDropdown != null) { filterTypeDropdown.value = 0; filterTypeDropdown.RefreshShownValue(); }
        if (filterLevelDropdown != null) { filterLevelDropdown.value = 0; filterLevelDropdown.RefreshShownValue(); }
        if (filterRankDropdown != null) { filterRankDropdown.value = 0; filterRankDropdown.RefreshShownValue(); }
        isUpdatingUI = false;

        characterList = new List<CharacterData>(allCharacters);
        BuildCharacterDropdown();

        isUpdatingUI = true;
        SyncCharacterSelectorUI();
        isUpdatingUI = false;

        SyncCharacterToUI();
        UpdateTitle();
    }

    private void SyncCharacterSelectorUI()
    {
        if (currentCharacter == null) return;

        if (characterIdSearchInput != null)
            characterIdSearchInput.SetTextWithoutNotify(currentCharacter.char_id.ToString());

        if (characterDropdown != null && characterList != null && characterList.Count > 0)
        {
            int idx = characterList.FindIndex(c => c != null && c.char_id == currentCharacter.char_id);
            if (idx >= 0)
                characterDropdown.SetValueWithoutNotify(idx);
        }
    }

    private void UpdateTitle()
    {
        if (titleText != null && currentCharacter != null)
            titleText.text = $"캐릭터 설정 - ID:{currentCharacter.char_id}  {currentCharacter.char_name}";
    }

    // ==========================
    // 페이지 전환
    // ==========================

    private void ShowPage(int page)
    {
        currentPage = page;

        if (page1Root != null) page1Root.SetActive(page == 1);
        if (page2Root != null) page2Root.SetActive(page == 2);

        if (page1Button != null) page1Button.interactable = page != 1;
        if (page2Button != null) page2Button.interactable = page != 2;
    }

    // ==========================
    // 데이터 → UI 동기화
    // ==========================

    private void SyncCharacterToUI()
    {
        if (currentCharacter == null) return;

        // Page1
        if (charIdInput != null)
        {
            charIdInput.SetTextWithoutNotify(currentCharacter.char_id.ToString());
            charIdInput.interactable = false; // ID 수정 X 추천
        }

        charNameInput?.SetTextWithoutNotify(currentCharacter.char_name ?? "");
        charLevelInput?.SetTextWithoutNotify(currentCharacter.char_lv.ToString());
        charRankInput?.SetTextWithoutNotify(currentCharacter.char_rank.ToString());
        charTypeInput?.SetTextWithoutNotify(currentCharacter.char_type.ToString());

        atkDmgInput?.SetTextWithoutNotify(currentCharacter.atk_dmg.ToString());
        atkSpeedInput?.SetTextWithoutNotify(currentCharacter.atk_speed.ToString("0.00", CultureInfo.InvariantCulture));
        atkRangeInput?.SetTextWithoutNotify(currentCharacter.atk_range.ToString("0.00", CultureInfo.InvariantCulture));
        atkAddCountInput?.SetTextWithoutNotify(currentCharacter.atk_addcount.ToString("0.00", CultureInfo.InvariantCulture));

        bulletCountInput?.SetTextWithoutNotify(currentCharacter.bullet_count.ToString());
        bulletSpeedInput?.SetTextWithoutNotify(currentCharacter.bullet_speed.ToString("0.00", CultureInfo.InvariantCulture));
        charHpInput?.SetTextWithoutNotify(currentCharacter.char_hp.ToString());

        crtChanceInput?.SetTextWithoutNotify(currentCharacter.crt_chance.ToString("0.00", CultureInfo.InvariantCulture));
        crtDmgInput?.SetTextWithoutNotify(currentCharacter.crt_dmg.ToString("0.00", CultureInfo.InvariantCulture));

        // Page2
        skillId1Input?.SetTextWithoutNotify(currentCharacter.skill_id1.ToString());
        skillId2Input?.SetTextWithoutNotify(currentCharacter.skill_id2.ToString());
        skillId3Input?.SetTextWithoutNotify(currentCharacter.skill_id3.ToString());
        skillId4Input?.SetTextWithoutNotify(currentCharacter.skill_id4.ToString());
        skillId5Input?.SetTextWithoutNotify(currentCharacter.skill_id5.ToString());
        skillId6Input?.SetTextWithoutNotify(currentCharacter.skill_id6.ToString());

        infoInput?.SetTextWithoutNotify(currentCharacter.Info ?? "");

        imagePrefabNameInput?.SetTextWithoutNotify(currentCharacter.image_PrefabName ?? "");
        dataAssetNameInput?.SetTextWithoutNotify(currentCharacter.data_AssetName ?? "");
        bulletPrefabNameInput?.SetTextWithoutNotify(currentCharacter.bullet_PrefabName ?? "");
        projectileAssetNameInput?.SetTextWithoutNotify(currentCharacter.projectile_AssetName ?? "");
        hitEffectAssetNameInput?.SetTextWithoutNotify(currentCharacter.hitEffect_AssetName ?? "");
        cardImageNameInput?.SetTextWithoutNotify(currentCharacter.card_imageName ?? "");
    }

    // ==========================
    // UI → 데이터 (입력 이벤트)
    // ==========================

    private void HookInputEvents()
    {
        // Page1
        if (charNameInput != null) charNameInput.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.char_name = v;
            MarkDirty();
        });

        if (charLevelInput != null) charLevelInput.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.char_lv = ParseInt(v, currentCharacter.char_lv);
            charLevelInput.SetTextWithoutNotify(currentCharacter.char_lv.ToString());
            MarkDirty();
        });

        if (charRankInput != null) charRankInput.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.char_rank = ParseInt(v, currentCharacter.char_rank);
            charRankInput.SetTextWithoutNotify(currentCharacter.char_rank.ToString());
            MarkDirty();
        });

        if (charTypeInput != null) charTypeInput.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.char_type = ParseInt(v, currentCharacter.char_type);
            charTypeInput.SetTextWithoutNotify(currentCharacter.char_type.ToString());
            MarkDirty();
        });

        if (atkDmgInput != null) atkDmgInput.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.atk_dmg = ParseInt(v, currentCharacter.atk_dmg);
            atkDmgInput.SetTextWithoutNotify(currentCharacter.atk_dmg.ToString());
            MarkDirty();
        });

        if (atkSpeedInput != null) atkSpeedInput.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.atk_speed = ParseFloat(v, currentCharacter.atk_speed);
            atkSpeedInput.SetTextWithoutNotify(currentCharacter.atk_speed.ToString("0.00", CultureInfo.InvariantCulture));
            MarkDirty();
        });

        if (atkRangeInput != null) atkRangeInput.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.atk_range = ParseFloat(v, currentCharacter.atk_range);
            atkRangeInput.SetTextWithoutNotify(currentCharacter.atk_range.ToString("0.00", CultureInfo.InvariantCulture));
            MarkDirty();
        });

        if (atkAddCountInput != null) atkAddCountInput.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.atk_addcount = ParseFloat(v, currentCharacter.atk_addcount);
            atkAddCountInput.SetTextWithoutNotify(currentCharacter.atk_addcount.ToString("0.00", CultureInfo.InvariantCulture));
            MarkDirty();
        });

        if (bulletCountInput != null) bulletCountInput.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.bullet_count = ParseInt(v, currentCharacter.bullet_count);
            bulletCountInput.SetTextWithoutNotify(currentCharacter.bullet_count.ToString());
            MarkDirty();
        });

        if (bulletSpeedInput != null) bulletSpeedInput.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.bullet_speed = ParseFloat(v, currentCharacter.bullet_speed);
            bulletSpeedInput.SetTextWithoutNotify(currentCharacter.bullet_speed.ToString("0.00", CultureInfo.InvariantCulture));
            MarkDirty();
        });

        if (charHpInput != null) charHpInput.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.char_hp = ParseInt(v, currentCharacter.char_hp);
            charHpInput.SetTextWithoutNotify(currentCharacter.char_hp.ToString());
            MarkDirty();
        });

        if (crtChanceInput != null) crtChanceInput.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.crt_chance = ParseFloat(v, currentCharacter.crt_chance);
            crtChanceInput.SetTextWithoutNotify(currentCharacter.crt_chance.ToString("0.00", CultureInfo.InvariantCulture));
            MarkDirty();
        });

        if (crtDmgInput != null) crtDmgInput.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.crt_dmg = ParseFloat(v, currentCharacter.crt_dmg);
            crtDmgInput.SetTextWithoutNotify(currentCharacter.crt_dmg.ToString("0.00", CultureInfo.InvariantCulture));
            MarkDirty();
        });

        // Page2
        if (skillId1Input != null) skillId1Input.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.skill_id1 = ParseInt(v, currentCharacter.skill_id1);
            skillId1Input.SetTextWithoutNotify(currentCharacter.skill_id1.ToString());
            MarkDirty();
        });

        if (skillId2Input != null) skillId2Input.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.skill_id2 = ParseInt(v, currentCharacter.skill_id2);
            skillId2Input.SetTextWithoutNotify(currentCharacter.skill_id2.ToString());
            MarkDirty();
        });

        if (skillId3Input != null) skillId3Input.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.skill_id3 = ParseInt(v, currentCharacter.skill_id3);
            skillId3Input.SetTextWithoutNotify(currentCharacter.skill_id3.ToString());
            MarkDirty();
        });

        if (skillId4Input != null) skillId4Input.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.skill_id4 = ParseInt(v, currentCharacter.skill_id4);
            skillId4Input.SetTextWithoutNotify(currentCharacter.skill_id4.ToString());
            MarkDirty();
        });

        if (skillId5Input != null) skillId5Input.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.skill_id5 = ParseInt(v, currentCharacter.skill_id5);
            skillId5Input.SetTextWithoutNotify(currentCharacter.skill_id5.ToString());
            MarkDirty();
        });

        if (skillId6Input != null) skillId6Input.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.skill_id6 = ParseInt(v, currentCharacter.skill_id6);
            skillId6Input.SetTextWithoutNotify(currentCharacter.skill_id6.ToString());
            MarkDirty();
        });

        if (infoInput != null) infoInput.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.Info = v;
            MarkDirty();
        });

        if (imagePrefabNameInput != null) imagePrefabNameInput.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.image_PrefabName = v;
            MarkDirty();
        });

        if (dataAssetNameInput != null) dataAssetNameInput.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.data_AssetName = v;
            MarkDirty();
        });

        if (bulletPrefabNameInput != null) bulletPrefabNameInput.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.bullet_PrefabName = v;
            MarkDirty();
        });

        if (projectileAssetNameInput != null) projectileAssetNameInput.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.projectile_AssetName = v;
            MarkDirty();
        });

        if (hitEffectAssetNameInput != null) hitEffectAssetNameInput.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.hitEffect_AssetName = v;
            MarkDirty();
        });

        if (cardImageNameInput != null) cardImageNameInput.onEndEdit.AddListener(v =>
        {
            if (currentCharacter == null) return;
            currentCharacter.card_imageName = v;
            MarkDirty();
        });
    }

    // ==========================
    // 유틸
    // ==========================

    private int ParseInt(string text, int fallback)
    {
        if (string.IsNullOrWhiteSpace(text)) return fallback;
        if (int.TryParse(text, out int v)) return v;
        return fallback;
    }

    private float ParseFloat(string text, float fallback)
    {
        if (string.IsNullOrWhiteSpace(text)) return fallback;
        text = text.Replace(',', '.');
        if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
            return v;
        return fallback;
    }

    private void MarkDirty()
    {
#if UNITY_EDITOR
        if (currentCharacter != null)
            UnityEditor.EditorUtility.SetDirty(currentCharacter);
#endif
    }
}
