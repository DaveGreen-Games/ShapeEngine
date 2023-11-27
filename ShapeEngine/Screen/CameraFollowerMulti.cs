using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;

namespace ShapeEngine.Screen;

public class CameraFollowerMulti : ICameraFollower
{
    private readonly List<ICameraFollowTarget> targets = new();

    private Rect bounds = new();
    private ShapeCamera? camera = null;
    
    public float TargetMargin = 250f;
    public Vector2 MinSize = new(100, 100);
    public float LerpSpeed = 0f; //instant
    
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
        // targetBounds.DrawLines(4f, WHITE);
        
    }
    public void Update(float dt, ShapeCamera camera)
    {
        if (targets.Count <= 0) return;
        // var cameraArea = camera.Area;
        
        var newCameraRect = new Rect(targets[0].GetCameraFollowPosition(), MinSize, new(0.5f));
        for (var i = 1; i < targets.Count; i++)
        {
            var target = targets[i];
            var pos = target.GetCameraFollowPosition();
            newCameraRect = newCameraRect.Enlarge(pos);
        }

        newCameraRect = newCameraRect.ApplyMarginsAbsolute(-TargetMargin, -TargetMargin, -TargetMargin, -TargetMargin);
        camera.SetCameraRect(newCameraRect);
        bounds = newCameraRect;
        // VARIANT 1
        // var curSize = camera.SizeRaw;
        // var newSize = newCameraRect.Size;
        // targetBounds = new(newCameraRect.Center, curSize, new(0.5f));
        // float xDif = newSize.X - curSize.X;
        // float yDif = newSize.Y - curSize.Y;
        // if (xDif > yDif)
        // {
        //      float newZoomLevel = 1f / (newSize.X / curSize.X);
        //      //prevZoomLevel = Lerp(prevZoomLevel, newZoomLevel, dt);
        //      prevZoomLevel = newZoomLevel;
        //      camera.SetZoom(prevZoomLevel);
        //      hor = true;
        // }
        // else
        // {
        //      float newZoomLevel = 1f / (newSize.Y / curSize.Y);
        //      //prevZoomLevel = Lerp(prevZoomLevel, newZoomLevel, dt);
        //      prevZoomLevel = newZoomLevel;
        //      camera.SetZoom(prevZoomLevel);
        //      hor = false;
        // }
        
        
        //// VARIANT 2
        // var curSize = camera.SizeRaw;
        // var newSize = newCameraRect.Size;
        // float newZoomLevel = MathF.Sqrt( 1f / (newSize.GetArea() / curSize.GetArea()) );
        // zoomLevel = newZoomLevel;
        // camera.SetZoom(newZoomLevel);
        
        
        
        // var curSize = cameraArea.Size;
        // var newSize = newCameraRect.Size;
        // float f = 1f - (newSize.GetArea() / curSize.GetArea());
        // camera.SetZoom(f);
        
        
        // camera.Position = newCameraRect.Center;
        
        
        
    }


    public void OnCameraAttached(ShapeCamera camera)
    {
        this.camera = camera; }

    public void OnCameraDetached(ShapeCamera camera)
    {
        this.camera = null; }
    
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