using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageLoad : MonoBehaviour
{
    [SerializeField] private TMP_InputField stageInput;
    [SerializeField] private Button stageLoadButton;
    [SerializeField] private TMP_Dropdown stageDropdown;
    [SerializeField] private TextMeshProUGUI LoadMassage;

    public int stageId = 601;   // fallback

    private readonly List<int> dropdownStageIds = new();
    private bool _isLoading;

    private async void Start()
    {
        // StageTable 준비 대기 (timeScale=0이어도 됨)
        while (DataTableManager.StageTable == null)
            await UniTask.Delay(50, DelayType.UnscaledDeltaTime);

        // ✅ 1) "현재 스테이지 id"로 stageId 갱신
        ResolveCurrentStageId();

        // ✅ 2) Dropdown 만들고 현재 id로 동기화
        BuildStageDropdown();
        stageDropdown.onValueChanged.AddListener(OnDropdownChanged);

        SyncDropdownByStageId(stageId);

        // ✅ 3) Input도 현재 id 기준으로 표시
        stageInput.text = stageId.ToString();
        stageInput.onEndEdit.AddListener(OnInputChanged);

        stageLoadButton.onClick.AddListener(OnClickLoadStage);

        LoadMassage.text = $"현재 스테이지 ID: {stageId}";
    }

    private void OnDestroy()
    {
        stageInput.onEndEdit.RemoveListener(OnInputChanged);
        stageLoadButton.onClick.RemoveListener(OnClickLoadStage);
        stageDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
    }

    // ✅ 현재 스테이지 id 찾기
    private void ResolveCurrentStageId()
    {
        var gameData = SaveLoadManager.Data;
        stageId = gameData.selectedStageID;

        if (stageId <= 0)
        {
            stageId = 601;
        }
    }

    private void BuildStageDropdown()
    {
        stageDropdown.ClearOptions();
        dropdownStageIds.Clear();

        var allStages = DataTableManager.StageTable.GetAllStages();
        var keys = new List<int>(allStages.Keys);
        keys.Sort();

        var options = new List<TMP_Dropdown.OptionData>();
        foreach (var id in keys)
        {
            var stageCsv = allStages[id];
            if (stageCsv == null) continue;

            dropdownStageIds.Add(id);
            options.Add(new TMP_Dropdown.OptionData($"{id}. {stageCsv.stage_name}"));
        }

        stageDropdown.AddOptions(options);
    }

    private void OnDropdownChanged(int index)
    {
        if (index < 0 || index >= dropdownStageIds.Count) return;

        stageId = dropdownStageIds[index];
        stageInput.text = stageId.ToString();

        LoadStage(stageId);
    }

    private void SyncDropdownByStageId(int id)
    {
        int idx = dropdownStageIds.IndexOf(id);

        // ✅ 목록에 없으면 첫 번째로 fallback
        if (idx < 0 && dropdownStageIds.Count > 0)
        {
            idx = 0;
            stageId = dropdownStageIds[0];
        }

        if (idx >= 0)
            stageDropdown.SetValueWithoutNotify(idx);

        stageDropdown.RefreshShownValue();
    }

    private void OnInputChanged(string text)
    {
        if (int.TryParse(text, out int newStageId))
        {
            stageId = newStageId;
            SyncDropdownByStageId(stageId);
        }
        else
        {
            stageInput.text = stageId.ToString();
        }
    }

    public void OnClickLoadStage() => LoadStage(stageId);

    public void LoadStage(int id)
    {
        if (_isLoading) return;
        _isLoading = true;

        stageId = id;
        stageInput.text = stageId.ToString();
        SyncDropdownByStageId(stageId);

        if (LoadMassage != null)
            LoadMassage.text = $"씬 리로드로 로딩중 {stageId}...";

        // ✅ prefs 저장 + Stage 씬 다시 로드
        LoadSceneManager.Instance.GoTestStage(stageId, 1);
    }
}
