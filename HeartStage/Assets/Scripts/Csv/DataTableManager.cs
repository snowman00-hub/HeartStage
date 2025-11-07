using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public static class DataTableManger
{
    private static readonly Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();
    private static UniTask _initialization;

    public static UniTask Initialization => _initialization;


    static DataTableManger()
    {
        _initialization = InitAsync();
    }

    public static async UniTask InitAsync()
    {
        foreach (var id in DataTableIds.StringTableIds)
        {
            var table = new StringTable();
            await table.LoadAsync(id);
            tables.Add(id, table);
        }
    }

    public static StringTable StringTable
    {
        get
        {
            return Get<StringTable>(DataTableIds.String);
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
