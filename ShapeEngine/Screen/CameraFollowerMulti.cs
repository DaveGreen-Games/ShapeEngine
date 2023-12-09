using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;

namespace ShapeEngine.Screen;

public class CameraFollowerMulti : ICameraFollower
{
    public ICameraFollowTarget? CenterTarget = null;
    private readonly List<ICameraFollowTarget> targets = new();

    private Rect prevCameraRect = new();
    public float TargetMargin = 250f;
    public Vector2 MinSize = new(100, 100);
    public float LerpSpeedPosition = 0f;
    public float LerpSpeedSize = 0f;
    
    public CameraFollowerMulti()
    {
        
        
    }
    
    public void Reset()
    {
        ClearTargets();
    }

    public void DrawDebugRect()
    {
        prevCameraRect.DrawLines(6f, RED);
        prevCameraRect.Center.Draw(4f, GREEN);
        
    }
    public Rect Update(float dt, Rect cameraRect)
    {
        if (targets.Count <= 0) return cameraRect;
        // var cameraArea = camera.Area;
        
        var newCameraRect = new Rect(targets[0].GetCameraFollowPosition(), MinSize, new(0.5f));
        
        for (var i = 1; i < targets.Count; i++)
        {
            var target = targets[i];
            var pos = target.GetCameraFollowPosition();
            newCameraRect = newCameraRect.Enlarge(pos);
        }

        if (CenterTarget != null)
        {
            var pos = CenterTarget.GetCameraFollowPosition();
            float left = newCameraRect.Left;
            float right = newCameraRect.Right;
            float top = newCameraRect.Top;
            float bottom = newCameraRect.Bottom;

            float leftDif = MathF.Abs( left - pos.X );
            float rightDif = MathF.Abs( right - pos.X);
            float topDif = MathF.Abs( top - pos.Y);
            float bottomDif = MathF.Abs( bottom - pos.Y);

            float width = MathF.Max(leftDif, rightDif) * 2;
            float height = MathF.Max(topDif, bottomDif) * 2;

            newCameraRect = new(pos, new Vector2(width, height), new Vector2(0.5f, 0.5f));
        }

        newCameraRect = newCameraRect.ApplyMarginsAbsolute(-TargetMargin, -TargetMargin, -TargetMargin, -TargetMargin);
        if (LerpSpeedPosition <= 0f && LerpSpeedSize <= 0f)
        {
            prevCameraRect = newCameraRect;
            return newCameraRect;
        }
        // var finalPosition = prevCameraRect.Center.LerpTowards(newCameraRect.Center, 0.25f, dt );
        // var finalSize = prevCameraRect.Size.LerpTowards(newCameraRect.Size, 0.25f, dt);
        
        var finalPosition = LerpSpeedPosition > 0 ? prevCameraRect.Center.MoveTowards(newCameraRect.Center, LerpSpeedPosition * dt) : newCameraRect.Center;
        var finalSize = LerpSpeedSize > 0 ? prevCameraRect.Size.MoveTowards(newCameraRect.Size, LerpSpeedSize * dt) : newCameraRect.Size;
        prevCameraRect = new(finalPosition, finalSize, new(0.5f));
        return prevCameraRect;
    }


    public void OnCameraAttached()
    {
        
    }

    public void OnCameraDetached()
    {
        
    }
    
    public bool AddTarget(ICameraFollowTarget newTarget)
    {
        if (targets.Contains(newTarget)) return false;
        targets.Add(newTarget);
        return true;
    }

    public bool RemoveTarget(ICameraFollowTarget target)
    {
        return targets.Remove(target);
    }

    public void ClearTargets()
    {
        targets.Clear();
    }
}