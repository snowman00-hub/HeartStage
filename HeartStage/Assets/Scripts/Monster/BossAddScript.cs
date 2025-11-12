using UnityEngine;

public class BossAddScript : MonoBehaviour
{
    private void Start()
    {
        ScriptAttacher.AttachById(this.gameObject, 9991);
    }
}
