using Unity.Properties;
using UnityEngine;
using System.Collections.Generic;

public class FanManager : MonoBehaviour
{
    [SerializeField] private int fansPerWave = 4;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float fanSpacing = 1.5f;

    private readonly string[] fanPrefabName =
    {
        "Fan_01", "Fan_02", "Fan_03", "Fan_04", "Fan_05", "Fan_06",
        "Fan_07", "Fan_08", "Fan_09", "Fan_10", "Fan_11", "Fan_12"
    };

    private List<GameObject> activeFans = new List<GameObject>();

    private void OnEnable()
    {
        MonsterSpawner.OnWaveCleared += SpawanFansWaveClear;
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        MonsterSpawner.OnWaveCleared -= SpawanFansWaveClear;
    }

    private void SpawanFansWaveClear()
    {
        for (int i = 0; i < fansPerWave; i++)
        {
            SpawnFans(i);
        }
    }

    private void SpawnFans(int fanIndex)
    {
        int randomFanIndex = Random.Range(0, fanPrefabName.Length);
        string selectedFanName = fanPrefabName[randomFanIndex];

        var fanPrefab = ResourceManager.Instance.Get<GameObject>(selectedFanName);

        if(fanPrefab == null)
        {
            return;
        }

        GameObject fan = Instantiate(fanPrefab, transform);

        FanBehavior fanBehavior = fan.GetComponent<FanBehavior>();
        if (fanBehavior == null)
        {
            fanBehavior = fan.AddComponent<FanBehavior>();
        }

        // 팬 설정
        fanBehavior.SetupFan(fanIndex, moveSpeed, fanSpacing);

        activeFans.Add(fan);
    }

    public void ClearAllFans()
    {
        foreach (var fan in activeFans)
        {
            if (fan != null)
            {
                Destroy(fan);
            }
        }
        activeFans.Clear();
    }
}
