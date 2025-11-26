using Cysharp.Threading.Tasks;
using UnityEngine;

public class JsonTest : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Save");
            SaveLoadManager.SaveToServer().Forget();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Load");
            SaveLoadManager.Load();
        }
    }
}