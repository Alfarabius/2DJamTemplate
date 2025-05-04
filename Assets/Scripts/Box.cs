using ToolTip;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Box : MonoBehaviour
{
    private Item _currentItem;
    
    [SerializeField] private string id = "box";
    [SerializeField] private string description = "This is an box description.";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Item")) return;
        _currentItem = other.GetComponent<Item>();
        if (_currentItem != null) _currentItem.OnItemReleased += HandleItemReleased;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Item") && _currentItem != null)
        {
            _currentItem.OnItemReleased -= HandleItemReleased;
            _currentItem = null;
        }
    }

    private void HandleItemReleased(Item item)
    {
        GameManager.Instance.HandleBoxCollision(transform.gameObject.tag, item);
    }
    
    private void OnMouseEnter()
    {
        if (GameManager.Instance.IsSomethingDragging())
            return;
        
        GameManager.Instance.SetCursorHovered(true);
    }

    private void OnMouseOver()
    {
        if (GameManager.Instance.IsSomethingDragging())
            return;
        
        var toolTipString = $"<color=#e9bfe3>{id}</color>\n{description} ";

        string GetTooltipTextFunc() => toolTipString;
        TooltipScreenSpaceUI.ShowTooltip_Static((System.Func<string>)GetTooltipTextFunc);
    }

    private void OnMouseExit()
    {
        GameManager.Instance.SetCursorHovered(false);
        TooltipScreenSpaceUI.HideTooltip_Static();
    }
}
