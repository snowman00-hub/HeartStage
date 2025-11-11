using UnityEditor;
using UnityEngine;
using System.IO;
using System.Globalization;
using CsvHelper;
using System.Collections.Generic;

public class ActiveSkillDataSOExporter : EditorWindow
{
    private string csvFilePath = "Assets/DataTables/ActiveSkillTable.csv"; // 덮어쓸 CSV 경로
    private string soFolderPath = "Assets/ScriptableObject/ActiveSkillData/"; // SO 경로

    [MenuItem("Tools/Export ActiveSkillData SO to CSV")]
    private static void ShowWindow()
    {
        var window = GetWindow<ActiveSkillDataSOExporter>();
        window.titleContent = new GUIContent("ActiveSkillData SO Exporter");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("SO → CSV Exporter (ActiveSkillData)", EditorStyles.boldLabel);

        soFolderPath = EditorGUILayout.TextField("SO Folder Path", soFolderPath);
        csvFilePath = EditorGUILayout.TextField("CSV File Path", csvFilePath);

        if (GUILayout.Button("Export to CSV"))
        {
            ExportToCSV();
        }
    }

    private void ExportToCSV()
    {
        string[] guids = AssetDatabase.FindAssets("t:ActiveSkillData", new[] { soFolderPath });
        List<ActiveSkillCSVData> dataList = new List<ActiveSkillCSVData>();

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ActiveSkillData so = AssetDatabase.LoadAssetAtPath<ActiveSkillData>(path);
            if (so != null)
            {
                dataList.Add(so.ToCSVData());
            }
        }

        if (dataList.Count == 0)
        {
            Debug.LogWarning($"해당 폴더에 ActiveSkillData SO가 없습니다: {soFolderPath}");
            return;
        }

        using (var writer = new StreamWriter(csvFilePath))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(dataList);
        }

        AssetDatabase.Refresh();
        Debug.Log($"총 {dataList.Count}개의 ActiveSkillData SO 데이터를 {csvFilePath}에 저장 완료!");
    }
}