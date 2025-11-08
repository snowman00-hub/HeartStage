using UnityEngine;

[CreateAssetMenu(fileName = "testTowerData", menuName = "Scriptable Objects/testTowerData")]
public class testTowerData : ScriptableObject
{
    public string assetName;
    public float hp;
    public float projectileSpeed;
    public float damage;
    public float attackInterval;
}
