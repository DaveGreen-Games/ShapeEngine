using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Random;
using ShapeEngine.Screen;
using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;

namespace Examples.Scenes.ExampleScenes
{
    public class ShipInputExample : ExampleScene
    {
        internal class SpaceShip : IFollowTarget
        {
            public const float Speed = 500;
            
            private Circle hull; // { get; private set; }
            private Vector2 movementDir;

            private readonly Color hullColor;
            private readonly Color outlineColor;
            private readonly Color cockpitColor;

            private InputAction iaMoveHor;
            private InputAction iaMoveVer;

            private Gamepad? gamepad;
            
            private void SetupInput(bool canUseKeyboard)
            {
                if (canUseKeyboard)
                {
                    var moveHorKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
                    var moveHorGP = new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_X, 0.1f, ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM, true);
                    var moveHorMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.HORIZONTAL, 0.2f, ShapeKeyboardButton.LEFT_SHIFT, true);
                    iaMoveHor = new(moveHorKB, moveHorGP, moveHorMW);
                
                    var moveVerKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
                    var moveVerGP = new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_Y, 0.1f, ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM, true);
                    var moveVerMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f, ShapeKeyboardButton.LEFT_SHIFT, true);
                    iaMoveVer = new(moveVerKB, moveVerGP, moveVerMW);
                }
                else
                {
                    var moveHorGP = new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_X, 0.1f, ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM, true);
                    iaMoveHor = new(moveHorGP);
                
                    var moveVerGP = new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_Y, 0.1f, ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM, true);
                    iaMoveVer = new(moveVerGP);
                }
                

                gamepad = ShapeLoop.Input.RequestGamepad();
            }
            
            public SpaceShip(Vector2 pos, float r, Color hullColor, Color cockpitColor, Color outlineColor, bool canUseKeyboard = false)
            {
                hull = new(pos, r);
                this.hullColor = hullColor;
                this.cockpitColor = cockpitColor;
                this.outlineColor = outlineColor;
                SetupInput(canUseKeyboard);
            }

            public string GetInputDescription(InputDevice inputDevice)
            {
                string hor = iaMoveHor.GetInputTypeDescription(inputDevice, true, 1, false, false);
                string ver = iaMoveVer.GetInputTypeDescription(inputDevice, true, 1, false, false);
                return $"Move Horizontal [{hor}] Vertical [{ver}]";
            }
            public void Reset(Vector2 pos, float r)
            {
                hull = new(pos, r);
            } 
            
            public void Update(float dt)
            {
                if (gamepad != null)
                {
                    if (!gamepad.Available || !gamepad.Connected)
                    {
                        gamepad = null;
                    }
                }

                if (gamepad == null)
                {
                    gamepad = ShapeLoop.Input.RequestGamepad();
                }

                var gamepadIndex = gamepad?.Index ?? -1;
                iaMoveHor.Gamepad = gamepadIndex;
                iaMoveVer.Gamepad = gamepadIndex;
                
                iaMoveHor.Update(dt);
                iaMoveVer.Update(dt);
                
                Vector2 dir = new(iaMoveHor.State.AxisRaw, iaMoveVer.State.AxisRaw);
                float lsq = dir.LengthSquared();
                if (lsq > 0f)
                {
                    movementDir = dir.Normalize();
                    var movement = movementDir * Speed * dt;
                    hull = new Circle(hull.Center + movement, hull.Radius);
                }
                
            }
            public void Draw()
            {
                var rightThruster = movementDir.RotateDeg(-25);
                var leftThruster = movementDir.RotateDeg(25);
                DrawCircleV(hull.Center - rightThruster * hull.Radius, hull.Radius / 6, outlineColor);
                DrawCircleV(hull.Center - leftThruster * hull.Radius, hull.Radius / 6, outlineColor);
                hull.Draw(hullColor);
                DrawCircleV(hull.Center + movementDir * hull.Radius * 0.66f, hull.Radius * 0.33f, cockpitColor);

                hull.DrawLines(4f, outlineColor);
            }


            public Vector2 GetPosition()
            {
                return hull.Center;
            }
        }

        internal interface IFollowTarget
        {
            public Vector2 GetPosition();
        }
        internal class CameraFollower : ICameraFollowTarget
        {
            private readonly List<IFollowTarget> targets = new();
            private readonly ShapeCamera camera;
            private Vector2 followPosition = new();
            public CameraFollower()
            {
                camera = new()
                {
                    Follower =
                    {
                        FollowSpeed = SpaceShip.Speed * 2f
                    }
                };
            }
            public Vector2 GetRandomPosition()
            {
                return camera.Area.GetRandomPointInside();
            }
            
            public void Reset()
            {
                camera.Reset();
                camera.Follower.SetTarget(this);
                SetCameraValues();
            }
            public void Activate()
            {
                GAMELOOP.Camera = camera;
                camera.Follower.SetTarget(this);
                SetCameraValues();
            }
            public void Deactivate()
            {
                GAMELOOP.ResetCamera();
            }
            
            
            public bool AddTarget(IFollowTarget newTarget)
            {
                if (targets.Contains(newTarget)) return false;
                targets.Add(newTarget);
                return true;
            }

            public bool RemoveTarget(IFollowTarget target)
            {
                return targets.Remove(target);
            }

            public void Update(float dt)
            {
                SetCameraValues();
            }
            public void FollowStarted()
            {
                
            }
            public void FollowEnded()
            {
                
            }
            public Vector2 GetCameraFollowPosition()
            {
                return followPosition;
            }
            private void UpdateCameraFollowBoundary()
            {
                var rect = camera.Area;
                float size = rect.Size.Min();
                var boundary = new Vector2(size * 0.15f, size * 0.4f);
                camera.Follower.BoundaryDis = new(boundary);
            }

            private void SetCameraValues()
            {
                var cameraArea = camera.Area;
                var totalPosition = new Vector2();
                var newCameraRect = new Rect(cameraArea.Center, new(), new(0.5f));
                
                foreach (var target in targets)
                {
                    var pos = target.GetPosition();
                    totalPosition += pos;
                    newCameraRect = newCameraRect.Enlarge(pos);
                }

                var curSize = cameraArea.Size;
                var newSize = newCameraRect.Size;
                float f = 1f - (newSize.GetArea() / curSize.GetArea());
                camera.SetZoom(f);
                followPosition = totalPosition / targets.Count;
                
                UpdateCameraFollowBoundary();
            }
        }
        
        private readonly Font font;
        private readonly Rect universe = new(new Vector2(0f), new Vector2(10000f), new Vector2(0.5f));
        private readonly List<Star> stars = new();

        private readonly CameraFollower cameraHandler = new();
        private readonly List<SpaceShip> spaceShips = new();
        private bool setupFinished = false;

        
        public ShipInputExample()
        {
            Title = "Ship Input Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);
                
            GenerateStars(2500);

            var spaceShip1 = new SpaceShip(new Vector2(), 30f, BLUE, PURPLE, LIGHTGRAY, true);
            var spaceShip2 = new SpaceShip(new Vector2(), 30f, GREEN, LIME, LIGHTGRAY);
            spaceShips.Add(spaceShip1);
            spaceShips.Add(spaceShip2);
            cameraHandler.AddTarget(spaceShip1);
            cameraHandler.AddTarget(spaceShip2);
        }
        private void GenerateStars(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                var pos = universe.GetRandomPointInside();

                ChanceList<float> sizes = new((45, 1.5f), (25, 2f), (15, 2.5f), (10, 3f), (3, 3.5f), (2, 4f));
                float size = sizes.Next();
                Star star = new(pos, size);
                stars.Add(star);
            }
        }
        
        public override void Activate(IScene oldScene)
        {
            if (!setupFinished)
            {
                foreach (var ship in spaceShips)
                {
                    ship.Reset(cameraHandler.GetRandomPosition(), 30f);
                }

                setupFinished = true;
            }
            
            cameraHandler.Activate();
        }

        public override void Deactivate()
        {
            cameraHandler.Deactivate();
            
        }
        public override GameObjectHandler? GetGameObjectHandler()
        {
            return null;
        }
        public override void Reset()
        {
            GAMELOOP.ScreenEffectIntensity = 1f;
            
            foreach (var ship in spaceShips)
            {
                ship.Reset(cameraHandler.GetRandomPosition(), 30f);
            }
            stars.Clear();
            GenerateStars(2500);
            
            cameraHandler.Reset();
        }

        
        // private void ShakeCamera()
        // {
        //     camera.Shake(ShapeRandom.randF(0.8f, 2f), new Vector2(100, 100), 0, 25, 0.75f);
        // }
        //
        
        protected override void HandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            // int gamepadIndex = GAMELOOP.CurGamepad?.Index ?? -1;
            // iaShakeCamera.Gamepad = gamepadIndex;
            // iaShakeCamera.Update(dt);
            //
            // iaRotateCamera.Gamepad = gamepadIndex;
            // iaRotateCamera.Update(dt);
            //
            // iaToggleDrawCameraFollowBoundary.Gamepad = gamepadIndex;
            // iaToggleDrawCameraFollowBoundary.Update(dt);
            //
            // if (iaToggleDrawCameraFollowBoundary.State.Pressed)
            // {
            //     drawCameraFollowBoundary = !drawCameraFollowBoundary;
            // }
            //
            // HandleRotation(dt);
            //
            // if (iaShakeCamera.State.Pressed) ShakeCamera();

        }

        
        protected override void UpdateExample(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            cameraHandler.Update(dt);
            foreach (var ship in spaceShips)
            {
                ship.Update(dt);
            }
        }
        protected override void DrawGameExample(ScreenInfo game)
        {
            foreach (var star in stars)
            {
                star.Draw();
            }

            foreach (var ship in spaceShips)
            {
                ship.Draw();
            }

            // if (drawCameraFollowBoundary)
            // {
            //     float thickness = 2f * camera.ZoomFactor;
            //     var boundarySize = camera.Follower.BoundaryDis.ToVector2();
            //     var boundaryCenter = camera.Position;
            //     var innerBoundary = new Circle(boundaryCenter, boundarySize.X);
            //     var outerBoundary = new Circle(boundaryCenter, boundarySize.Y);
            //
            //     var innerColor = ColorHighlight1.ChangeAlpha((byte)150);
            //     var outerColor = ColorHighlight2.ChangeAlpha((byte)150);
            //     
            //     innerBoundary.DrawLines(thickness, innerColor);
            //     outerBoundary.DrawLines(thickness, outerColor);
            // }
            
        }
        protected override void DrawGameUIExample(ScreenInfo ui)
        {
            
        }
        protected override void DrawUIExample(ScreenInfo ui)
        {
            // DrawInputDescription(GAMELOOP.UIRects.GetRect("bottom center"));
           
        }

       
        // private void DrawInputDescription(Rect rect)
        // {
        //     var rects = rect.SplitV(0.35f);
        //     var curDevice = Input.CurrentInputDevice;
        //     // var curDeviceNoMouse = Input.CurrentInputDeviceNoMouse;
        //     string shakeCameraText = iaShakeCamera.GetInputTypeDescription(curDevice, true, 1, false);
        //     string rotateCameraText = iaRotateCamera.GetInputTypeDescription(curDevice, true, 1, false);
        //     string toggleDrawText = iaToggleDrawCameraFollowBoundary.GetInputTypeDescription(Input.CurrentInputDeviceNoMouse, true, 1, false);
        //     string moveText = ship.GetInputDescription(curDevice);
        //     string onText = drawCameraFollowBoundary ? "ON" : "OFF";
        //     string textTop = $"Draw Camera Follow Boundary {onText} - Toggle {toggleDrawText}";
        //     string textBottom = $"{moveText} | Shake {shakeCameraText} | Rotate {rotateCameraText}";
        //     font.DrawText(textTop, rects.top, 1f, new Vector2(0.5f, 0.5f), ColorMedium);
        //     font.DrawText(textBottom, rects.bottom, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        // }
    }

}
