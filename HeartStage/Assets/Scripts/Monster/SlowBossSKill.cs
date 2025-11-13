using UnityEngine;

public class SlowBossSkill : IBossMonsterSkill
{
    public void useSkill(MonsterBehavior boss)
    {
        // 보스가 8초에 한번씩 아이돌의 공격 속도를 3초간 느리게 하는 야유 공격을 날린다.
    }

    private void SlowEffect(CharacterAttack target, float duration)
    {
       //float originalAttackSpeed = target.data.attackSpeed;
       //target.data.attackSpeed *= 1.5f;         
    }
}
