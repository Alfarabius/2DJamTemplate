using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ContainerManager : MonoBehaviour
{
    [Header("Исходные контейнеры")]
    [SerializeField] private List<GameObject> containerLevel1;
    [SerializeField] private List<GameObject> containerLevel2;
    [SerializeField] private List<GameObject> containerLevel3;

    [Header("Настройки распределения")]
    [SerializeField] private int combinedCapacity = 12;
    [SerializeField] private AnimationCurve level2Curve;
    [SerializeField] private AnimationCurve level3Curve;

    private List<GameObject> _combinedContainer = new List<GameObject>();

    private void Awake()
    {
        FillCombinedContainer();
    }

    private void FillCombinedContainer()
    {
        _combinedContainer.Clear();
        AddMandatoryItems();
        FillRemainingSlots();
    }

    private void AddMandatoryItems()
    {
        AddItems(containerLevel1, 1);
        AddItems(containerLevel2, 2);
        AddItems(containerLevel3, 2);
    }

    private void FillRemainingSlots()
    {
        int remainingSlots = combinedCapacity - _combinedContainer.Count;
        if (remainingSlots <= 0) return;

        float totalWeight = CalculateWeights(out float weightLevel2, out float weightLevel3);
        int slotsLevel2 = Mathf.RoundToInt(remainingSlots * weightLevel2 / totalWeight);
        int slotsLevel3 = Mathf.RoundToInt(remainingSlots * weightLevel3 / totalWeight);
        int slotsLevel1 = remainingSlots - slotsLevel2 - slotsLevel3;

        AddItems(containerLevel3, slotsLevel3);
        AddItems(containerLevel2, slotsLevel2);
        AddItems(containerLevel1, slotsLevel1);
    }

    private float CalculateWeights(out float weightLevel2, out float weightLevel3)
    {
        float moneyNormalized = Mathf.Clamp01(GameManager.Instance.MoneyNormalized);
        weightLevel2 = level2Curve.Evaluate(moneyNormalized);
        weightLevel3 = level3Curve.Evaluate(moneyNormalized);
        return weightLevel2 + weightLevel3 + 1f;
    }

    private void AddItems(List<GameObject> source, int count)
    {
        if (source == null || _combinedContainer == null || count <= 0 || source.Count == 0)
            return;

        if (count > source.Count)
        {
            Debug.Log("Count exceeds source size. Adding all available elements.");
            count = source.Count;
        }
        
        source.Reverse();

        List<GameObject> tempList = new List<GameObject>(source);
        tempList = tempList.OrderBy(x => Random.Range(0f, 1f)).ToList();

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, tempList.Count);
            _combinedContainer.Add(tempList[randomIndex]);
            tempList.RemoveAt(randomIndex);
        }
    }

    public GameObject GetItem()
    {
        GameObject item = _combinedContainer[0];
        _combinedContainer.RemoveAt(0);

        if (_combinedContainer.Count == 0)
            FillCombinedContainer();

        return item;
    }
}
