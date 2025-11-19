using UnityEngine;
using UnityEngine.EventSystems;

public class SynergyDetailCloseArea : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private SynergyDetailPanel detailPanel;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (detailPanel != null)
            detailPanel.Hide();
    }
}