using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public static class DataTableManager
{
    private static readonly Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();
    private static UniTask _initialization;

    public static UniTask Initialization => _initialization;

    static DataTableManager()
    {
        _initialization = InitAsync();
    }

    public static async UniTask InitAsync()
    {
        {
            var table = new ItemTable();
            await table.LoadAsync(DataTableIds.Item);
            tables.Add(DataTableIds.Item, table);
        }

        {
            var monsterTable = new MonsterTable();
            await monsterTable.LoadAsync(DataTableIds.Monster);
            tables.Add(DataTableIds.Monster, monsterTable);
        }

        {
            var stageWaveTable = new StageWaveTable();
            await stageWaveTable.LoadAsync(DataTableIds.StageWave);
            tables.Add(DataTableIds.StageWave, stageWaveTable);
        }

        {
            var table = new CharacterTable();
            var id = DataTableIds.Character;
            await table.LoadAsync(id);
            tables.Add(id, table);
        }
    }

    public static ItemTable ItemTable
    {
        get
        {
            return Get<ItemTable>(DataTableIds.Item);
        }
    }
    
    public static MonsterTable MonsterTable
    {
        get
        {
            return Get<MonsterTable>(DataTableIds.Monster);
        }
    }

    public static StageWaveTable StageWaveTable
    {
        get
        {
            return Get<StageWaveTable>(DataTableIds.StageWave);
        }
    }

    public static CharacterTable CharacterTable
    {
        get
        {
            return Get<CharacterTable>(DataTableIds.Character);
        }
    }

    public static T Get<T>(string id) where T : DataTable
    {
        if (!tables.ContainsKey(id))
        {
            Debug.LogError("테이블 없음");
            return null;
        }
        return tables[id] as T;
    }
}
