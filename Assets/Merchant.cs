using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Planning;
using PlanningAI.AStar;

public class Merchant : MonoBehaviour
{
    public PathNode CurrentGoal;
    private List<PathNode> _currentPath;
    private List<PlanState> _currentPlan; 

    public LocationBase HomeCity;
    public float Wood;
    public float Food;

    public void Update()
    {
        CheckForPath();
        WalkPath();
    }

    private void CheckForPlan()
    {
        if (_currentPlan != null)
            return;

        var curState = new PlanState
        {
            Merchant = this,
            Location = HomeCity
        };

        var astar = new AStar<PlanState>();

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
