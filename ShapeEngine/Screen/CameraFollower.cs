using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Lib;

namespace ShapeEngine.Screen;

public class CameraFollower
{
    public bool IsFollowing => Target != null;
    public ICameraFollowTarget? Target { get; private set; } = null;
    public ICameraFollowTarget? NewTarget { get; private set; } = null;
    public float BoundaryDis { get; set; } = 0f;
    
    /// <summary>
    /// How fast the camera keeps up with the target. 1 means it takes 1 second to go from the cameras position to the targets position.
    /// 2 means it takes half a second and 0.5f means it takes 2 seconds and so on. Negative values result in instant movement.
    /// </summary>
    public float FollowSpeed { get; set; } = 1f;
    public Vector2 Position { get; private set; } = new();

   

    private void SetNewPosition(Vector2 targetPostion, float dt)
    {
        float factor = FollowSpeed * dt;
        if (factor > 1f || factor < 0f)
        {
            Position = targetPostion;
            return;
        }
        //TODO how to make tween work here?
        //factor = STween.Tween(factor, TweenType.CUBIC_IN);
        //factor *= factor;
        
        Vector2 w = targetPostion - Position;
        float lengthSquared = w.LengthSquared();
        if (lengthSquared < 4) Position = targetPostion;
        else
        {
            float l = MathF.Sqrt(lengthSquared);
            Vector2 dir = w / l;
            Vector2 rate = dir * l * factor;
            Position += rate;
        }
        
    }
    
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
                    // Position = ImprovedLerp(Position, newPos, FollowSmoothness, dt);
                    // Position = SVec.Lerp(Position, newPos, dt * FollowSmoothness);
                    SetNewPosition(newPos, dt);
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
                        // Position = Position.Lerp(newPos, dt * FollowSmoothness);
                        // Position = ImprovedLerp(Position, newPos, FollowSmoothness, dt);
                        SetNewPosition(newPos, dt);
                    }
                }
                else SetNewPosition(newPos, dt);// Position = ImprovedLerp(Position, newPos, FollowSmoothness, dt);// Position = Position.Lerp(newPos, dt * FollowSmoothness);
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