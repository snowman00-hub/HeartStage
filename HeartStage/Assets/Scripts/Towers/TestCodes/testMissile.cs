using UnityEngine;

public class testMissile : MonoBehaviour
{
	private float moveSpeed;
	private Vector3 dir;
	private float damage;

    private void OnTriggerEnter2D(Collider2D collision)
    {
		if (collision.CompareTag(Tag.Enemy))
		{
			Destroy(collision.gameObject);
			Destroy(gameObject);
		}
    }

    private void Update()
    {
		transform.position += dir * moveSpeed * Time.deltaTime;
    }

    public void SetMissile(float speed, Vector3 dir, float damage)
	{
		moveSpeed = speed;
		this.dir = dir;
		this.damage = damage;
	}
}
