using System.Buffers;
using UnityEditor.SettingsManagement;
using UnityEngine;

public class MonsterProjectile : MonoBehaviour
{
    private Vector3 direction;
    public float speed;

    public void Init(Vector3 direction, float bulletSpeed)
    {
        speed = bulletSpeed;
        this.direction = direction.normalized;
    }

    private void Update()
    {
        transform.position +=  direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tag.Wall))
        {
            gameObject.SetActive(false);
        }
    }
}
