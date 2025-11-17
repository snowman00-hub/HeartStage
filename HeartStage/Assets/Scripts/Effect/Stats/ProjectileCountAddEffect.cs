using UnityEngine;

public class ProjectileCountAddEffect : EffectBase, IStatAddSource
{
    private const int EffectId = 3016; // 투사체 증가

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RegisterSelf()
    {
        EffectRegistry.Register(
            EffectId,
            (target, value, duration, tick) =>
                Add<ProjectileCountAddEffect>(target, duration, value, tick)
        );
    }

    protected override void OnApply()
    {
    }

    protected override void OnRemove()
    {
    }

    public bool TryGetAdd(StatType stat, out float add)
    {
        if (stat == StatType.ProjectileCount)
        {
            // mag = 1 → +1발, mag = 2 → +2발
            add = magnitude;
            return true;
        }

        add = 0f;
        return false;
    }
}
