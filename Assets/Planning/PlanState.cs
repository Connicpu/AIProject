using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlanningAI.AStar;
using UnityEngine;

namespace Planning
{
    public enum PlanAction
    {
        BuyWood,
        SellWood,
        BuyFood,
        SellFood
    }

    public class PlanState : INode<PlanState>
    {
        private readonly Dictionary<PlanState, float> _traversalCache = new Dictionary<PlanState, float>();
        private List<PlanState> _actions;

        public Merchant Merchant;
        public LocationBase Location;
        public PlanState Goal;

        public float Food;
        public float Wood;
        public float Gold;

        public PlanAction Action;

        public void InitActions()
        {
            _actions = new List<PlanState>();
            var cities = GameObject.FindGameObjectsWithTag("City")
                .Select(g => g.GetComponent<LocationBase>())
                .Where(loc => loc != Merchant.HomeCity).ToArray();
            var farms = GameObject.FindGameObjectsWithTag("Farm")
                .Select(g => g.GetComponent<LocationBase>())
                .Where(loc => loc != Merchant.HomeCity).ToArray();
            var lumberyards = GameObject.FindGameObjectsWithTag("LumberYard")
                .Select(g => g.GetComponent<LocationBase>())
                .Where(loc => loc != Merchant.HomeCity).ToArray();
            var locations = cities.Concat(farms).Concat(lumberyards).ToArray();

            var cheapestWood = locations.GetMin(loc => loc.GetPrice(RESOURCES.WOOD));
            var priciestWood = locations.GetMax(loc => loc.GetPrice(RESOURCES.WOOD));
            var cheapestFood = locations.GetMin(loc => loc.GetPrice(RESOURCES.FOOD));
            var priciestFood = locations.GetMax(loc => loc.GetPrice(RESOURCES.FOOD));

            var buyWoodCost = cheapestWood.GetPrice(RESOURCES.WOOD) * 10;
            var buyFoodCost = cheapestFood.GetPrice(RESOURCES.FOOD) * 10;
            var sellWoodCost = priciestWood.GetPrice(RESOURCES.WOOD) * 10;
            var sellFoodCost = priciestFood.GetPrice(RESOURCES.FOOD) * 10;

            var woodSurp = Wood - Goal.Wood;
            var foodSurp = Food - Goal.Food;

            // Check if we need money
            var neededMoney = 0.0f;
            if (Wood < Goal.Wood)
            {
                neededMoney = buyWoodCost;
            }
            if (Food < Goal.Food)
            {
                neededMoney = Math.Max(neededMoney, buyFoodCost);
            }

            // If we need money, figure out which resource is in the most surplus
            if (neededMoney*2 >= Gold)
            {
                if (woodSurp > foodSurp)
                {
                    // Sell some wood
                    _actions.Add(new PlanState
                    {
                        Merchant = Merchant,
                        Location = priciestWood,
                        Goal = Goal,
                        Food = Food,
                        Wood = Wood - 10,
                        Gold = Gold + sellWoodCost,
                        Action = PlanAction.SellWood,
                    });
                }
                else
                {
                    // Sell some food
                    _actions.Add(new PlanState
                    {
                        Merchant = Merchant,
                        Location = priciestWood,
                        Goal = Goal,
                        Food = Food - 10,
                        Wood = Wood,
                        Gold = Gold + sellFoodCost,
                        Action = PlanAction.SellFood,
                    });
                }
            }
            else
            {
                // Buy some wood
                if (Wood < Goal.Wood)
                {
                    _actions.Add(new PlanState
                    {
                        Merchant = Merchant,
                        Location = cheapestWood,
                        Goal = Goal,
                        Food = Food,
                        Wood = Wood + 10,
                        Gold = Gold - buyWoodCost,
                        Action = PlanAction.BuyWood,
                    });
                }

                // Buy some food
                if (Food < Goal.Food)
                {
                    _actions.Add(new PlanState
                    {
                        Merchant = Merchant,
                        Location = cheapestFood,
                        Goal = Goal,
                        Food = Food + 10,
                        Wood = Wood,
                        Gold = Gold - buyFoodCost,
                        Action = PlanAction.BuyFood,
                    });
                }
            }
        }

        public float GetTraversalCost(PlanState source)
        {
            float cost;
            if (_traversalCache.TryGetValue(source, out cost))
                return cost;

            source.Location.WalkNode.NavigateTo(Location.WalkNode, out cost);
            _traversalCache[source] = cost;

            return cost;
        }

        public float GetHeuristic(PlanState goal)
        {
            return 0;
        }

        public IEnumerable<PlanState> Neighbors
        {
            get
            {
                if (_actions == null)
                    InitActions();
                return _actions;
            }
        }

        public virtual bool Equivalent(PlanState other)
        {
            if (other is PlanGoal)
            {
                return other.Equivalent(this);
            }

            return Equals(this, other);
        }
    }
}
