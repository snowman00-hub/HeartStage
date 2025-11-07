using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public int ID;
    public string Name;
    public AssetReferenceT<Sprite> Sprite;
    public AssetReferenceT<RuntimeAnimatorController> Animation;
}
