using UnityEngine;

[CreateAssetMenu(fileName = "StageWaveData", menuName = "Scriptable Objects/StageWaveData")]
public class StageWaveData : ScriptableObject
{
    public int wave_id;
    public string wave_name;
    public float enemy_spown_time;
    public int EnemyID1;
    public int EnemyCount1;
    public int EnemyID2;
    public int EnemyCount2;
    public int EnemyID3;
    public int EnemyCount3;
    public int wave_reward;
    public string info;
    
    public void UpdateData(StageWaveCSVData csvData)
    {
        wave_id = csvData.wave_id;
        wave_name = csvData.wave_name;
        enemy_spown_time = csvData.enemy_spown_time;
        EnemyID1 = csvData.EnemyID1;
        EnemyCount1 = csvData.EnemyCount1;
        EnemyID2 = csvData.EnemyID2;
        EnemyCount2 = csvData.EnemyCount2;
        EnemyID3 = csvData.EnemyID3;
        EnemyCount3 = csvData.EnemyCount3;
        wave_reward = csvData.wave_reward;
        info = csvData.info;
    }

    public StageWaveCSVData ToCSVData()
    {
        StageWaveCSVData csvData = new StageWaveCSVData();
        csvData.wave_id = wave_id;
        csvData.wave_name = wave_name;
        csvData.enemy_spown_time = enemy_spown_time;
        csvData.EnemyID1 = EnemyID1;
        csvData.EnemyCount1 = EnemyCount1;
        csvData.EnemyID2 = EnemyID2;
        csvData.EnemyCount2 = EnemyCount2;
        csvData.EnemyID3 = EnemyID3;
        csvData.EnemyCount3 = EnemyCount3;
        csvData.wave_reward = wave_reward;
        csvData.info = info;
        return csvData;
    }
}

