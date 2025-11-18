using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeletStageWindow : MonoBehaviour
{
    //드래그 슬롯들
    public DraggableSlot[] DraggableSlots;
    //스테이지 자리 : (인덱스, 캐릭터ID)
    public Dictionary<int, int> StageIndexs;
    //스폰 자리
    public GameObject[] SpawnPos;
    public Button StartButton;
    public GameObject basePrefab;

    // 캐릭터 펜스 (싱글톤으로 사용)
    public CharacterFence fence;

    //패시브 타입 보여줄 이미지 바닥 색변경
    public Image[] PassiveImages;

    // 이번 배치에서 "패시브 바닥으로 판정된 타일들"
    private bool[] _passiveTiles;

    // 🔹 타일별 패시브 중첩 개수 (1,2,3...)
    private int[] _passiveStackCounts;

    //미리보기 오버레이
    private bool[] _previewPassiveTiles;

    //이 타일에 어떤 효과가 있는지
    private struct PassiveEffectData
    {
        public int effectId;
        public float value;

        public PassiveEffectData(int id, float v)
        {
            effectId = id;
            value = v;
        }
    }
    // 🔹 타일 인덱스 → 그 타일에 쌓인 모든 패시브 효과 리스트
    private Dictionary<int, List<PassiveEffectData>> PassiveIndexs;


    [Header("Passive Tile Colors")]
    [SerializeField] private Color passiveTileColor = new Color(1f, 165f / 255f, 0f); // 주황 느낌
    [SerializeField] private Color normalTileColor = Color.white;

    // 🔹 중첩 개수에 따른 색
    [SerializeField] private Color stack2Color = Color.green;       // 2중첩: 초록
    [SerializeField] private Color stack3Color = Color.blue;        // 3중첩: 파랑
    [SerializeField] private Color stack4Color = Color.yellow;      // 4중첩: 노랑
    [SerializeField] private Color stack5Color = Color.red;         // 5이상: 빨강

    [SerializeField] private Color previewColor = Color.cyan;       // 미리보기 색

    private void OnEnable()
    {
        StageIndexs = new Dictionary<int, int>();
        PassiveIndexs = new Dictionary<int, List<PassiveEffectData>>();

        if (DraggableSlots != null)
        {
            int len = DraggableSlots.Length;
            _passiveTiles = new bool[len];
            _passiveStackCounts = new int[len];

            // 각 슬롯에 자기 index 부여
            for (int i = 0; i < len; i++)
            {
                if (DraggableSlots[i] != null)
                    DraggableSlots[i].slotIndex = i;
            }
        }

        Time.timeScale = 0f;
        StartButton.onClick.AddListener(StartButtonClick);
        RebuildPassiveTiles();

        DraggableSlot.OnAnySlotChanged += RebuildPassiveTiles;
    }
    private void OnDisable()
    {
        StartButton.onClick.RemoveListener(StartButtonClick);
        DraggableSlot.OnAnySlotChanged -= RebuildPassiveTiles;
    }

    private Dictionary<int, int> GetStagePos()
    {
        StageIndexs.Clear();
        PassiveIndexs.Clear();

        int slotCount = DraggableSlots.Length;

        for (int i = 0; i < slotCount; i++)
        {
            var slot = DraggableSlots[i];
            if (slot == null || slot.characterData == null)
                continue;

            var cd = slot.characterData;
            StageIndexs[i] = cd.char_id;

            var data = DataTableManager.SkillTable.Get(cd.skill_id1);
            PassiveType passiveType = (PassiveType)data.passive_type;

            Debug.Log($"[GetStagePos] slot {i} / char {cd.char_name} / passiveType={passiveType}");

            if (passiveType == PassiveType.None)
                continue;

            // 🔹 이 캐릭터가 영향을 미치는 모든 타일에 대해
            foreach (int tileIndex in PassivePatternUtil.GetPatternTiles(i, passiveType, slotCount))
            {
                if (!PassiveIndexs.TryGetValue(tileIndex, out var list))
                {
                    list = new List<PassiveEffectData>();
                    PassiveIndexs[tileIndex] = list;
                }

                // skill_eff1 ~ 3이 0이 아니면 각각 효과로 추가
                if (data.skill_eff1 != 0)
                    list.Add(new PassiveEffectData(data.skill_eff1, data.skill_eff1_val));

                if (data.skill_eff2 != 0)
                    list.Add(new PassiveEffectData(data.skill_eff2, data.skill_eff2_val));

                if (data.skill_eff3 != 0)
                    list.Add(new PassiveEffectData(data.skill_eff3, data.skill_eff3_val));
            }
        }

        return StageIndexs;
    }

    private void StartButtonClick()
    {
        // 1) 현재 슬롯 상태 기준으로 바닥 패시브 타일 계산 + 색칠
        RebuildPassiveTiles();

        // 2) 스테이지 자리/패시브 효과 테이블 구성
        GetStagePos();

        // 3) 실제 캐릭터 배치 + 패시브 적용
        PlaceAll();

        SoundManager.Instance.PlaySFX("Ui_click_01");
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    private void PlaceAll()
    {
        foreach (var kvp in StageIndexs)
        {
            int slotIndex = kvp.Key;
            int characterId = kvp.Value;

            Vector3 spawnPosition = SpawnPos[slotIndex].transform.position;
            PlaceCharacter(characterId, spawnPosition, slotIndex);
        }
    }

    private void PlaceCharacter(int characterId, Vector3 worldPos, int slotIndex)
    {
        GameObject obj = Instantiate(basePrefab, worldPos, Quaternion.identity);
        var attack = obj.GetComponent<CharacterAttack>();

        AddPassiveEffects(obj, slotIndex);

        attack.id = characterId;

        // 체력 적용 (네 방식 그대로 유지)
        CharacterFence.Instance.Init();
    }

    private void AddPassiveEffects(GameObject obj, int slotIndex)
    {
        // 이 타일에 쌓인 패시브가 없으면 끝
        if (!PassiveIndexs.TryGetValue(slotIndex, out var effects) || effects.Count == 0)
        {
            Debug.Log($"[AddPassiveEffects] slot {slotIndex} 적용할 패시브 없음");
            return;
        }

        Debug.Log($"[AddPassiveEffects] slot {slotIndex} 패시브 {effects.Count}개 적용");

        foreach (var e in effects)
        {
            Debug.Log($" -> effect id={e.effectId}, val={e.value}");
            EffectRegistry.Apply(obj, e.effectId, e.value, 99999f);
        }
    }


    private void ResetPassiveTiles()
    {
        if (DraggableSlots == null) return;

        int len = DraggableSlots.Length;

        if (_passiveTiles == null || _passiveTiles.Length != len)
            _passiveTiles = new bool[len];
        if (_passiveStackCounts == null || _passiveStackCounts.Length != len)
            _passiveStackCounts = new int[len];

        System.Array.Clear(_passiveTiles, 0, len);
        System.Array.Clear(_passiveStackCounts, 0, len);

        // 색은 ApplyTileColors에서 처리
    }


    /// 현재 DraggableSlots 상태 + 각 캐릭터의 PassiveType을 기준으로
    /// 바닥 패시브 타일(_passiveTiles) 계산 + 색칠
    private void RebuildPassiveTiles()
    {
        ResetPassiveTiles();
        if (DraggableSlots == null) return;

        int slotCount = DraggableSlots.Length;

        // 0~14 슬롯 돌면서
        for (int i = 0; i < slotCount; i++)
        {
            var slot = DraggableSlots[i];
            if (slot == null || slot.characterData == null)
                continue;

            var cd = slot.characterData;
            var skill = DataTableManager.SkillTable.Get(cd.skill_id1);

            PassiveType passiveType = (PassiveType)skill.passive_type; // skill.passive_type이 int라고 가정

            if (passiveType == PassiveType.None)
                continue;

            // 기준칸 = i, 패턴 오프셋 적용
            foreach (int idx in PassivePatternUtil.GetPatternTiles(i, passiveType, slotCount))
            {
                _passiveTiles[idx] = true;
                _passiveStackCounts[idx]++;   // 🔹 중첩 개수 누적
            }
        }

        ApplyTileColors();
    }

    // 🔹 중첩 개수 → 색 변환
    private Color GetColorByStackCount(int stack)
    {
        if (stack <= 0) return normalTileColor;

        switch (stack)
        {
            case 1: return passiveTileColor; // 주황
            case 2: return stack2Color;      // 초록
            case 3: return stack3Color;      // 파랑
            case 4: return stack4Color;      // 노랑
            default: return stack5Color;     // 5 이상 빨강
        }
    }

    // 🔹 실제 바닥 타일 색을 중첩 개수에 맞게 반영
    private void ApplyTileColors()
    {
        if (PassiveImages == null || _passiveStackCounts == null) return;

        int len = Mathf.Min(PassiveImages.Length, _passiveStackCounts.Length);
        for (int i = 0; i < len; i++)
        {
            var img = PassiveImages[i];
            if (img == null) continue;

            int stack = _passiveStackCounts[i];
            img.color = GetColorByStackCount(stack);
        }
    }

    private bool IsPassiveTile(int slotIndex)
    {
        return _passiveStackCounts != null &&
               slotIndex >= 0 &&
               slotIndex < _passiveStackCounts.Length &&
               _passiveStackCounts[slotIndex] > 0;
    }

    public void ShowPassivePreview(int slotIndex, CharacterData cd)
    {
        if (cd == null) return;
        if (DraggableSlots == null || PassiveImages == null) return;

        int slotCount = DraggableSlots.Length;

        if (_previewPassiveTiles == null || _previewPassiveTiles.Length != slotCount)
            _previewPassiveTiles = new bool[slotCount];

        System.Array.Clear(_previewPassiveTiles, 0, _previewPassiveTiles.Length);

        var skill = DataTableManager.SkillTable.Get(cd.skill_id1);
        PassiveType type = (PassiveType)skill.passive_type;

        if (type == PassiveType.None) return;

        foreach (int idx in PassivePatternUtil.GetPatternTiles(slotIndex, type, slotCount))
        {
            if (idx >= 0 && idx < _previewPassiveTiles.Length)
                _previewPassiveTiles[idx] = true;
        }

        // 미리보기 색 적용 (겹치면 preview가 우선)
        int len = Mathf.Min(PassiveImages.Length, _passiveStackCounts != null ? _passiveStackCounts.Length : PassiveImages.Length);
        for (int i = 0; i < len; i++)
        {
            var img = PassiveImages[i];
            if (img == null) continue;

            bool isPreview = _previewPassiveTiles[i];

            if (isPreview)
                img.color = previewColor; // 미리보기 색
            else
                img.color = GetColorByStackCount(
                    (_passiveStackCounts != null && i < _passiveStackCounts.Length)
                        ? _passiveStackCounts[i] : 0);
        }
    }

    public void ClearPassivePreview()
    {
        _previewPassiveTiles = null;
        ApplyTileColors();
    }

}
