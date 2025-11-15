using UnityEngine;

public class BlindSkill : MonoBehaviour, ISkillBehavior
{
    // 실명 스킬
    public void Execute()
    {
        Debug.Log("BlindSkill()");
    }
} 