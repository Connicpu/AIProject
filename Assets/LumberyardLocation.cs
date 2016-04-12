using UnityEngine;
using System.Collections;

public class LumberyardLocation : LocationBase
{
  [Header("Lumberyard Parameters")]
  public float WoodProducedPerPerson = 2;

  protected override void Start()
  {
    base.Start();

    //start with more wood
    CurrentWood = 100;
  }

  protected override void Upkeep()
  {
    base.Upkeep();

    //produce wood
    CurrentWood += WoodProducedPerPerson * CurrentPopulation;
  }

  protected override RESOURCES GetResourceType()
  {
    return RESOURCES.WOOD;
  }

  protected override float GetProduction()
  {
    return WoodProducedPerPerson * CurrentPopulation;
  }
}
