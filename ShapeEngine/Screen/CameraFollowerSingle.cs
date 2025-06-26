using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Screen;

/// <summary>
/// Implements a camera follower that tracks a single target,
/// smoothly moving the camera towards the target's position with optional boundary and speed constraints.
/// Supports changing targets with smooth transitions and clearing the current target.
/// </summary>
/// <remarks>
/// Use this class to follow a single object in a scene,
/// with configurable speed and boundary distance for smooth camera movement.
/// Supports instant or timed target changes.
/// </remarks>
public class CameraFollowerSingle : ICameraFollower
{
    /// <summary>
    /// Indicates whether the follower is currently following a target.
    /// </summary>
    public bool IsFollowing => Target != null;
    /// <summary>
    /// The current target being followed.
    /// </summary>
    public ICameraFollowTarget? Target { get; private set; }
    /// <summary>
    /// The new target to transition to, if changing targets.
    /// </summary>
    public ICameraFollowTarget? NewTarget { get; private set; }
    
    /// <summary>
    /// The range of boundary distances used to determine how far the camera can move from the target before following.
    /// </summary>
    public ValueRange BoundaryDis;
    /// <summary>
    /// The speed at which the camera follows the target.
    /// </summary>
    public float Speed;
    
    private float changeTargetDuration;
    private float changeTargetTimer;
    private Vector2 changeTargetStartPosition;
    private Vector2 curPosition;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CameraFollowerSingle"/> class with the specified speed and boundary.
    /// </summary>
    /// <param name="speed">The speed at which to follow the target.</param>
    /// <param name="minBoundary">The minimum boundary distance.</param>
    /// <param name="maxBoundary">The maximum boundary distance.</param>
    public CameraFollowerSingle(float speed, float minBoundary, float maxBoundary)
    {
        Speed = speed;
        BoundaryDis = new(minBoundary, maxBoundary);
    }

    /// <summary>
    /// Updates the camera rectangle to follow the target, applying speed and boundary constraints.
    /// Handles smooth transitions when changing targets.
    /// </summary>
    /// <param name="dt">Delta time in seconds.</param>
    /// <param name="cameraRect">The current camera rectangle.</param>
    /// <returns>The updated camera rectangle.</returns>
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
    
    /// <summary>
    /// Resets the follower, clearing the current and new targets and resetting state.
    /// </summary>
    public void Reset()
    {
        NewTarget = null;
        Target = null;
        changeTargetDuration = 0f;
        changeTargetTimer = 0f;
        changeTargetStartPosition = new();
        curPosition = new();
    }

    /// <summary>
    /// Called when the follower is attached to a camera.
    /// </summary>
    public void OnCameraAttached() { }
    /// <summary>
    /// Called when the follower is detached from a camera.
    /// </summary>
    public void OnCameraDetached() { }
    /// <summary>
    /// Sets the current target to follow and immediately starts following it.
    /// </summary>
    /// <param name="target">The target to follow.</param>
    public void SetTarget(ICameraFollowTarget target)
    {
        Target = target;
        Target.FollowStarted();
        curPosition = Target.GetCameraFollowPosition();
    }
    /// <summary>
    /// Changes the target to follow, optionally with a smooth transition over a specified duration.
    /// </summary>
    /// <param name="newTarget">The new target to follow.</param>
    /// <param name="changeDuration">The duration of the transition in seconds. If zero or less, the change is instant.</param>
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
    /// <summary>
    /// Clears the current and new targets, stopping any following behavior.
    /// </summary>
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