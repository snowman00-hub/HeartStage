using UnityEditor;
using UnityEngine;
using System.IO;
using System.Globalization;
using CsvHelper;
using System.Collections.Generic;

public class SynergyDataSOExporter : EditorWindow
{
    private string csvFilePath = "Assets/DataTables/SynergyTable.csv"; // 덮어쓸 CSV 경로
    private string soFolderPath = "Assets/ScriptableObject/SynergyData/"; // SO 경로

    [MenuItem("Tools/Export SynergyData SO to CSV")]
    private static void ShowWindow()
    {
        var window = GetWindow<SynergyDataSOExporter>();
        window.titleContent = new GUIContent("SynergyData SO Exporter");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("SO → CSV Exporter (SynergyData)", EditorStyles.boldLabel);

        soFolderPath = EditorGUILayout.TextField("SO Folder Path", soFolderPath);
        csvFilePath = EditorGUILayout.TextField("CSV File Path", csvFilePath);

        if (GUILayout.Button("Export to CSV"))
        {
            ExportToCSV();
        }
    }

    private void ExportToCSV()
    {
        string[] guids = AssetDatabase.FindAssets("t:SynergyData", new[] { soFolderPath });
        List<SynergyCSVData> dataList = new List<SynergyCSVData>();

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SynergyData so = AssetDatabase.LoadAssetAtPath<SynergyData>(path);
            if (so != null)
            {
                dataList.Add(so.ToCSVData());
            }
        }
        // 🔹 여기서 id 기준으로 정렬해주기
        // CharacterCSVData에 있는 실제 필드명에 맞게 바꿔줘 (예: id, char_id 등)
        dataList.Sort((a, b) => a.synergy_id.CompareTo(b.synergy_id));
        // 만약 필드명이 char_id면:
        // dataList.Sort((a, b) => a.char_id.CompareTo(b.char_id));
        if (dataList.Count == 0)
        {
            Debug.LogWarning($"해당 폴더에 SynergyData SO가 없습니다: {soFolderPath}");
            return;
        }

        using (var writer = new StreamWriter(csvFilePath))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(dataList);
        }

        AssetDatabase.Refresh();
        Debug.Log($"총 {dataList.Count}개의 SynergyData SO 데이터를 {csvFilePath}에 저장 완료!");
    }
}