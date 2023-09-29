using ShapeEngine.Screen;
using Raylib_CsLo;
using System.Numerics;
using System.Runtime.InteropServices;
using ShapeEngine.Lib;
using ShapeEngine.Timing;

namespace ShapeEngine.Core;

public class ShapeShader
{
    public Shader Shader { get; private set; }
    public bool Enabled { get; set; }
    public uint ID { get; private set; }
    public int Order { get; set; }
    public bool Loaded { get; private set; }
    
    public ShapeShader(Shader shader, bool enabled = true, int order = 0)
    {
        this.Shader = shader;
        this.Enabled = enabled;
        this.ID = SID.NextID;
        this.Order = order;
        this.Loaded = true;
    }
    public ShapeShader(Shader shader, uint id, bool enabled = true, int order = 0)
    {
        this.Shader = shader;
        this.Enabled = enabled;
        this.ID = id;
        this.Order = order;
        this.Loaded = true;
    }
    public bool Unload()
    {
        if (!Loaded) return false;
        Loaded = false;
        UnloadShader(Shader);
        return true;
    }
    
    public static void SetValueFloat(Shader shader, string propertyName, float value)
    {
        int valueLocation = GetShaderLocation(shader, propertyName);
        SetShaderValue(shader, valueLocation, value, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
    }
    public static void SetValueVec(Shader shader, string propertyName, float[] values, ShaderUniformDataType dataType)
    {
        int valueLocation = GetShaderLocation(shader, propertyName);
        SetShaderValue(shader, valueLocation, values, dataType);
    }
    public static void SetValueVector2(Shader shader, string propertyName, float v1, float v2)
    {
        int valueLocation = GetShaderLocation(shader, propertyName);
        SetShaderValue(shader, valueLocation, new float[] { v1, v2 }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
    }
    public static void SetValueVector3(Shader shader, string propertyName, float v1, float v2, float v3)
    {
        int valueLocation = GetShaderLocation(shader, propertyName);
        SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3 }, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
    }
    public static void SetValueVector4(Shader shader, string propertyName, float v1, float v2, float v3, float v4)
    {
        int valueLocation = GetShaderLocation(shader, propertyName);
        SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3, v4 }, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
    }
    public static void SetValueVector2(Shader shader, string propertyName, Vector2 vec)
    {
        int valueLocation = GetShaderLocation(shader, propertyName);
        SetShaderValue(shader, valueLocation, new float[] { vec.X, vec.Y}, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
    }
    public static void SetValueColor(Shader shader, string propertyName, Raylib_CsLo.Color color)
    {
        int valueLocation = GetShaderLocation(shader, propertyName);
        SetShaderValue(shader, valueLocation, new float[] {color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f}, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
    }
}
public class ShaderContainer
{
    private Dictionary<uint, ShapeShader> shaders = new();
    
    public uint Add(ShapeShader shader)
    {
        if (shaders.ContainsKey(shader.ID)) shaders[shader.ID] = shader;
        else shaders.Add(shader.ID, shader);
        return shader.ID;
    }

    public ShapeShader? Get(uint id) => HasShader(id) ? shaders[id] : null;
    public bool Remove(uint id) => shaders.Remove(id);
    public bool Remove(ShapeShader shader) => shaders.Remove(shader.ID);
    public bool HasShaders() => shaders.Count > 0;
    public bool HasShader(uint id) => shaders.ContainsKey(id);
    public bool HasShader(ShapeShader shader) => shaders.ContainsKey(shader.ID);

    public void Close()
    {
        foreach (var shader in shaders.Values)
        {
            shader.Unload();
        }
    }
    
    public List<ShapeShader> GetActiveShaders()
    {
        var shadersToApply = shaders.Values.ToList().FindAll(s => s.Enabled);
        shadersToApply.Sort(delegate (ShapeShader a, ShapeShader b)
        {
            if (a.Order < b.Order) return -1;
            else if (a.Order > b.Order) return 1;
            else return 0;
        });
        return shadersToApply;
    }

    public List<ShapeShader> GetAllShaders() => shaders.Values.ToList();
    public List<uint> GetAllIDs() => shaders.Keys.ToList();
}
internal sealed class ShapeFlash
{
    private float maxDuration = 0.0f;
    private float flashTimer = 0.0f;
    private Raylib_CsLo.Color startColor = new(0, 0, 0, 0);
    private Raylib_CsLo.Color endColor = new(0, 0, 0, 0);
    private Raylib_CsLo.Color curColor = new(0, 0, 0, 0);

    public ShapeFlash(float duration, Raylib_CsLo.Color start, Raylib_CsLo.Color end)
    {

        maxDuration = duration;
        flashTimer = duration;
        startColor = start;
        curColor = start;
        endColor = end;
    }

    public void Update(float dt)
    {
        if (flashTimer > 0.0f)
        {
            flashTimer -= dt;
            float f = 1.0f - flashTimer / maxDuration;
            curColor = startColor.Lerp(endColor, f); // SColor.LerpColor(startColor, endColor, f);
            if (flashTimer <= 0.0f)
            {
                flashTimer = 0.0f;
                curColor = endColor;
            }
        }
    }
    public bool IsFinished() { return flashTimer <= 0.0f; }
    public Raylib_CsLo.Color GetColor() { return curColor; }

}
internal sealed class ShapeTexture
{
    public bool Loaded { get; private set; } = false;
    public RenderTexture RenderTexture { get; private set; } = new();
    public int Width { get; private set; } = 0;
    public int Height { get; private set; } = 0;

    public ShapeTexture(){}

    public void Load(Dimensions dimensions)
    {
        if (Loaded) return;
        Loaded = true;
        SetTexture(dimensions);
    }
    public void Unload()
    {
        if (!Loaded) return;
        Loaded = false;
        UnloadRenderTexture(RenderTexture);
    }
    public void UpdateDimensions(Dimensions dimensions)
    {
        if (!Loaded) return;

        if (Width == dimensions.Width && Height == dimensions.Height) return;
        
        UnloadRenderTexture(RenderTexture);
        SetTexture(dimensions);
    }
    public void Draw()
    {
        var destRec = new Rectangle
        {
        x = Width * 0.5f,
        y = Height * 0.5f,
        width = Width,
        height = Height
        };
        Vector2 origin = new()
        {
        X = Width * 0.5f,
        Y = Height * 0.5f
        };
        
        var sourceRec = new Rectangle(0, 0, Width, -Height);
        
        DrawTexturePro(RenderTexture.texture, sourceRec, destRec, origin, 0f, WHITE);
    }
    
    private void SetTexture(Dimensions dimensions)
    {
        Width = dimensions.Width;
        Height = dimensions.Height;
        RenderTexture = LoadRenderTexture(Width, Height);
    }
    
    //public void DrawTexture(int targetWidth, int targetHeight)
    //{
        //var destRec = new Rectangle
        //{
        //    x = targetWidth * 0.5f,
        //    y = targetHeight * 0.5f,
        //    width = targetWidth,
        //    height = targetHeight
        //};
        //Vector2 origin = new()
        //{
        //    X = targetWidth * 0.5f,
        //    Y = targetHeight * 0.5f
        //};
        //
        //
        //
        //var sourceRec = new Rectangle(0, 0, Width, -Height);
        //
        //DrawTexturePro(RenderTexture.texture, sourceRec, destRec, origin, 0f, WHITE);
    //
}


public class CameraFollower
{
    public bool IsFollowing => Target != null;
    public ICameraFollowTarget? Target { get; private set; } = null;
    public ICameraFollowTarget? NewTarget { get; private set; } = null;
    public float BoundaryDis { get; set; } = 0f;
    public float FollowSmoothness { get; set; } = 1f;
    public Vector2 Position { get; private set; } = new();

    
    internal void Update(float dt)
    {
        if (Target != null)
        {
            if (NewTarget != null)
            {
                Vector2 newPos = NewTarget.GetCameraFollowPosition();
                float disSq = (newPos - Position).LengthSquared();
                if (disSq < 25)
                {
                    Target.FollowEnded();
                    Target = NewTarget;
                    NewTarget = null;
                    Position = newPos;
                    Target.FollowStarted();
                }
                else
                {
                    Position = SVec.Lerp(Position, newPos, dt * FollowSmoothness);
                }
            }
            else
            {
                Vector2 newPos = Target.GetCameraFollowPosition();
                if (BoundaryDis > 0f)
                {
                    float boundaryDisSq = BoundaryDis * BoundaryDis;
                    float disSq = (Position - newPos).LengthSquared();
                    if (disSq > boundaryDisSq)
                    {
                        Position = Position.Lerp(newPos, dt * FollowSmoothness);
                    }
                }
                else Position = Position.Lerp(newPos, dt * FollowSmoothness);
            }
        }
    }

    public void SetTarget(ICameraFollowTarget target)
    {
        Target = target;
        Target.FollowStarted();
        Position = Target.GetCameraFollowPosition();
    }
    public void ChangeTarget(ICameraFollowTarget newTarget)
    {
        if (Target == null)
        {
            SetTarget(newTarget);
        }
        else
        {
            NewTarget = newTarget;
        }
    }
    public void ClearTarget()
    {
        Target?.FollowEnded();
        Target = null; 
        NewTarget = null;
    }
}
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
        BaseRotationDeg = SUtils.WrapAngleDeg(BaseRotationDeg);
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


public class ShapeLoop
{
    public static readonly string CURRENT_DIRECTORY = AppDomain.CurrentDomain.BaseDirectory; // Environment.CurrentDirectory;
    public static OSPlatform OS_PLATFORM { get; private set; } =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? OSPlatform.Windows :
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSPlatform.Linux :
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OSPlatform.OSX :
        OSPlatform.FreeBSD;

    public static bool DebugMode { get; private set; } = false;
    public static bool ReleaseMode { get; private set; } = true;
    
    
    public static bool IsWindows() => OS_PLATFORM == OSPlatform.Windows;
    public static bool IsLinux() => OS_PLATFORM == OSPlatform.Linux;
    public static bool IsOSX() => OS_PLATFORM == OSPlatform.OSX;
    
    public Raylib_CsLo.Color BackgroundColor = BLACK;
    public float ScreenEffectIntensity = 1.0f;
    
    public delegate void DimensionsChanged(DimensionConversionFactors conversion);
    public event DimensionsChanged? OnWindowDimensionsChanged;
    public string[] LaunchParams { get; protected set; } = Array.Empty<string>();

    
    public readonly ShaderContainer ScreenShaders = new();
    private readonly ShapeTexture gameTexture = new();
    
    private readonly ShapeTexture screenShaderBuffer = new();
    
    private readonly ShapeCamera basicCamera = new ShapeCamera();
    private ShapeCamera curCamera;
    public ShapeCamera Camera
    {
        get => curCamera;
        set
        {
            if (value == curCamera) return;
            curCamera.Deactive();
            curCamera = value;
            curCamera.Activate();
            curCamera.SetSize(CurScreenSize, DevelopmentDimensions);
        }
    }

    public void ResetCamera() => Camera = basicCamera;

    /// <summary>
    /// Scaling factors from current screen size to development resolution.
    /// </summary>
    public DimensionConversionFactors ScreenToDevelopment { get; private set; } = new();
    /// <summary>
    /// Scaling factors from development resolution to the current screen size.
    /// </summary>
    public DimensionConversionFactors DevelopmentToScreen { get; private set; } = new();
    public Dimensions DevelopmentDimensions { get; private set; } = new();
    public Dimensions CurScreenSize { get; private set; } = new();
    public Dimensions WindowMinSize { get; private set; } = new (128, 128);

    public ScreenInfo Game { get; private set; } = new();
    public ScreenInfo UI { get; private set; } = new();
    
    public float Delta { get; private set; } = 0f;

    private bool screenShaderAffectsUI = false;
    private bool quit = false;
    private bool restart = false;
    public MonitorDevice Monitor { get; private set; }
    public ICursor Cursor { get; private set; } = new NullCursor();
    public IScene CurScene { get; private set; } = new SceneEmpty();
    private List<ShapeFlash> shapeFlashes = new();
    private List<DeferredInfo> deferred = new();

    private int frameRateLimit = 60;
    public int FrameRateLimit
    {
        get => frameRateLimit;
        set
        {
            if (value < 30) frameRateLimit = 30;
            else if (value > 240) frameRateLimit = 240;
            
            if(!VSync) Raylib.SetTargetFPS(frameRateLimit);
        }
    }
    public int FPS => Raylib.GetFPS();
    public bool VSync
    {
        get =>Raylib.IsWindowState(ConfigFlags.FLAG_VSYNC_HINT);
        
        set
        {
            if (Raylib.IsWindowState(ConfigFlags.FLAG_VSYNC_HINT) == value) return;
            if (value)
            {
                Raylib.SetWindowState(ConfigFlags.FLAG_VSYNC_HINT);
                SetTargetFPS(Monitor.CurMonitor().Refreshrate);
            }
            else
            {
                Raylib.ClearWindowState(ConfigFlags.FLAG_VSYNC_HINT);
                SetTargetFPS(frameRateLimit);
            }
        }
    }
    public bool Fullscreen
    {
        get => Raylib.IsWindowFullscreen();
        set
        {
            if (value == Raylib.IsWindowFullscreen()) return;
            if(value)Raylib.SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
            else
            {
                Raylib.ClearWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
                Raylib.SetWindowSize(windowSize.Width, windowSize.Height);
                CenterWindow();
            }
            CheckWindowDimensionsChanged();
        }
    }
    public bool Maximized
    {
        get => Raylib.IsWindowMaximized();
        set
        {
            if (value == Raylib.IsWindowMaximized()) return;
            if(value)Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_MAXIMIZED);
            else Raylib.ClearWindowState(ConfigFlags.FLAG_WINDOW_MAXIMIZED);
            CheckWindowDimensionsChanged();
        }
    }

    private Dimensions windowSize = new();
    public Dimensions WindowSize
    {
        get => windowSize;
        set
        {
            var maxSize = Monitor.CurMonitor().Dimensions;
            int w = value.Width;
            if (w < WindowMinSize.Width) w = WindowMinSize.Width;
            else if (w > maxSize.Width) w = maxSize.Width;
            int h = value.Height;
            if (h < WindowMinSize.Height) h = WindowMinSize.Height;
            else if (h > maxSize.Height) h = maxSize.Height;
            
            windowSize = new(w, h);
            
            if (Fullscreen) return;
            SetWindowSize(windowSize.Width, windowSize.Height);
            CenterWindow();

            CheckWindowDimensionsChanged();
        }
    }
    public void CenterWindow()
    {
        if (Fullscreen) return;
        var monitor = Monitor.CurMonitor();

        int winPosX = monitor.Width / 2 - windowSize.Width / 2;
        int winPosY = monitor.Height / 2 - windowSize.Height / 2;
        SetWindowPosition(winPosX + (int)monitor.Position.X, winPosY + (int)monitor.Position.Y);
    }
    public void ResizeWindow(Dimensions newDimensions) => WindowSize = newDimensions;
    public void ResetWindow()
    {
        if (Fullscreen) Raylib.ClearWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
        WindowSize = Monitor.CurMonitor().Dimensions / 2;
    }

    
    
    public ShapeLoop(Dimensions developmentDimensions, bool multiShaderSupport = false, bool screenShadersAffectUI = false)
    {
        #if DEBUG
        DebugMode = true;
        ReleaseMode = false;
        #endif

        this.screenShaderAffectsUI = screenShadersAffectUI;
        this.DevelopmentDimensions = developmentDimensions;
        InitWindow(0, 0, "");
        
        ClearWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
        SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);

        Monitor = new MonitorDevice();
        SetupWindowDimensions();
        WindowMinSize = DevelopmentDimensions * 0.2f;
        Raylib.SetWindowMinSize(WindowMinSize.Width, WindowMinSize.Height);
        
        SetConversionFactors();
        
        VSync = true;
        FrameRateLimit = 60;

        curCamera = basicCamera;
        Camera.Activate();
        Camera.SetSize(CurScreenSize, DevelopmentDimensions);
        
        var mousePosUI = GetMousePosition();
        var mousePosGame = Camera.ScreenToWorld(mousePosUI);
        var screenArea = new Rect(0, 0, CurScreenSize.Width, CurScreenSize.Height);
        var cameraArea = Camera.Area;

        Game = new(cameraArea, mousePosGame);
        UI = new(screenArea, mousePosUI);
        
        
        gameTexture.Load(CurScreenSize);
        if (multiShaderSupport) screenShaderBuffer.Load(CurScreenSize);
    }
    public void SetupWindow(string windowName, bool undecorated, bool resizable, bool vsync = true, int fps = 60)
    {
        SetWindowTitle(windowName);
        if (undecorated) SetWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
        else ClearWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);

        if (resizable) SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        else ClearWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);

        FrameRateLimit = fps;
        VSync = vsync;
    }
    public ExitCode Run(params string[] launchParameters)
    {
        this.LaunchParams = launchParameters;

        quit = false;
        restart = false;
        Raylib.SetExitKey(-1);

        StartGameloop();
        RunGameloop();
        EndGameloop();
        CloseWindow();

        return new ExitCode(restart);
    }
    
    public void Restart()
    {
        restart = true;
        quit = true;
    }
    public void Quit()
    {
        restart = false;
        quit = true;
    }

    /// <summary>
    /// Switches to the new scene. Deactivate is called on the old scene and then Activate is called on the new scene.
    /// </summary>
    /// <param name="newScene"></param>
    public void GoToScene(IScene newScene)
    {
        if (newScene == CurScene) return;
        CurScene.Deactivate();
        newScene.Activate(CurScene);
        CurScene = newScene;
    }

    public void CallDeferred(Action action, int afterFrames = 0)
    {
        deferred.Add(new(action, afterFrames));
    }
    private void ResolveDeferred()
    {
        for (int i = deferred.Count - 1; i >= 0; i--)
        {
            var info = deferred[i];
            if (info.Call()) deferred.RemoveAt(i);
        }
    }

    
    public void Flash(float duration, Raylib_CsLo.Color startColor, Raylib_CsLo.Color endColor)
    {
        if (duration <= 0.0f) return;
        if (ScreenEffectIntensity <= 0f) return;
        byte startColorAlpha = (byte)(startColor.a * ScreenEffectIntensity);
        startColor.a = startColorAlpha;
        byte endColorAlpha = (byte)(endColor.a * ScreenEffectIntensity);
        endColor.a = endColorAlpha;

        ShapeFlash flash = new(duration, startColor, endColor);
        shapeFlashes.Add(flash);
    }

    public void ClearFlashes() => shapeFlashes.Clear();
    public bool SwitchCursor(ICursor newCursor)
    {
        if (Cursor != newCursor)
        {
            Cursor.Deactivate();
            newCursor.Activate(Cursor);
            Cursor = newCursor;
            return true;
        }
        return false;
    }
    private void DrawCursor(Vector2 screenSize, Vector2 pos) { if (Cursor != null) Cursor.Draw(screenSize, pos); }

    private void CalculateCurScreenSize()
    {
        if (IsWindowFullscreen())
        {
            var monitor = GetCurrentMonitor();
            var mw = GetMonitorWidth(monitor);
            var mh = GetMonitorHeight(monitor);
            var scaleFactor = GetWindowScaleDPI();
            int scaleX = (int)scaleFactor.X;
            int scaleY = (int)scaleFactor.Y;
            CurScreenSize = new(mw * scaleX, mh * scaleY);
        }
        else
        {
            var w = GetScreenWidth();
            var h = GetScreenHeight();
            CurScreenSize = new(w, h);
        }
    }

    private void StartGameloop()
    {
        LoadContent();
        BeginRun();
    }
    private void RunGameloop()
    {
        while (!quit)
        {
            if (WindowShouldClose())
            {
                Quit();
                continue;
            }
            var dt = GetFrameTime();
            Delta = dt;
            UpdateMonitorDevice(dt);
            CheckWindowDimensionsChanged();
            Camera.SetSize(CurScreenSize, DevelopmentDimensions);
            Camera.Update(dt);
            gameTexture.UpdateDimensions(CurScreenSize);
            screenShaderBuffer.UpdateDimensions(CurScreenSize);
            
            var mousePosUI = GetMousePosition();
            var mousePosGame = Camera.ScreenToWorld(mousePosUI);
            var screenArea = new Rect(0, 0, CurScreenSize.Width, CurScreenSize.Height);
            var cameraArea = Camera.Area;

            Game = new(cameraArea, mousePosGame);
            UI = new(screenArea, mousePosUI);
            
            UpdateFlashes(dt);
            Update(dt);

            BeginTextureMode(gameTexture.RenderTexture);
            ClearBackground(new(0,0,0,0));
            
            BeginMode2D(Camera.Camera);
            DrawGame(Game);
            EndMode2D();
            
            foreach (var flash in shapeFlashes) screenArea.Draw(flash.GetColor());
            if (screenShaderAffectsUI)
            {
                DrawUI(UI);
                DrawCursor(screenArea.Size, mousePosUI);
            }
            EndTextureMode();

            DrawToScreen(screenArea, mousePosUI);
            // var activeScreenShaders = ScreenShaders.GetActiveShaders();
            //
            // BeginDrawing();
            // ClearBackground(BackgroundColor);
            //
            // if (activeScreenShaders.Count > 0)
            // {
            //     BeginShaderMode(activeScreenShaders[0].Shader);
            //     gameTexture.Draw();
            //     EndShaderMode();
            // }
            // else
            // {
            //     gameTexture.Draw();
            // }
            //
            // //foreach (var flash in shapeFlashes) screenArea.Draw(flash.GetColor());
            //
            // DrawUI(UI);
            // DrawCursor(screenArea.Size, mousePosUI);
            // EndDrawing();
            
            
            //CheckWindowSizeChanged();

            
            ResolveDeferred();
        }
    }
    private void DrawToScreen(Rect screenArea, Vector2 mousePosUI)
    {
        var activeScreenShaders = ScreenShaders.GetActiveShaders();
        
        //multi shader support enabled and multiple screen shaders are active
        if (activeScreenShaders.Count > 1 && screenShaderBuffer.Loaded)
        {
            int lastIndex = activeScreenShaders.Count - 1;
            ShapeShader lastShader = activeScreenShaders[lastIndex];
            activeScreenShaders.RemoveAt(lastIndex);
            
            ShapeTexture source = gameTexture;
            ShapeTexture target = screenShaderBuffer;
            ShapeTexture temp;
            foreach (var shader in activeScreenShaders)
            {
                BeginTextureMode(target.RenderTexture);
                ClearBackground(new(0,0,0,0));
                BeginShaderMode(shader.Shader);
                source.Draw();
                EndShaderMode();
                EndTextureMode();
                temp = source;
                source = target;
                target = temp;
            }
            
            BeginDrawing();
            ClearBackground(BackgroundColor);

            BeginShaderMode(lastShader.Shader);
            target.Draw();
            EndShaderMode();

            if (!screenShaderAffectsUI)
            {
                DrawUI(UI);
                DrawCursor(screenArea.Size, mousePosUI);
            }
            
            EndDrawing();
            
        }
        else //single shader mode or only 1 screen shader is active
        {
            BeginDrawing();
            ClearBackground(BackgroundColor);

            if (activeScreenShaders.Count > 0)
            {
                BeginShaderMode(activeScreenShaders[0].Shader);
                gameTexture.Draw();
                EndShaderMode();
            }
            else
            {
                gameTexture.Draw();
            }
            
            if (!screenShaderAffectsUI)
            {
                DrawUI(UI);
                DrawCursor(screenArea.Size, mousePosUI);
            }
            EndDrawing();
            
        }
    }
    private void EndGameloop()
    {
        EndRun();
        UnloadContent();
        screenShaderBuffer.Unload();
        gameTexture.Unload();
    }
    private void UpdateMonitorDevice(float dt)
    {
        var newMonitor = Monitor.HasMonitorSetupChanged();
        if (newMonitor.Available)
        {
            MonitorChanged(newMonitor);
        }
    }
    

    #region Virtual

    /// <summary>
    /// Called first after starting the gameloop.
    /// </summary>
    protected virtual void LoadContent() { }
    /// <summary>
    /// Called after LoadContent but before the main loop has started.
    /// </summary>
    protected virtual void BeginRun() { }

    //protected virtual void HandleInput(float dt) { }
    protected virtual void Update(float dt) { }
    protected virtual void DrawGame(ScreenInfo game) { }
    protected virtual void DrawUI(ScreenInfo ui) { }

    /// <summary>
    /// Called before UnloadContent is called after the main gameloop has been exited.
    /// </summary>
    protected virtual void EndRun() { }
    /// <summary>
    /// Called after EndRun before the application terminates.
    /// </summary>
    protected virtual void UnloadContent() { }

    protected virtual void WindowSizeChanged(DimensionConversionFactors conversion) { }
    protected void UpdateScene() => CurScene.Update(Delta, Game, UI);

    protected void DrawGameScene() => CurScene.DrawGame(Game);

    protected void DrawUIScene() => CurScene.DrawUI(UI);

    #endregion
    
    public bool SetMonitor(int newMonitor)
    {
        var monitor = Monitor.SetMonitor(newMonitor);
        if (monitor.Available)
        {
            MonitorChanged(monitor);
            return true;
        }
        return false;
    }
    public void NextMonitor()
    {
        var nextMonitor = Monitor.NextMonitor();
        if (nextMonitor.Available)
        {
            MonitorChanged(nextMonitor);
        }
    }
    private void MonitorChanged(MonitorInfo monitor)
    {
        // var prevDimensions = CurScreenSize;

        // if (IsWindowFullscreen())
        // {
            // SetWindowMonitor(monitor.Index);
            // ChangeWindowDimensions(monitor.Dimensions, true);
            // SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
        // }
        // else
        // {
            // var windowDimensions = prevDimensions;
            // if (windowDimensions.Width > monitor.Width || windowDimensions.Height > monitor.Height)
            // {
                // windowDimensions = monitor.Dimensions / 2;
            // }
            // ChangeWindowDimensions(monitor.Dimensions, true);
            // SetWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
            // SetWindowMonitor(monitor.Index);
            // ClearWindowState(ConfigFlags.FLAG_FULLSCREEN_MODE);
            // ChangeWindowDimensions(windowDimensions, false);
        // }
        // if (VSync)
        // {
            // SetFPS(monitor.Refreshrate);
        // }
    }
    private void SetupWindowDimensions()
    {
        var monitor = Monitor.CurMonitor();
        WindowSize = monitor.Dimensions / 2;
        CenterWindow();
        CalculateCurScreenSize();
    }
    private void WriteDebugInfo()
    {
        Console.WriteLine("--------Shape Engine Monitor Info--------");
        if(Fullscreen)Console.WriteLine("Fullscreen is Enabled");
        else Console.WriteLine("Fullscreen is Disabled");
        
        if(IsWindowMaximized()) Console.WriteLine("Window is Maximized");
        else Console.WriteLine("Window is NOT Maximized");
        
        var dpi = Raylib.GetWindowScaleDPI();
        Console.WriteLine($"DPI: {dpi.X}/{dpi.Y}");

        var sWidth = Raylib.GetScreenWidth();
        var sHeight = Raylib.GetScreenHeight();
        Console.WriteLine($"Screen: {sWidth}/{sHeight}");

        var monitor = Raylib.GetCurrentMonitor();
        var mWidth = Raylib.GetMonitorWidth(monitor);
        var mHeight = Raylib.GetMonitorHeight(monitor);
        var mpWidth = Raylib.GetMonitorPhysicalWidth(monitor);
        var mpHeight = Raylib.GetMonitorPhysicalHeight(monitor);
        Console.WriteLine($"[{monitor}] Monitor: {mWidth}/{mHeight} Physical: {mpWidth}/{mpHeight}");


        var rWidth = Raylib.GetRenderWidth();
        var rHeight = Raylib.GetRenderHeight();
        Console.WriteLine($"Render Size: {rWidth}/{rHeight}");

        Monitor.CurMonitor().WriteDebugInfo();
        Console.WriteLine("---------------------------------------");
    }
    private void CheckWindowDimensionsChanged()
    {
        var prev = CurScreenSize;
        CalculateCurScreenSize();
        if (prev != CurScreenSize)
        {
            //TODO Matching aspect ratio
            //if !fullscreen
            //find matching resolution for dev aspect ratio
            //set window size to it
            //center window
            //calculate cur screen size
            
            
            SetConversionFactors();
            var conversion = new DimensionConversionFactors(prev, CurScreenSize);
            OnWindowDimensionsChanged?.Invoke(conversion);
            WindowSizeChanged(conversion);
            CurScene.WindowSizeChanged(conversion);
        }
    }
    private void UpdateFlashes(float dt)
    {
        for (int i = shapeFlashes.Count() - 1; i >= 0; i--)
        {
            var flash = shapeFlashes[i];
            flash.Update(dt);
            if (flash.IsFinished()) { shapeFlashes.RemoveAt(i); }
        }
    }

    private void SetConversionFactors()
    {
        ScreenToDevelopment = new(CurScreenSize, DevelopmentDimensions);
        DevelopmentToScreen = new(DevelopmentDimensions, CurScreenSize);
    }
}
    