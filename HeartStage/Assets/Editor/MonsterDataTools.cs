#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Cysharp.Threading.Tasks;

public class MonsterDataTools : EditorWindow
{
    private static bool _isProcessing = false;
    private static bool _isInitialized = false;

    [MenuItem("Tools/Monster Data Tools")]
    public static void ShowWindow()
    {
        GetWindow<MonsterDataTools>("Monster Data Tools");
    }

    private void OnGUI()
    {
        GUILayout.Label("Monster Data Tools", EditorStyles.boldLabel);
        GUILayout.Space(20);

        EditorGUI.BeginDisabledGroup(_isProcessing);

        GUILayout.Label("1. 데이터테이블에서 SO 자동 생성", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("MonsterTable의 모든 데이터를 읽어서 MonsterData SO를 자동 생성합니다.", MessageType.Info);

        if (GUILayout.Button("데이터테이블 → SO 생성", GUILayout.Height(40)))
        {
            CreateSOFromDataTable();
        }

        GUILayout.Space(20);

        GUILayout.Label("2. SO 데이터로 데이터테이블 덮어쓰기", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("프로젝트 내 모든 MonsterData SO를 찾아서 데이터테이블을 덮어쓰고 CSV로 저장합니다.", MessageType.Info);

        if (GUILayout.Button("SO → 데이터테이블 덮어쓰기", GUILayout.Height(40)))
        {
            OverwriteDataTableFromSO();
        }

        EditorGUI.EndDisabledGroup();

        if (_isProcessing)
        {
            EditorGUILayout.HelpBox("작업 진행 중...", MessageType.Info);
        }
    }

    private async void CreateSOFromDataTable()
    {
        if (_isProcessing) return;

        _isProcessing = true;
        try
        {
            await EnsureInitialized();

            var monsterTable = DataTableManager.MonsterTable;
            if (monsterTable == null)
            {
                Debug.LogError("MonsterTable이 초기화되지 않았습니다.");
                return;
            }

            string folderPath = "Assets/ScriptableObject/Monsters";
            EnsureFolderExists(folderPath);

            int createCount = 0;
            var allData = monsterTable.GetAllData();

            foreach (var data in allData)
            {
                string assetPath = $"{folderPath}/MonsterData_{data.id}.asset";
                MonsterData existingSO = AssetDatabase.LoadAssetAtPath<MonsterData>(assetPath);

                MonsterData so = existingSO ?? ScriptableObject.CreateInstance<MonsterData>();
                AssignDataToSO(so, data);

                if (existingSO == null)
                {
                    AssetDatabase.CreateAsset(so, assetPath);
                    Debug.Log($"새 SO 생성: {data.mon_name} (ID: {data.id})");
                }
                else
                {
                    EditorUtility.SetDirty(so);
                    Debug.Log($"기존 SO 업데이트: {data.mon_name} (ID: {data.id})");
                }

                createCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"MonsterData SO 작업 완료! 총 {createCount}개 처리됨");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SO 생성 실패: {e.Message}");
        }
        finally
        {
            _isProcessing = false;
            Repaint();
        }
    }

    private async void OverwriteDataTableFromSO()
    {
        if (_isProcessing) return;

        _isProcessing = true;
        try
        {
            await EnsureInitialized();

            var monsterTable = DataTableManager.MonsterTable;
            if (monsterTable == null)
            {
                Debug.LogError("MonsterTable이 초기화되지 않았습니다.");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:MonsterData");
            if (guids.Length == 0)
            {
                Debug.LogWarning("MonsterData SO를 찾을 수 없습니다.");
                return;
            }

            int updateCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonsterData so = AssetDatabase.LoadAssetAtPath<MonsterData>(path);

                if (so != null && so.id > 0)
                {
                    var tableData = so.ToTableData();
                    monsterTable.UpdateOrAdd(tableData);
                    updateCount++;
                    Debug.Log($"테이블 업데이트: {so.monsterName} (ID: {so.id})");
                }
            }

            string csvPath = EditorUtility.SaveFilePanel("Monster CSV 저장", "Assets", "monster_data_updated", "csv");
            if (!string.IsNullOrEmpty(csvPath))
            {
                monsterTable.SaveToCSV(csvPath);
                Debug.Log($"성공! {updateCount}개 SO 데이터를 테이블에 반영하고 {csvPath}에 저장했습니다.");
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.Log($"{updateCount}개 SO 데이터를 테이블에 반영했습니다. (CSV 저장 취소됨)");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"테이블 덮어쓰기 실패: {e.Message}");
        }
        finally
        {
            _isProcessing = false;
            Repaint();
        }
    }

    // 헬퍼 메서드들
    private async UniTask EnsureInitialized()
    {
        if (_isInitialized)
            return;
        
         await DataTableManager.Initialization;
        _isInitialized = true;
    }

    private void EnsureFolderExists(string folderPath)
    {
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObject"))
            AssetDatabase.CreateFolder("Assets", "ScriptableObject");
        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder("Assets/ScriptableObject", "Monsters");
    }

    private void AssignDataToSO(MonsterData so, Data data)
    {
        so.id = data.id;
        so.monsterName = data.mon_name;
        so.monsterType = data.mon_type;
        so.hp = data.hp;
        so.att = data.atk_dmg;
        so.attType = data.atk_type;
        so.attackSpeed = data.atk_speed;
        so.attackRange = data.atk_range;
        so.bulletSpeed = data.bullet_speed;
        so.moveSpeed = data.speed;
        so.minExp = data.min_level;
        so.maxExp = data.max_level;
    }
}
#endif