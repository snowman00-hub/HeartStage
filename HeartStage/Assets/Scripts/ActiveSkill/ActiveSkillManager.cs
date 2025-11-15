using System.Collections.Generic;
using UnityEngine;

public class ActiveSkillManager : MonoBehaviour
{
    public static ActiveSkillManager Instance;

    private Dictionary<int, SkillData> skillDB = new Dictionary<int, SkillData>();
    private Dictionary<GameObject, Dictionary<int, ISkillBehavior>> skillBehaviors = new Dictionary<GameObject, Dictionary<int, ISkillBehavior>>();
    private List<ActiveSkillTimer> activeTimers = new List<ActiveSkillTimer>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // 스킬테이블 데이터 -> SO에 덮어쓰기
    private void Start()
    {
        skillDB = DataTableManager.SkillTable.GetAll();
        foreach(var skill in skillDB)
        {
            skill.Value.UpdateData(DataTableManager.SkillTable.Get(skill.Value.skill_id));
        }
    }

    // 등록된 스킬들 쿨타임 자동으로 관리
    private void Update()
    {
        foreach (var timer in activeTimers)
        {
            timer.UpdateTimer(Time.deltaTime);

            if (timer.IsReady())
            {
                UseSkill(timer);
                timer.Reset();
            }
        }
    }

    // 스킬 사용하기
    private void UseSkill(ActiveSkillTimer timer)
    {
        var data = timer.SkillData;
        var caster = timer.Caster;

        if (skillBehaviors.TryGetValue(caster, out var skillDict) &&
            skillDict.TryGetValue(data.skill_id, out var behavior))
        {
            behavior.Execute();
        }
        else
        {

        }
    }

    // 사용할 스킬등록하기
    public void RegisterSkill(GameObject caster, int skillId)
    {
        if (skillDB.TryGetValue(skillId, out var data))
        {
            activeTimers.Add(new ActiveSkillTimer(caster, data));
        }
    }

    // 캐스터 죽으면 스킬해제하기
    public void UnRegisterSkill(GameObject caster, int skillId)
    {
        // 1️⃣ 타이머 해제
        var timer = activeTimers.Find(t => t.Caster == caster && t.SkillData.skill_id == skillId);
        if (timer != null)
        {
            activeTimers.Remove(timer);
        }

        // 2️⃣ Behavior 해제
        if (skillBehaviors.TryGetValue(caster, out var skillDict))
        {
            if (skillDict.Remove(skillId))
            {

            }

            // 3️⃣ 해당 캐릭터의 스킬이 더 이상 없으면 전체 제거
            if (skillDict.Count == 0)
            {
                skillBehaviors.Remove(caster);
            }
        }
    }

    // 실제 스킬 스크립트 등록
    public void RegisterSkillBehavior(GameObject caster, int skillId, ISkillBehavior behavior)
    {
        if (!skillBehaviors.TryGetValue(caster, out var skillDict))
        {
            skillDict = new Dictionary<int, ISkillBehavior>();
            skillBehaviors.Add(caster, skillDict);
        }

        if (!skillDict.ContainsKey(skillId))
        {
            skillDict.Add(skillId, behavior);
        }
    }
}

// 타이머 클래스
public class ActiveSkillTimer
{
    public SkillData SkillData { get; private set; }
    public GameObject Caster { get; private set; }

    private float currentTime;

    public ActiveSkillTimer(GameObject caster, SkillData data)
    {
        Caster = caster;
        SkillData = data;
        currentTime = data.skill_cool;
    }

    public void UpdateTimer(float deltaTime)
    {
        if (currentTime > 0)
            currentTime -= deltaTime;
    }

    public bool IsReady()
    {
        return currentTime <= 0f;
    }

    public void Reset()
    {
        currentTime = SkillData.skill_cool;
    }
}