using UnityEditor;
using UnityEngine;
using System.IO;
using System.Globalization;
using CsvHelper;
using System.Linq;

public class CharacterDataSOGenerator : EditorWindow
{
    private string csvFilePath = "Assets/DataTables/CharacterTable.csv"; // CSV 경로
    private string saveFolderPath = "Assets/Datas/TestData/"; // SO 저장 경로

    [MenuItem("Tools/Generate CharacterData SO")]
    private static void ShowWindow()
    {
        var window = GetWindow<CharacterDataSOGenerator>();
        window.titleContent = new GUIContent("CharacterData SO Generator");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("CSV → ScriptableObject Generator", EditorStyles.boldLabel);

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
            var records = csv.GetRecords<CharacterCSVData>().ToList();

            foreach (var record in records)
            {
                string assetName = $"{record.data_AssetName}.asset";
                string assetPath = Path.Combine(saveFolderPath, assetName);

                CharacterData so = ScriptableObject.CreateInstance<CharacterData>();
                so.UpdateData(record);

                AssetDatabase.CreateAsset(so, assetPath);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"총 {csvFilePath} 라인 수만큼 SO 생성 완료!");
    }
}