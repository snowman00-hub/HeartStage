using UnityEngine;
using UnityEditor;

public class NaNChecker : EditorWindow
{
    [MenuItem("Tools/Check for NaN Transforms")]
    static void CheckForNaN()
    {
        foreach (var t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            Vector3 pos = t.position;
            if (float.IsNaN(pos.x) || float.IsNaN(pos.y) || float.IsNaN(pos.z) ||
                float.IsInfinity(pos.x) || float.IsInfinity(pos.y) || float.IsInfinity(pos.z))
            {
                Debug.LogWarning($"{t.name} has invalid position: {pos}", t.gameObject);
            }
        }
    }
}