using ToolTip;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private string id = "item";
    [SerializeField] private string description = "This is an item description.";
    
    private void OnMouseOver()
    {
        System.Func<string> getTooltipTextFunc = () => $"{id} \n{description}";
        TooltipScreenSpaceUI.ShowTooltip_Static(getTooltipTextFunc);
    }

    private void OnMouseExit()
    {
        TooltipScreenSpaceUI.HideTooltip_Static();
    }
}
