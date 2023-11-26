using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;

namespace ShapeEngine.Screen;

public class CameraFollowerMulti : ICameraFollower
{
    private readonly List<ICameraFollowTarget> targets = new();
    // private Vector2 followPosition = new();

    private Rect bounds = new();
    public float TargetMargin = 25f;
    
    public CameraFollowerMulti()
    {
        
        
    }
    
    public void Reset()
    {
        
    }

    public void Draw()
    {
        bounds.DrawLines(6f, RED);
        bounds.Center.Draw(4f, GREEN);
    }
    public void Update(float dt, ShapeCamera camera)
    {
        var cameraArea = camera.Area;
        
        var newCameraRect = new Rect(cameraArea.Center, new(), new(0.5f));
        
        foreach (var target in targets)
        {
            var pos = target.GetCameraFollowPosition();
            newCameraRect = newCameraRect.Enlarge(pos);
        }

        newCameraRect = newCameraRect.ApplyMarginsAbsolute(-TargetMargin, -TargetMargin, -TargetMargin, -TargetMargin);
        
        var curSize = cameraArea.Size;
        var newSize = newCameraRect.Size;
        float f = 1f - (newSize.GetArea() / curSize.GetArea());
        camera.SetZoom(f);
        camera.Position = newCameraRect.Center;
        
        
        bounds = newCameraRect;
    }
    

    public void OnCameraAttached(ShapeCamera camera) { }

    public void OnCameraDetached(ShapeCamera camera) { }
    
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
    
}