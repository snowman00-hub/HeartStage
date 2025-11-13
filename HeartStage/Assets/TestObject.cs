using UnityEngine;

public class TestObject : MonoBehaviour, IDamageable
{
    public int maxHp = 100;
    public int hp;
    private void OnEnable()
    {
        hp = maxHp;
    }
    public void Die()
    {        
        gameObject.SetActive(false);
        Debug.Log($"{gameObject.name}이 죽었습니다.");
    }
    public void OnDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            Die();
        }
       // Debug.Log($"{gameObject.name}이 {damage}의 피해를 입었습니다. 남은 체력: {hp}");
    }    
}
