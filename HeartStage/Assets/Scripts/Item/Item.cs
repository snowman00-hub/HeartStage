using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Item : MonoBehaviour
{
    [SerializeField]private ItemCSVData itemCSVData;

    [SerializeField] private ItemData _itemData;

    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private AsyncOperationHandle<Sprite>? _imageHandle;
    private AsyncOperationHandle<RuntimeAnimatorController>? _animatorHandle;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        if (_spriteRenderer) _spriteRenderer.enabled = false;
    }

    public async UniTaskVoid Init(ItemCSVData data, CancellationToken ct = default)
    {
        _itemData.UpdateData(data);

        var itemspriteRef = new AssetReferenceT<Sprite>(_itemData.SpriteTestID);
        var itemanimatorRef = new AssetReferenceT<RuntimeAnimatorController>(_itemData.AnimationTestID);

        try
        {
            if(!string.IsNullOrEmpty(itemspriteRef.AssetGUID))
            {
                _imageHandle = itemspriteRef.LoadAssetAsync();
                var sprite = await _imageHandle.Value.ToUniTask(cancellationToken: ct);
                if (_spriteRenderer)
                {
                    _spriteRenderer.sprite = sprite;
                    _spriteRenderer.enabled = true;
                }
            }
            if(!string.IsNullOrEmpty(itemanimatorRef.AssetGUID))
            {
                _animatorHandle = itemanimatorRef.LoadAssetAsync();
                var animatorController = await _animatorHandle.Value.ToUniTask(cancellationToken: ct);
                if (_animator)
                {
                    _animator.runtimeAnimatorController = animatorController;
                }
            }
        }
        catch (System.OperationCanceledException)
        {
            // Handle cancellation if needed
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Item load failed: {ex}");
        }
    }

    private void OnDestroy()
    {
        if (_imageHandle.HasValue)
        { 
            Addressables.Release(_imageHandle.Value);
        }
        if(_animatorHandle.HasValue)
        {
            Addressables.Release(_animatorHandle.Value);
        }
        _imageHandle = null;
        _animatorHandle = null;
    }
}
