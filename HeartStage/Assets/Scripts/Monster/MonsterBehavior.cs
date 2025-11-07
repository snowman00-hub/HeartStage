using UnityEngine;
using UnityEngine.InputSystem;
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

    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            OnDamage(10);
            //Debug.Log($"데미지를 입었습니다!. \n현재 남은 HP : {monsterDataController.hp}");
        }
    }
}
