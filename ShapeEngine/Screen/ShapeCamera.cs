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

    /// <summary>
    /// Used to calculate a zoom adjustment factor to make the camera always display the same amount of area independent of the screen size
    /// </summary>
    public Dimensions TargetResolution { get; set; } = new(1920,1080);
    public Transform2D BaseTransform { get; set; }
    public Vector2 BasePosition
    {
        get => BaseTransform.Position;
        set => BaseTransform = BaseTransform.SetPosition(value);
    }
    public Size BaseSize
    {
        get => BaseTransform.ScaledSize;
        private set => BaseTransform = BaseTransform.SetSize(value);
    }
    public float BaseScale
    {
        get => BaseTransform.Scale;
        private set => BaseTransform = BaseTransform.SetScale(value);
    }
    public float BaseRotationDeg
    {
        get => BaseTransform.RotationRad * ShapeMath.RADTODEG;
        private set => BaseTransform = BaseTransform.SetRotationRad(value * ShapeMath.RADTODEG);
    }
    public AnchorPoint Alignement{ get; private set; } = new(0.5f);
    
    public Size BaseOffset => BaseTransform.BaseSize * Alignement.ToVector2();
    public Size Offset { get; private set; } = new();
    
    
    public float RotationDeg { get; private set; } = 0f;
    
    public float BaseZoomLevel { get; private set; } = 1f;
    public float ZoomLevel { get; private set; } = 1f;

    private float zoomAdjustment = 1f;


    public ShapeCamera() { }
    public ShapeCamera(Vector2 pos)
    {
        BaseTransform = BaseTransform.SetPosition(pos);
    }
    public ShapeCamera(Vector2 pos, AnchorPoint alignement)
    {
        BaseTransform = BaseTransform.SetPosition(pos);
        this.SetAlignement(alignement);
    }
    public ShapeCamera(Vector2 pos, AnchorPoint alignement, float zoomLevel)
    {
        BaseTransform = BaseTransform.SetPosition(pos);
        this.SetAlignement(alignement);
        this.SetZoom(zoomLevel);
    }
    public ShapeCamera(Vector2 pos, Dimensions targetResolution)
    {
        BaseTransform = BaseTransform.SetPosition(pos);
        TargetResolution = targetResolution;
    }
    public ShapeCamera(Vector2 pos, AnchorPoint alignement, Dimensions targetResolution)
    {
        BaseTransform = BaseTransform.SetPosition(pos);
        this.SetAlignement(alignement);
        TargetResolution = targetResolution;
    }
    public ShapeCamera(Vector2 pos, AnchorPoint alignement, float zoomLevel, Dimensions targetResolution)
    {
        BaseTransform = BaseTransform.SetPosition(pos);
        this.SetAlignement(alignement);
        this.SetZoom(zoomLevel);
        TargetResolution = targetResolution;
    }
    public ShapeCamera(Vector2 pos, AnchorPoint alignement, float zoomLevel, float rotationDeg)
    {
        BaseTransform = BaseTransform.SetPosition(pos);
        this.SetAlignement(alignement);
        this.SetZoom(zoomLevel);
        this.SetRotation(rotationDeg);
    }
    public ShapeCamera(Vector2 pos, float zoomLevel)
    {
        BaseTransform.SetPosition(pos);
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
        BaseTransform.Position.X - Offset.Width * ZoomFactor, 
        BaseTransform.Position.Y - Offset.Height * ZoomFactor, 
        BaseTransform.BaseSize.Width * ZoomFactor,
        BaseTransform.BaseSize.Height * ZoomFactor
    );
    public Camera2D Camera => new()
    {
        Target = BaseTransform.Position,
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
        RotationDeg = (BaseTransform.RotationRad * ShapeMath.RADTODEG) + shake.Get(ShakeRot) + cameraTweenTotalRotationDeg;
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
    internal void SetSize(Dimensions curScreenSize)
    {
        BaseTransform = BaseTransform.SetSize(curScreenSize.ToSize());

        if (TargetResolution.IsValid())
        {
            float curArea = curScreenSize.Area;
            float targetArea = TargetResolution.Area;
            zoomAdjustment = MathF.Sqrt(curArea / targetArea);
        }
        else zoomAdjustment = 1f;
    }
    // internal void SetSize(Dimensions curScreenSize, Dimensions targetDimensions)
    // {
    //     BaseTransform = BaseTransform.SetSize(curScreenSize.ToSize());
    //
    //     //VARIANT 2
    //     // float xDif = curScreenSize.Width - targetDimensions.Width;
    //     // float yDif = curScreenSize.Height - targetDimensions.Width;
    //     // if (xDif > yDif)
    //     // {
    //     //     zoomAdjustment = (float)curScreenSize.Width / (float)targetDimensions.Width;
    //     // }
    //     // else
    //     // {
    //     //     zoomAdjustment = (float)curScreenSize.Height / (float)targetDimensions.Height;
    //     // }
    //
    //     //VARIANT 1
    //     float curArea = curScreenSize.Area;
    //     float targetArea = targetDimensions.Area;
    //     zoomAdjustment = MathF.Sqrt( curArea / targetArea );
    //     
    //     //VARIANT 3
    //     // var size = Size;
    //     // var targetSize = targetDimensions.ToVector2();
    //     // var fX = targetSize.X / size.X;
    //     // var fY = targetSize.Y / size.Y;
    //     // zoomAdjustment = fX > fY ? fX : fY;
    //     
    //     
    //     // zoomAdjustment = 1f;
    // }

    private float CalculateZoomLevel(Size targetSize)
    {
        var size = BaseTransform.BaseSize / zoomAdjustment;
        var fX = 1f / (targetSize.Width / size.Width);
        var fY = 1f / (targetSize.Height / size.Height);
        return fX < fY ? fX : fY;

    }
    public void SetCameraRect(Rect newRect)
    {
        BaseTransform = BaseTransform.SetPosition(newRect.GetPoint(Alignement));
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
        BaseTransform = new Transform2D(new(), 0f);
        Alignement = new(0.5f);
        BaseZoomLevel = 1f;
    }
    
    public void Zoom(float change) => SetZoom(BaseZoomLevel + change);
    public void SetZoom(float zoomLevel)
    {
        BaseZoomLevel = zoomLevel;
        if (BaseZoomLevel > MaxZoomLevel) BaseZoomLevel = MaxZoomLevel;
        else if (BaseZoomLevel < MinZoomLevel) BaseZoomLevel = MinZoomLevel;
    }

    public void Rotate(float deg) => SetRotation((BaseTransform.RotationRad * ShapeMath.RADTODEG) + deg);
    public void SetRotation(float deg)
    {
        BaseTransform = BaseTransform.SetRotationRad(deg * ShapeMath.DEGTORAD).WrapRotationRad();
    }

    public void SetAlignement(AnchorPoint newAlignement) => Alignement = newAlignement;

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