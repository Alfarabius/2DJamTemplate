using ToolTip;
using UnityEngine;
using Random = UnityEngine.Random;

public class Item : MonoBehaviour
{
    [SerializeField] private ItemType type;
    [SerializeField] private string id = "item";
    [SerializeField] private string description = "This is an item description.";
    
    [SerializeField] private int basePrice = 50;

    [SerializeField] private int _marketPrice;
    [SerializeField] private int _sellerPrice;
    [SerializeField] private int _playerEstimate;

    [SerializeField] [Range(0f, 0.5f)] private float appraisalBonus = 0.0f;
    [SerializeField] [Range(0f, 0.5f)] private float bargainingBonus = 0.0f;

    private bool _isYourProperty = false;
    
    public int SellerPrice => _sellerPrice;
    public int MarketPrice => _marketPrice;
    
    public float AppraisalBonus => appraisalBonus;
    public float BargainingBonus => bargainingBonus;
    
    private AudioSource _audioSource;
    
    public bool IsYourProperty => _isYourProperty;
    
    public event System.Action<Item> OnItemReleased;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void SetProperty(bool status)
    {
        _isYourProperty = status;
    }

    public void MakeDiscount()
    {
        _sellerPrice = (int)(_sellerPrice * 0.75f);
    }
    
    public void SetPrices()
    {
        _marketPrice = Mathf.RoundToInt(basePrice * GameManager.Instance.GetDemandCoefficient(type));
        
        float skill = GameManager.Instance.PlayerAppraisalSkill;
        float error = 0.9f * (1 - skill);
        float randomError = Random.Range(-error, error);
        float estimate = _marketPrice * skill * (1 + randomError);
        _playerEstimate = Mathf.Max(1, Mathf.RoundToInt(estimate));
        
        _sellerPrice = Mathf.RoundToInt(_marketPrice * Random.Range(0.7f, 1.3f));
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
        
        var toolTipString = $"<color=#e9bfe3>{id}</color> \n{description} \n<color=#ffa2d4>Seller price: {_sellerPrice}</color> \n<color=#7987ce>Estimated item price: {_playerEstimate}</color>";

        if (_isYourProperty)
            toolTipString = $"<color=#e9bfe3>{id}</color> \n{description} \n<color=#8ad29d>Market price: {_marketPrice}</color> \nThis is yours property.";

        string GetTooltipTextFunc() => toolTipString;
        TooltipScreenSpaceUI.ShowTooltip_Static((System.Func<string>)GetTooltipTextFunc);
    }

    private void OnMouseExit()
    {
        GameManager.Instance.SetCursorHovered(false);
        TooltipScreenSpaceUI.HideTooltip_Static();
    }

    private void OnMouseUp()
    {
        OnItemReleased?.Invoke(this);
    }

    private void OnDestroy()
    {
        TooltipScreenSpaceUI.HideTooltip_Static();
    }
}

public enum ItemType
{
    Common,
    Cyber,
    Fable
}
