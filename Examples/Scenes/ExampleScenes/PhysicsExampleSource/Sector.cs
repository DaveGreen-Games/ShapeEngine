using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib.Drawing;

namespace Examples.Scenes.ExampleScenes.PhysicsExampleSource;

public class Sector : CollisionObject
{
    public Sector(Rect bounds)
    {
        Transform = new Transform2D(bounds.Center, 0f, bounds.Size, 1f);
        var leftSegment = bounds.LeftSegment;
        var leftCollider = new SegmentCollider(leftSegment, Transform.Position, 0.5f);
        leftCollider.ComputeCollision = false;
        leftCollider.ComputeIntersections = false;
        leftCollider.CollisionLayer = (uint)CollisionLayers.Sector;
        AddCollider(leftCollider);
        
        var rightSegment = bounds.RightSegment;
        var rightCollider = new SegmentCollider(rightSegment, Transform.Position, 0.5f);
        rightCollider.ComputeCollision = false;
        rightCollider.ComputeIntersections = false;
        rightCollider.CollisionLayer = (uint)CollisionLayers.Sector;
        AddCollider(rightCollider);
        
        var topSegment = bounds.TopSegment;
        var topCollider = new SegmentCollider(topSegment, Transform.Position, 0.5f);
        topCollider.ComputeCollision = false;
        topCollider.ComputeIntersections = false;
        topCollider.CollisionLayer = (uint)CollisionLayers.Sector;
        AddCollider(topCollider);
        
        var bottomSegment = bounds.BottomSegment;
        var bottomCollider = new SegmentCollider(leftSegment, Transform.Position, 0.5f);
        bottomCollider.ComputeCollision = false;
        bottomCollider.ComputeIntersections = false;
        bottomCollider.CollisionLayer = (uint)CollisionLayers.Sector;
        AddCollider(bottomCollider);
        
    }
    public override void DrawGame(ScreenInfo game)
    {
        foreach (var collider in Colliders)
        {
            collider.GetSegmentShape().Draw(12f, Colors.Special, LineCapType.Capped, 12);
        }
    }

    public override void DrawGameUI(ScreenInfo gameUi)
    {
        
    }
}