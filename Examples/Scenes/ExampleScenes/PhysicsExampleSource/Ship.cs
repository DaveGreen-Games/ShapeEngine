using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;

namespace Examples.Scenes.ExampleScenes.PhysicsExampleSource;

public class Ship : CollisionObject, ICameraFollowTarget
{
    public override void DrawGame(ScreenInfo game)
    {
        throw new NotImplementedException();
    }

    public override void DrawGameUI(ScreenInfo gameUi)
    {
        throw new NotImplementedException();
    }

    public void FollowStarted()
    {
        throw new NotImplementedException();
    }

    public void FollowEnded()
    {
        throw new NotImplementedException();
    }

    public Vector2 GetCameraFollowPosition()
    {
        throw new NotImplementedException();
    }
}