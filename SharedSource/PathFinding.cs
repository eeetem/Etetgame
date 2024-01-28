using System;
using System.Collections.Generic;
using System.Threading;
using DefconNull.ReplaySequence;
using Microsoft.Xna.Framework;

namespace DefconNull;

public static class PathFinding
{
    public static List<Node[,]> Nodes = new List<Node[,]>();
    public static List<bool> InUse = new List<bool>();

    public static void ResetNodes(PriorityQueue<Node, double>.UnorderedItemsCollection nodes)
    {
        foreach (var idkstfu in nodes)
        {
            var node = idkstfu.Element;
            node.CurrentCost = 0;
            node.EstimatedCost = 0;
            node.Parent = null;
            node.State = NodeState.Unconsidered;
        }
    }
    public static void ResetNodes(List<Node> nodes)
    {
        foreach (var node in nodes)
        {
            node.CurrentCost = 0;
            node.EstimatedCost = 0;
            node.Parent = null;
            node.State = NodeState.Unconsidered;
        }
    }

    private static int GetNextFreeLayer()
    {
        for (int i = 0; i < InUse.Count; i++)
        {
            if (!InUse[i]) return i;
        }
        return GenerateNewLayer();
    }

    private static int GenerateNewLayer()
    {
        int layer = Nodes.Count;
	
        Nodes.Add(new Node[100,100]);
	
        InUse.Add(false);
		


        for (int x = 0; x < 100; x++)
        {
            for (int y = 0; y < 100; y++)
            {
                Nodes[layer][x, y] = new Node(new Vector2Int(x,y));
            }
        }
        for (int x = 0; x < 100; x++)
        {
            for (int y = 0; y < 100; y++)
            {

                Node[] connections = new Node[8];
                if(x-1 > 0 && y-1 > 0)    connections[0] = Nodes[layer][x - 1, y - 1];
                if(y-1 > 0)               connections[1] = Nodes[layer][x    , y - 1];
                if(x+1 < 99 && y-1 > 0)   connections[2] = Nodes[layer][x + 1, y - 1];
                if(x+1 < 99)              connections[3] = Nodes[layer][x + 1, y];
                if(x-1 > 0)               connections[4] = Nodes[layer][x - 1, y];
                if(x-1 > 0 && y+1 < 99)   connections[5] = Nodes[layer][x - 1, y + 1];
                if(y+1 < 99)              connections[6] = Nodes[layer][x    , y + 1];
                if(x+1 < 99 && y+1 < 99)  connections[7] = Nodes[layer][x + 1, y + 1];
					

                Nodes[layer][x, y].ConnectedNodes = connections;
            }
        }

        return layer;
    }

    public static readonly object syncobj = new object();

    public static List<(Vector2Int,PathFindResult)> GetAllPaths(Vector2Int from, int range, bool generatePaths)
    {
        int layer = -1;
        lock (syncobj)
        {
            layer = GetNextFreeLayer();
            InUse[layer] = true;
        }
        var p = GetAllPaths(Nodes[layer][from.X, from.Y], range,generatePaths);
        InUse[layer] = false;
        return p;
    }
    public static List<(Vector2Int,PathFindResult)> GetAllPaths(Node from, int range, bool generatePaths)
    {

        var done = new List<Node>();
        var inRange = new List<(Vector2Int,PathFindResult)>();

        var open = new PriorityQueue<Node, double>();
        foreach (var node in from.ConnectedNodes)
        {
            if (node is null) continue;
            // Add connecting nodes if traversable
            if (node.Traversable(from))
            {
                // Calculate the Cost
                node.CurrentCost = from.CurrentCost + from.TraversalCost(node);
                node.State = NodeState.Open;
                // Enqueue
                open.Enqueue(node, node.TotalCost);
            }
        }
        from.State = NodeState.Closed;

        while (true)
        {
            // End Condition( Path not found )
            if (open.Count == 0)
            {
                ResetNodes(done);
                ResetNodes(open.UnorderedItems);
                return inRange;
            }

            // Selecting next Element from queue
            var current = open.Dequeue();
				
            // Add it to the done list
            done.Add(current);
					

            current.State = NodeState.Closed;
					
					
            if (current.CurrentCost <= range)
            {
                if(generatePaths)
                {
                    var path = GeneratePath(current);
                    inRange.Add((current.Position,new PathFindResult(path,current.CurrentCost)));
                }
                else
                {
                    inRange.Add((current.Position,new PathFindResult(new List<Vector2Int>(),current.CurrentCost)));
                }

                //Console.WriteLine("added with range: "+current.CurrentCost);
            }
            else
            {
                //Console.WriteLine("rejected with range: "+current.CurrentCost);
                continue;
            }

            AddOrUpdateConnected(current, null,open);
        }
		

    }
    public struct PathFindResult
    {
        public  List<Vector2Int> Path;
        public  double Cost;
        public PathFindResult(List<Vector2Int> path, double cost)
        {
            Path = path;
            Cost = cost;
        }

			
    }

    public static PathFindResult GetPath(Vector2Int from, Vector2Int to)
    {
        if(from == to) return new PathFindResult(new List<Vector2Int>(),0	);
        if (!WorldManager.IsPositionValid(from) || !WorldManager.IsPositionValid(to)) return new PathFindResult(new List<Vector2Int>(),0);
        int layer = -1;
        lock (syncobj)
        {
            layer = GetNextFreeLayer();
            InUse[layer] = true;
        }
        var p = GetPath(Nodes[layer][from.X, from.Y], Nodes[layer][to.X, to.Y]);
        InUse[layer] = false;
        return p;
    }

    public static PathFindResult GetPath(Node from, Node to)
    {

        var done = new List<Node>((int) (Vector2.Distance(from.Position,to.Position)*2f));
			
        var open = new PriorityQueue<Node,double>();
        foreach (var node in from.ConnectedNodes)
        {
            if(node is null) continue;
            // Add connecting nodes if traversable
            if (node.Traversable(from))
            {
                // Calculate the Costs
                node.CurrentCost = from.CurrentCost + from.TraversalCost(node);
                node.EstimatedCost = Utility.Distance(node.Position,to.Position);
                node.State = NodeState.Open;
                // Enqueue
                open.Enqueue(node, node.TotalCost);
            }
        }

        while (true)
        {
            // End Condition( Path not found )
            if (open.Count == 0)
            {
                ResetNodes(done);
                ResetNodes(open.UnorderedItems);
                return new PathFindResult(new List<Vector2Int>(), 0);
            }

            // Selecting next Element from queue
            var current = open.Dequeue();
            // Add it to the done list
            if(current.State == NodeState.Closed) continue;
            done.Add(current);

            current.State = NodeState.Closed;

            // EndCondition( Path was found )
            if (current == to)
            {
                var ret = GeneratePath(to); // Create the Path

                // Reset all Nodes that were used.
                double cost = to.CurrentCost;
                ResetNodes(done);
                ResetNodes(open.UnorderedItems);
						
                return new PathFindResult(ret, cost);
            }

            AddOrUpdateConnected(current, to, open);
        }
		
    }
    private static List<Vector2Int> GeneratePath(Node target)
    {
        var ret = new List<Vector2Int>();
        var current = target;
        while (!(current is null))
        {
            ret.Add(current.Position);
            current = current.Parent;
        }

        ret.Reverse();
        return ret;
    }
	
    private static void AddOrUpdateConnected(Node current, Node? to, PriorityQueue<Node,double> queue)
    {
        if (current.Parent != null && !current.Traversable(current.Parent))
        {
            throw new Exception(
                "how");
        }
        foreach (var connected in current.ConnectedNodes)
        {
            if (connected is null) return;

            
            if (!connected.Traversable(current,false) ||
                connected.State == NodeState.Closed)
            {
                continue; // Do ignore already checked and not traversable nodes.
            }

            // Adds a previously not "seen" node into the Queue
            if (connected.State == NodeState.Unconsidered)
            {
                connected.Parent = current;
                connected.CurrentCost = current.CurrentCost + current.TraversalCost(connected);
                if (to is not null)
                {
                    connected.EstimatedCost =  Utility.Distance(connected.Position, to.Position);
                }

                connected.State = NodeState.Open;
                queue.Enqueue(connected,connected.TotalCost);
            }
            else if (current != connected)
            {
                var newCCost = current.CurrentCost + current.TraversalCost(connected);
                if (newCCost < connected.CurrentCost)
                {
                    connected.Parent = current;
                    connected.CurrentCost = newCCost;
                    queue.Enqueue(connected,connected.TotalCost);
                }
            }
            else
            {
                // Codacy made me do it.
                throw new Exception(
                    "Detected the same node twice. Confusion how this could ever happen");
            } 
        }
	
    }
}

/// <summary>
///     Contains Positional and other information about a single node.
/// </summary>
public class Node : IComparable<Node>, IEquatable<Node>
{
    public Node(Vector2Int position)
    {
        Position = position;


    }

    /// <summary>
    ///     Gets the Total cost of the Node.
    ///     The Current Costs + the estimated costs.
    /// </summary>
    public double TotalCost => EstimatedCost + CurrentCost;

    /// <summary>
    ///     Gets or sets the Distance between this node and the target node.
    /// </summary>
    public double EstimatedCost { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to go from the start node to this node.
    /// </summary>
    public double CurrentCost { get; set; }


    /// <summary>
    ///     Gets or sets the state of the Node
    ///     Can be Unconsidered(Default), Open and Closed.
    /// </summary>
    public NodeState State { get; set; }
    /// <summary>
    ///     Gets a value indicating whether the node is traversable.
    /// </summary>
    public bool Traversable(Node from, bool ignoreControllables = false)
    {
        var tile = WorldManager.Instance.GetTileAtGrid(Position);
        return tile.Traversible(from.Position,ignoreControllables);
    }


    public Node?[] ConnectedNodes { get; set; } = new Node?[8];

    /// <summary>
    ///     Gets or sets he "previous" node that was processed before this node.
    /// </summary>
    public Node? Parent { get; set; }


    public readonly Vector2Int Position;

    /// <summary>
    ///     Compares the Nodes based on their total costs.
    ///     Total Costs: A* Pathfinding.
    ///     Current: Djikstra Pathfinding.
    ///     Estimated: Greedy Pathfinding.
    /// </summary>
    /// <param name="other">The other node.</param>
    /// <returns>A comparison between the costs.</returns>
    public int CompareTo(Node? other) => TotalCost.CompareTo(other?.TotalCost ?? 0);

    public bool Equals(Node? other) => CompareTo(other) == 0 && other?.Position == Position;

    public static bool operator ==(Node? left, Node? right) => left?.Equals(right) != false;

    public static bool operator >(Node left, Node right) => left.CompareTo(right) > 0;

    public static bool operator <(Node left, Node right) => left.CompareTo(right) < 0;

    public static bool operator !=(Node? left, Node? right) => !(left == right);

    public static bool operator <=(Node left, Node right) => left.CompareTo(right) <= 0;

    public static bool operator >=(Node left, Node right) => left.CompareTo(right) >= 0;

    public override bool Equals(object? obj) => obj is Node other && Equals(other);

    public override int GetHashCode() =>
        Position.GetHashCode();

    /// <summary>
    ///     Returns the distance to the other node.
    /// </summary>
    /// <param name="other">The other node.</param>
    /// <returns>Distance between this and other.</returns>

    public double TraversalCost(Node to)
    {

        var target = WorldManager.Instance.GetTileAtGrid(to.Position);
        return target.TraverseCostFrom(Position);


    }
}


public enum NodeState
{
    Unconsidered = 0,
    Open = 1,
    Closed = 2,
}