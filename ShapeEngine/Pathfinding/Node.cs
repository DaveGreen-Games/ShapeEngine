using System.Numerics;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Pathfinding;

/// <summary>
/// Represents an abstract node in a pathfinding graph, supporting weights, neighbors, and connections.
/// </summary>
internal abstract class Node : IComparable<Node>
{
    private class NodeWeight
    {
        private int blockCount;
        private float baseValue;
        private float flat;
        private float bonus;
        public bool Blocked => blockCount > 0;

        public void Apply(NodeCost cost)
        {
            if (!cost.Valid) return;
            switch (cost.Type)
            {
                case NodeCostType.Reset:
                    Reset();
                    break;

                case NodeCostType.SetBaseValue:
                    baseValue = cost.Value;
                    break;
                case NodeCostType.ResetThenSetBaseValue:
                    Reset();
                    baseValue = cost.Value;
                    break;
                case NodeCostType.Block:
                    blockCount++;
                    break;
                case NodeCostType.Unblock:
                    blockCount--;
                    if (blockCount < 0) blockCount = 0;
                    break;
                case NodeCostType.ResetThenBlock:
                    Reset();
                    blockCount++;
                    break;
                case NodeCostType.AddFlat:
                    flat += cost.Value;
                    break;
                case NodeCostType.RemoveFlat:
                    flat -= cost.Value;
                    break;
                case NodeCostType.ResetFlat:
                    flat = 0;
                    break;
                case NodeCostType.AddBonus:
                    bonus += cost.Value;
                    break;
                case NodeCostType.RemoveBonus:
                    bonus -= cost.Value;
                    break;
                case NodeCostType.ResetBonus:
                    bonus = 0;
                    break;



            }
        }

        public float Cur => blockCount > 0 ? 0 : CalculateCostMultiplier();
        
        private float GetBaseValueFactor()
        {
            var v = baseValue + flat;
            if (v > 0) return v; //less favorable
            if (v < 0) return 1f / (1f - v); //more favorable
            return 1f; //normal
        }
        private float GetBonusFactor()
        {
            if (bonus > 0) return bonus; //more favorable
            if (bonus < 0) return 1f / (1f - bonus); //less favorable
            return 1f; //normal
        }
        private float CalculateCostMultiplier()
        {
            return GetBaseValueFactor() * GetBonusFactor();
        }

        // private float CalculateCostMultiplier()
        // {
        //     const float minThreshold = 0.0001f;
        //     // Step 1: Start with base multiplier (1.0 = normal cost)
        //     // baseValue shifts the starting point (e.g., terrain type differences)
        //     float multiplier = 1f + baseValue;
        //
        //     // Step 2: Apply flat adjustment (additive modification)
        //     // This is useful for fixed cost changes like "add 2 cost for climbing stairs"
        //     // Multiple entities can stack their flat bonuses/penalties independently
        //     multiplier += flat;
        //     
        //     if (bonus == 0f)//no bonus to apply
        //     {
        //         return multiplier > 0f ? MathF.Max(minThreshold, multiplier) : MathF.Max(minThreshold, 1f / (1f - multiplier));
        //     }
        //     
        //     
        //     if (multiplier >= 0f)
        //     {
        //         // Step 3: Apply bonus as percentage modifier (multiplicative modification)
        //         // This is useful for percentage-based effects like "50% faster movement"
        //         
        //         if (bonus > 0)
        //         {
        //             // Positive bonus = less favorable = increase cost
        //             // +1 bonus = 2x cost (twice as slow)
        //             // +2 bonus = 3x cost (three times as slow)
        //             // Formula: current_multiplier * (1 + bonus)
        //             multiplier *= (1f + bonus);
        //         }
        //         else
        //         {
        //             // Negative bonus = more favorable = reduce cost
        //             // -1 bonus = 0.5x cost (twice as fast)
        //             // -2 bonus = 0.33x cost (three times as fast)
        //             // Formula: current_multiplier / (1 - bonus)
        //             // Since bonus is negative, (1 - bonus) becomes (1 + |bonus|)
        //             multiplier /= (1f - bonus);
        //         }
        //         return Math.Max(minThreshold, multiplier); 
        //     }
        //     
        //     
        //     //when multiplier is negative before bonus application, bonus logic is inverted
        //     if (bonus > 0)
        //     {
        //             
        //         multiplier /= (1f + bonus);
        //     }
        //     else
        //     {
        //             
        //         multiplier *= (1f - bonus);
        //     }
        //     
        //     // multiplier is still negative here
        //     // Return the inverse to represent "better than free" traversal
        //     return MathF.Max(minThreshold, 1f / (1f - multiplier));
        // }
        //
        
        // private float CalculateCostMultiplier()
        // {
        //     // Step 1: Start with base multiplier (1.0 = normal cost)
        //     // baseValue shifts the starting point (e.g., terrain type differences)
        //     float multiplier = 1f + baseValue;
        //
        //     // Step 2: Apply flat adjustment (additive modification)
        //     // This is useful for fixed cost changes like "add 2 cost for climbing stairs"
        //     // Multiple entities can stack their flat bonuses/penalties independently
        //     multiplier += flat;
        //
        //     // Step 3: Apply bonus as percentage modifier (multiplicative modification)
        //     // This is useful for percentage-based effects like "50% faster movement"
        //     if (bonus != 0)
        //     {
        //         if (bonus > 0)
        //         {
        //             // Positive bonus = less favorable = increase cost
        //             // +1 bonus = 2x cost (twice as slow)
        //             // +2 bonus = 3x cost (three times as slow)
        //             // Formula: current_multiplier * (1 + bonus)
        //             multiplier *= (1f + bonus);
        //         }
        //         else
        //         {
        //             // Negative bonus = more favorable = reduce cost
        //             // -1 bonus = 0.5x cost (twice as fast)
        //             // -2 bonus = 0.33x cost (three times as fast)
        //             // Formula: current_multiplier / (1 - bonus)
        //             // Since bonus is negative, (1 - bonus) becomes (1 + |bonus|)
        //             multiplier /= (1f - bonus);
        //         }
        //     }
        //
        //     // Step 4: Clamp to minimum threshold to avoid division by zero or negative costs
        //     // Ensures the node always has some traversal cost
        //     return Math.Max(0.001f, multiplier);
        // }
        
        public void Reset()
        {
            baseValue = 0;
            flat = 0;
            bonus = 0;
            blockCount = 0;
        }
    }

    #region Members

    #region Internal

    internal HashSet<Node>? Neighbors;
    internal HashSet<Node>? Connections;
    internal float GScore = float.PositiveInfinity;
    internal float FScore = float.PositiveInfinity;
    internal float H = float.PositiveInfinity;

    #endregion

    #region Private

    private readonly NodeWeight weight = new();
    private Dictionary<uint, NodeWeight>? weights;
    
    #endregion
        

    #endregion

    #region Public

    /// <summary>
    /// Resets the node's connections and weight values.
    /// </summary>
    public void Reset()
    {
        Connections?.Clear();
            
        weight.Reset();
        ResetWeights();
        // weights?.Clear();
    }


    #endregion
        
    #region Neighbors & Connections
    /// <summary>
    /// Gets whether the node has any neighbors.
    /// </summary>
    public bool HasNeighbors => Neighbors is { Count: > 0 };
    /// <summary>
    /// Gets whether the node has any connections.
    /// </summary>
    public bool HasConnections => Connections is { Count: > 0 };
    /// <summary>
    /// Adds a neighbor node.
    /// </summary>
    public bool AddNeighbor(Node node)
    {
        Neighbors ??= new();
        return Neighbors.Add(node);
    }
    /// <summary>
    /// Removes a neighbor node.
    /// </summary>
    public bool RemoveNeighbor(Node node)
    {
        if (Neighbors == null) return false;
        return Neighbors.Remove(node);
    }
    /// <summary>
    /// Sets the neighbors of this node.
    /// </summary>
    public void SetNeighbors(HashSet<Node>? neighbors)
    {
        Neighbors = neighbors;
    }
    /// <summary>
    /// Connects this node to another node.
    /// </summary>
    public bool Connect(Node other)
    {
        if (other == this) return false;
        Connections ??= new();
        return Connections.Add(other);
    }
    /// <summary>
    /// Disconnects this node from another node.
    /// </summary>
    public bool Disconnect(Node other)
    {
        if (other == this) return false;
        if (Connections == null) return false;
        return Connections.Remove(other);
    }
    #endregion

    #region CellValues

    /// <summary>
    /// Applies a node cost to this node, optionally for a specific layer.
    /// </summary>
    public void ApplyNodeValue(NodeCost cost)
    {
        if (cost.Layer > 0)
        {
            if (weights == null) weights = new();

            if (weights.TryGetValue(cost.Layer, out var nodeWeight))
            {
                nodeWeight.Apply(cost);
            }
            else
            {
                var w = new NodeWeight();
                w.Apply(cost);
                weights.Add(cost.Layer, w);
            }
            
            //i think this was wrong... it never applied the value if the layer existed
            // if (!weights.ContainsKey(value.Layer))
            // {
            //     var w = new NodeWeight();
            //     w.Apply(value);
            //     weights.Add(value.Layer, w);
            // }
        }
        else weight.Apply(cost);
    }
    /// <summary>
    /// Applies multiple node costs to this node.
    /// </summary>
    public void ApplyNodeValues(IEnumerable<NodeCost> values)
    {
        foreach (var value in values)
        {
            ApplyNodeValue(value);
        }
    }


    #endregion
        
    #region Weight

    /// <summary>
    /// Returns true if the node is traversable (not blocked).
    /// </summary>
    public bool IsTraversable() => !weight.Blocked; // weight > Blocked || weight < Blocked;
    /// <summary>
    /// Returns true if the node is traversable for the specified layer.
    /// </summary>
    public bool IsTraversable(uint layer)
    {
        if (weights == null) return !weight.Blocked; // IsTraversable();
        if (weights.TryGetValue(layer, out var value)) return !value.Blocked; // value > Blocked || value < Blocked;
        return !weight.Blocked; // weight > Blocked || weight < Blocked;
    }
    /// <summary>
    /// Gets the current weight value for this node.
    /// </summary>
    public float GetWeight() => weight.Cur;
    /// <summary>
    /// Gets the current weight value for this node and layer.
    /// </summary>
    public float GetWeight(uint layer)
    {
        if (weights == null) return weight.Cur;
        return weights.TryGetValue(layer, out var w) ? w.Cur : weight.Cur;
    }
    /// <summary>
    /// Returns true if the node has a weight for the specified layer.
    /// </summary>
    public bool HasWeight(uint layer) => weights?.ContainsKey(layer) ?? false;
    /// <summary>
    /// Resets the weight for the specified layer.
    /// </summary>
    public bool ResetWeight(uint layer)
    {
        if (layer <= 0)
        {
            weight.Reset();
            return true;
        }
            
        if (weights == null) return false;
        if (!weights.TryGetValue(layer, out var cell)) return false;
        cell.Reset();
        return true;

    }
    /// <summary>
    /// Resets all weights for this node.
    /// </summary>
    public void ResetWeights()
    {
        weight.Reset();
        if (weights == null || weights.Count <= 0) return;
        foreach (var key in weights.Keys)
        {
            weights[key].Reset();
        }
    }
    #endregion

    /// <summary>
    /// Calculates the distance from this node to the target node.
    /// </summary>
    internal abstract float DistanceToTarget(Node target);
    /// <summary>
    /// Calculates the weighted distance from this node to a neighbor node for a given layer.
    /// </summary>
    internal abstract float WeightedDistanceToNeighbor(Node neighbor, uint layer);
    /// <summary>
    /// Estimates the number of cells between this node and the target node.
    /// </summary>
    internal abstract int EstimateCellCount(Node target);
    /// <summary>
    /// Gets the position of this node in world space.
    /// </summary>
    public abstract Vector2 GetPosition();
    /// <summary>
    /// Gets the rectangle representing this node's area.
    /// </summary>
    public abstract Rect GetRect();
    /// <summary>
    /// Finds the closest traversable neighbor cell to this node.
    /// </summary>
    public Node? GetClosestTraversableCell()
    {

        if (Neighbors == null || Neighbors.Count <= 0) return null;

        HashSet<Node> lookedAt = new();
        List<Node> nextNeighbors = new();
        List<Node> currentNeighbors = new();
        foreach (var neighbor in Neighbors)
        {
            currentNeighbors.Add(neighbor);
            lookedAt.Add(neighbor);
        }
        lookedAt.Add(this);
        
        var minDisSq = float.PositiveInfinity;
        Node? closestNeighbor = null;
        while (currentNeighbors.Count > 0)
        {
            foreach (var neighbor in currentNeighbors)
            {
                if (neighbor.IsTraversable())
                {
                    var disSq = (GetPosition() - neighbor.GetPosition()).LengthSquared();
                    if (disSq < minDisSq)
                    {
                        minDisSq = disSq;
                        closestNeighbor = neighbor;
                    }
                }
                else nextNeighbors.Add(neighbor);
            }

            if (closestNeighbor != null) return closestNeighbor;
            currentNeighbors.Clear();
            GetNewNeighbors(nextNeighbors, ref lookedAt, ref currentNeighbors);
        }

        return null;
    }
    private void GetNewNeighbors(List<Node> collection, ref HashSet<Node> lookedAt, ref List<Node> newNeighbors)
    {
        foreach (var cell in collection)
        {
            if(cell.Neighbors == null || cell.Neighbors.Count <= 0) continue;
            foreach (var neighbor in cell.Neighbors)
            {
                if(lookedAt.Contains(neighbor)) continue;
                lookedAt.Add(neighbor);
                newNeighbors.Add(neighbor);
            }
        }
    }

    /// <summary>
    /// Compares this node to another node for priority queue ordering.
    /// </summary>
    public int CompareTo(Node? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
            
        int fScoreComparison = FScore.CompareTo(other.FScore);
        if (fScoreComparison != 0) return fScoreComparison;
        return H.CompareTo(other.H);
    }
}