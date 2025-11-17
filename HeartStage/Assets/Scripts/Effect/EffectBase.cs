using UnityEngine;

public abstract class EffectBase : MonoBehaviour
{
    [HideInInspector] public float duration;     // seconds
    [HideInInspector] public float magnitude;    // e.g., 0.15 = +15%
    [HideInInspector] public float tickInterval; // 0 = no ticks

    float remain, tickAcc;
    bool initialized;

    // Add<T>가 호출한 뒤에 Initialize()로 시작시킨다
    public void Initialize(float dur, float mag, float tick)
    {
        duration = dur;
        magnitude = mag;
        tickInterval = tick;

        remain = duration;
        tickAcc = 0f;
        initialized = true;

        OnApply();
    }
    void Update()
    {
        if (!initialized) return;

        float dt = Time.deltaTime;

        if (tickInterval > 0f)
        {
            tickAcc += dt;
            while (tickAcc >= tickInterval) { OnTick(tickInterval); tickAcc -= tickInterval; }
        }

        remain -= dt;
        if (remain <= 0f)
        {
            // 파괴 전에 OnRemove 보장
            OnRemove();
            Destroy(this);
        }
    }

    void OnDestroy()
    {
        // 이미 Update에서 OnRemove를 호출했을 수 있으므로,
        // initialized 체크로 이중 호출 방지 (필요시 제거해도 됨)
        if (initialized)
        {
            initialized = false;
            // OnRemove();  // 위에서 이미 호출함
        }
    }

    protected abstract void OnApply();
    protected virtual void OnTick(float dt) { }
    protected abstract void OnRemove();

    public static T Add<T>(GameObject target, float duration, float magnitude = 0f, float tickInterval = 0f)
        where T : EffectBase
    {
        var e = target.AddComponent<T>();   // 여기서는 아직 시작 X
        e.Initialize(duration, magnitude, tickInterval); // 여기서 시작
        return e;
    }

    public static bool Has<T>(GameObject go) where T : EffectBase
    => go.GetComponent<T>() != null;

    // 개수
    public static int Count<T>(GameObject go) where T : EffectBase
        => go.GetComponents<T>().Length;

    // 하나 꺼내기(있으면 true)
    public static bool TryGet<T>(GameObject go, out T comp) where T : EffectBase
    {
        comp = go.GetComponent<T>();
        return comp != null;
    }
}
