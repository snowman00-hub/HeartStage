using UnityEngine;
using NavMeshPlus.Components;
using Cysharp.Threading.Tasks;

public class NavMeshRebaker : MonoBehaviour
{
    [SerializeField] private NavMeshSurface surface2d;

    private async UniTask Start()
    {
        await UniTask.NextFrame();
        surface2d.BuildNavMesh();
    }
}