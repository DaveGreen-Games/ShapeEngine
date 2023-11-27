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
    //added ships checks gamepad index
    //if free gamepad is found than ship is in group a 
    //all ships that do not have a gamepad index are in group b and can be switched through with a button
    
    public class CameraGroupFollowExample : ExampleScene
    {
        internal class SpaceShip : ICameraFollowTarget
        {
            public const float Speed = 500;
            public bool Selected = false;
            
            private Circle hull;
            private Vector2 movementDir;

            private readonly Color hullColorActive = ColorMedium;
            private readonly Color outlineColorActive = ColorHighlight1;
            private readonly Color cockpitColorActive = ColorHighlight3;

            private readonly Color hullColorInactive = ColorMedium;
            private readonly Color outlineColorInactive = ColorLight;
            private readonly Color cockpitColorInactive = ColorRustyRed;
            
            private readonly InputAction iaMoveHor;
            private readonly InputAction iaMoveVer;
            private readonly InputAction iaAddShip;
            private readonly InputAction iaChangeShip;

            public event Action<Vector2>? OnSpawnShipRequested;
            public event Action<SpaceShip>? OnShipChangeRequested;
            public SpaceShip(Vector2 pos, float r)
            {
                hull = new(pos, r);
                
                
                var moveHorKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
                var moveHorGP = new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_X, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
                var moveHorMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.HORIZONTAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
                iaMoveHor = new(moveHorKB, moveHorMW, moveHorGP);
                
                var moveVerKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
                var moveVerGP = new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_Y, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
                var moveVerMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
                iaMoveVer = new(moveVerKB, moveVerMW, moveVerGP);
                
                var addShipKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
                var addShipGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
                var addshipMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
                iaAddShip = new(addShipKB, addshipMB, addShipGP);
                
                var nextShipKB = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
                var nextShipGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_RIGHT);
                var nextShipMB = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
                iaChangeShip = new(nextShipKB, nextShipMB, nextShipGP);
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

            public void Destroy()
            {
                //if(gamepad != null) ShapeLoop.Input.ReturnGamepad(gamepad);
            }
            public void Update(float dt)
            {

                if (Selected)
                {
                    int gamepadIndex = GAMELOOP.CurGamepad?.Index ?? -1;
                    iaMoveHor.Gamepad = gamepadIndex;
                    iaMoveHor.Gamepad = gamepadIndex;
                    iaMoveHor.Gamepad = gamepadIndex;
                    iaChangeShip.Gamepad = gamepadIndex;
                
                    iaMoveHor.Update(dt);
                    iaMoveVer.Update(dt);
                    iaAddShip.Update(dt);
                    iaChangeShip.Update(dt);
                        
                    if(iaChangeShip.State.Pressed) OnShipChangeRequested?.Invoke(this);
                
                    if(iaAddShip.State.Pressed) OnSpawnShipRequested?.Invoke(GetRandomSpawnPosition());
                    Vector2 dir = new(iaMoveHor.State.AxisRaw, iaMoveVer.State.AxisRaw);
                    
                    float lsq = dir.LengthSquared();
                    if (lsq > 0f)
                    {
                        movementDir = dir.Normalize();
                        var movement = movementDir * Speed * dt;
                        hull = new Circle(hull.Center + movement, hull.Radius);
                    }
                }
                else
                {
                    var dir = ShapeRandom.randVec2();
                    movementDir = ShapeVec.Lerp(movementDir, dir, dt * 4).Normalize();
                    var movement = movementDir * Speed * 0.25f * dt;
                    hull = new Circle(hull.Center + movement, hull.Radius);
                }
                
                
                
            }

            private Vector2 GetRandomSpawnPosition()
            {
                return GetPosition() + ShapeRandom.randVec2(0, hull.Radius * 2);
            }
            public void Draw()
            {
                var rightThruster = movementDir.RotateDeg(-25);
                var leftThruster = movementDir.RotateDeg(25);
                
                var outlineColor = Selected ? outlineColorActive : outlineColorInactive;
                var hullColor = Selected ? hullColorActive : hullColorInactive;
                var cockpitColor = Selected ? cockpitColorActive : cockpitColorInactive;
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

            public void FollowStarted()
            {
            }

            public void FollowEnded()
            {
            }

            public Vector2 GetCameraFollowPosition()
            {
                return GetPosition();
            }
        }

        // internal interface IFollowTarget
        // {
        //     public Vector2 GetPosition();
        // }
        // internal class CameraFollower : ICameraFollowTarget
        // {
        //     private readonly List<IFollowTarget> targets = new();
        //     private readonly ShapeCamera camera;
        //     private Vector2 followPosition = new();
        //     // public CameraFollower()
        //     // {
        //     //     camera = new()
        //     //     {
        //     //         Follower =
        //     //         {
        //     //             FollowSpeed = SpaceShip.Speed * 2f
        //     //         }
        //     //     };
        //     // }
        //     public Vector2 GetRandomPosition()
        //     {
        //         return camera.Area.GetRandomPointInside();
        //     }
        //     
        //     public void Reset()
        //     {
        //         camera.Reset();
        //         // camera.Follower.SetTarget(this);
        //         SetCameraValues();
        //     }
        //     public void Activate()
        //     {
        //         GAMELOOP.Camera = camera;
        //         // camera.Follower.SetTarget(this);
        //         SetCameraValues();
        //     }
        //     public void Deactivate()
        //     {
        //         GAMELOOP.ResetCamera();
        //     }
        //     
        //     
        //     public bool AddTarget(IFollowTarget newTarget)
        //     {
        //         if (targets.Contains(newTarget)) return false;
        //         targets.Add(newTarget);
        //         return true;
        //     }
        //
        //     public bool RemoveTarget(IFollowTarget target)
        //     {
        //         return targets.Remove(target);
        //     }
        //
        //     public void Update(float dt)
        //     {
        //         SetCameraValues();
        //     }
        //     public void FollowStarted()
        //     {
        //         
        //     }
        //     public void FollowEnded()
        //     {
        //         
        //     }
        //     public Vector2 GetCameraFollowPosition()
        //     {
        //         return followPosition;
        //     }
        //     private void UpdateCameraFollowBoundary()
        //     {
        //         var rect = camera.Area;
        //         float size = rect.Size.Min();
        //         var boundary = new Vector2(size * 0.15f, size * 0.4f);
        //         // camera.Follower.BoundaryDis = new(boundary);
        //     }
        //
        //     private void SetCameraValues()
        //     {
        //         var cameraArea = camera.Area;
        //         var totalPosition = new Vector2();
        //         var newCameraRect = new Rect(cameraArea.Center, new(), new(0.5f));
        //         
        //         foreach (var target in targets)
        //         {
        //             var pos = target.GetPosition();
        //             totalPosition += pos;
        //             newCameraRect = newCameraRect.Enlarge(pos);
        //         }
        //
        //         var curSize = cameraArea.Size;
        //         var newSize = newCameraRect.Size;
        //         float f = 1f - (newSize.GetArea() / curSize.GetArea());
        //         camera.SetZoom(f);
        //         followPosition = totalPosition / targets.Count;
        //         
        //         UpdateCameraFollowBoundary();
        //     }
        // }
        //
        
        private readonly Font font;
        private readonly Rect universe = new(new Vector2(0f), new Vector2(10000f), new Vector2(0.5f));
        private readonly List<Star> stars = new();

        private readonly CameraFollowerMulti cameraFollower = new();
        private readonly ShapeCamera camera = new();
        private readonly List<SpaceShip> spaceShips = new();
        private SpaceShip? ActiveSpaceShip = null;

        // private List<SpaceShip> spawned = new();
        
        
        
        public CameraGroupFollowExample()
        {
            Title = "Camera Group Follow Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);
                
            GenerateStars(2500);
            camera.Follower = cameraFollower;

            AddShip(new(0));
            
            
            
            // SetShipActive(spaceShip1);
        }

        private void OnChangeShipRequested(SpaceShip ship)
        {
            if(ship == ActiveSpaceShip) NextShip();
        }
        private void SelectShip(SpaceShip ship)
        {
            if (!spaceShips.Contains(ship)) return;
            if (ActiveSpaceShip == ship) return;
            
            if (ActiveSpaceShip != null) ActiveSpaceShip.Selected = false;
            ActiveSpaceShip = ship;
            ActiveSpaceShip.Selected = true;
        }

        private void NextShip()
        {
            if (spaceShips.Count <= 0) return;
            
            if(ActiveSpaceShip == null) SelectShip(spaceShips[0]);
            else
            {
                var index = spaceShips.IndexOf(ActiveSpaceShip);
                index = ShapeMath.WrapIndex(spaceShips.Count, index + 1);
                SelectShip(spaceShips[index]);
            }
        }

        private void AddShip(Vector2 pos)
        {
            var spaceShip = new SpaceShip(pos, 30f);
            spaceShip.OnSpawnShipRequested += AddShip;
            spaceShip.OnShipChangeRequested += OnChangeShipRequested;
            spaceShips.Add(spaceShip);
            // spawned.Add(spaceShip);
            cameraFollower.AddTarget(spaceShip);
            if (ActiveSpaceShip == null)
            {
                SelectShip(spaceShip);
            }
        }
        private void DestroyShips()
        {
            // var area = camera.Area;
            // foreach (var ship in spaceShips)
            // {
            //     ship.Reset(area.GetRandomPointInside(), 30f);
            // }
            foreach (var ship in spaceShips)
            {
                ship.Destroy();
                ship.OnSpawnShipRequested -= AddShip;
                ship.OnShipChangeRequested -= OnChangeShipRequested;
            }
            ActiveSpaceShip = null;
            
            spaceShips.Clear();
            cameraFollower.Reset();
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
            GAMELOOP.Camera = camera;
            
        }

        public override void Deactivate()
        {
            GAMELOOP.ResetCamera();
        }
        public override GameObjectHandler? GetGameObjectHandler()
        {
            return null;
        }
        public override void Reset()
        {
            GAMELOOP.ScreenEffectIntensity = 1f;
            DestroyShips();
            stars.Clear();
            GenerateStars(2500);
            
            camera.Reset();
            cameraFollower.Reset();
            
            AddShip(new(0));
        }

        
        // private void ShakeCamera()
        // {
        //     camera.Shake(ShapeRandom.randF(0.8f, 2f), new Vector2(100, 100), 0, 25, 0.75f);
        // }
        //
        
        protected override void HandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            
            // iaChangeShip.Update(dt);
            // if (iaChangeShip.State.Pressed)
            // {
            //      NextShip();       
            // }
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
            for (int i = spaceShips.Count - 1; i >= 0; i--)
            {
                var ship = spaceShips[i];
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
            
            cameraFollower.Draw();
            
        }
        protected override void DrawGameUIExample(ScreenInfo ui)
        {
            
        }
        protected override void DrawUIExample(ScreenInfo ui)
        {
            // var rects = GAMELOOP.UIRects.GetRect("bottom center").SplitV(0.5f);
            // DrawInputDescription(GAMELOOP.UIRects.GetRect("bottom center"));
            // DrawCameraSizeInfo(rects.top);
            // DrawCameraInfo(rects.bottom);
           
        }

        // private void DrawCameraSizeInfo(Rect rect)
        // {
        //     var targetSize = ShapeMath.RoundToDecimals(camera.TargetSize, 2);
        //     var curSize = ShapeMath.RoundToDecimals(camera.CurSize, 2);
        //     var dif = ShapeMath.RoundToDecimals(camera.Dif, 2);
        //     
        //     string text = $"Size: Cur {curSize} | Target {targetSize} | Dif {dif}";
        //     font.DrawText(text, rect, 1f, new Vector2(0.5f, 0.5f), ColorHighlight3);
        // }
        // private void DrawCameraInfo(Rect rect)
        // {
        //     //var pos = camera.Position;
        //     // var x = (int)pos.X;
        //     // var y = (int)pos.Y;
        //     // var rot = (int)camera.RotationDeg;
        //     var zoomLevel = ShapeMath.RoundToDecimals(camera.ZoomLevel, 2);// (int)(ShapeUtils.GetFactor(camera.ZoomLevel, 0.1f, 5f) * 100f);
        //     var zoomFactor = ShapeMath.RoundToDecimals(camera.ZoomFactor, 2);
        //     // var prevZoomLevel = ShapeMath.RoundToDecimals(cameraFollower.prevZoomLevel, 2);
        //     string text = $"Zoom: Level {zoomLevel} | Factor {zoomFactor}";
        //     font.DrawText(text, rect, 1f, new Vector2(0.5f, 0.5f), ColorHighlight3);
        // }
       
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
