using System.Buffers;
using UnityEditor.SettingsManagement;
using UnityEngine;

public class MonsterProjectile : MonoBehaviour
{
    private Vector3 direction;
    public float speed;
    public int damage;  

    public void Init(Vector3 direction, float bulletSpeed, int damage)
    {
        speed = bulletSpeed;
        this.direction = direction.normalized;
        this.damage = damage;   
    }

    private void Update()
    {
        transform.position +=  direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Tag.Wall))
        {
            var testObj = other.GetComponent<IDamageable>();
            if (testObj != null)
            {
                testObj.OnDamage(damage);
            }
            gameObject.SetActive(false);            
        }
    }
}
