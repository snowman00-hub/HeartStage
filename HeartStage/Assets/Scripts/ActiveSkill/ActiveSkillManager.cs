using System.Collections.Generic;
using UnityEngine;

public class ActiveSkillManager : MonoBehaviour
{
    public static ActiveSkillManager Instance;

    public GameObject caster;

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
    }

    private void Update()
    {
        foreach (var timer in activeTimers)
        {
            timer.UpdateTimer(Time.deltaTime);

            if (timer.IsReady())
            {
                UseSkill(timer.SkillData);
                timer.Reset();
            }
        }
    }

    private void UseSkill(ActiveSkillData data)
    {
        if (skillBehaviors.TryGetValue(data.use_type, out var behavior))
        {
            behavior.Execute(caster, data);
        }
        else
        {
            Debug.LogWarning($"스킬 사용 실패: {data.skill_name}");
        }
    }

    public void RegisterSkill(int skillId)
    {
        if (skillDB.TryGetValue(skillId, out var data))
        {
            activeTimers.Add(new ActiveSkillTimer(data));
            Debug.Log($"Skill {data.skill_name} registered.");
        }
    }

    public void UnregisterSkill(int skillId)
    {
        var timer = activeTimers.Find(t => t.SkillData.skill_id == skillId);
        if (timer != null)
        {
            activeTimers.Remove(timer);
            Debug.Log($"Skill {timer.SkillData.skill_name} unregistered.");
        }
    }
}

public class ActiveSkillTimer
{
    public ActiveSkillData SkillData { get; private set; }
    private float currentTime;

    public ActiveSkillTimer(ActiveSkillData data)
    {
        SkillData = data;
        currentTime = data.skill_cool; // 시작 시 쿨타임 설정
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