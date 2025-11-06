using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks.CompilerServices;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private AssetReference monsterPrefab;

    [Header("Field")]
    private MonsterData monsterData;
    private int spawneTimeTest = 1;
    private async void Start()
    {
        await SpawnMonstersLoop(spawneTimeTest);
    }
    private async UniTask SpawnMonstersLoop(int spawneTimeTest)
    {
        while (true)
        {
            await SpawnMonster(spawneTimeTest);
        }
    }

    private async UniTask SpawnMonster(int spawneTimeTest)
    {
        int randomRange = Random.Range(0, Screen.width);
        Vector3 screenPosition = new Vector3(randomRange, Screen.height, Camera.main.nearClipPlane);
        Vector3 spawnPosTest = Camera.main.ScreenToWorldPoint(screenPosition);

        await UniTask.Delay(spawneTimeTest * 2000);
        await Addressables.InstantiateAsync(monsterPrefab, spawnPosTest, Quaternion.identity);
    }
}
