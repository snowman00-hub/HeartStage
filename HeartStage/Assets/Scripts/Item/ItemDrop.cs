using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ItemDrop : MonoBehaviour
{
    [SerializeField] private float _dropRadius = 1f;
    [SerializeField] private AssetReferenceGameObject _itemPrefab;

    private float testtimer = 0f;

    private async UniTask DropAsync(int dropItemId, Vector3 dropPos)
    {
        var table = DataTableManager.ItemTable;
        var itemData = table.Get(dropItemId);
        var item = await _itemPrefab.InstantiateAsync(dropPos + Random.insideUnitSphere * _dropRadius, Quaternion.identity);
        item.GetComponent<Item>()?.Init(itemData);
    }

    private void Update()
    {
        testtimer += Time.deltaTime;
        if (testtimer >= 1f)
        {
            DropAsync(1001, transform.position).Forget();
            testtimer = 0f;
        }
    }
}

