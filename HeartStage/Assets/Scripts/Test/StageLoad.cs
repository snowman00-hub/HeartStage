using Cysharp.Threading.Tasks;
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
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private TextMeshProUGUI LoadMassage;

    public int stageId = 601;

    private readonly List<int> dropdownStageIds = new();
    private bool _isLoading;

    private async void Start()
    {
        // StageTable 준비 대기 (timeScale=0이어도 됨)
        while (DataTableManager.StageTable == null)
            await UniTask.Delay(50, DelayType.UnscaledDeltaTime);

        stageInput.text = stageId.ToString();
        stageInput.onEndEdit.AddListener(OnInputChanged);
        stageLoadButton.onClick.AddListener(OnClickLoadStage);

        BuildStageDropdown();
        stageDropdown.onValueChanged.AddListener(OnDropdownChanged);

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
        if (idx >= 0)
            stageDropdown.SetValueWithoutNotify(idx);
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

    public async void LoadStage(int id)
    {
        if (_isLoading) return;
        _isLoading = true;

        try
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

            LoadMassage.text = $"로딩중 {stageId}...";

            if (monsterSpawner != null)
                await monsterSpawner.ChangeStage(stageId, false);

            // 스포너 변경 끝난 뒤 UI 적용
            stageSetupWindow.ApplyStage(stageCsv);
            StageManager.Instance.SetCurrentStageData(stageCsv);
            StageManager.Instance.SetBackgroundByStageData(stageCsv);
            StageManager.Instance.SetWaveInfo(stageCsv.stage_step1, 1);

            if (stageSetupWindow != null)
                stageSetupWindow.gameObject.SetActive(true);

            LoadMassage.text = $"{stageId} 로딩완료";

            StageManager.Instance.SetTimeScale(0f);
        }
        finally
        {
            _isLoading = false;
        }
    }
}
