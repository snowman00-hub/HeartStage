using Cysharp.Threading.Tasks;
using UnityEngine;

public class csvTest : MonoBehaviour
{
    private async UniTaskVoid Start()
    {
        // 반드시 초기화가 끝날 때까지 기다림
        await DataTableManager.Initialization;

        var table = DataTableManager.StringTable;
        if (table == null)
        {
            Debug.LogError("StringTable이 초기화되지 않았습니다.");
            return;
        }

        var data = table.Get("8001");
        Debug.Log(data);
    }
}