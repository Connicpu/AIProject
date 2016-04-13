using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Planning;
using PlanningAI.AStar;

public class Merchant : MonoBehaviour
{
    public PathNode CurrentGoal;
    private List<PathNode> _currentPath;

    private PlanState _currentAction;
    private List<PlanState> _currentPlan;

    public LocationBase HomeCity;
    public float Wood;
    public float Food;

    public void Update()
    {
        CheckForPlan();
        ExecutePlan();

        CheckForPath();
        WalkPath();
    }

    private void CheckForPlan()
    {
        if (_currentPath != null)
            return;
        if (_currentPlan != null)
            return;

        var goal = new PlanGoal
        {
            Food = HomeCity.CurrentFood + 10,
            Wood = HomeCity.CurrentWood + 10,
        };

        var curState = new PlanState
        {
            Merchant = this,
            Location = HomeCity,
            Goal = goal,
            Food = HomeCity.CurrentFood,
            Wood = HomeCity.CurrentWood,
            Gold = HomeCity.CurrentMoney,
            Action = PlanAction.Start,
        };

        var astar = new AStar<PlanState>();
        var result = astar.RunToCompletion(curState, goal);
        if (result == AStarResult.Failed)
        {
            if (FindClosestNode() != HomeCity.WalkNode)
            {
                CurrentGoal = HomeCity.WalkNode;
            }
            return;
        }

        _currentPlan = astar.TraverseFromGoal().Reverse().ToList();
    }

    private void ExecutePlan()
    {
        if (_currentPath != null)
            return;

        if (_currentAction == null)
        {
            if (_currentPlan == null)
                return;
            if (_currentPlan.Count == 0)
            {
                _currentPlan = null;
                return;
            }

            _currentAction = _currentPlan[0];
            _currentPlan.RemoveAt(0);
            CurrentGoal = _currentAction.Location.WalkNode;
        }
        else
        {
            var them = _currentAction.Location;
            float price;
            switch (_currentAction.Action)
            {
                case PlanAction.Start:
                    break;
                case PlanAction.BuyWood:
                    price = them.GetPrice(RESOURCES.WOOD) * 5;
                    HomeCity.CurrentWood += 5;
                    HomeCity.CurrentMoney -= price;
                    them.CurrentWood -= 5;
                    them.CurrentMoney += price;
                    break;
                case PlanAction.SellWood:
                    price = them.GetPrice(RESOURCES.WOOD) * 5;
                    HomeCity.CurrentWood -= 5;
                    HomeCity.CurrentMoney += price;
                    them.CurrentWood += 5;
                    them.CurrentMoney -= price;
                    break;
                case PlanAction.BuyFood:
                    price = them.GetPrice(RESOURCES.FOOD) * 5;
                    HomeCity.CurrentFood += 5;
                    HomeCity.CurrentMoney -= price;
                    them.CurrentFood -= 5;
                    them.CurrentMoney += price;
                    break;
                case PlanAction.SellFood:
                    price = them.GetPrice(RESOURCES.FOOD) * 5;
                    HomeCity.CurrentFood -= 5;
                    HomeCity.CurrentMoney += price;
                    them.CurrentFood += 5;
                    them.CurrentMoney -= price;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _currentAction = null;
        }
    }

    private void CheckForPath()
    {
        if (_currentPath != null || CurrentGoal == null) return;

        var myNode = FindClosestNode();
        _currentPath = myNode.NavigateTo(CurrentGoal);

        if (_currentPath == null)
        {
            CurrentGoal = null;
        }
        else
        {
            gameObject.GetComponent<Animator>().SetBool("Moving", true);
        }
    }

    private void WalkPath()
    {
        if (_currentPath == null || _currentPath.Count == 0)
            return;

        var targetNode = _currentPath[0];
        var target = targetNode.gameObject.transform.position;
        var curPos = gameObject.transform.position;
        var moveVec = target - curPos;
        var dist = moveVec.magnitude;
        var moveAmt = Time.deltaTime * 2;
        if (dist < moveAmt + 0.05)
        {
            gameObject.transform.position = target;
            _currentPath.RemoveAt(0);

            if (_currentPath.Count != 0) return;

            _currentPath = null;
            gameObject.GetComponent<Animator>().SetBool("Moving", false);
            return;
        }

        gameObject.transform.LookAt(target);

        if (dist > moveAmt)
        {
            moveVec.Normalize();
            moveVec *= moveAmt;
        }

        gameObject.transform.position += moveVec;
    }

    private PathNode FindClosestNode()
    {
        var nodes = GameObject.FindGameObjectsWithTag("PathNode");
        var minv = float.MaxValue;
        GameObject min = null;
        foreach (var node in nodes)
        {
            var dist = (node.transform.position - gameObject.transform.position).magnitude;
            if (!(dist < minv)) continue;
            minv = dist;
            min = node;
        }

        return min == null ? null : min.GetComponent<PathNode>();
    }
}
