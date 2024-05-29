using System.Numerics;

namespace ShapeEngine.Core.Interfaces;

public interface ICameraFollowTarget
{
    public void FollowStarted();
    public void FollowEnded();
    public Vector2 GetCameraFollowPosition();
}