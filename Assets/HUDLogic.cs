using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public struct HUDInfo
{
  public RESOURCES ResourceType;
  public float Production;
  public int Population;
  public int Houses;
  public float Food;
  public float FoodValue;
  public float Wood;
  public float WoodValue;
  public float Money;
  //public float MoneyValue;

  public HUDInfo(RESOURCES resourceType, float prod, int pop, int house, float food, float fvalue,
    float wood, float wvalue, float money)
  {
    ResourceType = resourceType;
    Production = prod;
    Population = pop;
    Houses = house;
    Food = food;
    FoodValue = fvalue;
    Wood = wood;
    WoodValue = wvalue;
    Money = money;
  }
}

public class HUDLogic : MonoBehaviour {

  public float ExpireAfter;
  float ExpirationTimer;

	void Start ()
  {
    ExpirationTimer = 0;
	}
	
	void Update ()
  {
    ExpirationTimer -= Time.deltaTime;
    if (ExpirationTimer <= 0) {
      gameObject.SetActive(false);
    }
	}

  public void DisplayInfo(HUDInfo hudInfo)
  {
    Transform title = transform.GetChild(0).GetChild(0);
    switch(hudInfo.ResourceType) {
      case RESOURCES.FOOD:
        title.GetComponent<Text>().text = "Farm";
        break;
      case RESOURCES.WOOD:
        title.GetComponent<Text>().text = "Lumber Yard";
        break;
      case RESOURCES.MONEY:
        title.GetComponent<Text>().text = "City";
        break;
    }

    Transform body = transform.GetChild(0).GetChild(1);
    string line = "";
    line += "Produces " + hudInfo.Production;
    switch (hudInfo.ResourceType) {
      case RESOURCES.FOOD:
        line += " food/day\n";
        break;
      case RESOURCES.WOOD:
        line += " wood/day\n";
        break;
      case RESOURCES.MONEY:
        line += " money/day\n";
        break;
    }
    line += "Population: " + hudInfo.Population + "\n";
    line += "Houses: " + hudInfo.Houses + "\n";
    line += "Food: " + hudInfo.Food + " Value: " + hudInfo.FoodValue + "\n";
    line += "Wood: " + hudInfo.Wood + " Value: " + hudInfo.WoodValue + "\n";
    line += "Money: " + hudInfo.Money;
    body.GetComponent<Text>().text = line;

    gameObject.SetActive(true);
    ExpirationTimer = ExpireAfter;
  }
}
