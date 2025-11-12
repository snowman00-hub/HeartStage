using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class ScriptAttacher
{
    private static readonly Dictionary<string, Type> _cache;
    private static readonly Dictionary<int, string> _idToScript = new()
    {
        { 1242, "SonicAttackSkill" },
        { 9991, "DeceptionBossSkill"},
        { 9992, "SpeedBuffBossSkill"},
    };

    static ScriptAttacher()
    {
        _cache = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(MonoBehaviour).IsAssignableFrom(t))
            .GroupBy(t => t.Name)
            .ToDictionary(g => g.Key, g => g.First());
    }

    public static void AttachById(GameObject obj, int id)
    {
        if (_idToScript.TryGetValue(id, out var scriptName))
        {
            AttachByName(obj, scriptName);
        }
        else
        {
            Debug.Log($"ID {id}에 해당하는 스크립트를 찾을 수 없습니다!");
        }
    }

    public static void AttachByName(GameObject obj, string scriptName)
    {
        if (_cache.TryGetValue(scriptName, out var type))
        {
            obj.AddComponent(type);
        }
        else
        {
            Debug.LogError($"'{scriptName}' 타입을 찾을 수 없습니다!");
        }
    }
}
