using UnityEngine;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;

public class CharacterInfoTab : MonoBehaviour, IPointerClickHandler
{
    private DragMe dragMe;

    private void Start()
    {
        dragMe = GetComponent<DragMe>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OpenInfoNextFrame().Forget();
    }

    private async UniTaskVoid OpenInfoNextFrame()
    {
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

        // 드래그 직후면 탭 금지
        if (dragMe != null && dragMe.DragJustEnded)
            return;

        // 세로 드래그였으면 탭 금지
        if (dragMe != null && dragMe.IsVerticalDrag)
            return;

        WindowManager.Instance.OpenOverlay(WindowType.CharacterInfo);
        CharacterInfoWindow.Instance.Init(dragMe.characterData);
    }
}
