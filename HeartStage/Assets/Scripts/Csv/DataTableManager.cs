using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

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

        {
            var table = new SkillTable();
            var id = DataTableIds.Skill;
            await table.LoadAsync(id);
            tables.Add(id, table);
        }

        { 
            var table = new EffectTable();
            var id = DataTableIds.Effect;
            await table.LoadAsync(id);
            tables.Add(id, table);
        }

        {
            var table = new StageTable();
            var id = DataTableIds.Stage;
            await table.LoadAsync(id);
            tables.Add(id, table);
        }

        {
            var table = new SelectTable();
            var id = DataTableIds.Select;
            await table.LoadAsync(id);
            tables.Add(id, table);
        }

        { 
            var table = new SynergyTable();
            var id = DataTableIds.Synergy;
            await table.LoadAsync(id);
            tables.Add(id, table);
        }

        {
            var table = new RewardTable();
            var id = DataTableIds.Reward;
            await table.LoadAsync(id);
            tables.Add(id, table);
        }

        {
            var table = new ShopTable();
            var id = DataTableIds.Shop;
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
    
    public static SkillTable SkillTable
    {
        get
        {
            return Get<SkillTable>(DataTableIds.Skill);
        }
    }
    public static EffectTable EffectTable
    {
        get
        {
            return Get<EffectTable>(DataTableIds.Effect);
        }
    }
    public static StageTable StageTable
    {
        get
        {
            return Get<StageTable>(DataTableIds.Stage);
        }
    }

    public static SelectTable SelectTable
    {
        get
        {
            return Get<SelectTable>(DataTableIds.Select);
        }
    }

    public static SynergyTable SynergyTable
    {
        get
        {
            return Get<SynergyTable>(DataTableIds.Synergy);
        }
    }

    public static RewardTable RewardTable
    {
        get
        {
            return Get<RewardTable>(DataTableIds.Reward);
        }
    }

    public static ShopTable ShopTable
    {
        get
        {
            return Get<ShopTable>(DataTableIds.Shop);
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
