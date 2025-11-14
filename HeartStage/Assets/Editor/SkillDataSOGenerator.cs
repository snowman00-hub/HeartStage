using CsvHelper;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class SkillDataSOGenerator : EditorWindow
{
    private string csvFilePath = "Assets/DataTables/SkillTable.csv"; // CSV 경로
    private string saveFolderPath = "Assets/ScriptableObject/SkillData/"; // SO 저장 경로

    [MenuItem("Tools/Generate SkillData SO")]
    private static void ShowWindow()
    {
        var window = GetWindow<SkillDataSOGenerator>();
        window.titleContent = new GUIContent("SkillData SO Generator");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("CSV → SkillData ScriptableObject Generator", EditorStyles.boldLabel);

        csvFilePath = EditorGUILayout.TextField("CSV File Path", csvFilePath);
        saveFolderPath = EditorGUILayout.TextField("SO Save Folder", saveFolderPath);

        if (GUILayout.Button("Generate SOs"))
        {
            GenerateSOs();
        }
    }

    private void GenerateSOs()
    {
        if (!File.Exists(csvFilePath))
        {
            Debug.LogError($"CSV 파일이 존재하지 않습니다: {csvFilePath}");
            return;
        }

        if (!Directory.Exists(saveFolderPath))
            Directory.CreateDirectory(saveFolderPath);

        using (var reader = new StreamReader(csvFilePath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<SkillCSVData>().ToList();

            foreach (var record in records)
            {
                // 파일 이름: skill_name 기준 (없으면 skill_id)
                string assetName = !string.IsNullOrEmpty(record.skill_name)
                    ? $"{record.skill_name}.asset"
                    : $"Skill_{record.skill_id}.asset";

                string assetPath = Path.Combine(saveFolderPath, assetName);

                // ScriptableObject 생성 및 데이터 갱신
                SkillData so = ScriptableObject.CreateInstance<SkillData>();
                so.UpdateData(record);

                AssetDatabase.CreateAsset(so, assetPath);

                // Addressable 등록
                AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
                if (settings != null)
                {
                    string guid = AssetDatabase.AssetPathToGUID(assetPath);
                    AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
                    entry.address = Path.GetFileNameWithoutExtension(assetPath);
                    entry.SetLabel(AddressableLabel.Stage, true, true);
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"ActiveSkillData ScriptableObject 생성 완료!");
    }
}
