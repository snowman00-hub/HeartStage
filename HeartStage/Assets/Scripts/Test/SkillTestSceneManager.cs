using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

/// <summary>
/// DataTableManager.SkillTable에서 SkillData를 자동으로 불러오고,
/// Addressables 라벨(SFX/Hit) 기반으로 이펙트 키 목록을 가져와서
/// - 액티브 스킬의 projectile / hit 이펙트 키
/// - skill_range
/// 를 편집 + 프리뷰하는 테스트 매니저.
/// </summary>
public class SkillTestManager : MonoBehaviour
{
    [Header("라벨 설정 (Addressables)")]
    [SerializeField] private string projectileEffectLabel = "SFX"; // 발사 이펙트 라벨
    [SerializeField] private string hitEffectLabel = "Hit";        // 히트 이펙트 라벨

    [Header("Skill / Projectile UI")]
    [SerializeField] private TMP_Dropdown skillDropdown;       // 스킬 선택
    [SerializeField] private TMP_InputField projectileKeyInput;    // projectile 키 입력
    [SerializeField] private TMP_Dropdown projectileKeyDropdown;   // projectile 키 목록

    [Header("Hit Effect UI")]
    [SerializeField] private TMP_InputField hitKeyInput;       // hit 키 입력
    [SerializeField] private TMP_Dropdown hitKeyDropdown;      // hit 키 목록
    [SerializeField] private Button hitPreviewButton;          // hit 프리뷰 버튼
    [SerializeField] private Button hitApplyButton;            // hit 적용 버튼

    [Header("Range UI")]
    [SerializeField] private TMP_InputField rangeInput;        // 사거리 입력
    [SerializeField] private Slider rangeSlider;               // 사거리 슬라이더 (선택)

    [Header("프리뷰 위치")]
    [SerializeField] private Transform previewPoint;

    [Header("Projectile 버튼")]
    [SerializeField] private Button projectilePreviewButton;   // projectile 프리뷰
    [SerializeField] private Button projectileApplyButton;     // projectile 적용

    [Header("슬라이더 기본값")]
    [SerializeField] private float defaultSliderMin = 0f;
    [SerializeField] private float defaultSliderMax = 10f;

    // 내부 데이터
    private Dictionary<int, SkillData> skillDB;
    private List<SkillData> skillList;
    private List<string> projectileKeys;   // Addressables 라벨(SFX)에서 가져온 키들
    private List<string> hitKeys;          // Addressables 라벨(Hit)에서 가져온 키들

    private SkillData currentSkill;
    private string currentProjectileKey;
    private string currentHitKey;

    // 프리뷰 관리
    private GameObject currentPreviewInstance;
    private AsyncOperationHandle<GameObject> currentHandle;
    private bool hasHandle = false;

    private void Start()
    {
        // 1) 스킬 테이블 로딩 (이미 LoadAsync 되어 있다고 가정)
        skillDB = DataTableManager.SkillTable.GetAll();
        skillList = new List<SkillData>(skillDB.Values);

        // skill_id 기준 정렬
        skillList.Sort((a, b) => a.skill_id.CompareTo(b.skill_id));

        // 2) UI 빌드
        BuildSkillDropdown();
        SetupRangeSliderIfNeeded();

        // 3) 주소 라벨 기반으로 이펙트 키 목록 빌드
        StartCoroutine(BuildProjectileKeyDropdownFromLabel(projectileEffectLabel));
        StartCoroutine(BuildHitKeyDropdownFromLabel(hitEffectLabel));

        // 4) 이벤트 연결
        skillDropdown.onValueChanged.AddListener(OnSkillChanged);

        if (projectileKeyDropdown != null)
            projectileKeyDropdown.onValueChanged.AddListener(OnProjectileKeyDropdownChanged);

        if (hitKeyDropdown != null)
            hitKeyDropdown.onValueChanged.AddListener(OnHitKeyDropdownChanged);

        if (projectileKeyInput != null)
            projectileKeyInput.onEndEdit.AddListener(OnProjectileKeyInputChanged);

        if (hitKeyInput != null)
            hitKeyInput.onEndEdit.AddListener(OnHitKeyInputChanged);

        if (rangeInput != null)
            rangeInput.onEndEdit.AddListener(OnRangeInputChanged);

        if (rangeSlider != null)
            rangeSlider.onValueChanged.AddListener(OnRangeSliderChanged);

        if (projectilePreviewButton != null)
            projectilePreviewButton.onClick.AddListener(OnProjectilePreviewClicked);

        if (projectileApplyButton != null)
            projectileApplyButton.onClick.AddListener(OnProjectileApplyClicked);

        if (hitPreviewButton != null)
            hitPreviewButton.onClick.AddListener(OnHitPreviewClicked);

        if (hitApplyButton != null)
            hitApplyButton.onClick.AddListener(OnHitApplyClicked);

        // 5) 초기 스킬 선택
        if (skillList.Count > 0)
            OnSkillChanged(0);
    }

    private void OnDestroy()
    {
        ReleasePreview();
    }

    private bool IsActiveSkill(SkillData data)
    {
        return data.skill_type == 1 && data.passive_type == 0;
    }

    // ==========================
    // UI 빌드
    // ==========================

    private void BuildSkillDropdown()
    {
        var options = new List<TMP_Dropdown.OptionData>();

        foreach (var s in skillList)
        {
            if (!IsActiveSkill(s))
                continue;

            string label = $"{s.skill_id} - {s.skill_name}";
            options.Add(new TMP_Dropdown.OptionData(label));
        }

        skillDropdown.ClearOptions();
        skillDropdown.AddOptions(options);
    }

    private void SetupRangeSliderIfNeeded()
    {
        if (rangeSlider == null) return;

        // 기본값 그대로면 우리가 한 번 세팅
        if (Mathf.Approximately(rangeSlider.minValue, 0f) &&
            Mathf.Approximately(rangeSlider.maxValue, 1f))
        {
            rangeSlider.minValue = defaultSliderMin;
            rangeSlider.maxValue = defaultSliderMax;
        }
    }

    /// <summary>
    /// Addressables 라벨을 기준으로 projectile 이펙트 키 목록을 가져와 드롭다운에 채운다.
    /// </summary>
    private IEnumerator BuildProjectileKeyDropdownFromLabel(string label)
    {
        if (projectileKeyDropdown == null)
            yield break;

        var handle = Addressables.LoadResourceLocationsAsync(label, typeof(GameObject));
        yield return handle;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[SkillTest] 라벨 '{label}' 로 projectile 이펙트 목록 로드 실패");
            Addressables.Release(handle);
            yield break;
        }

        projectileKeys = new List<string>();
        foreach (var loc in handle.Result)
        {
            // PrimaryKey == 우리가 InstantiateAsync 때 쓸 키
            projectileKeys.Add(loc.PrimaryKey);
        }

        projectileKeys.Sort();

        var options = new List<TMP_Dropdown.OptionData>();
        foreach (var key in projectileKeys)
            options.Add(new TMP_Dropdown.OptionData(key));

        projectileKeyDropdown.ClearOptions();
        projectileKeyDropdown.AddOptions(options);

        Addressables.Release(handle);
    }

    /// <summary>
    /// Addressables 라벨을 기준으로 hit 이펙트 키 목록을 가져와 드롭다운에 채운다.
    /// </summary>
    private IEnumerator BuildHitKeyDropdownFromLabel(string label)
    {
        if (hitKeyDropdown == null)
            yield break;

        var handle = Addressables.LoadResourceLocationsAsync(label, typeof(GameObject));
        yield return handle;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[SkillTest] 라벨 '{label}' 로 hit 이펙트 목록 로드 실패");
            Addressables.Release(handle);
            yield break;
        }

        hitKeys = new List<string>();
        foreach (var loc in handle.Result)
        {
            hitKeys.Add(loc.PrimaryKey);
        }

        hitKeys.Sort();

        var options = new List<TMP_Dropdown.OptionData>();
        foreach (var key in hitKeys)
            options.Add(new TMP_Dropdown.OptionData(key));

        hitKeyDropdown.ClearOptions();
        hitKeyDropdown.AddOptions(options);

        Addressables.Release(handle);
    }

    // ==========================
    // 스킬 선택 / UI 동기화
    // ==========================

    private void OnSkillChanged(int index)
    {
        if (index < 0 || index >= skillList.Count)
            return;

        currentSkill = skillList[index];

        currentProjectileKey = currentSkill.skillprojectile_prefab;
        currentHitKey = currentSkill.skillhit_prefab;

        Debug.Log($"[SkillTest] 스킬 선택: {currentSkill.skill_id} - {currentSkill.skill_name}");

        // projectile 키 UI
        if (projectileKeyInput != null)
            projectileKeyInput.SetTextWithoutNotify(currentProjectileKey ?? "");

        if (projectileKeyDropdown != null && projectileKeys != null)
        {
            int idx = projectileKeys.IndexOf(currentProjectileKey);
            if (idx >= 0)
                projectileKeyDropdown.SetValueWithoutNotify(idx);
        }

        // hit 키 UI
        if (hitKeyInput != null)
            hitKeyInput.SetTextWithoutNotify(currentHitKey ?? "");

        if (hitKeyDropdown != null && hitKeys != null)
        {
            int idx = hitKeys.IndexOf(currentHitKey);
            if (idx >= 0)
                hitKeyDropdown.SetValueWithoutNotify(idx);
        }

        // range UI
        if (rangeInput != null)
            rangeInput.SetTextWithoutNotify(currentSkill.skill_range.ToString("0.00"));

        if (rangeSlider != null)
        {
            float clamped = Mathf.Clamp(currentSkill.skill_range, rangeSlider.minValue, rangeSlider.maxValue);
            rangeSlider.SetValueWithoutNotify(clamped);
        }

        ApplyRangeToPreview();
    }

    private void OnProjectileKeyDropdownChanged(int index)
    {
        if (projectileKeys == null ||
            index < 0 || index >= projectileKeys.Count)
            return;

        currentProjectileKey = projectileKeys[index];

        if (projectileKeyInput != null)
            projectileKeyInput.SetTextWithoutNotify(currentProjectileKey);

        Debug.Log($"[SkillTest] projectile 키 선택: {currentProjectileKey}");
    }

    private void OnProjectileKeyInputChanged(string newText)
    {
        currentProjectileKey = newText;
        Debug.Log($"[SkillTest] projectile 키 입력 변경: {currentProjectileKey}");
    }

    private void OnHitKeyDropdownChanged(int index)
    {
        if (hitKeys == null ||
            index < 0 || index >= hitKeys.Count)
            return;

        currentHitKey = hitKeys[index];

        if (hitKeyInput != null)
            hitKeyInput.SetTextWithoutNotify(currentHitKey);

        Debug.Log($"[SkillTest] hit 키 선택: {currentHitKey}");
    }

    private void OnHitKeyInputChanged(string newText)
    {
        currentHitKey = newText;
        Debug.Log($"[SkillTest] hit 키 입력 변경: {currentHitKey}");
    }

    // ==========================
    // Range 변경
    // ==========================

    private void OnRangeInputChanged(string newText)
    {
        if (currentSkill == null || string.IsNullOrWhiteSpace(newText))
            return;

        newText = newText.Replace(',', '.');

        if (!float.TryParse(newText, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
        {
            Debug.LogWarning($"[SkillTest] range 파싱 실패: {newText}");
            if (rangeInput != null)
                rangeInput.SetTextWithoutNotify(currentSkill.skill_range.ToString("0.00"));
            return;
        }

        if (value < 0f) value = 0f;

        currentSkill.skill_range = value;

        if (rangeSlider != null)
        {
            float clamped = Mathf.Clamp(value, rangeSlider.minValue, rangeSlider.maxValue);
            rangeSlider.SetValueWithoutNotify(clamped);
        }

        Debug.Log($"[SkillTest] range 변경(입력): {currentSkill.skill_name} -> {value}");

        ApplyRangeToPreview();
        MarkCurrentSkillDirty();
    }

    private void OnRangeSliderChanged(float value)
    {
        if (currentSkill == null)
            return;

        currentSkill.skill_range = value;

        if (rangeInput != null)
            rangeInput.SetTextWithoutNotify(value.ToString("0.00"));

        Debug.Log($"[SkillTest] range 변경(슬라이더): {currentSkill.skill_name} -> {value}");

        ApplyRangeToPreview();
        MarkCurrentSkillDirty();
    }

    /// <summary>
    /// 현재 프리뷰 오브젝트에 skill_range 반영.
    /// CircleCollider2D 있으면 radius, 없으면 스케일만 조절.
    /// </summary>
    private void ApplyRangeToPreview()
    {
        if (currentPreviewInstance == null || currentSkill == null)
            return;

        float r = Mathf.Max(0.01f, currentSkill.skill_range);

        var circle = currentPreviewInstance.GetComponentInChildren<CircleCollider2D>();
        if (circle != null)
        {
            circle.radius = r;
        }

        currentPreviewInstance.transform.localScale = Vector3.one * r;
    }

    // ==========================
    // 프리뷰 & 적용
    // ==========================

    private void OnProjectilePreviewClicked()
    {
        currentProjectileKey = (currentProjectileKey ?? "").Trim();

        if (string.IsNullOrEmpty(currentProjectileKey))
        {
            Debug.LogWarning("[SkillTest] projectile 키가 비어있습니다.");
            return;
        }

        if (previewPoint == null)
        {
            Debug.LogWarning("[SkillTest] previewPoint 가 설정되어 있지 않습니다.");
            return;
        }

        ReleasePreview();

        Debug.Log($"[SkillTest] projectile 프리뷰 시도: {currentProjectileKey}");

        var handle = Addressables.InstantiateAsync(
            currentProjectileKey,
            previewPoint.position,
            Quaternion.identity
        );

        hasHandle = true;
        currentHandle = handle;

        handle.Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                currentPreviewInstance = op.Result;
                Debug.Log($"[SkillTest] projectile 프리뷰 생성 성공: {currentProjectileKey}");
                ApplyRangeToPreview();
            }
            else
            {
                Debug.LogError($"[SkillTest] projectile 프리뷰 생성 실패: {currentProjectileKey}");
                if (op.OperationException != null)
                    Debug.LogError(op.OperationException);
            }
        };
    }

    private void OnHitPreviewClicked()
    {
        currentHitKey = (currentHitKey ?? "").Trim();

        if (string.IsNullOrEmpty(currentHitKey))
        {
            Debug.LogWarning("[SkillTest] hit 키가 비어있습니다.");
            return;
        }

        if (previewPoint == null)
        {
            Debug.LogWarning("[SkillTest] previewPoint 가 설정되어 있지 않습니다.");
            return;
        }

        ReleasePreview();

        Debug.Log($"[SkillTest] hit 프리뷰 시도: {currentHitKey}");

        var handle = Addressables.InstantiateAsync(
            currentHitKey,
            previewPoint.position,
            Quaternion.identity
        );

        hasHandle = true;
        currentHandle = handle;

        handle.Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                currentPreviewInstance = op.Result;
                Debug.Log($"[SkillTest] hit 프리뷰 생성 성공: {currentHitKey}");
                ApplyRangeToPreview();
            }
            else
            {
                Debug.LogError($"[SkillTest] hit 프리뷰 생성 실패: {currentHitKey}");
                if (op.OperationException != null)
                    Debug.LogError(op.OperationException);
            }
        };
    }

    private void OnProjectileApplyClicked()
    {
        if (currentSkill == null)
        {
            Debug.LogWarning("[SkillTest] 선택된 스킬이 없습니다.");
            return;
        }

        currentSkill.skillprojectile_prefab = currentProjectileKey;
        Debug.Log($"[SkillTest] {currentSkill.skill_name} 의 skillprojectile_prefab = '{currentProjectileKey}'");

        MarkCurrentSkillDirty();
    }

    private void OnHitApplyClicked()
    {
        if (currentSkill == null)
        {
            Debug.LogWarning("[SkillTest] 선택된 스킬이 없습니다.");
            return;
        }

        currentSkill.skillhit_prefab = currentHitKey;
        Debug.Log($"[SkillTest] {currentSkill.skill_name} 의 skillhit_prefab = '{currentHitKey}'");

        MarkCurrentSkillDirty();
    }

    private void ReleasePreview()
    {
        if (currentPreviewInstance != null)
        {
            Destroy(currentPreviewInstance);
            currentPreviewInstance = null;
        }

        if (hasHandle)
        {
            Addressables.Release(currentHandle);
            hasHandle = false;
        }
    }

    private void MarkCurrentSkillDirty()
    {
#if UNITY_EDITOR
        if (currentSkill != null)
            UnityEditor.EditorUtility.SetDirty(currentSkill);
#endif
    }
}
