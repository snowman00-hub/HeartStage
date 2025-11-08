using System.Text;
using UnityEngine;
public class MonsterBehavior : MonoBehaviour, IAttack, IDamageable
{
    [Header("Field")]
    private MonsterData monsterData;
    //private bool isAlive = true;

    public void Init(MonsterData data)
    {
        monsterData = data;
        Debug.Log($"몬스터 초기화 HP: {monsterData.hp}, ATT: {monsterData.att}");
    }

    public void Attack()
    {
        // 애니메이션 처리        
    }

    public void OnDamage(int damage)
    {
        if(monsterData != null)
        {
            monsterData.hp -= damage;
        }

        if(monsterData.hp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        // 몬스터 Die 애니메이션 처리
        //isAlive = false;
        gameObject.SetActive(false);
        Debug.Log("몬스터가 사망했습니다.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Tag.Wall))
        {
            var target = other.GetComponent<IDamageable>();
            if (target != null)
            {
                target.OnDamage(monsterData.att);
                Debug.Log($"몬스터가 {other.name}을 공격했습니다! 데미지: {monsterData.att}");
            }
        }
    }

    private void Update()
    {

    }
}