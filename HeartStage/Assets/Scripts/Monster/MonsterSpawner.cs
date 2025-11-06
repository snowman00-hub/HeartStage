using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks.CompilerServices;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GameObject monsterPrefab;

    [Header("Field")]
    private MonsterData monsterData;
    private int spawneTimeTest = 1;
    private Vector3 spawnPosTest = new Vector3(0, 0, 0);


    private async void Start()
    {
        await SpawnMonster(spawneTimeTest);
    }

    private async UniTask SpawnMonster(int spawneTimeTest)
    {
        await UniTask.Delay(spawneTimeTest * 1000);
        var monster = InstantiateAsync(monsterPrefab, spawnPosTest, Quaternion.identity);
    }

    private void Destroy()
    {
        Destroy(this.gameObject, 2);
    }
    private void OnDestroy()
    {
        
    }

}
