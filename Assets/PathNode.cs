using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlanningAI.AStar;

public class PathNode : MonoBehaviour, INode<PathNode>
{
    public List<PathNode> ConnectedNeighbors;

    public float GetHeuristic(PathNode goal)
    {
        return GetTraversalCost(goal);
    }

    public float GetTraversalCost(PathNode source)
    {
        var myPos = gameObject.transform.position;
        var otherPos = source.gameObject.transform.position;
        return (myPos - otherPos).magnitude;
    }

    public bool Equivalent(PathNode other)
    {
        return this == other;
    }

    public IEnumerable<PathNode> Neighbors
    {
        get { return ConnectedNeighbors; }
    }

    public List<PathNode> NavigateTo(PathNode goal)
    {
        var astar = new AStar<PathNode>();
        var result = astar.RunToCompletion(this, goal);
        if (result == AStarResult.Failed)
        {
            return null;
        }

        var list = astar.TraverseFromGoal().ToList();
        list.Reverse();
        return list;
    }

    public List<PathNode> NavigateTo(PathNode goal, out float distance)
    {
        var nodes = NavigateTo(goal);
        if (nodes == null)
        {
            distance = -1;
            return null;
        }

        distance = 0;
        for (var i = 1; i < nodes.Count; ++i)
        {
            distance += nodes[i - 1].GetTraversalCost(nodes[i]);
        }
        return nodes;
    }
}
