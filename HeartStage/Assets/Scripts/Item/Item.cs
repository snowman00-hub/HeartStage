using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

public class Item : MonoBehaviour
{
    [SerializeField]private ItemData m_itemData;

    private SpriteRenderer m_spriteRenderer;
    private Animator m_animator;

    private AsyncOperationHandle<Sprite>? m_imageHandle;
    private AsyncOperationHandle<RuntimeAnimatorController>? m_animatorHandle;

    private void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_animator = GetComponent<Animator>();
    }

    private async UniTaskVoid Start()
    {
        var ct = this.GetCancellationTokenOnDestroy();

        if (m_spriteRenderer)
        {
            m_spriteRenderer.enabled = false;
        }

        try 
        {
            if(!string.IsNullOrEmpty(m_itemData.Sprite.AssetGUID))
            {
                m_imageHandle = m_itemData.Sprite.LoadAssetAsync();
                var sprite = await m_imageHandle.Value.ToUniTask(cancellationToken: ct);
                if (m_spriteRenderer)
                {
                    m_spriteRenderer.sprite = sprite;
                    m_spriteRenderer.enabled = true;
                }
            }
            if(!string.IsNullOrEmpty(m_itemData.Animation.AssetGUID))
            {
                m_animatorHandle = m_itemData.Animation.LoadAssetAsync();
                var animatorController = await m_animatorHandle.Value.ToUniTask(cancellationToken: ct);
                if (m_animator)
                {
                    m_animator.runtimeAnimatorController = animatorController;
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
        if (m_imageHandle.HasValue)
        { 
            Addressables.Release(m_imageHandle.Value);
        }
        if(m_animatorHandle.HasValue)
        {
            Addressables.Release(m_animatorHandle.Value);
        }
        m_imageHandle = null;
        m_animatorHandle = null;
    }
}
