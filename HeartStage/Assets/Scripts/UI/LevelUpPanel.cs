using System.Collections.Generic;
using UnityEngine;

public class LevelUpPanel : MonoBehaviour
{
    public static LevelUpPanel Instance;

    public List<LevelUpSelectSlot> slots;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        //var randomDatas = 
    }
}