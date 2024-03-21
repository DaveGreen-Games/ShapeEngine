using System.Numerics;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Pathfinding;

internal abstract class Node : IComparable<Node>
{
    private class NodeWeight
    {
        // public bool Blocked = false;
        private int blockCount = 0;
        private int overrideBlockCount = 0;
        private float baseValue = 0;
        private float flat = 0;
        private float bonus = 0;
        public bool Blocked => blockCount > 0;

        public void Apply(NodeValue value)
        {
            if (!value.Valid) return;
            switch (value.Type)
            {
                case NodeValueType.Flat:
                    flat += value.Value;
                    break;
                case NodeValueType.Bonus:
                    bonus += value.Value;
                    break;
                case NodeValueType.Set:
                    baseValue = value.Value;
                    break;
                case NodeValueType.SetOverrideBlock:
                    baseValue = value.Value;
                    overrideBlockCount++;
                    break;
                case NodeValueType.SetReset:
                    Reset();
                    baseValue = value.Value;
                    break;
                case NodeValueType.Block:
                    blockCount++;
                    break;
                case NodeValueType.Clear:
                    Reset();
                    break;
            }
        }

        public void Remove(NodeValue value)
        {
            if (!value.Valid) return;
            switch (value.Type)
            {
                case NodeValueType.Flat:
                    flat -= value.Value;
                    break;
                case NodeValueType.Bonus:
                    bonus -= value.Value;
                    break;
                case NodeValueType.Set:
                    if (Math.Abs(baseValue - value.Value) < 0.0001f) baseValue = 0;
                    break;
                case NodeValueType.SetOverrideBlock:
                    baseValue = 0;
                    overrideBlockCount--;
                    break;
                case NodeValueType.SetReset:
                    baseValue = 0;
                    break;
                case NodeValueType.Block:
                    blockCount--;
                    break;
            }
        }
        public float Cur => blockCount > 0 && overrideBlockCount <= 0 ? 0 : GetBaseValueFactor() * GetBonusFactor();
        // public float Cur => blockCount > 0 ? 0 : MathF.Max(baseValue + flat, 0) * GetBonusFactor();

        private float GetBaseValueFactor()
        {
            var v = baseValue + flat;
            if (v > 0) return 1f / v; //more favorable
            if (v < 0) return MathF.Abs(v); //less favorable
            return 1f; //normal
        } 
        private float GetBonusFactor()
        {
            if (bonus > 0) return 1f / bonus; //more favorable
            if (bonus < 0) return MathF.Abs(bonus); //less favorable
            return 1f; //normal
        } 
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

    internal HashSet<Node>? Neighbors = null;
    internal HashSet<Node>? Connections = null;
    internal float GScore = float.PositiveInfinity;
    internal float FScore = float.PositiveInfinity;
    internal float H = float.PositiveInfinity;

    #endregion

    #region Private

    private readonly NodeWeight weight = new();
    private Dictionary<uint, NodeWeight>? weights = null;
    
    #endregion
        

    #endregion

    #region Public

    // public Node(Grid.Coordinates coordinates, Pathfinder parent)
    // {
    //     Coordinates = coordinates;
    //     this.parent = parent;
    //     // Value = new(1f);
    // }
    public void Reset()
    {
        Connections?.Clear();
            
        weight.Reset();
        ResetWeights();
        // weights?.Clear();
    }


    #endregion
        
    #region Neighbors & Connections
    public bool HasNeighbors => Neighbors is { Count: > 0 };
    public bool HasConnections => Connections is { Count: > 0 };
        
    public bool AddNeighbor(Node node)
    {
        Neighbors ??= new();
        return Neighbors.Add(node);
    }

    public bool RemoveNeighbor(Node node)
    {
        if (Neighbors == null) return false;
        return Neighbors.Remove(node);
    }
    public void SetNeighbors(HashSet<Node>? neighbors)
    {
        Neighbors = neighbors;
    }
    public bool Connect(Node other)
    {
        if (other == this) return false;
        Connections ??= new();
        return Connections.Add(other);
    }
    public bool Disconnect(Node other)
    {
        if (other == this) return false;
        if (Connections == null) return false;
        return Connections.Remove(other);
    }
    #endregion

    #region CellValues

    public void ApplyNodeValue(NodeValue value)
    {
        if (value.Layer > 0)
        {
            if (weights == null) weights = new();
            if (!weights.ContainsKey(value.Layer))
            {
                var w = new NodeWeight();
                w.Apply(value);
                weights.Add(value.Layer, w);
            }
        }
        else weight.Apply(value);
    }
    public void RemoveNodeValue(NodeValue value)
    {
        if (value.Layer > 0)
        {
            if (weights == null) return;
            if (!weights.ContainsKey(value.Layer)) return;
            weights[value.Layer].Remove(value);
        }
        else weight.Remove(value);
    }
        
    public void ApplyNodeValues(IEnumerable<NodeValue> values)
    {
        foreach (var value in values)
        {
            ApplyNodeValue(value);
        }
    }
    public void RemoveNodeValues(IEnumerable<NodeValue> values)
    {
        foreach (var value in values)
        {
            RemoveNodeValue(value);
        }
    }


    #endregion
        
    #region Weight

    public bool IsTraversable() => !weight.Blocked; // weight > Blocked || weight < Blocked;
    public bool IsTraversable(uint layer)
    {
        if (weights == null) return !weight.Blocked; // IsTraversable();
        if (weights.TryGetValue(layer, out var value)) return !value.Blocked; // value > Blocked || value < Blocked;
        return !weight.Blocked; // weight > Blocked || weight < Blocked;
    }
    public float GetWeight() => weight.Cur;
    public float GetWeight(uint layer)
    {
        if (weights == null) return weight.Cur;
        return weights.TryGetValue(layer, out var w) ? w.Cur : weight.Cur;
    }

    public bool HasWeight(uint layer) => weights?.ContainsKey(layer) ?? false;
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

    internal abstract float DistanceToTarget(Node target);
    internal abstract float WeightedDistanceToNeighbor(Node neighbor, uint layer);

    internal abstract int EstimateCellCount(Node target);
    public abstract Vector2 GetPosition();
    public abstract Rect GetRect();
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

    
    
    public int CompareTo(Node? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
            
        int fScoreComparison = FScore.CompareTo(other.FScore);
        if (fScoreComparison != 0) return fScoreComparison;
        return H.CompareTo(other.H);
    }
}