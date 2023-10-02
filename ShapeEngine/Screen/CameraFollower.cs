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
    public RangeFloat BoundaryDis = new(0, 0);
    
    /// <summary>
    /// Speed in pixels per second that the camera follows the target.
    /// </summary>
    public float FollowSpeed { get; set; } = 1f;
    public Vector2 Position { get; private set; } = new();

    private const float positionReachedToleranceSquared = 1f;

    private void SetNewPosition(Vector2 targetPostion, float dt, float f = 1f)
    {
        //var 1
        // float speed = ShapeMath.Clamp(FollowSpeed, 0, (1f / dt) * 0.75f);
        // float factor = speed * dt * f;
        // if (factor <= 0f) return;
        // if (factor >= 1f)
        // {
        //     Position = targetPostion;
        //     return;
        // }
        //
        // Vector2 w = targetPostion - Position;
        // float lengthSquared = w.LengthSquared();
        // if (lengthSquared < positionReachedToleranceSquared)
        // {
        //     Position = targetPostion;
        //     return;
        // }
        //
        // float l = MathF.Sqrt(lengthSquared);
        // Vector2 dir = w / l;
        // Vector2 rate = dir * l * factor;
        // Position += rate;

        Vector2 w = targetPostion - Position;
        float l = w.Length();
        float speed = ShapeMath.Clamp(FollowSpeed * dt * f, 0f, l);
        Vector2 dir = w / l;
        Vector2 movement = dir * speed;
        Position += movement;
    }
    
    internal void Update(float dt)
    {
        if (Target != null)
        {
            if (NewTarget != null)
            {
                Vector2 newPos = NewTarget.GetCameraFollowPosition();
                float disSq = (newPos - Position).LengthSquared();
                if (disSq < positionReachedToleranceSquared)
                {
                    Target.FollowEnded();
                    Target = NewTarget;
                    NewTarget = null;
                    Position = newPos;
                    Target.FollowStarted();
                }
                else
                {
                    SetNewPosition(newPos, dt);
                }
            }
            else
            {
                Vector2 newPos = Target.GetCameraFollowPosition();
                if (BoundaryDis.Min > 0f || BoundaryDis.Max > 0f)
                {
                    float disSq = (Position - newPos).LengthSquared();
                    float minBoundarySq = BoundaryDis.Min * BoundaryDis.Min;
                    float maxBoundarySq = BoundaryDis.Max * BoundaryDis.Max;
                    if (disSq < minBoundarySq)
                    {
                        //dont follow
                    }
                    else if (disSq < maxBoundarySq)
                    {
                        float f = ShapeUtils.GetFactor(disSq, minBoundarySq, maxBoundarySq);
                        SetNewPosition(newPos, dt, f);
                    }
                    else 
                    {
                        SetNewPosition(newPos, dt);
                    }
                }
                else SetNewPosition(newPos, dt);
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


/*
Vector2 newPos = Target.GetCameraFollowPosition();
   //TODO fix boundary problems (maybe tween)
   //TODO try tween system
   //var 1
   if (BoundaryDis > 0f)
   {
   float disSq = (Position - newPos).LengthSquared();
   if (boundaryTouched)
   {
   SetNewPosition(newPos, dt);
   if (disSq < 4f) boundaryTouched = false;
   }
   else
   {
   float boundaryDisSq = BoundaryDis * BoundaryDis;
   if (disSq > boundaryDisSq)
   {
   // Position = Position.Lerp(newPos, dt * FollowSmoothness);
   // Position = ImprovedLerp(Position, newPos, FollowSmoothness, dt);
   boundaryTouched = true;
   SetNewPosition(newPos, dt);
   }
   }
   
   }
   
   //var 2
   // if (BoundaryDis > 0f)
   // {
   //     float disSq = (Position - newPos).LengthSquared();
   //     float boundaryDisSq = BoundaryDis * BoundaryDis;
   //     if (disSq >= boundaryDisSq) SetNewPosition(newPos, dt);
   //     else
   //     {
   //         float f = disSq / boundaryDisSq;
   //         SetNewPosition(newPos, dt * f * f);
   //     }
   // }
   else SetNewPosition(newPos, dt);// Position = ImprovedLerp(Position, newPos, FollowSmoothness, dt);// Position = Position.Lerp(newPos, dt * FollowSmoothness);
*/