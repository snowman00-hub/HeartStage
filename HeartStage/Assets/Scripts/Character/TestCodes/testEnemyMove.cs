using UnityEngine;

public class testEnemyMove : MonoBehaviour
{
    public float moveSpeed = 0.5f;

    private void Update()
    {
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;
    }
}