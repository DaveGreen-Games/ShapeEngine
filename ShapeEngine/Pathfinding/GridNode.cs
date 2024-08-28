using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Pathfinding;

internal class GridNode : Node
{
    private static readonly float DiagonalLength = MathF.Sqrt(2f);
    
    public Rect Rect
    {
        get
        {
            var pos = parent.Bounds.TopLeft + parent.CellSize * Coordinates.ToVector2();
            return new Rect(pos, parent.CellSize, new AnchorPoint(0f));
        }
    }
    
    public readonly Grid.Coordinates Coordinates;
    private readonly Pathfinder parent;

    public GridNode(Grid.Coordinates coordinates, Pathfinder parent)
    {
        this.Coordinates = coordinates;
        this.parent = parent;
    }
    
    
    internal override float DistanceToTarget(Node target)
    {
        if (target is not GridNode rn) return float.PositiveInfinity;
        var cc = Coordinates;
        var nc = rn.Coordinates;

        var c = nc - cc;
        return c.Distance;
        
    }
    internal override float WeightedDistanceToNeighbor(Node neighbor, uint layer)
    {
        if (neighbor is not GridNode rn) return float.PositiveInfinity;
        var cc = Coordinates;
        var nc = rn.Coordinates;

        var c = nc - cc;
        if (c.Col != 0 && c.Row != 0) return DiagonalLength * neighbor.GetWeight(layer);
        return 1f * neighbor.GetWeight(layer);
    }
    internal override int EstimateCellCount(Node target)
    {
        if (target is GridNode gridNode)
        {
            var dif = Coordinates - gridNode.Coordinates;
            return dif.Distance;
        }

        return -1;
    }
    public override Vector2 GetPosition() => (parent.Bounds.TopLeft + parent.CellSize * Coordinates.ToVector2()) + (parent.CellSize / 2f);
    public override Rect GetRect() => Rect;
    
}