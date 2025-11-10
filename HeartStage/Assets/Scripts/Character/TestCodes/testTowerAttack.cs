using System.Collections.Generic;
using UnityEngine;

public class testTowerAttack : MonoBehaviour
{
    private CharacterData data;
    //private CharacterBuffController buffController;

    private float attackTimer = 0f;

    private List<GameObject> enemys = new List<GameObject>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.Monster))
        {
            enemys.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(Tag.Monster))
        {
            enemys.Remove(collision.gameObject);
        }
    }

    private void Start()
    {
        var csvData = DataTableManager.CharacterTable.Get(11);
        data = ResourceManager.Instance.Get<CharacterData>(csvData.data_AssetName);
        data.UpdateData(csvData);
        var bulletGo = ResourceManager.Instance.Get<GameObject>(data.bullet_PrefabName);
        PoolManager.Instance.CreatePool(data.ID.ToString(), bulletGo);

        //buffController = GetComponent<CharacterBuffController>();
        //buffController.SetData(data);
    }

    private void Update()
    {
        attackTimer += Time.deltaTime;
        //if (enemys.Count > 0 && attackTimer >= data.atk_interval)
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
        GameObject missile = PoolManager.Instance.Get(data.ID.ToString());
        var dir = (targetPos - transform.position).normalized;
        missile.GetComponent<testMissile>().SetMissile(data.ID.ToString(), transform.position, data.bullet_speed, dir, data.atk_dmg);
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