using System;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    // --- StatType 인덱싱 매핑(중요) ---
    private Dictionary<StatType, int> indexOf;
    private StatType[] statList;

    // 곱형 버프 id 발급기
    private int _nextBuffId = 100000;

    // StatType enum은 기존 것을 그대로 사용한다고 가정
    // ex) Attack, Defense, MaxHP, MoveSpeed ...

    // --- 기본값 & 최종값 캐시 ---
    private float[] baseVals;
    private float[] finalVals;

    // --- 누적 버킷(재계산용 임시) ---
    private float[] addFlat;         // Σ Addfloat
    private float[] addPercent;      // Σ AddPercent
    private float[] mulPercentProd;  // Π (1 + MulPercent)

    // 활성화된 모든 수정자(버프/장비/오라)
    private readonly List<StatModifier> activeMods = new();

    private int statCount;

    void Awake()
    {
        statList = (StatType[])Enum.GetValues(typeof(StatType));
        indexOf = new Dictionary<StatType, int>(statList.Length);
        for (int i = 0; i < statList.Length; i++) indexOf[statList[i]] = i;
        statCount = statList.Length;
        baseVals = new float[statCount];
        finalVals = new float[statCount];

        addFlat = new float[statCount];
        addPercent = new float[statCount];
        mulPercentProd = new float[statCount];

        // mulPercentProd는 곱셈이므로 초기값 1
        for (int i = 0; i < statCount; i++)
            mulPercentProd[i] = 1f;

        // 필요하면 여기서 기본값 세팅 or 외부에서 SetBase로 세팅
        Recalculate(); // 초기 계산
    }

    // --- 기본값 세팅 ---
    public void SetBase(StatType stat, float value)
    {
        baseVals[indexOf[stat]] = value;
    }

    public float GetBase(StatType stat) => baseVals[indexOf[stat]];
    public float GetFinal(StatType stat) => finalVals[indexOf[stat]];

    // --- Modifier 등록/해제 ---
    public void ApplyModifier(StatModifier m)
    {
        activeMods.Add(m);
        // 필요 시 바로 반영하고 싶으면 Recalculate() 호출
        // 성능을 위해 한 프레임에 여러 개 붙는다면 마지막에 한 번만 Recalculate()를 부르도록 외부에서 조절해도 됨
    }

    public void RemoveModifier(StatModifier m)
    {
        // 같은 struct를 remove할 수도 있고, id/source로 찾아서 제거할 수도 있음
        // 아래는 id 매칭 우선, 실패 시 동일한 모든 필드 매칭 시도
        int idx = activeMods.FindIndex(x => x.id == m.id && m.id != 0);
        if (idx < 0) idx = activeMods.FindIndex(x => x.statType == m.statType && x.modOp == m.modOp && Mathf.Approximately(x.value, m.value) && x.source == m.source);
        if (idx >= 0) activeMods.RemoveAt(idx);
        // 역시 Recalculate는 외부 타이밍에 맞춰 호출
    }

    // id로 통째로 지우고 싶을 때(버프 만료 등)
    public void RemoveModifiersById(int id)
    {
        if (id == 0) return;
        activeMods.RemoveAll(x => x.id == id);
    }

    // source 태그로 지우고 싶을 때
    public void RemoveModifiersBySource(string source)
    {
        if (string.IsNullOrEmpty(source)) return;
        activeMods.RemoveAll(x => x.source == source);
    }

    
    // --- 곱형 편의 API: factor는 1.15f(=+15%), 0.8f(=-20%) 같은 계수 ---
    public int AddMul(StatType stat, float factor, string source = null)
    {
        if (factor <= 0f) factor = 0.0001f;
        var mod = new StatModifier
        {
            statType = stat,
            modOp = ModOp.MulPercent,
            value = factor - 1f, // 내부는 0.15 처럼 퍼센트로 누적
            source = source,
            id = _nextBuffId++,
        };
        ApplyModifier(mod);
        Recalculate();
        return mod.id;
    }

    public bool RemoveMul(int id)
    {
        if (id == 0) return false;
        RemoveModifiersById(id);
        Recalculate();
        return true;
    }
// --- 최종값 재계산(핵심) ---
    public void Recalculate()
    {
        // 누적 버킷 초기화
        Array.Clear(addFlat, 0, statCount);
        Array.Clear(addPercent, 0, statCount);
        for (int i = 0; i < statCount; i++)
            mulPercentProd[i] = 1f;

        // 활성 모디파이어를 스탯별로 누적
        foreach (var m in activeMods)
        {
            int i = indexOf[m.statType];
            switch (m.modOp)
            {
                case ModOp.Addfloat:     // 주의: 네 enum 철자 그대로 씀
                    addFlat[i] += m.value;
                    break;
                case ModOp.AddPercent:
                    addPercent[i] += m.value; // 0.10 == +10%
                    break;
                case ModOp.MulPercent:
                    mulPercentProd[i] *= (1f + m.value); // 누적 곱
                    break;
            }
        }

        // 공식: (base + ΣAddfloat) * (1 + ΣAddPercent) * Π(1 + MulPercent)
        for (int i = 0; i < statCount; i++)
        {
            float x = (baseVals[i] + addFlat[i]) * (1f + addPercent[i]) * mulPercentProd[i];
            finalVals[i] = x;
        }
    }
}