using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Lib;

namespace ShapeEngine.Screen;

public class CameraFollower
{
    public bool IsFollowing => Target != null;
    public ICameraFollowTarget? Target { get; private set; } = null;
    public ICameraFollowTarget? NewTarget { get; private set; } = null;
    public float BoundaryDis { get; set; } = 0f;
    public float FollowSmoothness { get; set; } = 1f;
    public Vector2 Position { get; private set; } = new();

    
    internal void Update(float dt)
    {
        if (Target != null)
        {
            if (NewTarget != null)
            {
                Vector2 newPos = NewTarget.GetCameraFollowPosition();
                float disSq = (newPos - Position).LengthSquared();
                if (disSq < 25)
                {
                    Target.FollowEnded();
                    Target = NewTarget;
                    NewTarget = null;
                    Position = newPos;
                    Target.FollowStarted();
                }
                else
                {
                    Position = SVec.Lerp(Position, newPos, dt * FollowSmoothness);
                }
            }
            else
            {
                Vector2 newPos = Target.GetCameraFollowPosition();
                if (BoundaryDis > 0f)
                {
                    float boundaryDisSq = BoundaryDis * BoundaryDis;
                    float disSq = (Position - newPos).LengthSquared();
                    if (disSq > boundaryDisSq)
                    {
                        Position = Position.Lerp(newPos, dt * FollowSmoothness);
                    }
                }
                else Position = Position.Lerp(newPos, dt * FollowSmoothness);
            }
        }
    }

    public void SetTarget(ICameraFollowTarget target)
    {
        Target = target;
        Target.FollowStarted();
        Position = Target.GetCameraFollowPosition();
    }
    public void ChangeTarget(ICameraFollowTarget newTarget)
    {
        if (Target == null)
        {
            SetTarget(newTarget);
        }
        else
        {
            NewTarget = newTarget;
        }
    }
    public void ClearTarget()
    {
        Target?.FollowEnded();
        Target = null; 
        NewTarget = null;
    }
}