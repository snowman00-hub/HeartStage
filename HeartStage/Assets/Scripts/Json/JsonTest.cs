using UnityEngine;

public class JsonTest : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Save");
            SaveLoadManager.Save();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Load");
            SaveLoadManager.Load();
        }
    }
}