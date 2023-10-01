using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Timing;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Screen;

public sealed class ShapeCamera
{
    public static float MinZoomLevel = 0.1f;
    public static float MaxZoomLevel = 10f;
    
    public float Intensity = 1.0f;

    private const int ShakeX = 0;
    private const int ShakeY = 1;
    private const int ShakeZoom = 2;
    private const int ShakeRot = 3;
    private Shake shake = new(4);
    
    private Sequencer<ICameraTween> cameraTweens = new();
    private float cameraTweenTotalRotationDeg = 0f;
    private float cameraTweenTotalScale = 1f;
    private Vector2 cameraTweenTotalOffset = new();

    public CameraFollower Follower { get; private set; } = new();
    
    public Vector2 Position { get; set; } = new();
    
    public Vector2 Size { get; private set; } = new();
    public Vector2 Alignement{ get; private set; } = new(0.5f);
    
    public Vector2 BaseOffset => Size * Alignement;
    public Vector2 Offset { get; private set; } = new();
    
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
    }

    public void Deactive()
    {
        cameraTweens.OnItemUpdated -= OnCameraTweenUpdated;
        Follower.ClearTarget();
    }
    public Rect Area => new
    (
        Position.X - Offset.X * ZoomFactor, 
        Position.Y - Offset.Y * ZoomFactor, 
        Size.X * ZoomFactor,
        Size.Y * ZoomFactor
    );
    public Camera2D Camera => new()
    {
        target = Position,
        offset = Offset,
        zoom = ZoomLevel,
        rotation = RotationDeg
    };

    public float ZoomFactor => 1f / ZoomLevel;

    
    internal void Update(float dt)
    {
        Follower.Update(dt);
        cameraTweens.Update(dt);
        shake.Update(dt);
        
        Vector2 shakeOffset = new(shake.Get(ShakeX), shake.Get(ShakeY));
        
        Offset = BaseOffset + shakeOffset + cameraTweenTotalOffset;
        RotationDeg = BaseRotationDeg + shake.Get(ShakeRot) + cameraTweenTotalRotationDeg;
        ZoomLevel = (shake.Get(ShakeZoom) + BaseZoomLevel) / cameraTweenTotalScale;
        ZoomLevel *= zoomAdjustment;
        cameraTweenTotalOffset = new(0f);
        cameraTweenTotalRotationDeg = 0f;
        cameraTweenTotalScale = 1f;

        if (Follower.IsFollowing) Position = Follower.Position;
    }
    internal void SetSize(Dimensions curScreenSize, Dimensions targetDimensions)
    {
        Size = curScreenSize.ToVector2();

        float curArea = curScreenSize.Area;
        float targetArea = targetDimensions.Area;

        zoomAdjustment = MathF.Sqrt( curArea / targetArea );
    }

    public bool HasSequences() => cameraTweens.HasSequences();
    public bool HasTweenSequence(uint id) => cameraTweens.HasSequence(id);
    public uint StartTweenSequence(params ICameraTween[] tweens) => cameraTweens.StartSequence(tweens);
    public void StopTweenSequence(uint id) => cameraTweens.CancelSequence(id);
    public void StopTweens() => cameraTweens.Stop();
    
    public void Reset()
    {
        Follower.ClearTarget();
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
        var returnValue = GetScreenToWorld2D(pos, Camera);
        if (returnValue.IsNan()) return pos;
        else return returnValue;
    }

    public Vector2 WorldToScreen(Vector2 pos)
    {
        var returnValue = GetWorldToScreen2D(pos, Camera);
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
        cameraTweenTotalScale *= tween.GetScale();

    }
    private static float Wrap(float value, float min, float max) => value - (max - min) * MathF.Floor((float) (( value -  min) / ( max -  min)));
    
}