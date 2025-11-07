using UnityEngine;

public class TestObject : MonoBehaviour, IDamageable
{
    public int maxHp = 100;
    public int hp;
    private void OnEnable()
    {
        hp = 100;
    }
    public void Die()
    {        
        Destroy(gameObject);
    }

    public void OnDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            Die();
        }
    }
}
