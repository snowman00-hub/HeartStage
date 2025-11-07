using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class testTowerAttack : MonoBehaviour
{
    public string missileName = "MissileTest"; // asset 네임과 동일해야 함.
    public AssetReferenceGameObject missilePrefabRef;

    public testTowerData data;
    private float attackTimer = 0f;

    private List<GameObject> enemys = new List<GameObject>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.Enemy))
        {
            enemys.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.Enemy))
        {
            enemys.Remove(collision.gameObject);
        }
    }

    private void Start()
    {
        PoolManager.Instance.CreatePool(missileName);
    }

    private void Update()
    {
        attackTimer += Time.deltaTime;
        if (enemys.Count > 0 && attackTimer >= data.attackInterval)
        {
            GameObject target = GetClosestEnemy();
            if (target != null)
            {
                Fire(target.transform.position);
                attackTimer = 0f;
            }
        }
    }

    private void Fire(Vector3 targetPos)
    {
        GameObject missile = PoolManager.Instance.Get(missileName);
        var dir = (targetPos - transform.position).normalized;
        missile.GetComponent<testMissile>().SetMissile(missileName, transform.position, data.projectileSpeed, dir, data.damage);
    }

    private GameObject GetClosestEnemy()
    {
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (var enemy in enemys)
        {
            if (enemy == null)
                continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }

        return closest;
    }
}