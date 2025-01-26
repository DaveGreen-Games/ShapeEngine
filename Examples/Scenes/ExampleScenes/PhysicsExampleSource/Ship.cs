using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;

namespace Examples.Scenes.ExampleScenes.PhysicsExampleSource;

//TODO: make physics controls
// - thrust forward
// - rotate left/right
// - drag for thrusting and drag for no input
// - ship should be controlled by phsics -> use velocity and add force etc.
public class Ship : CollisionObject, ICameraFollowTarget
{
    public override void DrawGame(ScreenInfo game)
    {
        
    }

    public override void DrawGameUI(ScreenInfo gameUi)
    {
        
    }

    public void FollowStarted()
    {
        
    }

    public void FollowEnded()
    {
        
    }

    public Vector2 GetCameraFollowPosition() => Transform.Position;
}