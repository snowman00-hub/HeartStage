using System.Collections.Generic;

public static class DragSourceRegistry
{
    private static readonly Dictionary<CharacterData, DragMe> map = new();

    public static void Register(CharacterData cd, DragMe src)
    {
        if (cd == null || src == null) return;
        map[cd] = src;
    }

    public static void Unregister(CharacterData cd, DragMe src)
    {
        if (cd == null || src == null) return;
        if (map.TryGetValue(cd, out var cur) && cur == src)
            map.Remove(cd);
    }

    public static DragMe GetSource(CharacterData cd)
    {
        if (cd != null && map.TryGetValue(cd, out var src)) return src;
        return null;
    }
}
