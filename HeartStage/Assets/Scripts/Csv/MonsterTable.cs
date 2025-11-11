using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.IO;

public enum atk_type
{
    closeRange = 1,
    longRange = 2
}

public enum mon_type
{
    normal = 1,
    boss = 2
}

public class MonsterCSVData
{
    public int id { get; set; }
    public string mon_name { get; set; }
    public int mon_type { get; set; }
    public int stage_num { get; set; }
    public int atk_type { get; set; }
    public int atk_dmg { get; set; }
    public int atk_speed { get; set; }
    public int atk_range { get; set; }
    public int bullet_speed { get; set; }
    public int hp { get; set; }
    public int speed { get; set; }
    public int skill_id { get; set; }
    public int min_level { get; set; }
    public int max_level { get; set; }
    public string image_AssetName { get; set; }
}

public class MonsterTable : DataTable
{
    private readonly Dictionary<int, MonsterCSVData> table = new Dictionary<int, MonsterCSVData>();

    public override async UniTask LoadAsync(string filename)
    {
        table.Clear();
        AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(filename);
        TextAsset ta = await handle.Task;

        if (!ta)
        {
            Debug.LogError($"TextAsset 로드 실패: {filename}");
            return;
        }

        var list = LoadCSV<MonsterCSVData>(ta.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.id))
            {
                table.Add(item.id, item);
            }
            else
            {
                Debug.LogError($"몬스터 아이디 중복: {item.id}");
            }
        }

        Addressables.Release(handle);
    }

    public MonsterCSVData Get(int monsterId)
    {
        if (!table.ContainsKey(monsterId))
        {
            Debug.LogWarning($"몬스터 아이디를 찾을 수 없음: {monsterId}");
            return null;
        }
        return table[monsterId];
    }
    public void UpdateOrAdd(MonsterCSVData data)
    {
        table[data.id] = data;
    }

    // 테이블의 모든 데이터 가져오기 (SO 생성용)
    public IEnumerable<MonsterCSVData> GetAllData()
    {
        return table.Values;
    }

    // CSV로 저장
    public void SaveToCSV(string filePath)
    {
        var dataList = table.Values.ToList();

        var csv = new StringBuilder();
        // CSV 헤더
        csv.AppendLine("id,mon_name,mon_type,stage_num,atk_type,atk_dmg,atk_speed,atk_range,bullet_speed,hp,speed,skill_id,min_level,max_level,image_AssetName");

        // 데이터 행
        foreach (var data in dataList)
        {
            csv.AppendLine($"{data.id},{data.mon_name},{data.mon_type},{data.stage_num},{data.atk_type},{data.atk_dmg},{data.atk_speed},{data.atk_range},{data.bullet_speed},{data.hp},{data.speed},{data.skill_id},{data.min_level},{data.max_level},{data.image_AssetName}");
        }

        File.WriteAllText(filePath, csv.ToString());
    }

}
