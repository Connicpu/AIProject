using UnityEngine;
using System.Collections;

public enum RESOURCES
{
    FOOD = 0,
    WOOD,
    MONEY,

    END //For iteration
}

public class LocationBase : MonoBehaviour
{
    public struct ResourceData
    {
        public RESOURCES ResourceType;
        public float Stockpile;
        public float Demand;

        public float Value;
        public float BuyAdjust;
        public float SellAdjust;

        public float PreferredRatio;
        //public int YesterdaysQuantity;

        public ResourceData(RESOURCES resourceType, int startingQty)
        {
            ResourceType = resourceType;
            Stockpile = startingQty;
            Demand = 0;

            Value = 100;
            BuyAdjust = 1.001001f;
            SellAdjust = 0.999f;

            PreferredRatio = 5;
            //YesterdaysQuantity = 0;
        }
    }

    [Header("General")]
    public LocationBase[] AdjacentLocations;
    float TimePerDay = 30f;
    public ResourceData[] Resources =
    {
        new ResourceData(RESOURCES.FOOD, 50),
        new ResourceData(RESOURCES.WOOD, 50),
        new ResourceData(RESOURCES.MONEY, 500)
    };

    [Header("General")]
    public PathNode WalkNode;

    protected virtual void Start()
    {
        ToolTip = GameObject.Find("HUD").GetComponent<HUDLogic>();

        Resources[(int)RESOURCES.MONEY].Value = 1;
        Resources[(int)RESOURCES.MONEY].BuyAdjust = 1;
        Resources[(int)RESOURCES.MONEY].SellAdjust = 1;

        Resources[(int)RESOURCES.FOOD].PreferredRatio = AdjacentLocations.Length + 1;
        Resources[(int)RESOURCES.WOOD].PreferredRatio = AdjacentLocations.Length + 1;

        StartCoroutine(DayCycle());
    }
    public IEnumerator DayCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(TimePerDay);
            Upkeep();
        }
    }
    protected virtual void Upkeep()
    {
        WoodUpkeep();
        FoodUpkeep();

        if (AutomatedTrade)
            FakeTradeBetweenAdjacentLocations();
    }
    public float CurrentFood
    {
        get
        {
            return Resources[(int)RESOURCES.FOOD].Stockpile;
        }
        set
        {
            if (Resources[(int)RESOURCES.FOOD].Stockpile > value)
            {
                float difference = Resources[(int)RESOURCES.FOOD].Stockpile - value;
                Resources[(int)RESOURCES.FOOD].Value *= Mathf.Pow(Resources[(int)RESOURCES.FOOD].BuyAdjust, difference);
        if (Resources[(int)RESOURCES.FOOD].Value > 271.96f)
          Resources[(int)RESOURCES.FOOD].Value = 271.96f;
            }
            else
            {
                float difference = value - Resources[(int)RESOURCES.FOOD].Stockpile;
                Resources[(int)RESOURCES.FOOD].Value *= Mathf.Pow(Resources[(int)RESOURCES.FOOD].SellAdjust, difference);
                if (Resources[(int)RESOURCES.FOOD].Value < 36.77f)
                    Resources[(int)RESOURCES.FOOD].Value = 36.77f;
            }
            Resources[(int)RESOURCES.FOOD].Stockpile = value;
        }
    }
    public float CurrentWood
    {
        get
        {
            return Resources[(int)RESOURCES.WOOD].Stockpile;
        }
        set
        {
            if (Resources[(int)RESOURCES.WOOD].Stockpile > value)
            {
                float difference = Resources[(int)RESOURCES.WOOD].Stockpile - value;
                Resources[(int)RESOURCES.WOOD].Value *= Mathf.Pow(Resources[(int)RESOURCES.WOOD].BuyAdjust, difference);
        if (Resources[(int)RESOURCES.WOOD].Value > 271.96f)
          Resources[(int)RESOURCES.WOOD].Value = 271.96f;
            }
            else {
                float difference = value - Resources[(int)RESOURCES.WOOD].Stockpile;
                Resources[(int)RESOURCES.WOOD].Value *= Mathf.Pow(Resources[(int)RESOURCES.WOOD].SellAdjust, difference);
                if (Resources[(int)RESOURCES.WOOD].Value < 36.77f)
                    Resources[(int)RESOURCES.WOOD].Value = 36.77f;
            }
            Resources[(int)RESOURCES.WOOD].Stockpile = value;
        }
    }
    public float CurrentMoney
    {
        get
        {
            return Resources[(int)RESOURCES.MONEY].Stockpile;
        }
        set
        {
            if (Resources[(int)RESOURCES.MONEY].Stockpile > value)
            {
                float difference = Resources[(int)RESOURCES.MONEY].Stockpile - value;
                Resources[(int)RESOURCES.MONEY].Value *= Mathf.Pow(Resources[(int)RESOURCES.MONEY].BuyAdjust, difference);
        if (Resources[(int)RESOURCES.MONEY].Value > 271.96f)
          Resources[(int)RESOURCES.MONEY].Value = 271.96f;
            }
            else {
                float difference = value - Resources[(int)RESOURCES.MONEY].Stockpile;
                Resources[(int)RESOURCES.MONEY].Value *= Mathf.Pow(Resources[(int)RESOURCES.MONEY].SellAdjust, difference);
                if (Resources[(int)RESOURCES.MONEY].Value < 36.77f)
                    Resources[(int)RESOURCES.MONEY].Value = 36.77f;
            }
            Resources[(int)RESOURCES.MONEY].Stockpile = value;
        }
    }

    #region PEOPLE & FOOD
    [Header("People")]
    public int CurrentPopulation = 100;
    int TodaysPopulationGrowth = 0;
    int MostPopulationGrowth = 0;
    int CalculatePopulationGrowthToday()
    {
        float idealGrowth = NumberOfHouses * PeoplePerHouse;
        if (idealGrowth == 0)
            return 0;
        float euler = 2.718282f;
        float deviationRate = 0.5f;

        float exponentNum = -1 * (CurrentPopulation - 0.5f * idealGrowth) * (CurrentPopulation - 0.5f * idealGrowth);
        float exponentDen = 2 * (0.5f * idealGrowth * deviationRate) * (0.5f * idealGrowth * deviationRate);

        return Mathf.FloorToInt(0.05f * idealGrowth * Mathf.Pow(euler, exponentNum / exponentDen));
    }
    float FoodRequiredPerPerson = 0.1f;
    void FoodUpkeep()
    {
        CurrentFood -= Resources[(int)RESOURCES.FOOD].Demand;
        TodaysPopulationGrowth = CalculatePopulationGrowthToday();
        CurrentPopulation += TodaysPopulationGrowth;
        if (TodaysPopulationGrowth > MostPopulationGrowth)
            MostPopulationGrowth = TodaysPopulationGrowth;
        Resources[(int)RESOURCES.FOOD].Demand = CurrentPopulation * FoodRequiredPerPerson;


        if (Resources[(int)RESOURCES.FOOD].Stockpile < 0)
        {
            //print("STARVATION AT " + gameObject.name);
            int populationLost = -Mathf.FloorToInt(Resources[(int)RESOURCES.FOOD].Stockpile / FoodRequiredPerPerson);
            if (populationLost > CurrentPopulation)
                populationLost = CurrentPopulation;
            CurrentPopulation -= populationLost;
            if (CurrentPopulation < 0)
                CurrentPopulation = 0;
            CurrentFood += populationLost * FoodRequiredPerPerson;
            //Resources[(int)RESOURCES.FOOD].Demand = CurrentPopulation * FoodRequiredPerPerson;
        }
    }
    #endregion

    #region HOUSING & WOOD
    [Header("Housing")]
    public int NumberOfHouses = 50;
    int PeoplePerHouse = 4;
    float WoodMaintenencePerHouse = 0.1f;
    float WoodRequiredToBuildHouse = 0.4f;
    int CalculateHousesBuiltToday()
    {
        int housesBuilt = 0;
        if (Mathf.Ceil(TodaysPopulationGrowth / PeoplePerHouse) * WoodRequiredToBuildHouse > Resources[(int)RESOURCES.WOOD].Stockpile) {
        {
            housesBuilt = Mathf.FloorToInt(Resources[(int)RESOURCES.WOOD].Stockpile / WoodRequiredToBuildHouse);
            NumberOfHouses += housesBuilt;
            Resources[(int)RESOURCES.WOOD].Stockpile -= housesBuilt * WoodRequiredToBuildHouse;
        }
        else {
            housesBuilt = Mathf.CeilToInt(TodaysPopulationGrowth / PeoplePerHouse);
            NumberOfHouses += housesBuilt;
            Resources[(int)RESOURCES.WOOD].Stockpile -= housesBuilt * WoodRequiredToBuildHouse;
        }
        return housesBuilt;
    }
    void WoodUpkeep()
    {
        Resources[(int)RESOURCES.WOOD].Demand = Mathf.Ceil(NumberOfHouses * WoodMaintenencePerHouse
      + Mathf.Ceil(TodaysPopulationGrowth / PeoplePerHouse) * WoodRequiredToBuildHouse);
        CurrentWood -= NumberOfHouses * WoodMaintenencePerHouse;

        if (Resources[(int)RESOURCES.WOOD].Stockpile < 0)
        {
            //print("COLLAPSE AT " + gameObject.name);
            int housesLost = -Mathf.FloorToInt(Resources[(int)RESOURCES.WOOD].Stockpile / WoodRequiredToBuildHouse);
            if (housesLost > NumberOfHouses)
                housesLost = NumberOfHouses;
            NumberOfHouses -= housesLost;
            if (NumberOfHouses < 0)
                NumberOfHouses = 0;
            CurrentWood += housesLost * WoodRequiredToBuildHouse;
            //Resources[(int)RESOURCES.WOOD].Demand = Mathf.Ceil(NumberOfHouses * WoodMaintenencePerHouse
            //  + Mathf.Ceil(MostPopulationGrowth / PeoplePerHouse) * WoodRequiredToBuildHouse);
        }
        else {
            CalculateHousesBuiltToday();
        }
    }
    #endregion

    #region TRADE
    [Header("Trading")]
    public bool AutomatedTrade = false;
    void FakeTradeBetweenAdjacentLocations()
    {
        #region FOOD_TRADE
        if (Resources[(int)RESOURCES.FOOD].Stockpile < Resources[(int)RESOURCES.FOOD].Demand
        * Resources[(int)RESOURCES.FOOD].PreferredRatio)
        {
            //find cheapest food location
            float cheapest = float.PositiveInfinity;
            LocationBase bestLocation = null;
            LocationBase secondBestLocation = null;
            foreach (LocationBase location in AdjacentLocations)
            {
                float price = location.GetPrice(RESOURCES.FOOD);
                if (price < cheapest)
                {
                    cheapest = price;
                    secondBestLocation = bestLocation;
                    bestLocation = location;
                }
            }
            if (bestLocation == null)
                return;
            //buy food to match demand and preference
            float currentNeed = Resources[(int)RESOURCES.FOOD].Demand * Resources[(int)RESOURCES.FOOD].PreferredRatio - CurrentFood;
            float requiredValue = currentNeed * cheapest;

            if (requiredValue <= CurrentMoney * Resources[(int)RESOURCES.MONEY].Value)
            {
                //buy with money
                float offerMoney = requiredValue / Resources[(int)RESOURCES.MONEY].Value;
                float refund = 0;
                float returningQuantity = bestLocation.TradeResourceWithLocation(RESOURCES.FOOD, offerMoney, RESOURCES.MONEY, out refund);
                CurrentFood += returningQuantity;
                CurrentMoney -= offerMoney - refund;
                currentNeed -= returningQuantity;
            }
            else if (requiredValue <= CurrentWood * Resources[(int)RESOURCES.WOOD].Value)
            {
                //barter with wood
                float offerWood = requiredValue / Resources[(int)RESOURCES.WOOD].Value;
                float refund = 0;
                float returningQuantity = bestLocation.TradeResourceWithLocation(RESOURCES.FOOD, offerWood, RESOURCES.WOOD, out refund);
                CurrentFood += returningQuantity;
                CurrentWood -= offerWood - refund;
                currentNeed -= returningQuantity;
            }
            else {
                //buy with what money is available
                float refund = 0;
                float returningQuantity = bestLocation.TradeResourceWithLocation(RESOURCES.FOOD, CurrentMoney, RESOURCES.MONEY, out refund);
                CurrentFood += returningQuantity;
                CurrentMoney -= CurrentMoney - refund;
                currentNeed -= returningQuantity;
            }

            if (currentNeed > 0 && secondBestLocation != null)
            {
                requiredValue = currentNeed * secondBestLocation.GetPrice(RESOURCES.FOOD);
                if (requiredValue <= CurrentMoney * Resources[(int)RESOURCES.MONEY].Value)
                {
                    //buy with money
                    float offerMoney = requiredValue / Resources[(int)RESOURCES.MONEY].Value;
                    float refund = 0;
                    CurrentFood += secondBestLocation.TradeResourceWithLocation(RESOURCES.FOOD, offerMoney, RESOURCES.MONEY, out refund);
                    CurrentMoney -= offerMoney - refund;
                }
                else if (requiredValue <= CurrentWood * Resources[(int)RESOURCES.WOOD].Value)
                {
                    //barter with wood
                    float offerWood = requiredValue / Resources[(int)RESOURCES.WOOD].Value;
                    float refund = 0;
                    CurrentFood += secondBestLocation.TradeResourceWithLocation(RESOURCES.FOOD, offerWood, RESOURCES.WOOD, out refund);
                    CurrentWood -= offerWood - refund;
                }
                else {
                    //buy with what money is available
                    float refund = 0;
                    CurrentFood += secondBestLocation.TradeResourceWithLocation(RESOURCES.FOOD, CurrentMoney, RESOURCES.MONEY, out refund);
                    CurrentMoney -= CurrentMoney - refund;
                }
            }
        }
        //else if (Resources[(int)RESOURCES.FOOD].Stockpile > Resources[(int)RESOURCES.FOOD].Demand
        //* Resources[(int)RESOURCES.FOOD].PreferredRatio) {
        //  //sell surplus food to highest adjacent
        //  float highest = 0;
        //  LocationBase bestLocation = null;
        //  foreach (LocationBase location in AdjacentLocations) {
        //    float price = location.GetPrice(RESOURCES.FOOD);
        //    if (price > highest) {
        //      highest = price;
        //      bestLocation = location;
        //    }
        //  }
        //  if (bestLocation == null)
        //    return;
        //  //sell surplus
        //  float currentSurplus = CurrentFood - Resources[(int)RESOURCES.FOOD].Demand * Resources[(int)RESOURCES.FOOD].PreferredRatio;
        //  if (CurrentWood * Resources[(int)RESOURCES.WOOD].Value < CurrentMoney * Resources[(int)RESOURCES.MONEY].Value) {
        //    //barter for wood;
        //    float refund = 0;
        //    CurrentWood += bestLocation.TradeResourceWithLocation(RESOURCES.WOOD, currentSurplus, RESOURCES.FOOD, out refund);
        //    CurrentFood -= currentSurplus - refund;
        //  }
        //  else {
        //    //sell for money
        //    float refund = 0;
        //    CurrentMoney += bestLocation.TradeResourceWithLocation(RESOURCES.MONEY, currentSurplus, RESOURCES.FOOD, out refund);
        //    CurrentFood -= currentSurplus - refund;
        //  }
        //}
        #endregion

        #region WOOD_TRADE
        if (Resources[(int)RESOURCES.WOOD].Stockpile < Resources[(int)RESOURCES.WOOD].Demand
        * Resources[(int)RESOURCES.WOOD].PreferredRatio)
        {
            //find cheapest wood location
            float cheapest = float.PositiveInfinity;
            LocationBase bestLocation = null;
            LocationBase secondBestLocation = null;
            foreach (LocationBase location in AdjacentLocations)
            {
                float price = location.GetPrice(RESOURCES.WOOD);
                if (price < cheapest)
                {
                    cheapest = price;
                    secondBestLocation = bestLocation;
                    bestLocation = location;
                }
            }
            if (bestLocation == null)
                return;
            //buy wood to match demand and preference
            float currentNeed = Resources[(int)RESOURCES.WOOD].Demand * Resources[(int)RESOURCES.WOOD].PreferredRatio - CurrentWood;
            float requiredValue = currentNeed * cheapest;

            if (requiredValue <= CurrentMoney * Resources[(int)RESOURCES.MONEY].Value)
            {
                //buy with money
                float offerMoney = requiredValue / Resources[(int)RESOURCES.MONEY].Value;
                float refund = 0;
                float returningQuantity = bestLocation.TradeResourceWithLocation(RESOURCES.WOOD, offerMoney, RESOURCES.MONEY, out refund);
                CurrentWood += returningQuantity;
                CurrentMoney -= offerMoney - refund;
                currentNeed -= returningQuantity;
            }
            else if (requiredValue <= CurrentFood * Resources[(int)RESOURCES.FOOD].Value)
            {
                //barter with food
                float offerFood = requiredValue / Resources[(int)RESOURCES.FOOD].Value;
                float refund = 0;
                float returningQuantity = bestLocation.TradeResourceWithLocation(RESOURCES.WOOD, offerFood, RESOURCES.FOOD, out refund);
                CurrentWood += returningQuantity;
                CurrentFood -= offerFood - refund;
                currentNeed -= returningQuantity;
            }
            else {
                //buy with what money is available
                float refund = 0;
                float returningQuantity = bestLocation.TradeResourceWithLocation(RESOURCES.WOOD, CurrentMoney, RESOURCES.MONEY, out refund);
                CurrentWood += returningQuantity;
                CurrentMoney -= CurrentMoney - refund;
                currentNeed -= returningQuantity;
            }

            if (currentNeed > 0 && secondBestLocation != null)
            {
                requiredValue = currentNeed * secondBestLocation.GetPrice(RESOURCES.WOOD);
                if (requiredValue <= CurrentMoney * Resources[(int)RESOURCES.MONEY].Value)
                {
                    //buy with money
                    float offerMoney = requiredValue / Resources[(int)RESOURCES.MONEY].Value;
                    float refund = 0;
                    CurrentWood += secondBestLocation.TradeResourceWithLocation(RESOURCES.WOOD, offerMoney, RESOURCES.MONEY, out refund);
                    CurrentMoney -= offerMoney - refund;
                }
                else if (requiredValue <= CurrentFood * Resources[(int)RESOURCES.FOOD].Value)
                {
                    //barter with food
                    float offerFood = requiredValue / Resources[(int)RESOURCES.FOOD].Value;
                    float refund = 0;
                    CurrentWood += secondBestLocation.TradeResourceWithLocation(RESOURCES.WOOD, offerFood, RESOURCES.FOOD, out refund);
                    CurrentFood -= offerFood - refund;
                }
                else {
                    //buy with what money is available
                    float refund = 0;
                    CurrentWood += secondBestLocation.TradeResourceWithLocation(RESOURCES.WOOD, CurrentMoney, RESOURCES.MONEY, out refund);
                    CurrentMoney -= CurrentMoney - refund;
                }
            }
        }
        //else if (Resources[(int)RESOURCES.WOOD].Stockpile > Resources[(int)RESOURCES.WOOD].Demand
        //* Resources[(int)RESOURCES.WOOD].PreferredRatio) {
        //  //sell surplus wood to highest adjacent
        //  float highest = 0;
        //  LocationBase bestLocation = null;
        //  foreach (LocationBase location in AdjacentLocations) {
        //    float price = location.GetPrice(RESOURCES.WOOD);
        //    if (price > highest) {
        //      highest = price;
        //      bestLocation = location;
        //    }
        //  }
        //  if (bestLocation == null)
        //    return;
        //  //sell surplus
        //  float currentSurplus = CurrentWood - Resources[(int)RESOURCES.WOOD].Demand * Resources[(int)RESOURCES.WOOD].PreferredRatio;
        //  if (CurrentFood * Resources[(int)RESOURCES.FOOD].Value < CurrentMoney * Resources[(int)RESOURCES.MONEY].Value) {
        //    //barter for food;
        //    float refund = 0;
        //    CurrentFood += bestLocation.TradeResourceWithLocation(RESOURCES.FOOD, currentSurplus, RESOURCES.WOOD, out refund);
        //    CurrentWood -= currentSurplus - refund;
        //  }
        //  else {
        //    //sell for money
        //    float refund = 0;
        //    CurrentMoney += bestLocation.TradeResourceWithLocation(RESOURCES.MONEY, currentSurplus, RESOURCES.WOOD, out refund);
        //    CurrentWood -= currentSurplus - refund;
        //  }
        //}
        #endregion
    }
    public float TradeResourceWithLocation(RESOURCES boughtResource, float sellingQuantity, RESOURCES sellingResource, out float refund)
    {
        float offeredValue = Resources[(int)sellingResource].Value * sellingQuantity;
        float existingValue = (Resources[(int)boughtResource].Stockpile - Resources[(int)boughtResource].Demand - 10) * Resources[(int)boughtResource].Value;
        if (existingValue < 0)
            existingValue = 0;

        if (offeredValue > existingValue)
        {
            //return what is available
            float returningQuantity = existingValue / Resources[(int)boughtResource].Value;
            Resources[(int)boughtResource].Stockpile -= returningQuantity;
            //refund
            float refundingValue = offeredValue - existingValue;
            refund = refundingValue / Resources[(int)sellingResource].Value;
            //update values
            Resources[(int)boughtResource].Value *= Mathf.Pow(Resources[(int)boughtResource].BuyAdjust, returningQuantity);
            Resources[(int)sellingResource].Value *= Mathf.Pow(Resources[(int)sellingResource].SellAdjust, sellingQuantity - refund);
            if (Resources[(int)sellingResource].Value < 36.77f)
                Resources[(int)sellingResource].Value = 36.77f;
            return returningQuantity;
        }
        else
        {
            //return equivalent value
            float returningQuantity = offeredValue / Resources[(int)boughtResource].Value;
            Resources[(int)boughtResource].Stockpile -= returningQuantity;
            //update values
            Resources[(int)boughtResource].Value *= Mathf.Pow(Resources[(int)boughtResource].BuyAdjust, returningQuantity);
            Resources[(int)sellingResource].Value *= Mathf.Pow(Resources[(int)sellingResource].SellAdjust, sellingQuantity);
            if (Resources[(int)sellingResource].Value < 36.77f)
                Resources[(int)sellingResource].Value = 36.77f;
            refund = 0;
            return returningQuantity;
        }
    }
    public float GetPrice(RESOURCES resource)
    {
        return Resources[(int)resource].Value;
    }
    #endregion

    #region DISPLAY
    HUDLogic ToolTip;
    protected virtual RESOURCES GetResourceType() { return RESOURCES.END; }
    protected virtual float GetProduction() { return 0; }
    void OnMouseOver()
    {
        HUDInfo hudInfo = new HUDInfo(GetResourceType(), Mathf.Round(GetProduction()), CurrentPopulation,
          NumberOfHouses, Mathf.Round(CurrentFood), Mathf.Round(Resources[(int)RESOURCES.FOOD].Value),
          Mathf.Round(CurrentWood), Mathf.Round(Resources[(int)RESOURCES.WOOD].Value),
          Mathf.Round(CurrentMoney));

        ToolTip.DisplayInfo(hudInfo);

        //lines to adjacent locations?
    }
    void Update()
    {
        //if (float.IsNaN(CurrentFood))
        //  CurrentFood = 0;
        //Food = CurrentFood;
        //Wood = CurrentWood;
        //Money = CurrentMoney;

        //update size based on population
        transform.localScale = new Vector3(transform.localScale.x, Mathf.Sqrt(Mathf.Sqrt(CurrentPopulation)), transform.localScale.z);
    }
    #endregion
}
