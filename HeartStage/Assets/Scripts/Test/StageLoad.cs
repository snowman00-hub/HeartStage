using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageLoad : MonoBehaviour
{
    [SerializeField] private StageSetupWindow stageSetupWindow;
    [SerializeField] private TMP_InputField stageInput;
    [SerializeField] private Button stageLoadButton;
    [SerializeField] private TMP_Dropdown stageDropdown;

    public int stageId = 601;

    // dropdown index -> stageId 매핑
    private readonly List<int> dropdownStageIds = new();

    private void Start()
    {
        if (stageSetupWindow == null || stageInput == null || stageLoadButton == null || stageDropdown == null)
        {
            Debug.LogError("[StageLoad] Inspector reference missing.");
            enabled = false;
            return;
        }

        stageInput.text = stageId.ToString();

        stageInput.onEndEdit.AddListener(OnInputChanged);
        stageLoadButton.onClick.AddListener(OnClickLoadStage);

        BuildStageDropdown();
        stageDropdown.onValueChanged.AddListener(OnDropdownChanged);

        // 시작할 때 dropdown도 현재 stageId에 맞춰 동기화
        SyncDropdownByStageId(stageId);
    }

    private void OnDestroy()
    {
        stageInput.onEndEdit.RemoveListener(OnInputChanged);
        stageLoadButton.onClick.RemoveListener(OnClickLoadStage);
        stageDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
    }

    private void BuildStageDropdown()
    {
        stageDropdown.ClearOptions();
        dropdownStageIds.Clear();

        var allStages = DataTableManager.StageTable.GetAllStages();

        // 정렬 (id 순)
        var keys = new List<int>(allStages.Keys);
        keys.Sort();

        var options = new List<TMP_Dropdown.OptionData>();

        foreach (var id in keys)
        {
            var stageCsv = allStages[id];
            if (stageCsv == null) continue;

            dropdownStageIds.Add(id);

            // 표시 이름(원하는 필드명으로 바꿔)
            string label = $"{id}. {stageCsv.stage_name}";
            options.Add(new TMP_Dropdown.OptionData(label));
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
        if (idx >= 0)
            stageDropdown.SetValueWithoutNotify(idx);
    }

    private void OnInputChanged(string text)
    {
        if (int.TryParse(text, out int newStageId))
        {
            stageId = newStageId;
            SyncDropdownByStageId(stageId); // 인풋 바꾸면 드롭다운도 따라오게
        }
        else
        {
            stageInput.text = stageId.ToString();
        }
    }

    public void OnClickLoadStage()
    {
        LoadStage(stageId);
    }

    public void LoadStage(int id)
    {
        stageId = id;
        stageInput.text = stageId.ToString();
        SyncDropdownByStageId(stageId);

        var stageCsv = DataTableManager.StageTable.GetStage(stageId);
        if (stageCsv == null)
        {
            Debug.LogError($"Stage ID {stageId} not found in StageTable.");
            return;
        }

        // 버튼 클릭 시 스테이지 설정 창에 데이터 적용
        stageSetupWindow.ApplyStage(stageCsv);
        StageManager.Instance.SetCurrentStageData(stageCsv);
        StageManager.Instance.SetBackgroundByStageData(stageCsv);
        StageManager.Instance.SetWaveInfo(stageCsv.stage_step1, 1);
    }
}
