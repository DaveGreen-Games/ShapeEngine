
using System.Numerics;
using Raylib_CsLo;

namespace ShapeEngineCore.Globals.Screen
{
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


        public Vector2 Translation { get { return Vec.Lerp(startTranslation, endTranslation, f); } }
        public float RotDeg { get { return Utils.LerpFloat(startRotDeg, endRotDeg, f); } }
        public float Scale { get { return Utils.LerpFloat(startScale, endScale, f); } }

        public void Update(float dt)
        {
            if (timer > 0f && duration > 0f)
            {
                timer -= MathF.Min(timer, dt);
                f = Ease.Simple(1.0f - timer / duration, easingType);
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
                curX = Lerp(RNG.randF(-1.0f, 1.0f), curX, smoothness) * f;
                curY = Lerp(RNG.randF(-1.0f, 1.0f), curY, smoothness) * f;
                curRotDeg = Lerp(RNG.randF(-1.0f, 1.0f), curRotDeg, smoothness) * f;
                curZoom = Lerp(RNG.randF(-1.0f, 1.0f), curZoom, smoothness) * f;
            }
        }
    }
    public class Camera
    {
        private GameObject? target = null;
        private GameObject? newTarget = null;
        private float boundaryDis = 0f;
        private float followSmoothness = 0f;
        private float baseZoom = 1f;
        private float zoomStretchFactor = 1f;
        private float baseRotationDeg = 0f;
        private float curZoomFactor = 1f;
        private float curRotDeg = 0f;
        private Vector2 curTranslation = new(0f);
        private Vector2 baseOffset = new(0f);
        private Camera2D worldSpaceCamera;
        private Camera2D screenSpaceCamera;

        public bool PIXEL_SMOOTHING_ENABLED = true;
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

        private CameraShake shake = new();

        private CameraOrderChainHandler cameraOrderChainHandler = new();

        public void AddCameraOrderChain(string name, params CameraOrder[] orders)
        {
            cameraOrderChainHandler.AddChain(name, orders);
        }
        public void AddCameraOrderChain(string name, bool removeable, params CameraOrder[] orders)
        {
            cameraOrderChainHandler.AddChain(name, removeable, orders);
        }
        public void RemoveCameraOrderChain(string name)
        {
            cameraOrderChainHandler.RemoveChain(name);
        }
        public void ClearCameraOrderChains() { cameraOrderChainHandler.ClearChains(); }


        public GameObject? Target { get { return target; } set { target = value; } }
        
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

        public Camera2D Cam { get { return worldSpaceCamera; } }
        public Camera2D ScreenSpaceCam { get { return screenSpaceCamera; } }
        public Camera(Vector2 size, float zoom, float zoomStretchFactor, float rotation, float boundaryDis = 0f, float followSmoothness = 1f, bool pixelSmoothing = true)
        {
            this.followSmoothness = followSmoothness;
            this.boundaryDis = boundaryDis;
            this.baseOffset = size / 2;
            this.baseZoom = zoom;
            this.zoomStretchFactor = zoomStretchFactor;
            this.baseRotationDeg = rotation;
            this.PIXEL_SMOOTHING_ENABLED = pixelSmoothing;
            this.screenSpaceCamera = new() { offset = baseOffset, rotation = baseRotationDeg, zoom = baseZoom, target = new(0f) };
            this.worldSpaceCamera = new() { offset = baseOffset, rotation = baseRotationDeg, zoom = baseZoom, target = new(0f) };
        }
        public void ChangeSize(Vector2 newSize, float factor)
        {
            baseOffset = newSize / 2;
            zoomStretchFactor = factor;
        }
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

        public float ZoomFactor { get { return curZoomFactor; } set { curZoomFactor = value; } }
        public void ZoomBy(float amountFactor) { curZoomFactor += amountFactor; }
        public void ResetZoom() { curZoomFactor = 1f; }
        public float RotationDeg { get { return curRotDeg; } set { curRotDeg = value; } }
        public void Rotate(float deg) { curRotDeg += deg; }
        public void ResetRotation() { curRotDeg = 0f; }

        public Vector2 Translation { get { return curTranslation; } set { curTranslation = value; } }
        public void Translate(Vector2 amount) { curTranslation += amount; }
        public void ResetTranslation() { curTranslation = new(0f); }
        public void SetTarget(GameObject target)
        {
            this.target = target;
            rawCameraTarget = target.GetCameraPosition(rawCameraTarget, 0f, followSmoothness, boundaryDis);
        }
        public void ChangeTarget(GameObject newTarget)
        {
            if (target == null)
            {
                target = newTarget;
            }
            else
            {
                this.newTarget = newTarget;
            }
        }

        public void ClearTarget() { target = null; newTarget = null; }
        public void Shake(float duration, Vector2 strength, float zoomStrength = 0f, float rotStrength = 0f, float smoothness = 0.75f)
        {
            float intensity = ScreenHandler.CAMERA_SHAKE_INTENSITY;
            shake.Start
                (
                    duration,
                    strength * intensity,
                    zoomStrength * intensity,
                    rotStrength * intensity,
                    smoothness
                );
        }
        public void StopShake() { shake.Stop(); }

        public Vector2 TransformPositionToUI(Vector2 gamePos)
        {
            float zoomFactor = 1 / rawCameraZoom;
            Vector2 cPos = rawCameraTarget - rawCameraOffset * zoomFactor;
            Vector2 p = (gamePos - cPos) * ScreenHandler.GAME_TO_UI * rawCameraZoom;
            return p;
        }

        public Vector2 TransformPositionToGame(Vector2 uiPos)
        {
            float zoomFactor = 1 / rawCameraZoom;
            Vector2 p = uiPos * ScreenHandler.UI_TO_GAME * zoomFactor;
            Vector2 cPos = rawCameraTarget - rawCameraOffset * zoomFactor;
            p += cPos;
            return p;
        }
        public Rectangle GetCameraArea()
        {
            float zoomFactor = 1 / rawCameraZoom;
            Vector2 cPos = rawCameraTarget - rawCameraOffset * zoomFactor;
            return new(cPos.X, cPos.Y, rawCameraOffset.X * zoomFactor, rawCameraOffset.Y * zoomFactor);
        }
        public void Update(float dt)
        {
            cameraOrderChainHandler.Update(dt);
            shake.Update(dt);
            rawCameraOffset = baseOffset + shake.Offset + curTranslation + cameraOrderChainHandler.TotalTranslation;
            rawCameraRotationDeg = baseRotationDeg + shake.RotDeg + curRotDeg + cameraOrderChainHandler.TotalRotDeg;
            rawCameraZoom = ( (baseZoom * zoomStretchFactor) + shake.Zoom) * curZoomFactor / cameraOrderChainHandler.TotalScale;
            if (target != null)
            {
                if (newTarget != null)
                {
                    Vector2 curPos = rawCameraTarget; // screenSpaceCamera.target;// target.GetPosition();
                    Vector2 newPos = newTarget.GetCameraPosition(curPos, 0f, followSmoothness, boundaryDis);
                    float disSq = Vec.LengthSquared(newPos - curPos);
                    if (disSq < 25)
                    {
                        target = newTarget;
                        newTarget = null;
                        rawCameraTarget = newPos;
                    }
                    else
                    {
                        rawCameraTarget = Vec.Lerp(curPos, newPos, dt * followSmoothness);
                    }
                }
                else
                {
                    rawCameraTarget = target.GetCameraPosition(rawCameraTarget, dt, followSmoothness, boundaryDis);
                }
            }


            if (PIXEL_SMOOTHING_ENABLED)
            {
                float virtualRatio = ScreenHandler.CUR_WINDOW_SIZE.width / ScreenHandler.GAME.GetTextureWidth();
                worldSpaceCamera.target.X = (int)rawCameraTarget.X;
                screenSpaceCamera.target.X = rawCameraTarget.X - worldSpaceCamera.target.X;
                screenSpaceCamera.target.X *= virtualRatio;

                worldSpaceCamera.target.Y = (int)rawCameraTarget.Y;
                screenSpaceCamera.target.Y = rawCameraTarget.Y - worldSpaceCamera.target.Y;
                screenSpaceCamera.target.Y *= virtualRatio;

                worldSpaceCamera.offset.X = (int)rawCameraOffset.X;
                screenSpaceCamera.offset.X = rawCameraOffset.X - worldSpaceCamera.offset.X;
                screenSpaceCamera.offset.X *= virtualRatio;

                worldSpaceCamera.offset.Y = (int)rawCameraOffset.Y;
                screenSpaceCamera.offset.Y = rawCameraOffset.Y - worldSpaceCamera.offset.Y;
                screenSpaceCamera.offset.Y *= virtualRatio;

                worldSpaceCamera.rotation = rawCameraRotationDeg;
                worldSpaceCamera.zoom = rawCameraZoom;
            }
            else
            {
                worldSpaceCamera.target = rawCameraTarget;
                worldSpaceCamera.offset = rawCameraOffset;
                worldSpaceCamera.zoom = rawCameraZoom;
                worldSpaceCamera.rotation = rawCameraRotationDeg;
            }
            
        }

    }


}
