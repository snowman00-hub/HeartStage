using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpSelectSlot : MonoBehaviour
{
    public TextMeshProUGUI selectName;
    public Image selectImage;
    public TextMeshProUGUI selectDesc;

    [HideInInspector]
    public SelectData selectData;

    public void ChooseSlot()
    {
        var buffIds = new List<(int id, float value)>();
        if(selectData.effect_type1 != 0)
        {
            buffIds.Add((selectData.effect_type1, selectData.value1));
        }
        if (selectData.effect_type2 != 0)
        {
            buffIds.Add((selectData.effect_type2, selectData.value2));
        }

        GameObject[] towers = GameObject.FindGameObjectsWithTag(Tag.Tower);
        foreach (GameObject tower in towers)
        {
            foreach(var buff in buffIds)
            {
                Debug.Log($"{buff.id} 장착");
                EffectRegistry.Apply(tower, buff.id, buff.value, 99999);
            }
        }
        StageManager.Instance.RestoreTimeScale();
    }

    public void Init(SelectData data)
    {
        selectData = data;
        selectName.text = data.select_name;
        var texture = ResourceManager.Instance.Get<Texture2D>(data.prefab);
        selectImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        selectDesc.text = data.info;
    }
}