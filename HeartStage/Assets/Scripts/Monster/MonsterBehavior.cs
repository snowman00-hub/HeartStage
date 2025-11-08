using System.Text;
using UnityEngine;
public class MonsterBehavior : MonoBehaviour, IAttack, IDamageable
{
    [Header("Field")]
    private MonsterDataController monsterDataController;
    //private bool isAlive = true;

    private void Awake()
    {
        monsterDataController = GetComponent<MonsterDataController>();
    }

    public void Attack()
    {
        // 애니메이션 처리        
    }

    public void OnDamage(int damage)
    {
        if(monsterDataController != null)
        {
            monsterDataController.hp -= damage;
        }

        if(monsterDataController.hp <= 0)
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
        string tagName = string.Empty;

        var sb = new StringBuilder();
        sb.Clear();
        sb.Append("Enemy");

        tagName = sb.ToString();

        if (!other.CompareTag(tagName))
        {
            var target = other.GetComponent<IDamageable>();
            if (target != null)
            {
                target.OnDamage(monsterDataController.att);
                Debug.Log($"몬스터가 {other.name}을 공격했습니다! 데미지: {monsterDataController.att}");
            }
        }
    }

    private void Update()
    {

    }
}