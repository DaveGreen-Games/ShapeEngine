using ShapeLib;
using System.Numerics;
using Raylib_CsLo;
using ShapeCore;
using ShapeTiming;

namespace ShapeScreen
{
    public interface ICamera
    {
        public Rect GetArea();
        public Vector2 TransformPositionToUI(Vector2 gamePos, float gameToUIFactor);
        public Vector2 TransformPositionToGame(Vector2 uiPos, float uiToGameFactor);
        public void ChangeSize(Vector2 newSize);//, float zoomFactor);
        public void Update(float dt, float curWindowWidth, float curGameWidth);

        public Camera2D GetCamera();
        public bool IsPixelSmoothingCameraEnabled();
        public Camera2D GetPixelSmoothingCamera();

    }
    public class CameraBasic : ICamera
    {
        public float BaseRotationDeg { get; private set; } = 0f;
        public Vector2 BaseOffset { get; private set; } = new(0f);
        public float BaseZoom { get; private set; } = 1f;

        public Vector2 Translation { get; set; }
        public float RotationDeg { get; set; }
        public float ZoomFactor { get; set; }
        //public float ZoomStretchFactor { get; private set; } = 1f;

        public Camera2D WorldCamera { get; private set; }



        public CameraBasic(Vector2 size, float zoom, float rotation)//, float zoomStretchFactor)
        {
            ChangeSize(size);//, zoomStretchFactor);
            this.BaseZoom = zoom;
            this.BaseRotationDeg = rotation;
            this.WorldCamera = new() { offset = BaseOffset, rotation = BaseRotationDeg, zoom = 1f, target = new(0f) };
        }

        public void Update(float dt, float curWindowWidth, float curGameWidth)
        {
            Vector2 rawCameraOffset = BaseOffset + Translation;
            float rawCameraRotationDeg = BaseRotationDeg + RotationDeg;
            float rawCameraZoom = (BaseZoom /* *ZoomStretchFactor */) * ZoomFactor;
            Vector2 rawCameraTarget = WorldCamera.target;

            var c = new Camera2D();
            c.target = rawCameraTarget;
            c.offset = rawCameraOffset;
            c.zoom = rawCameraZoom;
            c.rotation = rawCameraRotationDeg;
            WorldCamera = c;

        }
        public void ChangeSize(Vector2 newSize)//, float factor)
        {
            BaseOffset = newSize / 2;
            //ZoomStretchFactor = factor;
        }

        public void ResetZoom() { ZoomFactor = 1f; }
        public void ResetRotation() { RotationDeg = 0f; }
        public void ResetTranslation() { Translation = new(0f); }


        public Vector2 TransformPositionToUI(Vector2 gamePos, float gameToUIFactor)
        {
            float zoomFactor = 1 / WorldCamera.zoom;
            Vector2 cPos = WorldCamera.target - WorldCamera.offset * zoomFactor;
            Vector2 p = (gamePos - cPos) * gameToUIFactor * WorldCamera.zoom;
            return p;
        }
        public Vector2 TransformPositionToGame(Vector2 uiPos, float uiToGameFactor)
        {
            float zoomFactor = 1 / WorldCamera.zoom;
            Vector2 p = uiPos * uiToGameFactor * zoomFactor;
            Vector2 cPos = WorldCamera.target - WorldCamera.offset * zoomFactor;
            p += cPos;
            return p;
        }
        public Rect GetArea()
        {
            float zoomFactor = 1 / WorldCamera.zoom;
            Vector2 cPos = WorldCamera.target - WorldCamera.offset * zoomFactor;
            return new(cPos.X, cPos.Y, WorldCamera.offset.X * zoomFactor, WorldCamera.offset.Y * zoomFactor);
        }
        public bool IsPixelSmoothingCameraEnabled() { return false; }
        public Camera2D GetPixelSmoothingCamera() { return WorldCamera; }
        public Camera2D GetCamera() { return WorldCamera; }
    }
    public class CameraAdvanced : ICamera
    {
        
        public float CAMERA_SHAKE_INTENSITY = 1.0f;

        private const int ShakeX = 0;
        private const int ShakeY = 1;
        private const int ShakeZoom = 2;
        private const int ShakeRot = 3;
        private Shake shake = new(4);

        public Sequencer<ICameraTween> CameraTweens { get; private set; } = new();
        private float cameraTweenTotalRotationDeg = 0f;
        private float cameraTweenTotalScale = 1f;
        private Vector2 cameraTweenTotalOffset = new();

        public float BaseRotationDeg { get; private set; } = 0f;
        public Vector2 BaseOffset { get; private set; } = new(0f);
        public float BaseZoom { get; private set; } = 1f;
        
        public Vector2 Translation {get; set;}
        public float RotationDeg   {get; set;}
        public float ZoomFactor    {get; set;}
        public float BoundaryDis { get; set; } = 0f;
        public float ZoomStretchFactor { get; private set; } = 1f;
        public float FollowSmoothness { get; set; } = 1f;
        public bool PixelSmoothingEnabled = true;

        public IGameObject? Target { get; private set; } = null;
        public IGameObject? NewTarget { get; private set; } = null;
        public Camera2D WorldCamera { get; private set; }
        public Camera2D ScreenCamera { get; private set; }
        


        public CameraAdvanced(Vector2 size, float zoom, float rotation)//, float zoomStretchFactor)
        {
            ChangeSize(size);//, zoomStretchFactor);
            this.BaseZoom = zoom;
            this.BaseRotationDeg = rotation;
            this.ScreenCamera = new() { offset = BaseOffset, rotation = BaseRotationDeg, zoom = 1f, target = new(0f) };
            this.WorldCamera = new() { offset = BaseOffset, rotation = BaseRotationDeg, zoom = 1f, target = new(0f) };
            this.CameraTweens.OnItemUpdated += OnCameraTweenUpdated;
        }

        public void Update(float dt, float curWindowWidth, float curGameWidth)
        {
            CameraTweens.Update(dt);
            shake.Update(dt);
            Vector2 shakeOffset = new(shake.Get(ShakeX), shake.Get(ShakeY));
            Vector2 rawCameraOffset = BaseOffset + shakeOffset + Translation + cameraTweenTotalOffset;
            float rawCameraRotationDeg = BaseRotationDeg + shake.Get(ShakeRot) + RotationDeg + cameraTweenTotalRotationDeg;
            float rawCameraZoom = ( ( (BaseZoom * ZoomStretchFactor) + shake.Get(ShakeZoom)) * ZoomFactor ) / cameraTweenTotalScale;
            cameraTweenTotalOffset = new(0f);
            cameraTweenTotalRotationDeg = 0f;
            cameraTweenTotalScale = 1f;
            
            Vector2 rawCameraTarget = WorldCamera.target;
            if (Target != null)
            {
                if (NewTarget != null)
                {
                    Vector2 curPos = rawCameraTarget;
                    Vector2 newPos = NewTarget.GetCameraFollowPosition(curPos);
                    float disSq = (newPos - curPos).LengthSquared();
                    if (disSq < 25)
                    {
                        Target = NewTarget;
                        NewTarget = null;
                        rawCameraTarget = newPos;
                    }
                    else
                    {
                        rawCameraTarget = SVec.Lerp(curPos, newPos, dt * FollowSmoothness);
                    }
                }
                else
                {
                    Vector2 curPos = rawCameraTarget;
                    Vector2 newPos = Target.GetCameraFollowPosition(curPos);
                    if(BoundaryDis > 0f)
                    {
                        float boundaryDisSq = BoundaryDis * BoundaryDis;
                        float disSq = (curPos - newPos).LengthSquared();
                        if(disSq > boundaryDisSq)
                        {
                            rawCameraTarget = curPos.Lerp(newPos, dt * FollowSmoothness);
                        }
                    }
                    else rawCameraTarget = curPos.Lerp(newPos, dt * FollowSmoothness);
                }
            }


            if (PixelSmoothingEnabled)
            {
                float virtualRatio = curWindowWidth / curGameWidth;
                var worldCam = new Camera2D();
                var screenCam = new Camera2D();
                worldCam.target.X = (int)rawCameraTarget.X;
                screenCam.target.X = rawCameraTarget.X - WorldCamera.target.X;
                screenCam.target.X *= virtualRatio;

                worldCam.target.Y = (int)rawCameraTarget.Y;
                screenCam.target.Y = rawCameraTarget.Y - WorldCamera.target.Y;
                screenCam.target.Y *= virtualRatio;

                worldCam.offset.X = (int)rawCameraOffset.X;
                screenCam.offset.X = rawCameraOffset.X - WorldCamera.offset.X;
                screenCam.offset.X *= virtualRatio;

                worldCam.offset.Y = (int)rawCameraOffset.Y;
                screenCam.offset.Y = rawCameraOffset.Y - WorldCamera.offset.Y;
                screenCam.offset.Y *= virtualRatio;

                worldCam.rotation = rawCameraRotationDeg;
                worldCam.zoom = rawCameraZoom;
                WorldCamera = worldCam;
                ScreenCamera = screenCam;
            }
            else
            {
                var c = new Camera2D();
                c.target = rawCameraTarget;
                c.offset = rawCameraOffset;
                c.zoom = rawCameraZoom;
                c.rotation = rawCameraRotationDeg;
                WorldCamera = c;
            }
            
        }
        
        private void OnCameraTweenUpdated(ICameraTween tween)
        {
            cameraTweenTotalOffset += tween.GetOffset();
            cameraTweenTotalRotationDeg += tween.GetRotationDeg();
            cameraTweenTotalScale *= tween.GetScale();

        }
        public void ChangeSize(Vector2 newSize)//, float factor)
        {
            BaseOffset = newSize / 2;
            //ZoomStretchFactor = factor;
        }

        public void ResetZoom() { ZoomFactor = 1f; }
        public void ResetRotation() { RotationDeg = 0f; }
        public void ResetTranslation() { Translation = new(0f); }

        public void SetTarget(IGameObject target)
        {
            this.Target = target;
            var wCam = WorldCamera;
            wCam.target = target.GetCameraFollowPosition(wCam.target);//, 0f, FollowSmoothness, BoundaryDis);
            WorldCamera = wCam;
        }
        public void ChangeTarget(IGameObject newTarget)
        {
            if (Target == null)
            {
                Target = newTarget;
            }
            else
            {
                NewTarget = newTarget;
            }
        }
        public void ClearTarget() { Target = null; NewTarget = null; }
        
        public void Shake(float duration, Vector2 strength, float zoomStrength = 0f, float rotStrength = 0f, float smoothness = 0.75f)
        {
            float intensity = CAMERA_SHAKE_INTENSITY;
            shake.Start
                (
                    duration,
                    smoothness,
                    strength.X * intensity,
                    strength.Y * intensity,
                    zoomStrength * intensity,
                    rotStrength * intensity
                );
        }
        public void StopShake() { shake.Stop(); }


        public Vector2 TransformPositionToUI(Vector2 gamePos, float gameToUIFactor)
        {
            float zoomFactor = 1 / WorldCamera.zoom;
            Vector2 cPos = WorldCamera.target - WorldCamera.offset * zoomFactor;
            Vector2 p = (gamePos - cPos) * gameToUIFactor * WorldCamera.zoom;
            return p;
        }
        public Vector2 TransformPositionToGame(Vector2 uiPos, float uiToGameFactor)
        {
            float zoomFactor = 1 / WorldCamera.zoom;
            Vector2 p = uiPos * uiToGameFactor * zoomFactor;
            Vector2 cPos = WorldCamera.target - WorldCamera.offset * zoomFactor;
            p += cPos;
            return p;
        }
        public Rect GetArea()
        {
            float zoomFactor = 1 / WorldCamera.zoom;
            Vector2 cPos = WorldCamera.target - WorldCamera.offset * zoomFactor;
            return new(cPos.X, cPos.Y, WorldCamera.offset.X * zoomFactor, WorldCamera.offset.Y * zoomFactor);
        }
        public bool IsPixelSmoothingCameraEnabled() { return PixelSmoothingEnabled; }
        public Camera2D GetPixelSmoothingCamera() { return ScreenCamera; }
        public Camera2D GetCamera() { return WorldCamera; }
    }

}





/*
public class CameraOrder
{
    float startRotDeg = 0f;
    float endRotDeg = 0f;
    Vector2 startTranslation = new(0f);
    Vector2 endTranslation = new(0f);
    float startScale = 1f;
    float endScale = 1f;

    float duration = 0f;
    float timer = 0f;
    float f = 0f;
    EasingType easingType = EasingType.LINEAR_NONE;

    public CameraOrder(float duration, Vector2 startTranslation, Vector2 endTranslation, float startRotDeg, float endRotDeg, float startScale, float endScale, EasingType easingType = EasingType.LINEAR_NONE)
    {
        this.duration = duration;
        timer = duration;
        f = 0f;

        this.startTranslation = startTranslation;
        this.endTranslation = endTranslation;
        this.startRotDeg = startRotDeg;
        this.endRotDeg = endRotDeg;
        this.startScale = startScale;
        this.endScale = endScale;
        this.easingType = easingType;
    }
    public CameraOrder(float duration, Vector2 startTranslation, Vector2 endTranslation, EasingType easingType = EasingType.LINEAR_NONE)
    {
        this.duration = duration;
        timer = duration;
        f = 0f;

        this.startTranslation = startTranslation;
        this.endTranslation = endTranslation;
        this.easingType = easingType;
    }
    public CameraOrder(float duration, float startScale, float endScale, EasingType easingType = EasingType.LINEAR_NONE)
    {
        this.duration = duration;
        timer = duration;
        f = 0f;

        this.startScale = startScale;
        this.endScale = endScale;
        this.easingType = easingType;
    }
    public CameraOrder(float duration, float startRotDeg, float endRotDeg, float startScale, float endScale, EasingType easingType = EasingType.LINEAR_NONE)
    {
        this.duration = duration;
        timer = duration;
        f = 0f;

        this.startRotDeg = startRotDeg;
        this.endRotDeg = endRotDeg;
        this.startScale = startScale;
        this.endScale = endScale;
        this.easingType = easingType;
    }
    public CameraOrder(float duration, Vector2 startTranslation, Vector2 endTranslation, float startRotDeg, float endRotDeg, EasingType easing = EasingType.LINEAR_NONE)
    {
        this.duration = duration;
        timer = duration;
        f = 0f;

        this.startTranslation = startTranslation;
        this.endTranslation = endTranslation;
        this.startRotDeg = startRotDeg;
        this.endRotDeg = endRotDeg;
        easingType = easingType;
    }


    public Vector2 Translation { get { return SVec.Lerp(startTranslation, endTranslation, f); } }
    public float RotDeg { get { return SUtils.LerpFloat(startRotDeg, endRotDeg, f); } }
    public float Scale { get { return SUtils.LerpFloat(startScale, endScale, f); } }

    public void Update(float dt)
    {
        if (timer > 0f && duration > 0f)
        {
            timer -= MathF.Min(timer, dt);
            f = SEase.Simple(1.0f - timer / duration, easingType);
        }
    }

    public bool IsFinished() { return duration > 0 && timer <= 0f; }
}
public class CameraOrderChain
{
    private List<CameraOrder> orders = new();
    private bool removable = true;
    public CameraOrderChain(params CameraOrder[] transformOrders)
    {
        Array.Reverse(transformOrders);
        orders = transformOrders.ToList();
    }
    public CameraOrderChain(bool removable, params CameraOrder[] transformOrders)
    {
        this.removable = removable;
        Array.Reverse(transformOrders);
        orders = transformOrders.ToList();
    }

    public float RotDeg
    {
        get
        {
            if (orders.Count <= 0) return 0f;
            else return orders[orders.Count - 1].RotDeg;
        }
    }
    public float Scale
    {
        get
        {
            if (orders.Count <= 0) return 1f;
            else return orders[orders.Count - 1].Scale;
        }
    }
    public Vector2 Translation
    {
        get
        {
            if (orders.Count <= 0) return new(0f);
            else return orders[orders.Count - 1].Translation;
        }
    }
    public bool IsFinished() { return orders.Count <= 0; }
    public void Update(float dt)
    {
        bool finished = orders[orders.Count - 1].IsFinished();
        if (!removable && orders.Count <= 1 && finished) return;
        if (finished)
        {
            if (removable) orders.RemoveAt(orders.Count - 1);
            else
            {
                if (orders.Count > 1) orders.RemoveAt(orders.Count - 1);
            }
        }
        if (IsFinished()) return;

        int index = orders.Count - 1;
        var order = orders[index];
        order.Update(dt);
    }
}
public class CameraOrderChainHandler
{
    private Dictionary<string, CameraOrderChain> chains = new();
    private float totalRotDeg = 0f;
    private float totalScale = 1f;
    private Vector2 totalTranslation = new(0f);
    public float TotalRotDeg { get { return totalRotDeg; } }
    public float TotalScale { get { return totalScale; } }
    public Vector2 TotalTranslation { get { return totalTranslation; } }

    public void AddChain(string name, params CameraOrder[] orders)
    {
        if (name == "" || orders.Length <= 0) return;
        if (chains.ContainsKey(name)) chains[name] = new CameraOrderChain(orders);
        else chains.Add(name, new(orders));
    }
    public void AddChain(string name, bool removeable, params CameraOrder[] orders)
    {
        if (name == "" || orders.Length <= 0) return;
        if (chains.ContainsKey(name)) chains[name] = new CameraOrderChain(removeable, orders);
        else chains.Add(name, new(removeable, orders));
    }
    public void RemoveChain(string name)
    {
        chains.Remove(name);
        if (chains.Count <= 0)
        {
            totalRotDeg = 0f;
            totalScale = 1f;
            totalTranslation = new(0f);
        }
    }
    public void ClearChains()
    {
        chains.Clear();
        totalRotDeg = 0f;
        totalScale = 1f;
        totalTranslation = new(0f);
    }
    public void Update(float dt)
    {
        if (chains.Count <= 0) return;

        totalRotDeg = 0f;
        totalScale = 1f;
        totalTranslation = new(0f);
        List<string> remove = new();
        foreach (var chain in chains)
        {
            chain.Value.Update(dt);
            totalRotDeg += chain.Value.RotDeg;
            totalScale *= chain.Value.Scale;
            totalTranslation += chain.Value.Translation;

            if (chain.Value.IsFinished()) remove.Add(chain.Key);

        }

        foreach (string name in remove)
        {
            chains.Remove(name);
        }
        if (chains.Count <= 0)
        {
            totalRotDeg = 0f;
            totalScale = 1f;
            totalTranslation = new(0f);
        }
    }
}
*/
/*
//rework with generic shake
internal class CameraShake
{
    private float timer = 0.0f;
    private float duration = 0.0f;
    private float smoothness = 0.0f;
    private float curX = 0.0f;
    private float curY = 0.0f;
    private float curRotDeg = 0.0f;
    private float curZoom = 0.0f;
    private float f = 0.0f;

    private Vector2 offsetStrength = new(0f);
    private float rotStrengthDeg = 0f;
    private float zoomStrength = 0f;

    public CameraShake() { }

    public Vector2 Offset { get { return new(X, Y); } }
    public float X { get { return curX * offsetStrength.X; } }
    public float Y { get { return curY * offsetStrength.Y; } }
    public float RotDeg { get { return curRotDeg * rotStrengthDeg; } }
    public float Zoom { get { return curZoom * zoomStrength; } }
    public bool IsActive() { return timer > 0.0f; }
    public float F { get { return f; } }


    public void Start(float duration, Vector2 strength, float zoomStrength = 0f, float rotStrengthDeg = 0f, float smoothness = 0.75f)
    {
        timer = duration;
        this.duration = duration;
        this.smoothness = smoothness;
        offsetStrength = strength;
        this.rotStrengthDeg = rotStrengthDeg;
        this.zoomStrength = zoomStrength;
        curX = 0.0f;
        curX = 0.0f;
        curZoom = 0f;
        curRotDeg = 0f;
        f = 0.0f;

    }
    public void Stop()
    {
        timer = 0f;
        f = 0f;
        curX = 0.0f;
        curX = 0.0f;
        curZoom = 0f;
        curRotDeg = 0f;
        f = 0.0f;
    }
    public void Update(float dt)
    {
        if (timer > 0.0f)
        {
            timer -= dt;
            if (timer <= 0.0f)
            {
                timer = 0.0f;
                curX = 0.0f;
                curY = 0.0f;
                curZoom = 0f;
                curRotDeg = 0f;
                f = 0.0f;
                return;
            }
            f = timer / duration;
            curX = Lerp(SRNG.randF(-1.0f, 1.0f), curX, smoothness) * f;
            curY = Lerp(SRNG.randF(-1.0f, 1.0f), curY, smoothness) * f;
            curRotDeg = Lerp(SRNG.randF(-1.0f, 1.0f), curRotDeg, smoothness) * f;
            curZoom = Lerp(SRNG.randF(-1.0f, 1.0f), curZoom, smoothness) * f;
        }
    }
}
*/
//private IGameObject? target = null;
//private float baseRotationDeg = 0f;
//private Vector2 baseOffset = new(0f);
//private float curZoomFactor = 1f;
//private float curRotDeg = 0f;
//private Vector2 curTranslation = new(0f);
//private Camera2D worldSpaceCamera;
//private Camera2D screenSpaceCamera;
//private CameraOrderChainHandler cameraOrderChainHandler = new();
//public void ZoomBy(float amountFactor) { ZoomFactor += amountFactor; }
//public void Rotate(float deg) { RotationDeg += deg; }
//public void Translate(Vector2 amount) { Translation += amount; }
/*
/// <summary>
/// Original world position without pixel perfect smoothing applied. Used for transformation to and from ui and the camera area.
/// </summary>
private Vector2 rawCameraTarget = new(0f);
/// <summary>
/// Original offset without pixel perfect smoothing applied. Used for transformation to and from ui and the camera area.
/// </summary>
private Vector2 rawCameraOffset = new(0f);
private float rawCameraZoom = 0f;
private float rawCameraRotationDeg = 0f;
*/
/*
/// <summary>
/// Original world position of the camera before pixel perfect smoothing is applied.
/// </summary>
public Vector2 RawPos { get { return rawCameraTarget; } }
/// <summary>
/// Original offset of the camera before pixel perfect smoothing is applied.
/// </summary>
public Vector2 RawOffset { get { return rawCameraOffset; } }
/// <summary>
/// Original zoom of the camera before pixel perfect smoothing is applied.
/// </summary>
public float RawZoom { get { return rawCameraZoom; } }
/// <summary>
/// Original rotation of the camera before pixel perfect smoothing is applied.
/// </summary>
public float RawRotDeg { get { return rawCameraRotationDeg; } }
*/
/*
/// <summary>
/// Current camera rotation in degrees after pixel perfect smoothing is applied.
/// </summary>
public float CameraRotDeg { get { return worldSpaceCamera.rotation; } }
/// <summary>
/// Current camera rotation in radians after pixel perfect smoothing is applied.
/// </summary>
public float CameraRotRad { get { return worldSpaceCamera.rotation * DEG2RAD; } }
/// <summary>
/// Current camera position after pixel perfect smoothing is applied.
/// </summary>
public Vector2 CameraPos { get { return worldSpaceCamera.target; } }
/// <summary>
/// Current camera offset after pixel perfect smoothing is applied.
/// </summary>
public Vector2 CameraOffset { get { return worldSpaceCamera.offset; } }
/// <summary>
/// Current camera zoom after pixel perfect smoothing is applied.
/// </summary>
public float CameraZoom { get { return worldSpaceCamera.zoom; } }
*/
