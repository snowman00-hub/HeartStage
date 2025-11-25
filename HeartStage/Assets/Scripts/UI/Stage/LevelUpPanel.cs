using System.Collections.Generic;
using UnityEngine;

public class LevelUpPanel : MonoBehaviour
{
    public List<LevelUpSelectSlot> slots;
    
    private void OnEnable()
    {
        var randomDatas = DataTableManager.SelectTable.GetRandomThree();
        for(int i = 0; i < 3; i++)
        {
            slots[i].Init(randomDatas[i]);
        }
    }
}