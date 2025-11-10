using System.Collections.Generic;
using UnityEngine;

public class CharacterAttack : MonoBehaviour
{
    private CharacterData data;
    private List<GameObject> monsters = new List<GameObject>();
    private float nextAttackTime;
    private float cleanupTimer = 0f;

    private void Start()
    {
        var csvData = DataTableManager.CharacterTable.Get(11010101);
        data = ResourceManager.Instance.Get<CharacterData>(csvData.data_AssetName);
        data.UpdateData(csvData);

        var bulletGo = ResourceManager.Instance.Get<GameObject>(data.bullet_PrefabName);
        PoolManager.Instance.CreatePool(data.ID.ToString(), bulletGo);
    }

    private void Update()
    {
        cleanupTimer += Time.deltaTime;
        if (cleanupTimer >= 1f)
        {
            cleanupTimer = 0f;
            monsters.RemoveAll(m => m == null); // 죽은 몬스터 정리
        }

        if (monsters.Count == 0) 
            return;

        if (Time.time < nextAttackTime)
            return;

        GameObject target = GetClosestEnemy();
        if (target != null)
        {
            Fire(target.transform.position);
            nextAttackTime = Time.time + data.atk_speed;
        }
    }

    private void Fire(Vector3 targetPos)
    {
        GameObject projectile = PoolManager.Instance.Get(data.ID.ToString());
        if (projectile == null)
            return;

        var dir = (targetPos - transform.position).normalized;
        projectile.GetComponent<CharacterProjectile>()
            .SetMissile(data.ID.ToString(), transform.position, dir, data.bullet_speed, data.atk_dmg);
    }

    private GameObject GetClosestEnemy()
    {
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (var monster in monsters)
        {
            float dist = Vector3.Distance(transform.position, monster.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = monster;
            }
        }

        return closest;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.Monster))
        {
            if (!monsters.Contains(collision.gameObject))
                monsters.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.Monster))
        {
            monsters.Remove(collision.gameObject);
        }
    }
}