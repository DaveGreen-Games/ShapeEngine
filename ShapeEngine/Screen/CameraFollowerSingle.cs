using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;

namespace ShapeEngine.Screen;

public class CameraFollowerSingle : ICameraFollower
{
    public bool IsFollowing => Target != null;
    public ICameraFollowTarget? Target { get; private set; } = null;
    public ICameraFollowTarget? NewTarget { get; private set; } = null;
    // public bool Active { get; private set; } = false;
    public ValueRange BoundaryDis;
    public float Speed;
    
    private float changeTargetDuration = 0f;
    private float changeTargetTimer = 0f;
    private Vector2 changeTargetStartPosition = new();
    private Vector2 curPosition = new();
    
    public CameraFollowerSingle(float speed, float minBoundary, float maxBoundary)
    {
        Speed = speed;
        BoundaryDis = new(minBoundary, maxBoundary);
    }

    public Rect Update(float dt, Rect cameraRect)
    {
        // if (!Active) return;
        
        if (NewTarget != null) //change target in progress
        {
            if (changeTargetTimer > 0f)
            {
                changeTargetTimer -= dt;
                if (changeTargetTimer <= 0f)
                {
                    changeTargetDuration = 0f;
                    changeTargetTimer = 0f;
                    Target?.FollowEnded();
                    Target = NewTarget;
                    NewTarget = null;
                    curPosition = Target.GetCameraFollowPosition();
                    Target.FollowStarted();
                }
                else
                {
                    float f = 1f - (changeTargetTimer / changeTargetDuration);
                    curPosition = changeTargetStartPosition.Lerp(NewTarget.GetCameraFollowPosition(), f);
                }
                
                //camera.Position = curPosition;
                return new(curPosition, cameraRect.Size, new(0.5f));
            }
        }
        else if(Target != null && Speed > 0) //Follow current target
        {
            var newPos = Target.GetCameraFollowPosition();
            if (BoundaryDis.Max > 0f)
            {
                float disSq = (curPosition - newPos).LengthSquared();
                float minBoundarySq =  BoundaryDis.Min <= 0f ? 0f : BoundaryDis.Min * BoundaryDis.Min;
                float maxBoundarySq = BoundaryDis.Max * BoundaryDis.Max;
                if (disSq > minBoundarySq)
                {
                    float f = ShapeMath.GetFactor(disSq, minBoundarySq, maxBoundarySq);
                    curPosition = LerpPosition(curPosition,newPos, Speed * f, dt);
                }
            }
            else curPosition = LerpPosition(curPosition, newPos, Speed, dt);
            
            //camera.Position = curPosition;
            return new(curPosition, cameraRect.Size, new(0.5f));
        }

        return cameraRect;
    }
    
    // void ICameraFollower.Update(float dt, ShapeCamera camera)
    // {
    //     Update(dt, camera);
    // }

    public void Reset()
    {
        NewTarget = null;
        Target = null;
        changeTargetDuration = 0f;
        changeTargetTimer = 0f;
        changeTargetStartPosition = new();
        curPosition = new();
    }

    public void OnCameraAttached() { }
    
    public void OnCameraDetached() { }
   

    public void SetTarget(ICameraFollowTarget target)
    {
        Target = target;
        Target.FollowStarted();
        curPosition = Target.GetCameraFollowPosition();
    }
    public void ChangeTarget(ICameraFollowTarget newTarget, float changeDuration = 1f)
    {
        if (changeDuration <= 0f)
        {
            changeTargetDuration = 0f;
            changeTargetTimer = 0f;
            changeTargetStartPosition = new();
            SetTarget(newTarget);
        }
        else
        {
            changeTargetDuration = changeDuration;
            changeTargetTimer = changeDuration;
            changeTargetStartPosition = curPosition;
            NewTarget = newTarget;
        }
    }
    public void ClearTarget()
    {
        Target?.FollowEnded();
        Target = null; 
        NewTarget = null;
        changeTargetDuration = 0f;
        changeTargetTimer = 0f;
    }

    private Vector2 LerpPosition(Vector2 from, Vector2 to, float speed, float dt)
    {
        var w = to - from;
        float lSquared = w.LengthSquared();
        if (lSquared <= 0f) return from;
        
        float l = MathF.Sqrt(lSquared);
        var dir = w / l;
        var speedFrame = speed * dt;
        if (speedFrame > l) return to;
        var movement = dir * speedFrame;
        return from + movement;
    }
    
}