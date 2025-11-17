public class ConditionState
{
    /// <summary>총 지속 시간 (디버깅용/참고용)</summary>
    public float TotalDuration { get; private set; }

    /// <summary>남은 지속 시간</summary>
    public float RemainingDuration { get; private set; }

    public ConditionState(float duration)
    {
        TotalDuration = duration;
        RemainingDuration = duration;
    }

    /// <summary>dt만큼 시간 감소, 끝났는지 여부 반환</summary>
    public bool Tick(float deltaTime)
    {
        RemainingDuration -= deltaTime;
        return RemainingDuration <= 0f;
    }

    /// <summary>지속 시간을 갱신(리셋/연장 등)</summary>
    public void Refresh(float duration)
    {
        TotalDuration = duration;
        RemainingDuration = duration;
    }
}
