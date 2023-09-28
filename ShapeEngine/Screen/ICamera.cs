using ShapeEngine.Lib;
using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Timing;

namespace ShapeEngine.Screen
{
    /*public interface ICamera
    {
        public Vector2 WorldToScreen(Vector2 absolutePos);
        public Vector2 ScreenToWorld(Vector2 relativePos);
        public void AdjustSize(Vector2 newSize);
        public Camera2D GetCamera();
        public Rect GetArea();
        public float GetZoomFactor();
        public float GetZoomFactorInverse();
        public void Update(float dt);
        //public bool IsPixelSmoothingCameraEnabled();
        //public Camera2D GetPixelSmoothingCamera();

    }
    public class BasicCamera : ICamera
    {
        public float BaseRotationDeg { get; private set; } = 0f;
        public Vector2 BaseOffset { get; private set; } = new(0f);
        public float BaseZoom { get; private set; } = 1f;
        public Vector2 BaseSize { get; private set; } = new(0f);
        public Vector2 Origin { get; private set; } = new(0f);
        public Vector2 Translation { get; set; } = new(0f);
        public Vector2 Position { get; set; } = new(0f);
        public float RotationDeg { get; set; } = 0f;
        public float Zoom { get; set; } = 1f;

        //public Camera2D WorldCamera { get; protected set; }



        public BasicCamera(Vector2 pos, Vector2 size, Vector2 origin, float baseZoom, float rotation)
        {
            this.BaseSize = size;
            this.Origin = origin;
            this.BaseOffset = size * origin;
            this.BaseZoom = baseZoom;
            this.BaseRotationDeg = rotation;
            //this.WorldCamera = new() { offset = BaseOffset, rotation = BaseRotationDeg, zoom = baseZoom * Zoom, target = pos };
        }
        public void AdjustSize(Vector2 newSize)
        {
            this.BaseSize = newSize;
            this.BaseOffset = newSize * Origin;
        }
        public void AdjustOrigin(Vector2 newOrigin)
        {
            this.Origin = newOrigin;
            this.BaseOffset = this.BaseSize * this.Origin;
        }
        public virtual void Update(float dt) 
        { 
            //WorldCamera = SetCameraValues(Position, BaseOffset + Translation, BaseZoom * Zoom, BaseRotationDeg + RotationDeg); 
        }
        //protected Camera2D SetCameraValues(Vector2 target, Vector2 offset, float zoom, float rotationDeg)
        //{
        //    var c = new Camera2D();
        //    c.target = target;
        //    c.offset = offset;
        //    c.zoom = zoom;
        //    c.rotation = rotationDeg;
        //    return c;
        //}
        public void ResetZoom() { Zoom = 1f; }
        public void ResetRotation() { RotationDeg = 0f; }
        public void ResetTranslation() { Translation = new(0f); }


        public virtual Vector2 WorldToScreen(Vector2 pos)
        {
            return Raylib.GetWorldToScreen2D(pos, GetCamera());// - Origin * BaseSize;
            //float zoomFactor = 1 / WorldCamera.zoom;
            //Vector2 cPos = WorldCamera.target - WorldCamera.offset * zoomFactor;
            //Vector2 p = (absolutePos - cPos) * WorldCamera.zoom;
            //return p;
        }
        public virtual Vector2 ScreenToWorld(Vector2 pos)
        {
            return Raylib.GetScreenToWorld2D(pos, GetCamera());// + Origin * BaseSize;
            //float zoomFactor = 1 / WorldCamera.zoom;
            //Vector2 p = relativePos * zoomFactor;
            //Vector2 cPos = WorldCamera.target - WorldCamera.offset * zoomFactor;
            //p += cPos;
            //return p;
        }


        public virtual Rect GetArea()
        {
            var c = GetCamera();
            float zoomFactor = 1f / c.zoom;
            Vector2 size = BaseSize * zoomFactor;
            return new(c.target, size, Origin);
        }
        public virtual Camera2D GetCamera() 
        {
            //Position, BaseOffset + Translation, BaseZoom* Zoom, BaseRotationDeg +RotationDeg
            return new Camera2D { target = Position, offset = BaseOffset + Translation, zoom = BaseZoom * Zoom, rotation = BaseRotationDeg + RotationDeg }; 
        }

        public float GetZoomFactor() { return GetCamera().zoom; }
        public float GetZoomFactorInverse() { return 1f / GetCamera().zoom; }

        //public bool IsPixelSmoothingCameraEnabled() { return false; }
        //public Camera2D GetPixelSmoothingCamera() { return WorldCamera; }
    }
    public class EffectCamera : BasicCamera
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

        protected Vector2 rawCameraOffset = new();
        protected float rawCameraZoom = 1f;
        protected float rawCameraRotationDeg = 0f;

        public EffectCamera(Vector2 pos, Vector2 size, Vector2 origin, float baseZoom, float rotation) : base(pos, size, origin, baseZoom, rotation)
        {
            this.CameraTweens.OnItemUpdated += OnCameraTweenUpdated;
        }

        public override void Update(float dt)
        {
            CameraTweens.Update(dt);
            shake.Update(dt);
            Vector2 shakeOffset = new(shake.Get(ShakeX), shake.Get(ShakeY));
            this.rawCameraOffset = BaseOffset + shakeOffset + cameraTweenTotalOffset + Translation;
            this.rawCameraRotationDeg = BaseRotationDeg + shake.Get(ShakeRot) + RotationDeg + cameraTweenTotalRotationDeg;
            this.rawCameraZoom = ((BaseZoom + shake.Get(ShakeZoom)) * Zoom) / cameraTweenTotalScale;
            cameraTweenTotalOffset = new(0f);
            cameraTweenTotalRotationDeg = 0f;
            cameraTweenTotalScale = 1f;

            //WorldCamera = SetCameraValues(Position, rawCameraOffset, rawCameraZoom, rawCameraRotationDeg);
        }
        public override Camera2D GetCamera()
        {
            return new Camera2D() { target = Position, offset = rawCameraOffset, zoom = rawCameraZoom, rotation = rawCameraRotationDeg};
        }
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

        private void OnCameraTweenUpdated(ICameraTween tween)
        {
            cameraTweenTotalOffset += tween.GetOffset();
            cameraTweenTotalRotationDeg += tween.GetRotationDeg();
            cameraTweenTotalScale *= tween.GetScale();

        }
    }
    public class FollowCamera : EffectCamera
    {
        public IAreaObject? Target { get; private set; } = null;
        public IAreaObject? NewTarget { get; private set; } = null;
        public float BoundaryDis { get; set; } = 0f;
        public float FollowSmoothness { get; set; } = 1f;
        public FollowCamera(Vector2 pos, Vector2 size, Vector2 origin, float baseZoom, float rotation) : base(pos, size, origin, baseZoom, rotation)
        {
        }
        public override void Update(float dt)
        {
            Vector2 rawCameraTarget = Position;
            if (Target != null)
            {
                if (NewTarget != null)
                {
                    Vector2 curPos = rawCameraTarget;
                    Vector2 newPos = new();// NewTarget.GetCameraFollowPosition(curPos);
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
                    Vector2 newPos = new(); // Target.GetCameraFollowPosition(curPos);
                    if (BoundaryDis > 0f)
                    {
                        float boundaryDisSq = BoundaryDis * BoundaryDis;
                        float disSq = (curPos - newPos).LengthSquared();
                        if (disSq > boundaryDisSq)
                        {
                            rawCameraTarget = curPos.Lerp(newPos, dt * FollowSmoothness);
                        }
                    }
                    else rawCameraTarget = curPos.Lerp(newPos, dt * FollowSmoothness);
                }
            }
            Position = rawCameraTarget;
            base.Update(dt);
        }

        public void SetTarget(IAreaObject target)
        {
            this.Target = target;
            Position = new(); //target.GetCameraFollowPosition(Position);
        }
        public void ChangeTarget(IAreaObject newTarget)
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
    }
    */
    
}
