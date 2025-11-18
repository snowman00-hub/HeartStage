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

    // 캐릭터 펜스
    public CharacterFence fence;

    //패시브 타입 보여줄 이미지 바닥 색변경
    public Image[] PassiveImages;

    // 이번 배치에서 "패시브 바닥으로 판정된 타일들"
    private bool[] _passiveTiles;
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
    private Dictionary<int, List<PassiveEffectData>> PassiveIndexs;


    [Header("Passive Tile Colors")]
    [SerializeField] private Color passiveTileColor = Color.yellow;
    [SerializeField] private Color normalTileColor = Color.white;

    private void OnEnable()
    {
        StageIndexs = new Dictionary<int, int>();
        PassiveIndexs = new Dictionary<int, List<PassiveEffectData>>();


        if (DraggableSlots != null)
        {
            _passiveTiles = new bool[DraggableSlots.Length];

            // 여기 추가: 각 슬롯에 자기 index 부여
            for (int i = 0; i < DraggableSlots.Length; i++)
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

        // 체력 적용
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

        if (_passiveTiles == null || _passiveTiles.Length != DraggableSlots.Length)
            _passiveTiles = new bool[DraggableSlots.Length];

        System.Array.Clear(_passiveTiles, 0, _passiveTiles.Length);

        if (PassiveImages != null)
        {
            int len = Mathf.Min(PassiveImages.Length, _passiveTiles.Length);
            for (int i = 0; i < len; i++)
            {
                if (PassiveImages[i] != null)
                    PassiveImages[i].color = normalTileColor;
            }
        }
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

            Debug.Log($"[RebuildPassiveTiles] slot {i}, char {cd.char_name}, skill_id1={cd.skill_id1}, passiveType={passiveType}({skill.passive_type})");

            if (passiveType == PassiveType.None)
                continue;

            // 기준칸 = i, 패턴 오프셋 적용
            foreach (int idx in PassivePatternUtil.GetPatternTiles(i, passiveType, slotCount))
            {
                Debug.Log($"    -> 패턴 타일 포함 index {idx}");
                _passiveTiles[idx] = true;
            }
        }

        // 계산 결과를 바닥 이미지 색에 반영
        if (PassiveImages != null)
        {
            Debug.Log("[RebuildPassiveTiles] 패시브 타일 색칠 시작");
            int len = Mathf.Min(PassiveImages.Length, _passiveTiles.Length);
            for (int i = 0; i < len; i++)
            {
                var img = PassiveImages[i];
                if (img == null) continue;

                img.color = _passiveTiles[i] ? passiveTileColor : normalTileColor;
            }
        }

        // 마지막으로 전체 결과 한 번 요약
        string debugLine = "[RebuildPassiveTiles] 최종 passiveTiles: ";
        for (int i = 0; i < _passiveTiles.Length; i++)
            debugLine += _passiveTiles[i] ? $" {i}" : "";
        Debug.Log(debugLine);
    }

    private bool IsPassiveTile(int slotIndex)
    {
        return _passiveTiles != null &&
               slotIndex >= 0 &&
               slotIndex < _passiveTiles.Length &&
               _passiveTiles[slotIndex];
    }
    public void ShowPassivePreview(int slotIndex, CharacterData cd)
    {
        if (cd == null) return;

        // 먼저 전체 타일 리셋
        ClearPassivePreview();

        var skill = DataTableManager.SkillTable.Get(cd.skill_id1);
        PassiveType type = (PassiveType)skill.passive_type;

        if (type == PassiveType.None) return;

        _previewPassiveTiles = new bool[DraggableSlots.Length];

        foreach (int idx in PassivePatternUtil.GetPatternTiles(slotIndex, type, DraggableSlots.Length))
        {
            _previewPassiveTiles[idx] = true;
        }

        // 미리보기 색 적용 (겹치면 preview가 우선)
        for (int i = 0; i < PassiveImages.Length; i++)
        {
            if (_previewPassiveTiles[i])
                PassiveImages[i].color = Color.cyan;   // 미리보기 색
            else
                PassiveImages[i].color = _passiveTiles[i] ?
                                         passiveTileColor :
                                         normalTileColor;
        }
    }

    public void ClearPassivePreview()
    {
        if (PassiveImages == null) return;

        for (int i = 0; i < PassiveImages.Length; i++)
        {
            PassiveImages[i].color = _passiveTiles[i] ?
                                     passiveTileColor :
                                     normalTileColor;
        }

        _previewPassiveTiles = null;
    }

}
