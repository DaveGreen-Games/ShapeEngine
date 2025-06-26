using System.Numerics;
using Raylib_cs;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;
using ShapeEngine.Timing;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Screen;

/// <summary>
/// 2D camera for view transformations, zoom, rotation, shake, and following targets.
/// Supports coordinate conversion, tweening, and area management.
/// </summary>
public sealed class ShapeCamera
{
    /// <summary>
    /// Minimum allowed zoom level.
    /// </summary>
    public static float MinZoomLevel = 0.05f;
    
    /// <summary>
    /// Maximum allowed zoom level.
    /// </summary>
    public static float MaxZoomLevel = 20f;
    
    /// <summary>
    /// Camera shake intensity multiplier.
    /// </summary>
    public float Intensity = 1.0f;
    
    /// <summary>
    /// Shake X-axis index.
    /// </summary>
    private const int ShakeX = 0;
    /// <summary>
    /// Shake Y-axis index.
    /// </summary>
    private const int ShakeY = 1;
    /// <summary>
    /// Shake zoom index.
    /// </summary>
    private const int ShakeZoom = 2;
    /// <summary>
    /// Shake rotation index.
    /// </summary>
    private const int ShakeRot = 3;
    
    /// <summary>
    /// Shake effect handler.
    /// </summary>
    private Shake shake = new(4);
    
    /// <summary>
    /// Tween sequencer for smooth camera transitions.
    /// </summary>
    private Sequencer<ICameraTween> cameraTweens = new();
   
    /// <summary>
    /// Total rotation (degrees) from tweens this update.
    /// </summary>
    private float cameraTweenTotalRotationDeg;
    /// <summary>
    /// Total zoom factor from tweens this update.
    /// </summary>
    private float cameraTweenTotalZoomFactor = 1f;
    /// <summary>
    /// Total offset from tweens this update.
    /// </summary>
    private Size cameraTweenTotalOffset = new();

    /// <summary>
    /// Current camera follower.
    /// </summary>
    private ICameraFollower? follower;
    
    /// <summary>
    /// Gets or sets the camera follower. Detaches previous and attaches new follower.
    /// </summary>
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
    /// Target resolution for zoom adjustment.
    /// <remarks>
    /// The zoom adjustment factor is used
    /// to make the camera always display the same amount of area independent of the screen size.
    /// </remarks>
    /// </summary>
    public Dimensions TargetResolution { get; set; } = new(1920,1080);

    /// <summary>
    /// Camera's base transform (position, size, rotation, scale).
    /// </summary>
    public Transform2D BaseTransform { get; set; }

    /// <summary>
    /// Camera's base position.
    /// </summary>
    public Vector2 BasePosition
    {
        get => BaseTransform.Position;
        set => BaseTransform = BaseTransform.SetPosition(value);
    }

    /// <summary>
    /// Camera's base size.
    /// </summary>
    public Size BaseSize
    {
        get => BaseTransform.ScaledSize;
        private set => BaseTransform = BaseTransform.SetSize(value);
    }

    /// <summary>
    /// Camera's base scale.
    /// </summary>
    public float BaseScale
    {
        get => BaseTransform.Scale;
        private set => BaseTransform = BaseTransform.SetScale(value);
    }

    /// <summary>
    /// Camera's base rotation in degrees.
    /// </summary>
    public float BaseRotationDeg
    {
        get => BaseTransform.RotationRad * ShapeMath.RADTODEG;
        private set => BaseTransform = BaseTransform.SetRotationRad(value * ShapeMath.RADTODEG);
    }

    /// <summary>
    /// Camera alignment anchor point.
    /// </summary>
    public AnchorPoint Alignement{ get; private set; } = new(0.5f);

    /// <summary>
    /// Offset from base size and alignment.
    /// </summary>
    public Size BaseOffset => BaseTransform.BaseSize * Alignement.ToVector2();

    /// <summary>
    /// Current camera offset (base + shake + tween).
    /// </summary>
    public Size Offset { get; private set; } = new();

    /// <summary>
    /// Current camera rotation in degrees.
    /// </summary>
    public float RotationDeg { get; private set; }

    /// <summary>
    /// Base zoom level before shake/tween.
    /// </summary>
    public float BaseZoomLevel { get; private set; } = 1f;

    /// <summary>
    /// Current zoom level (includes shake/tween/adjustment).
    /// </summary>
    public float ZoomLevel { get; private set; } = 1f;

    private float zoomAdjustment = 1f;

    /// <summary>
    /// Initializes a new camera at default position.
    /// </summary>
    public ShapeCamera() { }

    /// <summary>
    /// Initializes a new camera at the specified position.
    /// </summary>
    /// <param name="pos">Base position.</param>
    public ShapeCamera(Vector2 pos)
    {
        BaseTransform = BaseTransform.SetPosition(pos);
    }

    /// <summary>
    /// Initializes a new camera at the specified position and alignment.
    /// </summary>
    /// <param name="pos">Base position.</param>
    /// <param name="alignement">Anchor alignment.</param>
    public ShapeCamera(Vector2 pos, AnchorPoint alignement)
    {
        BaseTransform = BaseTransform.SetPosition(pos);
        this.SetAlignement(alignement);
    }

    /// <summary>
    /// Initializes a new camera at the specified position, alignment, and zoom level.
    /// </summary>
    /// <param name="pos">Base position.</param>
    /// <param name="alignement">Anchor alignment.</param>
    /// <param name="zoomLevel">Initial zoom level.</param>
    public ShapeCamera(Vector2 pos, AnchorPoint alignement, float zoomLevel)
    {
        BaseTransform = BaseTransform.SetPosition(pos);
        this.SetAlignement(alignement);
        this.SetZoom(zoomLevel);
    }

    /// <summary>
    /// Initializes a new camera at the specified position and target resolution.
    /// </summary>
    /// <param name="pos">Base position.</param>
    /// <param name="targetResolution">Target resolution for zoom adjustment.</param>
    public ShapeCamera(Vector2 pos, Dimensions targetResolution)
    {
        BaseTransform = BaseTransform.SetPosition(pos);
        TargetResolution = targetResolution;
    }

    /// <summary>
    /// Initializes a new camera at the specified position, alignment, and target resolution.
    /// </summary>
    /// <param name="pos">Base position.</param>
    /// <param name="alignement">Anchor alignment.</param>
    /// <param name="targetResolution">Target resolution for zoom adjustment.</param>
    public ShapeCamera(Vector2 pos, AnchorPoint alignement, Dimensions targetResolution)
    {
        BaseTransform = BaseTransform.SetPosition(pos);
        this.SetAlignement(alignement);
        TargetResolution = targetResolution;
    }

    /// <summary>
    /// Initializes a new camera at the specified position, alignment, zoom level, and target resolution.
    /// </summary>
    /// <param name="pos">Base position.</param>
    /// <param name="alignement">Anchor alignment.</param>
    /// <param name="zoomLevel">Initial zoom level.</param>
    /// <param name="targetResolution">Target resolution for zoom adjustment.</param>
    public ShapeCamera(Vector2 pos, AnchorPoint alignement, float zoomLevel, Dimensions targetResolution)
    {
        BaseTransform = BaseTransform.SetPosition(pos);
        this.SetAlignement(alignement);
        this.SetZoom(zoomLevel);
        TargetResolution = targetResolution;
    }

    /// <summary>
    /// Initializes a new camera at the specified position, alignment, zoom level, and rotation.
    /// </summary>
    /// <param name="pos">Base position.</param>
    /// <param name="alignement">Anchor alignment.</param>
    /// <param name="zoomLevel">Initial zoom level.</param>
    /// <param name="rotationDeg">Initial rotation in degrees.</param>
    public ShapeCamera(Vector2 pos, AnchorPoint alignement, float zoomLevel, float rotationDeg)
    {
        BaseTransform = BaseTransform.SetPosition(pos);
        this.SetAlignement(alignement);
        this.SetZoom(zoomLevel);
        this.SetRotation(rotationDeg);
    }

    /// <summary>
    /// Initializes a new camera at the specified position and zoom level.
    /// </summary>
    /// <param name="pos">Base position.</param>
    /// <param name="zoomLevel">Initial zoom level.</param>
    public ShapeCamera(Vector2 pos, float zoomLevel)
    {
        BaseTransform.SetPosition(pos);
        this.SetZoom(zoomLevel);
    }

    /// <summary>
    /// Activates the camera (enables tween updates).
    /// </summary>
    public void Activate()
    {
        cameraTweens.OnItemUpdated += OnCameraTweenUpdated;
        // Follower?.Activate();
    }

    /// <summary>
    /// Deactivates the camera (disables tween updates).
    /// </summary>
    public void Deactivate()
    {
        cameraTweens.OnItemUpdated -= OnCameraTweenUpdated;
        // Follower?.Deactivate();
        // Follower.ClearTarget();
    }

    /// <summary>
    /// Gets the current camera area in world coordinates.
    /// </summary>
    public Rect Area => new
    (
        BaseTransform.Position.X - Offset.Width * ZoomFactor, 
        BaseTransform.Position.Y - Offset.Height * ZoomFactor, 
        BaseTransform.BaseSize.Width * ZoomFactor,
        BaseTransform.BaseSize.Height * ZoomFactor
    );

    /// <summary>
    /// Gets the Raylib Camera2D struct for rendering.
    /// </summary>
    public Camera2D Camera => new()
    {
        Target = BaseTransform.Position,
        Offset = Offset.ToVector2(),
        Zoom = ZoomLevel,
        Rotation = RotationDeg
    };

    /// <summary>
    /// Gets the zoom factor (inverse of zoom level).
    /// </summary>
    public float ZoomFactor => 1f / ZoomLevel;

    /// <summary>
    /// Updates the camera state (tweens, shake, follower).
    /// </summary>
    /// <param name="dt">Delta time in seconds.</param>
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
            var rect = follower.Update(dt, Area);
            SetCameraRect(rect);
        }
    }

    /// <summary>
    /// Sets the camera size and adjusts zoom for the current screen size.
    /// </summary>
    /// <param name="curScreenSize">Current screen dimensions.</param>
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

    /// <summary>
    /// Calculates the zoom level to fit a target size.
    /// </summary>
    /// <param name="targetSize">Target area size.</param>
    /// <returns>Zoom level.</returns>
    private float CalculateZoomLevel(Size targetSize)
    {
        var size = BaseTransform.BaseSize / zoomAdjustment;
        var fX = 1f / (targetSize.Width / size.Width);
        var fY = 1f / (targetSize.Height / size.Height);
        return fX < fY ? fX : fY;

    }

    /// <summary>
    /// Sets the camera's area to the specified rectangle.
    /// </summary>
    /// <param name="newRect">Target rectangle.</param>
    public void SetCameraRect(Rect newRect)
    {
        BaseTransform = BaseTransform.SetPosition(newRect.GetPoint(Alignement));
        if (newRect.Size != Area.Size) SetZoom( CalculateZoomLevel(newRect.Size) );
    }
    
    /// <summary>
    /// Returns true if any camera tween sequences are active.
    /// </summary>
    public bool HasSequences() => cameraTweens.HasSequences();

    /// <summary>
    /// Returns true if a tween sequence with the given ID exists.
    /// </summary>
    /// <param name="id">Sequence ID.</param>
    public bool HasTweenSequence(uint id) => cameraTweens.HasSequence(id);

    /// <summary>
    /// Starts a new tween sequence.
    /// </summary>
    /// <param name="tweens">Tweens to run.</param>
    /// <returns>Sequence ID.</returns>
    public uint StartTweenSequence(params ICameraTween[] tweens) => cameraTweens.StartSequence(tweens);

    /// <summary>
    /// Stops a tween sequence by ID.
    /// </summary>
    /// <param name="id">Sequence ID.</param>
    public void StopTweenSequence(uint id) => cameraTweens.CancelSequence(id);

    /// <summary>
    /// Stops all camera tweens.
    /// </summary>
    public void StopTweens() => cameraTweens.Stop();

    /// <summary>
    /// Resets the camera to default state.
    /// </summary>
    public void Reset()
    {
        Follower?.Reset();
        BaseTransform = new Transform2D(new(), 0f);
        Alignement = new(0.5f);
        BaseZoomLevel = 1f;
    }

    /// <summary>
    /// Changes the zoom by the given amount.
    /// </summary>
    /// <param name="change">Zoom delta.</param>
    public void Zoom(float change) => SetZoom(BaseZoomLevel + change);

    /// <summary>
    /// Sets the base zoom level.
    /// </summary>
    /// <param name="zoomLevel">New zoom level.</param>
    public void SetZoom(float zoomLevel)
    {
        BaseZoomLevel = zoomLevel;
        if (BaseZoomLevel > MaxZoomLevel) BaseZoomLevel = MaxZoomLevel;
        else if (BaseZoomLevel < MinZoomLevel) BaseZoomLevel = MinZoomLevel;
    }

    /// <summary>
    /// Rotates the camera by the given degrees.
    /// </summary>
    /// <param name="deg">Degrees to rotate.</param>
    public void Rotate(float deg) => SetRotation((BaseTransform.RotationRad * ShapeMath.RADTODEG) + deg);

    /// <summary>
    /// Sets the camera's base rotation.
    /// </summary>
    /// <param name="deg">Rotation in degrees.</param>
    public void SetRotation(float deg)
    {
        BaseTransform = BaseTransform.SetRotationRad(deg * ShapeMath.DEGTORAD).WrapRotationRad();
    }

    /// <summary>
    /// Sets the camera alignment anchor.
    /// </summary>
    /// <param name="newAlignement">New anchor point.</param>
    public void SetAlignement(AnchorPoint newAlignement) => Alignement = newAlignement;

    /// <summary>
    /// Converts a screen position to world coordinates.
    /// </summary>
    /// <param name="pos">Screen position.</param>
    /// <returns>World position.</returns>
    public Vector2 ScreenToWorld(Vector2 pos)
    {
        var result = Raylib.GetScreenToWorld2D(pos, Camera);
        return result.IsNan() ? pos : result;
    }

    /// <summary>
    /// Converts a world position to screen coordinates.
    /// </summary>
    /// <param name="pos">World position.</param>
    /// <returns>Screen position.</returns>
    public Vector2 WorldToScreen(Vector2 pos)
    {
        var result = Raylib.GetWorldToScreen2D(pos, Camera);
        return result.IsNan() ? pos : result;
    }

    /// <summary>
    /// Converts a screen rectangle to world coordinates.
    /// </summary>
    /// <param name="r">Screen rectangle.</param>
    /// <returns>World rectangle.</returns>
    public Rect ScreenToWorld(Rect r)
    {
        var newPos = ScreenToWorld(r.TopLeft);
        var newSize = r.Size * ZoomFactor;
        return new(newPos, newSize, new(0f));
    }

    /// <summary>
    /// Converts a world rectangle to screen coordinates.
    /// </summary>
    /// <param name="r">World rectangle.</param>
    /// <returns>Screen rectangle.</returns>
    public Rect WorldToScreen(Rect r)
    {
        var newPos = WorldToScreen(r.TopLeft);
        var newSize = r.Size / ZoomFactor;
        return new(newPos, newSize, new(0f));
    }

    /// <summary>
    /// Starts a camera shake effect.
    /// </summary>
    /// <param name="duration">Shake duration (seconds).</param>
    /// <param name="strength">Shake strength (X, Y).</param>
    /// <param name="zoomStrength">Zoom shake strength.</param>
    /// <param name="rotStrength">Rotation shake strength.</param>
    /// <param name="smoothness">Shake smoothness (0-1).</param>
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

    /// <summary>
    /// Stops the camera shake effect.
    /// </summary>
    public void StopShake() { shake.Stop(); }
    
    /// <summary>
    /// Handles camera tween update events.
    /// </summary>
    /// <param name="tween">Tween being updated.</param>
    private void OnCameraTweenUpdated(ICameraTween tween)
    {
        cameraTweenTotalOffset += tween.GetOffset();
        cameraTweenTotalRotationDeg += tween.GetRotationDeg();
        cameraTweenTotalZoomFactor *= tween.GetZoomFactor();
        cameraTweenTotalZoomFactor = MathF.Max(cameraTweenTotalZoomFactor, 0.05f); //cant be zero!!!
    }

    /// <summary>
    /// Wraps a value between min and max.
    /// </summary>
    /// <param name="value">Value to wrap.</param>
    /// <param name="min">Minimum value.</param>
    /// <param name="max">Maximum value.</param>
    /// <returns>Wrapped value.</returns>
    private static float Wrap(float value, float min, float max) => value - (max - min) * MathF.Floor((float) (( value -  min) / ( max -  min)));
}