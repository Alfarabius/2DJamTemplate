using System;
using ToolTip;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    [SerializeField] private string id = "item";
    [SerializeField] private string description = "This is an item description.";
    
    private DragAndDrop _dragAndDrop;

    private void Awake()
    {
        _dragAndDrop = GetComponent<DragAndDrop>();
    }

    private void OnMouseOver()
    {
        if (_dragAndDrop.IsDragging())
        {
            
            return;
        }
        
        System.Func<string> getTooltipTextFunc = () => $"{id} \n{description}";
        TooltipScreenSpaceUI.ShowTooltip_Static(getTooltipTextFunc);
    }

    private void OnMouseExit()
    {
        TooltipScreenSpaceUI.HideTooltip_Static();
    }
}
