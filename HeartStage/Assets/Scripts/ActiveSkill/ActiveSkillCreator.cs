using UnityEngine;

public class ActiveSkillCreator : MonoBehaviour
{
    public static ActiveSkillCreator Instance;

    public GameObject sonicAttackPrefab;

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

    public void CreateSonicAttack(GameObject caster, ActiveSkillData data)
    {

    }
}