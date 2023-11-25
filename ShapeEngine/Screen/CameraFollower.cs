using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Lib;

namespace ShapeEngine.Screen;

// public abstract class CameraFollowerBasic
// {
//     protected ShapeCamera camera;
//     public CameraFollowerBasic(ShapeCamera camera)
//     {
//         this.camera = camera;
//     }
//
//     public void ChangeCamera(ShapeCamera newCamera) => camera = newCamera;
//
//     public abstract void Update(float dt);
//     public abstract void Activate();
//     public abstract void Deactivate();
// }

//NEW Version
// public class CameraFollowerSingle2 : ICameraFollower
// {
//     public bool IsFollowing => Target != null;
//     public ICameraFollowTarget? Target { get; private set; } = null;
//     public ICameraFollowTarget? NewTarget { get; private set; } = null;
//     public bool Active { get; private set; } = false;
//
//     public float Speed = 0f;
//     public float Boundary = 0f;
//     
//     
//     private float changeTargetDuration = 0f;
//     private float changeTargetTimer = 0f;
//     private Vector2 changeTargetStartPosition = new();
//     private Vector2 curPosition = new();
//     
//     public CameraFollowerSingle2(float speed, float boundary)
//     {
//         this.Speed = speed;
//         this.Boundary = boundary;
//     }
//
//     public void Update(float dt, ShapeCamera camera)
//     {
//         if (!Active) return;
//         
//         if (NewTarget != null) //change target in progress
//         {
//             if (changeTargetTimer > 0f)
//             {
//                 changeTargetTimer -= dt;
//                 if (changeTargetTimer <= 0f)
//                 {
//                     changeTargetDuration = 0f;
//                     changeTargetTimer = 0f;
//                     Target?.FollowEnded();
//                     Target = NewTarget;
//                     NewTarget = null;
//                     curPosition = Target.GetCameraFollowPosition();
//                     Target.FollowStarted();
//                 }
//                 else
//                 {
//                     float f = 1f - (changeTargetTimer / changeTargetDuration);
//                     curPosition = changeTargetStartPosition.Lerp(NewTarget.GetCameraFollowPosition(), f);
//                 }
//                 
//                 camera.Position = curPosition;
//             }
//         }
//         else if(Target != null) //Follow current target
//         {
//             var newPos = Target.GetCameraFollowPosition();
//             if (Boundary > 0f)
//             {
//                 float disSq = (curPosition - newPos).LengthSquared();
//                 float boundarySq = Boundary * Boundary;
//                 float f = disSq < boundarySq ? disSq / boundarySq : 1f;
//                 curPosition = LerpPosition(curPosition,newPos, Speed * f, dt);
//             }
//             else curPosition = LerpPosition(curPosition, newPos, Speed, dt);
//             
//             camera.Position = curPosition;
//         }
//
//         
//     }
//     public void Activate()
//     {
//         if (Active) return;
//         Active = true;
//         if (Target != null) curPosition = Target.GetCameraFollowPosition();
//     }
//     public void Deactivate()
//     {
//         Active = false;
//     }
//
//     public void Reset()
//     {
//         NewTarget = null;
//         Target = null;
//         changeTargetDuration = 0f;
//         changeTargetTimer = 0f;
//         changeTargetStartPosition = new();
//         curPosition = new();
//     }
//     
//     public void SetTarget(ICameraFollowTarget target)
//     {
//         Target = target;
//         Target.FollowStarted();
//         curPosition = Target.GetCameraFollowPosition();
//     }
//     public void ChangeTarget(ICameraFollowTarget newTarget, float changeDuration = 1f)
//     {
//         if (changeDuration <= 0f)
//         {
//             changeTargetDuration = 0f;
//             changeTargetTimer = 0f;
//             changeTargetStartPosition = new();
//             SetTarget(newTarget);
//         }
//         else
//         {
//             changeTargetDuration = changeDuration;
//             changeTargetTimer = changeDuration;
//             changeTargetStartPosition = curPosition;
//             NewTarget = newTarget;
//         }
//     }
//     public void ClearTarget()
//     {
//         Target?.FollowEnded();
//         Target = null; 
//         NewTarget = null;
//         changeTargetDuration = 0f;
//         changeTargetTimer = 0f;
//     }
//
//     private Vector2 LerpPosition(Vector2 from, Vector2 to, float speed, float dt)
//     {
//         var w = to - from;
//         float lSquared = w.LengthSquared();
//         if (lSquared <= 0f) return from;
//         
//         float l = MathF.Sqrt(lSquared);
//         var dir = w / l;
//         var speedFrame = speed * dt;
//         if (speedFrame > l) return to;
//         var movement = dir * speedFrame;
//         return from + movement;
//     }
// }


//implement multi target following
//-> add new interface to shape camera
//update examples
public interface ICameraFollower
{
    public void Update(float dt, ShapeCamera camera);
    public void Activate();
    public void Deactivate();
    public void Reset();
}
public class CameraFollowerSingle : ICameraFollower
{
    public bool IsFollowing => Target != null;
    public ICameraFollowTarget? Target { get; private set; } = null;
    public ICameraFollowTarget? NewTarget { get; private set; } = null;
    public bool Active { get; private set; } = false;
    public RangeFloat BoundaryDis;
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

    public void Update(float dt, ShapeCamera camera)
    {
        if (!Active) return;
        
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
                
                camera.Position = curPosition;
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
                    float f = ShapeUtils.GetFactor(disSq, minBoundarySq, maxBoundarySq);
                    curPosition = LerpPosition(curPosition,newPos, Speed * f, dt);
                }
            }
            else curPosition = LerpPosition(curPosition, newPos, Speed, dt);
            
            camera.Position = curPosition;
        }

        
    }
    public void Activate()
    {
        if (Active) return;
        Active = true;
        if (Target != null) curPosition = Target.GetCameraFollowPosition();
    }
    public void Deactivate()
    {
        Active = false;
    }

    public void Reset()
    {
        NewTarget = null;
        Target = null;
        changeTargetDuration = 0f;
        changeTargetTimer = 0f;
        changeTargetStartPosition = new();
        curPosition = new();
    }
    
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

// public class CameraFollowerMulti : CameraFollowerBasic
// {
//     public CameraFollowerMulti(ShapeCamera camera) : base(camera)
//     {
//     }
//
//     public override void Update(float dt)
//     {
//         throw new NotImplementedException();
//     }
//
//     public override void Activate()
//     {
//         throw new NotImplementedException();
//     }
//
//     public override void Deactivate()
//     {
//         throw new NotImplementedException();
//     }
// }



//OLD Version
public class CameraFollower
{
    public bool IsFollowing => Target != null;
    public ICameraFollowTarget? Target { get; private set; } = null;
    public ICameraFollowTarget? NewTarget { get; private set; } = null;
    
    /// <summary>
    /// As long as the target is within the minimum boundary the camera will not follow.
    /// In between the minimum & maximum boundary the follow speed is linear interpolated.
    /// Outside of the maximum boundary dis the camera follows with 100% of the FollowSpeed;
    /// </summary>
    public RangeFloat BoundaryDis = new(0, 0);
    
    /// <summary>
    /// Speed in pixels per second that the camera follows the target.
    /// </summary>
    public float FollowSpeed { get; set; } = 1f;

    private float changeTargetDuration = 0f;
    private float changeTargetTimer = 0f;
    private Vector2 changeTargetStartPosition = new();
    public Vector2 Position { get; private set; } = new();

    //private const float positionReachedToleranceSquared = 1f;

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
        float lSquared = w.LengthSquared();
        if (lSquared <= 0f) return;
        float l = MathF.Sqrt(lSquared);// w.Length();
        float speed = ShapeMath.Clamp(FollowSpeed * dt * f, 0f, l);
        Vector2 dir = w / l;
        Vector2 movement = dir * speed;
        Position += movement;
    }
    
    internal void Update(float dt)
    {
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
                    Position = Target.GetCameraFollowPosition();
                    Target.FollowStarted();
                }
                else
                {
                    float f = 1f - (changeTargetTimer / changeTargetDuration);
                    Position = changeTargetStartPosition.Lerp(NewTarget.GetCameraFollowPosition(), f);
                }
            }
        }
        else if(Target != null) //Follow current target
        {
            Vector2 newPos = Target.GetCameraFollowPosition();
            if (BoundaryDis.Max > 0f)
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

    public void SetTarget(ICameraFollowTarget target)
    {
        Target = target;
        Target.FollowStarted();
        Position = Target.GetCameraFollowPosition();
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
            changeTargetStartPosition = Position;
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
    
}


/*
Vector2 newPos = Target.GetCameraFollowPosition();
  
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