using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


public class StageWaveCSVData
{
    public string wave_stage { get; set; }
    public int wave_id { get; set; }
    public string wave_name { get; set; }
    public string stage_id { get; set; }
    public int wave_count { get; set; }
    public float spown_time { get; set; }
    public int EnemyID1 { get; set; }
    public int EnemyCount1 { get; set; }
    public int EnemyID2 { get; set; }
    public int EnemyCount2 { get; set; }
    public int EnemyID3 { get; set; }
    public int EnemyCount3 { get; set; }
    public int wave_reward { get; set; }
    public string description { get; set; }
}


public class StageWaveTable : DataTable
{
    private readonly Dictionary<int, StageWaveCSVData> table = new Dictionary<int, StageWaveCSVData>();
    private readonly List<StageWaveCSVData> orderedWaves = new List<StageWaveCSVData>(); // 순서 보장용 리스트 추가

    public override async UniTask LoadAsync(string filename)
    {
        table.Clear();
        orderedWaves.Clear(); // 추가

        AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(filename);
        TextAsset ta = await handle.Task;

        if (!ta)
        {
            Debug.LogError($"TextAsset 로드 실패: {filename}");
            return;
        }

        var list = LoadCSV<StageWaveCSVData>(ta.text);

        foreach (var item in list)
        {
            if (!table.ContainsKey(item.wave_id))
            {
                table.Add(item.wave_id, item);
                orderedWaves.Add(item); // CSV 파일 순서대로 추가
            }
            else
            {
                Debug.LogError($"웨이브 아이디 중복: {item.wave_id}");
            }
        }

        Addressables.Release(handle);
    }

    public StageWaveCSVData Get(int waveId)
    {
        if (!table.ContainsKey(waveId))
        {
            Debug.LogWarning($"웨이브 아이디를 찾을 수 없음: {waveId}");
            return null;
        }
        return table[waveId];
    }

    // 모든 웨이브 데이터 반환 (Dictionary 형태)
    public Dictionary<int, StageWaveCSVData> GetAll()
    {
        return new Dictionary<int, StageWaveCSVData>(table);
    }

    // CSV 파일 순서대로 정렬된 웨이브 리스트 반환
    public List<StageWaveCSVData> GetOrderedWaves()
    {
        return new List<StageWaveCSVData>(orderedWaves);
    }

    // 현재 웨이브의 다음 웨이브 반환 (CSV 순서 기준)
    public StageWaveCSVData GetNextWave(int currentWaveId)
    {
        int currentIndex = orderedWaves.FindIndex(w => w.wave_id == currentWaveId);

        if (currentIndex >= 0 && currentIndex < orderedWaves.Count - 1)
        {
            return orderedWaves[currentIndex + 1];
        }

        return null; // 다음 웨이브가 없음
    }
}
