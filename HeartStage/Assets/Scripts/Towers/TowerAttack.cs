using System.Collections.Generic;
using UnityEngine;

public class TowerAttack : MonoBehaviour
{
    public GameObject missilePrefab;

    public float missileSpeed = 5f;
    public float attackInterval = 2f;
    public float damage = 10f;
    private float attackTimer = 0f;

    private List<GameObject> enemys = new List<GameObject>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            enemys.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            enemys.Remove(collision.gameObject);
        }
    }

    private void Update()
    {
        attackTimer += Time.deltaTime;
        if (enemys.Count > 0 && attackTimer >= attackInterval)
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
        GameObject missile = Instantiate(missilePrefab, transform.position, Quaternion.identity);
        var dir = (targetPos - missile.transform.position).normalized;
        missile.GetComponent<testMissile>().SetMissile(missileSpeed, dir, damage);
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