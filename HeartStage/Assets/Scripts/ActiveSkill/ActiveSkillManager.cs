using System.Collections.Generic;
using UnityEngine;

public class ActiveSkillManager : MonoBehaviour
{
    public static ActiveSkillManager Instance;

    private Dictionary<int, ActiveSkillData> skillDB = new Dictionary<int, ActiveSkillData>();
    private Dictionary<int, ISkillBehavior> skillBehaviors = new Dictionary<int, ISkillBehavior>();
    private List<ActiveSkillTimer> activeTimers = new List<ActiveSkillTimer>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    // 로비로 복귀 시 파괴하기
    // Destroy(ActiveSkillManager.Instance.gameObject);

    private void Start()
    {
        skillDB = DataTableManager.ActiveSkillTable.GetAll();

        RegisterSkillBehavior(1242, new BlindSkill());
    }

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

    private void UseSkill(ActiveSkillTimer timer)
    {
        var data = timer.SkillData;
        var caster = timer.Caster;

        if (skillBehaviors.TryGetValue(data.skill_id, out var behavior))
        {
            behavior.Execute(caster, data);
        }
        else
        {
            Debug.LogWarning($"스킬 사용 실패: {data.skill_name}");
        }
    }
    // 사용할 스킬등록하기
    public void RegisterSkill(GameObject caster, int skillId)
    {
        if (skillDB.TryGetValue(skillId, out var data))
        {
            activeTimers.Add(new ActiveSkillTimer(caster, data));
            Debug.Log($"Skill {data.skill_name} registered for {caster.name}.");
        }
    }
    // 캐스터 죽으면 스킬해제하기
    public void UnRegisterSkill(int skillId)
    {
        var timer = activeTimers.Find(t => t.SkillData.skill_id == skillId);
        if (timer != null)
        {
            activeTimers.Remove(timer);
            Debug.Log($"Skill {timer.SkillData.skill_name} unregistered.");
        }
    }
    // 실제 스킬 스크립트 등록
    public void RegisterSkillBehavior(int skillId, ISkillBehavior behavior)
    {
        if (!skillBehaviors.ContainsKey(skillId))
        {
            skillBehaviors.Add(skillId, behavior);
            Debug.Log($"SkillBehavior 등록 완료: {skillId}");
        }
    }
}

public class ActiveSkillTimer
{
    public ActiveSkillData SkillData { get; private set; }
    public GameObject Caster { get; private set; }

    private float currentTime;

    public ActiveSkillTimer(GameObject caster, ActiveSkillData data)
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