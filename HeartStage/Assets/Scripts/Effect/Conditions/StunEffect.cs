using System.Collections.Generic;
using UnityEngine;

public class StunEffect : EffectBase
{
    private static readonly HashSet<GameObject> Stunned = new();

    protected override void OnApply() 
    {
        Stunned.Add(gameObject);
    }
    protected override void OnRemove() 
    { 
        Stunned.Remove(gameObject);
    }

    public static bool IsStunned(GameObject target) => Stunned.Contains(target);
}

// 사용 예시:
//if (StunEffect.IsStunned(ownerGameObject)) return; // 행동 불가