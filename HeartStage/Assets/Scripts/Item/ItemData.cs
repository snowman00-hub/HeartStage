using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public int ID;
    public string Name;
    public AssetReferenceT<Sprite> Sprite;
    public AssetReferenceT<RuntimeAnimatorController> Animation;

    public void Init(ItemCSVData data)
    {
        ID = data.ID;
        Name = data.Name;
        Sprite = new AssetReferenceT<Sprite>(data.SpriteTestID);
        Animation = new AssetReferenceT<RuntimeAnimatorController>(data.AnimationTestID);
    }
}

[Serializable]
public class ItemCSVData
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string SpriteTestID { get; set; }
    public string AnimationTestID { get; set; }
}
