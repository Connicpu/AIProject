using UnityEngine;
using System.Collections;

public class FarmLocation : LocationBase
{
  [Header("Farm Parameters")]
  public float FoodProducedPerPerson = 2;

  protected override void Start()
  {
    base.Start();

    //start with more food
    CurrentFood = 100;
  }

  protected override void Upkeep()
  {
    base.Upkeep();

    //produce food
    CurrentFood += FoodProducedPerPerson * CurrentPopulation;
  }

  protected override RESOURCES GetResourceType()
  {
    return RESOURCES.FOOD;
  }

  protected override float GetProduction()
  {
    return FoodProducedPerPerson * CurrentPopulation;
  }
}
