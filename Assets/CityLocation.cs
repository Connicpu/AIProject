using UnityEngine;
using System.Collections;

public class CityLocation : LocationBase
{
    [Header("City Parameters")]
    public float MoneyProducedPerPerson = 200;

    protected override void Start()
    {
        base.Start();

        //start with more money
        CurrentMoney = 1000;
    }

    protected override void Upkeep()
    {
        base.Upkeep();

        //produce money
        CurrentMoney += MoneyProducedPerPerson * CurrentPopulation;
        BuySurplus();
    }

    void BuySurplus()
    {
        foreach (LocationBase location in AdjacentLocations)
        {
            float apportionMoney = CurrentMoney / AdjacentLocations.Length / (float)RESOURCES.MONEY;
            for (int i = 0; i < (int)RESOURCES.MONEY; ++i)
                if (location.GetPrice((RESOURCES)i) < Resources[i].Value)
                {
                    //buy with money
                    float refund = 0;
                    float returningQuantity = location.TradeResourceWithLocation((RESOURCES)i, apportionMoney, RESOURCES.MONEY, out refund);
                    Resources[i].Stockpile += returningQuantity;
                    Resources[i].Value *= Mathf.Pow(Resources[i].SellAdjust, returningQuantity);
                    if (Resources[i].Value < 36.77f)
                        Resources[i].Value = 36.77f;
                    CurrentMoney -= apportionMoney - refund;
                }
        }
    }

    protected override RESOURCES GetResourceType()
    {
        return RESOURCES.MONEY;
    }

    protected override float GetProduction()
    {
        return MoneyProducedPerPerson * CurrentPopulation;
    }
}
