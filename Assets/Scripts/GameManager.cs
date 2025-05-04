using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using ToolTip;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    
    [SerializeField] private Transform focusPoint;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI skillsText;
    
    [SerializeField] private List<Item> items = new List<Item>();
    private ContainerManager _containerManager;

    public int money = 300;
    [SerializeField] private int maxMoney = 1000;

    public float MoneyNormalized => money / maxMoney;
    
    [SerializeField] private List<AudioClip> hoverSounds;
    [SerializeField] private AudioClip sellSound;
    [SerializeField] private AudioClip buySound;
    [SerializeField] private AudioClip returnSound;
    [SerializeField] private AudioClip okOkSound;
    [SerializeField] private AudioClip noNoSound;
    [SerializeField] private AudioSource globalSoundSource;
    
    private int _hoverSoundIndex = 0;

    private bool _isSomethingDragging;
    private bool _isCursorHovered;
    
    public bool IsSomethingDragging() => _isSomethingDragging;
    public bool IsCursorHovered() => _isCursorHovered;
    
    [System.Serializable]
    public struct DemandCoefficient
    {
        public ItemType type;
        [Range(0.5f, 2f)] public float value;
    }

    [SerializeField] private List<DemandCoefficient> demandCoefficientsList;
    [SerializeField] [Range(0.1f, 1f)] private float playerAppraisalSkill = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float playerBargainingSkill = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float sellerDissatisfaction = 0.1f;
    private float _currentSellerDissatisfaction;
    
    [SerializeField] private float appraisalBonus = 0f;
    [SerializeField] private float bargainingBonus = 0f;

    private Dictionary<ItemType, float> _demandCoefficients;

    public float PlayerAppraisalSkill => playerAppraisalSkill + playerAppraisalSkill * appraisalBonus;
    
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private GameObject gameOverTMP;
    [SerializeField] private GameObject winTMP;
    [SerializeField] private float fadeDuration = 1f;

    public void StartFadeIn() => StartCoroutine(FadeRoutine(1f, 0f));
    public void StartFadeOut() => StartCoroutine(FadeRoutine(0f, 1f));

    private IEnumerator FadeRoutine(float startAlpha, float targetAlpha)
    {
        fadeCanvasGroup.gameObject.SetActive(true);
        
        fadeCanvasGroup.alpha = startAlpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }

    public void SetSomethingDragging(bool state)
    {
        _isSomethingDragging = state;
    }

    public void SetCursorHovered(bool state)
    {
        _isCursorHovered = state;
    }

    public void PlayNextHoverSound(AudioSource audioSource)
    {
        _hoverSoundIndex = Mathf.Clamp(_hoverSoundIndex + 1, 0, hoverSounds.Count - 1);
        audioSource.PlayOneShot(hoverSounds[_hoverSoundIndex ]);
        if (_hoverSoundIndex == hoverSounds.Count - 1)
            _hoverSoundIndex = 0;
    }
    
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("GameManager");
                    _instance = obj.AddComponent<GameManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        StartFadeIn();
        
        items.AddRange(FindObjectsOfType<Item>());
        
        _currentSellerDissatisfaction = sellerDissatisfaction;
        _containerManager = GetComponent<ContainerManager>();
        
        UpdateAllMarket();
        UpdateInfoText();
    }

    private void UpdateInfoText()
    {
        moneyText.text = $"Money:{money}/{maxMoney}";
        skillsText.text = $"Bargaining skill:{bargainingBonus}";
    }
    
    private void InitializeCoefficients()
    {
        _demandCoefficients = new Dictionary<ItemType, float>();
        foreach (var entry in demandCoefficientsList)
        {
            _demandCoefficients[entry.type] = entry.value;
        }
    }

    public float GetDemandCoefficient(ItemType type)
    {
        return _demandCoefficients.GetValueOrDefault(type, 1f);
    }

    private void UpdateAllCoefficients()
    {
        for (int i = 0; i < demandCoefficientsList.Count; i++)
        {
            DemandCoefficient entry = demandCoefficientsList[i];
            entry.value = Random.Range(0.5f, 2f);
            demandCoefficientsList[i] = entry;
        }
        InitializeCoefficients();
    }

    [ContextMenu("Update all market")]
    public void UpdateAllMarket()
    {
        UpdateAllCoefficients();
        foreach (var item in items)
        {
            item.SetPrices();
        }
        RecalculateSkillBonuses();
    }

    public void RecalculateSkillBonuses()
    {
        bargainingBonus = 0f;
        appraisalBonus = 0f;
        
        foreach (var itm in items.Where(itm => itm.IsYourProperty))
        {
            bargainingBonus += itm.BargainingBonus;
            appraisalBonus += itm.AppraisalBonus;
        }
    }

    public void HandleBoxCollision(string boxTag, Item item)
    {
        if (!item) return;
        
        if (boxTag == "ReturnBox" && !item.IsYourProperty)
        { 
            ReturnProcedure(item);
        }
        else if (boxTag == "BuyBox" && !item.IsYourProperty)
        {
            money -= item.SellerPrice;
            globalSoundSource.PlayOneShot(buySound);
            item.SetProperty(true);
            _currentSellerDissatisfaction = sellerDissatisfaction;
            StartCoroutine(MoveItemOnFocusPoint(item));
        }
        else if (boxTag == "SellBox" && item.IsYourProperty)
        {
            money += item.MarketPrice;
            globalSoundSource.PlayOneShot(sellSound);
            items.Remove(item);
            var itm = item.gameObject;
            Destroy(itm.GetComponent<Item>());
            Destroy(itm);
        }
        else
        {
            StartCoroutine(MoveItemOnFocusPoint(item));
        }
        
        RecalculateSkillBonuses();
        UpdateInfoText();
        
        var yourItemsCount = items.Count(itm => itm.IsYourProperty);

        if (yourItemsCount == 0 && money <= 0)
            GameOver(true);
        else if (money >= maxMoney)
            GameOver(false);
    }

    private void ReturnProcedure(Item item)
    {
        var successChance = Mathf.Clamp01(playerBargainingSkill + playerBargainingSkill * bargainingBonus - _currentSellerDissatisfaction);
        
        if (Random.Range(0f, 1f) < successChance)
        {
            globalSoundSource.PlayOneShot(okOkSound);
            StartCoroutine(MoveItemOnFocusPoint(item));
            item.MakeDiscount();
            _currentSellerDissatisfaction *= 2;
        }
        else
        {
            _currentSellerDissatisfaction = sellerDissatisfaction;
            globalSoundSource.PlayOneShot(returnSound);
            items.Remove(item);
            
            var itm = item.gameObject;
            Destroy(itm.GetComponent<Item>());
            Destroy(itm);
        }
    }

    private IEnumerator MoveItemOnFocusPoint(Item item)
    {
        if (!item)
            yield break;
        
        float elapsed = 0f;
        Vector3 newPosition = focusPoint.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));

        while (elapsed < 0.5f)
        {
            if(!item)
                yield break;
            
            item.transform.position = Vector3.Lerp(item.transform.position, newPosition, elapsed / 0.5f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        item.transform.position = newPosition;
    }

    private void GameOver(bool state)
    {
        StartFadeOut();
        gameOverTMP.SetActive(state);
        winTMP.SetActive(!state);
        StartCoroutine(RestartRoutine());
    }
    
    private IEnumerator RestartRoutine()
    {
        yield return new WaitForSeconds(5f);
        Application.Quit();
    }

    public void NextCustomer()
    {
        if (items.Count >= 6)
        {
            globalSoundSource.PlayOneShot(noNoSound);
            return;
        }
        
        var itm = _containerManager.GetItem();
        
        Vector3 newPosition = focusPoint.position + new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), Random.Range(-2f, 2f));
        
        var newItem = Instantiate(itm, newPosition, Quaternion.identity);
        
        items.Add(newItem.gameObject.GetComponent<Item>());
        UpdateAllMarket();
    }
}
