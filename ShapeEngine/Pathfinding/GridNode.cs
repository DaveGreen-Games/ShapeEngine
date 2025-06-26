using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Pathfinding;

/// <summary>
/// Represents a node in a grid for pathfinding, with coordinates and a reference to its parent Pathfinder.
/// </summary>
internal class GridNode : Node
{
    private static readonly float DiagonalLength = MathF.Sqrt(2f);
    
    /// <summary>
    /// Gets the rectangle representing this grid node's area.
    /// </summary>
    public Rect Rect
    {
        get
        {
            var pos = parent.Bounds.TopLeft + parent.CellSize * Coordinates.ToVector2();
            return new Rect(pos, parent.CellSize, new AnchorPoint(0f));
        }
    }
    /// <summary>
    /// The coordinates of this node in the grid.
    /// </summary>
    public readonly Grid.Coordinates Coordinates;
    /// <summary>
    /// The parent Pathfinder that owns this node.
    /// </summary>
    private readonly Pathfinder parent;

    /// <summary>
    /// Initializes a new instance of the <see cref="GridNode"/> class.
    /// </summary>
    /// <param name="coordinates">The grid coordinates of the node.</param>
    /// <param name="parent">The parent Pathfinder.</param>
    public GridNode(Grid.Coordinates coordinates, Pathfinder parent)
    {
        this.Coordinates = coordinates;
        this.parent = parent;
    }
    
    
    /// <inheritdoc/>
    internal override float DistanceToTarget(Node target)
    {
        if (target is not GridNode rn) return float.PositiveInfinity;
        var cc = Coordinates;
        var nc = rn.Coordinates;

        var c = nc - cc;
        return c.Distance;
        
    }
    /// <inheritdoc/>
    internal override float WeightedDistanceToNeighbor(Node neighbor, uint layer)
    {
        if (neighbor is not GridNode rn) return float.PositiveInfinity;
        var cc = Coordinates;
        var nc = rn.Coordinates;

        var c = nc - cc;
        if (c.Col != 0 && c.Row != 0) return DiagonalLength * neighbor.GetWeight(layer);
        return 1f * neighbor.GetWeight(layer);
    }
    /// <inheritdoc/>
    internal override int EstimateCellCount(Node target)
    {
        if (target is GridNode gridNode)
        {
            var dif = Coordinates - gridNode.Coordinates;
            return dif.Distance;
        }

        return -1;
    }
    /// <inheritdoc/>
    public override Vector2 GetPosition() => (parent.Bounds.TopLeft + parent.CellSize * Coordinates.ToVector2()) + (parent.CellSize / 2f);
    /// <inheritdoc/>
    public override Rect GetRect() => Rect;
    
}