using UnityEngine;

public class HeartBombSkill : BaseProjectileSkill
{
    private void Awake()
    {
        prefabName = "HeartBomb";
        poolId = "HeartBomb";
        skillId = 31208;
    }

    protected override void SetupCollider(GameObject clone)
    {
        var col = clone.GetComponent<CircleCollider2D>();
        col.radius = skillData.skill_range;
    }

    protected override Vector3 GetStartPosition() => GetCenterInMonsters();
    protected override Vector3 GetDirection() => Vector3.zero;

    private Vector3 GetCenterInMonsters()
    {
        var objs = GameObject.FindGameObjectsWithTag(Tag.Monster);
        if (objs.Length == 0) return Vector3.zero;

        Vector3 sum = Vector3.zero;
        int count = 0;
        foreach (var obj in objs)
        {
            if (obj.transform.position.y <= 10f)
            {
                sum += obj.transform.position;
                count++;
            }
        }
        return count == 0 ? Vector3.zero : sum / count;
    }
}