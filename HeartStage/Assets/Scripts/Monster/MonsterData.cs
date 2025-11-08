using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Scriptable Objects/MonsterData")]
public class MonsterData : ScriptableObject
{
    //public int id;
    public int hp = 100;
    public int att = 5;
    public float moveSpeed = 1f;
}