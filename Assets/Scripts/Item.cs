using System;
using ToolTip;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    [SerializeField] private string id = "item";
    [SerializeField] private string description = "This is an item description.";
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnMouseEnter()
    {
        if (GameManager.Instance.IsSomethingDragging())
            return;
        
        GameManager.Instance.SetCursorHovered(true);
        GameManager.Instance.PlayNextHoverSound(_audioSource);
    }

    private void OnMouseOver()
    {
        if (GameManager.Instance.IsSomethingDragging())
            return;
        
        System.Func<string> getTooltipTextFunc = () => $"{id} \n{description}";
        TooltipScreenSpaceUI.ShowTooltip_Static(getTooltipTextFunc);
    }

    private void OnMouseExit()
    {
        GameManager.Instance.SetCursorHovered(false);
        TooltipScreenSpaceUI.HideTooltip_Static();
    }
}
