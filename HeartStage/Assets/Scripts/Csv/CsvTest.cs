using UnityEngine;

public class csvTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log(DataTableManger.StringTable.Get(StringIds.Test));
    }
}