using UnityEngine;

public class testMissile : MonoBehaviour
{
	private string missileName;
	private float moveSpeed;
	private Vector3 dir;
	private float damage;

    private void OnTriggerEnter2D(Collider2D collision)
    {
		if (collision.CompareTag(Tag.Enemy))
		{
			Destroy(collision.gameObject);
			PoolManager.Instance.Release(missileName, gameObject);
		}
    }

    private void Update()
    {
		transform.position += dir * moveSpeed * Time.deltaTime;
    }

    public void SetMissile(string name,Vector3 startPos, float speed, Vector3 dir, float damage)
	{
		missileName = name;
		transform.position = startPos;
		moveSpeed = speed;
		this.dir = dir;
		this.damage = damage;
	}
}