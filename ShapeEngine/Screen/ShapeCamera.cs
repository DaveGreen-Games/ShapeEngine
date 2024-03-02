using System.Numerics;
using Raylib_cs;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Timing;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Screen;

// public readonly struct CameraFollowInformation
// {
//     public readonly bool Valid;
//     public readonly Vector2 Position;
//     public readonly Vector2 Alignement;
//     public readonly float ZoomFactor;
//
//     public CameraFollowInformation()
//     {
//         this.Valid = false;
//         this.Position = new();
//         this.Alignement = new();
//         this.ZoomFactor = 1f;
//     }
//
//     public CameraFollowInformation(Vector2 position, Vector2 alignement, float zoomFactor)
//     {
//         this.Valid = true;
//         this.Position = position;
//         this.Alignement = alignement;
//         this.ZoomFactor = zoomFactor;
//     }
// }
// public interface ICameraFollower
// {
//     public CameraFollowInformation GetFollowInformation();
//     public void Update(float dt);
//     public void Clear();
// }
//
// public class CameraFollowerSingle : ICameraFollower
// {
//     public CameraFollowInformation GetFollowInformation()
//     {
//         throw new NotImplementedException();
//     }
//
//     public void Update(float dt)
//     {
//         throw new NotImplementedException();
//     }
//
//     public void Clear()
//     {
//         throw new NotImplementedException();
//     }
// }
//
// public class CameraFollowerMulti : ICameraFollower
// {
//     public CameraFollowInformation GetFollowInformation()
//     {
//         throw new NotImplementedException();
//     }
//
//     public void Update(float dt)
//     {
//         throw new NotImplementedException();
//     }
//
//     public void Clear()
//     {
//         throw new NotImplementedException();
//     }
// }


public sealed class ShapeCamera
{
    public static float MinZoomLevel = 0.05f;
    public static float MaxZoomLevel = 20f;
    
    public float Intensity = 1.0f;

    private const int ShakeX = 0;
    private const int ShakeY = 1;
    private const int ShakeZoom = 2;
    private const int ShakeRot = 3;
    private Shake shake = new(4);
    
    private Sequencer<ICameraTween> cameraTweens = new();
    private float cameraTweenTotalRotationDeg = 0f;
    private float cameraTweenTotalZoomFactor = 1f;
    private Size cameraTweenTotalOffset = new();

    private ICameraFollower? follower = null;
    public ICameraFollower? Follower
    {
        get => follower;
        set
        {
            follower?.OnCameraDetached();
            follower = value;
            follower?.OnCameraAttached();
        }
    }
    public Vector2 Position { get; set; } = new();
    
    public Size Size { get; private set; } = new();
    // public Vector2 SizeRaw => Size / zoomAdjustment;
    // private Vector2 sizeRaw => Size / zoomAdjustment;
    public Vector2 Alignement{ get; private set; } = new(0.5f);
    
    public Size BaseOffset => Size * Alignement;
    public Size Offset { get; private set; } = new();
    
    public float BaseRotationDeg { get; private set; } = 0f;
    public float RotationDeg { get; private set; } = 0f;
    
    public float BaseZoomLevel { get; private set; } = 1f;
    public float ZoomLevel { get; private set; } = 1f;

    private float zoomAdjustment = 1f;


    public ShapeCamera() { }
    public ShapeCamera(Vector2 pos)
    {
        this.Position = pos;
    }
    public ShapeCamera(Vector2 pos, Vector2 alignement)
    {
        this.Position = pos;
        this.SetAlignement(alignement);
    }
    public ShapeCamera(Vector2 pos, Vector2 alignement, float zoomLevel)
    {
        this.Position = pos;
        this.SetAlignement(alignement);
        this.SetZoom(zoomLevel);
    }
    public ShapeCamera(Vector2 pos, Vector2 alignement, float zoomLevel, float rotationDeg)
    {
        this.Position = pos;
        this.SetAlignement(alignement);
        this.SetZoom(zoomLevel);
        this.SetRotation(rotationDeg);
    }
    public ShapeCamera(Vector2 pos, float zoomLevel)
    {
        this.Position = pos;
        this.SetZoom(zoomLevel);
    }

    public void Activate()
    {
        cameraTweens.OnItemUpdated += OnCameraTweenUpdated;
        // Follower?.Activate();
    }

    public void Deactivate()
    {
        cameraTweens.OnItemUpdated -= OnCameraTweenUpdated;
        // Follower?.Deactivate();
        // Follower.ClearTarget();
    }
    public Rect Area => new
    (
        Position.X - Offset.Width * ZoomFactor, 
        Position.Y - Offset.Height * ZoomFactor, 
        Size.Width * ZoomFactor,
        Size.Height * ZoomFactor
    );
    public Camera2D Camera => new()
    {
        Target = Position,
        Offset = Offset.ToVector2(),
        Zoom = ZoomLevel,
        Rotation = RotationDeg
    };

    public float ZoomFactor => 1f / ZoomLevel;

    
    internal void Update(float dt)
    {
        cameraTweens.Update(dt);
        shake.Update(dt);
        
        var shakeOffset = new Size(shake.Get(ShakeX), shake.Get(ShakeY));
        
        Offset = BaseOffset + shakeOffset + cameraTweenTotalOffset;
        RotationDeg = BaseRotationDeg + shake.Get(ShakeRot) + cameraTweenTotalRotationDeg;
        ZoomLevel = (shake.Get(ShakeZoom) + BaseZoomLevel) * cameraTweenTotalZoomFactor;
        ZoomLevel *= zoomAdjustment;
        cameraTweenTotalOffset = new(0f);
        cameraTweenTotalRotationDeg = 0f;
        cameraTweenTotalZoomFactor = 1f;

        if (follower != null)
        {
            // var area = new Rect
            // (
            //     Position.X - Offset.X, 
            //     Position.Y - Offset.Y, 
            //     Size.X,
            //     Size.Y
            // );
            var rect = follower.Update(dt, Area);
            SetCameraRect(rect);
        }
    }
    internal void SetSize(Dimensions curScreenSize, Dimensions targetDimensions)
    {
        Size = curScreenSize.ToSize();

        //VARIANT 2
        // float xDif = curScreenSize.Width - targetDimensions.Width;
        // float yDif = curScreenSize.Height - targetDimensions.Width;
        // if (xDif > yDif)
        // {
        //     zoomAdjustment = (float)curScreenSize.Width / (float)targetDimensions.Width;
        // }
        // else
        // {
        //     zoomAdjustment = (float)curScreenSize.Height / (float)targetDimensions.Height;
        // }

        //VARIANT 1
        float curArea = curScreenSize.Area;
        float targetArea = targetDimensions.Area;
        zoomAdjustment = MathF.Sqrt( curArea / targetArea );
        
        //VARIANT 3
        // var size = Size;
        // var targetSize = targetDimensions.ToVector2();
        // var fX = targetSize.X / size.X;
        // var fY = targetSize.Y / size.Y;
        // zoomAdjustment = fX > fY ? fX : fY;
        
        
        // zoomAdjustment = 1f;
    }

    private float CalculateZoomLevel(Size targetSize)
    {
        var size = Size / zoomAdjustment;
        var fX = 1f / (targetSize.Width / size.Width);
        var fY = 1f / (targetSize.Height / size.Height);
        return fX < fY ? fX : fY;

    }
    public void SetCameraRect(Rect newRect)
    {
        Position = newRect.GetPoint(Alignement);
        if (newRect.Size != Area.Size) SetZoom( CalculateZoomLevel(newRect.Size) );
    }
    
    public bool HasSequences() => cameraTweens.HasSequences();
    public bool HasTweenSequence(uint id) => cameraTweens.HasSequence(id);
    public uint StartTweenSequence(params ICameraTween[] tweens) => cameraTweens.StartSequence(tweens);
    public void StopTweenSequence(uint id) => cameraTweens.CancelSequence(id);
    public void StopTweens() => cameraTweens.Stop();
    
    public void Reset()
    {
        Follower?.Reset();
        Position = new();
        Alignement = new(0.5f);
        BaseZoomLevel = 1f;
        BaseRotationDeg = 0f;
    }
    
    public void Zoom(float change) => SetZoom(BaseZoomLevel + change);
    public void SetZoom(float zoomLevel)
    {
        BaseZoomLevel = zoomLevel;
        if (BaseZoomLevel > MaxZoomLevel) BaseZoomLevel = MaxZoomLevel;
        else if (BaseZoomLevel < MinZoomLevel) BaseZoomLevel = MinZoomLevel;
    }

    public void Rotate(float deg) => SetRotation(BaseRotationDeg + deg);
    public void SetRotation(float deg)
    {
        BaseRotationDeg = deg;
        //RotationDeg = Wrap(RotationDeg, 0f, 360f);
        BaseRotationDeg = ShapeMath.WrapAngleDeg(BaseRotationDeg);
    }

    public void SetAlignement(Vector2 newAlignement) => Alignement = Vector2.Clamp(newAlignement, Vector2.Zero, Vector2.One);

    public Vector2 ScreenToWorld(Vector2 pos)
    {
        var returnValue = Raylib.GetScreenToWorld2D(pos, Camera);
        if (returnValue.IsNan()) return pos;
        else return returnValue;
    }

    public Vector2 WorldToScreen(Vector2 pos)
    {
        var returnValue = Raylib.GetWorldToScreen2D(pos, Camera);
        if (returnValue.IsNan()) return pos;
        else return returnValue;
    }

    public Rect ScreenToWorld(Rect r)
    {
        var newPos = ScreenToWorld(r.TopLeft);
        var newSize = r.Size * ZoomFactor;
        return new(newPos, newSize, new(0f));
    }

    public Rect WorldToScreen(Rect r)
    {
        var newPos = WorldToScreen(r.TopLeft);
        var newSize = r.Size / ZoomFactor;
        return new(newPos, newSize, new(0f));
    }
    
    
    public void Shake(float duration, Vector2 strength, float zoomStrength = 0f, float rotStrength = 0f, float smoothness = 0.75f)
    {
        if (Intensity <= 0) return;
        shake.Start
        (
            duration,
            smoothness,
            strength.X * Intensity,
            strength.Y * Intensity,
            zoomStrength * Intensity,
            rotStrength * Intensity
        );
    }
    public void StopShake() { shake.Stop(); }
    
    private void OnCameraTweenUpdated(ICameraTween tween)
    {
        cameraTweenTotalOffset += tween.GetOffset();
        cameraTweenTotalRotationDeg += tween.GetRotationDeg();
        cameraTweenTotalZoomFactor *= tween.GetZoomFactor();
        cameraTweenTotalZoomFactor = MathF.Max(cameraTweenTotalZoomFactor, 0.05f); //cant be zero!!!
    }
    private static float Wrap(float value, float min, float max) => value - (max - min) * MathF.Floor((float) (( value -  min) / ( max -  min)));
    
}