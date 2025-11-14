public enum StatType
{
    Attack,
    MoveSpeed,
    Defense,
    // ...
}

public interface IStatMulSource
{
    bool TryGetMul(StatType stat, out float mul);
}