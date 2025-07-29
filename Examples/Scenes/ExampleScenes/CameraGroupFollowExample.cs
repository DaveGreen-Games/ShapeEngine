using Raylib_cs;
using ShapeEngine.Core;
using ShapeEngine.StaticLib;
using ShapeEngine.Random;
using ShapeEngine.Screen;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.RectDef;
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
            public const float Speed = 500f;
            public const float Size = 30f;
            public const float MaxDistance = 2500f;
            public const float MinDistance = 250f;
            public const float TurningSpeedDeg = 120f;

            private bool selected = false;
            public bool Selected
            {
                get => selected;
                set
                {
                    if (value != selected)
                    {
                        invisibleTimer = 2f;
                        selected = value;
                    }
                    
                }
            }
            
            private Circle hull;
            private Vector2 movementDir;

            private readonly PaletteColor hullColorActive = Colors.PcMedium;
            private readonly PaletteColor outlineColorActive = Colors.PcHighlight;
            private readonly PaletteColor cockpitColorActive = Colors.PcSpecial;
            
            private readonly PaletteColor hullColorInactive = Colors.PcMedium;
            private readonly PaletteColor outlineColorInactive = Colors.PcMedium;
            private readonly PaletteColor cockpitColorInactive = Colors.PcLight;
            
            private readonly InputAction iaMoveHor;
            private readonly InputAction iaMoveVer;
            private readonly InputActionTree inputActionTree;
            private float outOfBoundsTimer = 0f;
            private float invisibleTimer;

            public SpaceShip(Vector2 pos)
            {
                hull = new(pos, Size);
                movementDir = Rng.Instance.RandVec2();
                invisibleTimer = 2f;
                
                InputActionSettings defaultSettings = new();
                
                var modifierKeySetGpReversed = new ModifierKeySet(ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
                
                var moveHorKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
                var moveHorGP = new InputTypeGamepadJoyAxis(ShapeGamepadJoyAxis.LEFT_X, 0.15f, false, modifierKeySetGpReversed);
                // var moveHorMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.HORIZONTAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
                iaMoveHor = new(defaultSettings,moveHorKB, moveHorGP);
                
                var moveVerKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
                var moveVerGP = new InputTypeGamepadJoyAxis(ShapeGamepadJoyAxis.LEFT_Y, 0.15f, false, modifierKeySetGpReversed);
                // var moveVerMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
                iaMoveVer = new(defaultSettings,moveVerKB, moveVerGP);

                inputActionTree = [iaMoveHor, iaMoveVer];
            }

            public bool Overlap(SpaceShip other)
            {
                if (invisibleTimer > 0f) return false;
                
                return hull.OverlapShape(other.hull);
            }
            public string GetInputDescription(InputDeviceType inputDeviceType)
            {
                if (inputDeviceType == InputDeviceType.Mouse)
                {
                    return "Move Horizontal [Mx] Vertical [My]";
                }
                
                string hor = iaMoveHor.GetInputTypeDescription(inputDeviceType, true, 1, false, false);
                string ver = iaMoveVer.GetInputTypeDescription(inputDeviceType, true, 1, false, false);
                return $"Move Horizontal [{hor}] Vertical [{ver}]";
            }
            
            public void Destroy()
            {
                //if(gamepad != null) ShapeLoop.Input.ReturnGamepad(gamepad);
            }
            public void Update(float dt, Vector2 target)
            {
                if (outOfBoundsTimer > 0f)
                {
                    outOfBoundsTimer -= dt;
                    if (outOfBoundsTimer < 0f) outOfBoundsTimer = 0f;
                }

                if (invisibleTimer > 0f)
                {
                    invisibleTimer -= dt;
                    if (invisibleTimer < 0f) invisibleTimer = 0f;
                }
                
                float radius = Size * (Selected ? 1f : 0.75f);
                if (Selected)
                {
                    inputActionTree.CurrentGamepad = GAMELOOP.CurGamepad;
                    inputActionTree.Update(dt);
                    
                    if (ShapeInput.CurrentInputDeviceType == InputDeviceType.Mouse)
                    {
                        var dir = ExampleScene.CalculateMouseMovementDirection(GAMELOOP.GameScreenInfo.MousePos, GAMELOOP.Camera);
                        float lsq = dir.LengthSquared();
                        if (lsq > 0f)
                        {
                            movementDir = dir;
                            var movement = movementDir * Speed * dt;
                            hull = new Circle(hull.Center + movement, radius);
                        }
                        else
                        {
                            hull = new Circle(hull.Center, radius);
                        }
                    }
                    else
                    {
                        Vector2 dir = new(iaMoveHor.State.AxisRaw, iaMoveVer.State.AxisRaw);
                    
                        float lsq = dir.LengthSquared();
                        if (lsq > 0f)
                        {
                            movementDir = dir.Normalize();
                            var movement = movementDir * Speed * dt;
                            hull = new Circle(hull.Center + movement, radius);
                        }
                        else
                        {
                            hull = new Circle(hull.Center, radius);
                        }
                    }
                    
                    
                }
                else
                {
                    Vector2 dir;
                    float speed;
                    float turnSpeedDeg;
                    var targetDir = target - hull.Center;
                    var targetDis = targetDir.LengthSquared();
                    if (targetDis < MinDistance * MinDistance)
                    {
                        outOfBoundsTimer = 0;
                        dir = Rng.Instance.RandVec2();
                        speed = Speed * 0.25f;
                        turnSpeedDeg = TurningSpeedDeg; 
                    }
                    else if (targetDis > MaxDistance * MaxDistance || outOfBoundsTimer > 0)
                    {
                        dir = targetDir;
                        speed = Speed * 2f;
                        turnSpeedDeg = TurningSpeedDeg * 2f;
                        if(outOfBoundsTimer <= 0) outOfBoundsTimer = 5f;
                    }
                    else
                    {
                        dir = Rng.Instance.RandVec2();
                        speed = Speed * 0.25f;
                        turnSpeedDeg = TurningSpeedDeg;
                    }
                    
                    var rotRad = ShapeMath.AimAt(movementDir.AngleRad(), dir.AngleRad(), turnSpeedDeg * ShapeMath.DEGTORAD, dt);
                    movementDir = movementDir.Rotate(rotRad);
                    // var angle = ShapeMath.GetShortestAngleRad(movementDir.AngleRad(), dir.AngleRad());
                    // var angleMovementRad = MathF.Min(angle, TurningSpeed * DEG2RAD * dt);
                    // movementDir = movementDir.Rotate(angleMovementRad);
                    
                    // movementDir = ShapeVec.Lerp(movementDir, dir, dt).Normalize();
                    
                    var movement = movementDir * speed * dt;
                    hull = new Circle(hull.Center + movement, radius);
                }
                
                
                
            }

            public Vector2 GetRandomSpawnPosition()
            {
                return GetPosition() + Rng.Instance.RandVec2(0, hull.Radius * 2);
            }
            public void Draw()
            {
                var rightThruster = movementDir.RotateDeg(-25);
                var leftThruster = movementDir.RotateDeg(25);
                
                var outlineColor = Selected ? outlineColorActive : outlineColorInactive;
                var hullColor = Selected ? hullColorActive : hullColorInactive;
                var cockpitColor = Selected ? cockpitColorActive : cockpitColorInactive;
                CircleDrawing.DrawCircle(hull.Center - rightThruster * hull.Radius, hull.Radius / 6, outlineColor.ColorRgba, 12);
                CircleDrawing.DrawCircle(hull.Center - leftThruster * hull.Radius, hull.Radius / 6, outlineColor.ColorRgba, 12);
                hull.Draw(hullColor.ColorRgba);
                CircleDrawing.DrawCircle(hull.Center + movementDir * hull.Radius * 0.66f, hull.Radius * 0.33f, cockpitColor.ColorRgba, 12);

                hull.DrawLines(4f, outlineColor.ColorRgba);

                if (invisibleTimer > 0f)
                {
                    Circle shield = new(hull.Center, hull.Radius + 12f);
                    shield.DrawLines(2f, Colors.Special);
                }
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
        
        private readonly Font font;
        private readonly Rect universe = new(new Vector2(0f), new Size(10000f), new AnchorPoint(0.5f));
        private readonly List<Star> stars = new();

        private readonly CameraFollowerMulti cameraFollower = new();
        private readonly ShapeCamera camera = new();
        private readonly List<SpaceShip> spaceShips = new();
        private SpaceShip? ActiveSpaceShip = null;

        private readonly InputAction iaAddShip;
        private readonly InputAction iaNextShip;
        private readonly InputAction iaCenterTarget;
        private readonly InputActionTree inputActionTree;
        private bool centerTargetActive = false;
        
        
        public CameraGroupFollowExample()
        {
            Title = "Camera Group Follow Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);
                
            GenerateStars(2500);
            camera.Follower = cameraFollower;

            AddShip();
            
            InputActionSettings defaultSettings = new();
            
            var addShipKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
            var addShipGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
            var addshipMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
            iaAddShip = new(defaultSettings,addShipKB, addshipMB, addShipGP);
                
            var nextShipKB = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
            var nextShipGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_RIGHT);
            var nextShipMB = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            iaNextShip = new(defaultSettings,nextShipKB, nextShipMB, nextShipGP);
            
            var centerTargetKB = new InputTypeKeyboardButton(ShapeKeyboardButton.E);
            var centerTargetGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_TOP);
            iaCenterTarget = new(defaultSettings,centerTargetKB, centerTargetGP);
            
            inputActionTree = [iaAddShip, iaNextShip, iaCenterTarget];
        }

       
        private void SelectShip(SpaceShip ship)
        {
            if (!spaceShips.Contains(ship)) return;
            if (ActiveSpaceShip == ship) return;
            
            if (ActiveSpaceShip != null) ActiveSpaceShip.Selected = false;
            ActiveSpaceShip = ship;
            ActiveSpaceShip.Selected = true;
            
            if(centerTargetActive) cameraFollower.CenterTarget = ship;
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

        private void AddShip()
        {
            var pos = ActiveSpaceShip?.GetRandomSpawnPosition() ?? new();
            var spaceShip = new SpaceShip(pos);
            spaceShips.Add(spaceShip);
            cameraFollower.AddTarget(spaceShip);
            if (ActiveSpaceShip == null)
            {
                SelectShip(spaceShip);
            }
        }

        
        private void DestroyShips()
        {
            foreach (var ship in spaceShips)
            {
                ship.Destroy();
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

        protected override void OnActivate(Scene oldScene)
        {
            GAMELOOP.Camera = camera;
            
        }

        protected override void OnDeactivate()
        {
            GAMELOOP.ResetCamera();
        }
        
        public override void Reset()
        {
            GAMELOOP.ScreenEffectIntensity = 1f;
            DestroyShips();
            stars.Clear();
            GenerateStars(2500);
            
            camera.Reset();
            cameraFollower.Reset();
            
            AddShip();
        }

        
        // private void ShakeCamera()
        // {
        //     camera.Shake(ShapeRandom.randF(0.8f, 2f), new Vector2(100, 100), 0, 25, 0.75f);
        // }
        //
        private void DestroyShip(SpaceShip ship)
        {
            if (ActiveSpaceShip == null || ship == ActiveSpaceShip || spaceShips.Count < 2) return;

            if (spaceShips.Remove(ship))
            {
                ship.Destroy();
                cameraFollower.RemoveTarget(ship);
            }
        }

        private SpaceShip? FindClosestShip(SpaceShip origin)
        {
            var pos = origin.GetPosition();
            SpaceShip? next = null;
            float minDisSq = float.PositiveInfinity;
            foreach (var ship in spaceShips)
            {
                if(ship == origin) continue;
                var dir = ship.GetPosition() - pos;
                var disSq = dir.LengthSquared();
                if (disSq < minDisSq)
                {
                    minDisSq = disSq;
                    next = ship;
                }
            }

            return next;
        }
        protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI)
        {
            // int gamepadIndex = GAMELOOP.CurGamepad?.Index ?? -1;
            var gamepad = GAMELOOP.CurGamepad;
            
            GAMELOOP.MouseControlEnabled = gamepad?.IsDown(ShapeGamepadTriggerAxis.RIGHT, 0.1f) ?? true;
            
            inputActionTree.CurrentGamepad = gamepad;
            inputActionTree.Update(dt);

            if (iaCenterTarget.State.Pressed)
            {
                centerTargetActive = !centerTargetActive;
                if (!centerTargetActive)
                {
                    cameraFollower.CenterTarget = null;
                }
                else
                {
                    if (ActiveSpaceShip != null) cameraFollower.CenterTarget = ActiveSpaceShip;
                }
            }
            
            if (iaNextShip.State.Pressed)
            {
                if (ActiveSpaceShip != null)
                {
                    var next = FindClosestShip(ActiveSpaceShip);
                    if(next != null) SelectShip(next);
                }
            }

            if (iaAddShip.State.Pressed)
            {
                AddShip();
            }
        }

        
        protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            var targetPos = ActiveSpaceShip?.GetPosition() ?? new();

            SpaceShip? next = null;
            for (int i = spaceShips.Count - 1; i >= 0; i--)
            {
                var ship = spaceShips[i];
                ship.Update(time.Delta, targetPos);


                if (ActiveSpaceShip != null && ship != ActiveSpaceShip && ship.Overlap(ActiveSpaceShip))
                {
                    DestroyShip(ship);
                }
            }
            if(next != null) SelectShip(next);
        }
        protected override void OnDrawGameExample(ScreenInfo game)
        {
            foreach (var star in stars)
            {
                star.Draw();
            }

            foreach (var ship in spaceShips)
            {
                ship.Draw();
            }
            
            cameraFollower.DrawDebugRect();
            
        }
        protected override void OnDrawGameUIExample(ScreenInfo gameUi)
        {
            
        }
        protected override void OnDrawUIExample(ScreenInfo ui)
        {
            var rects = GAMELOOP.UIRects.GetRect("bottom center").SplitV(0.35f);
            DrawDescription(rects.top);
            DrawInputDescription(rects.bottom);
           
        }

        private void DrawDescription(Rect rect)
        {
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Medium;
            textFont.DrawTextWrapNone("Bump into other ships to destroy them.", rect, new(0.5f));
            // font.DrawText("Bump into other ships to destroy them.", rect, 1f, new Vector2(0.5f, 0.5f), ColorMedium);
        }
        private void DrawInputDescription(Rect rect)
        {
            var curDevice = ShapeInput.CurrentInputDeviceType;
            var curDeviceNoMouse = ShapeInput.CurrentInputDeviceTypeNoMouse;
            string addShipText = iaAddShip.GetInputTypeDescription(curDevice, true, 1, false);
            string nextShipText = iaNextShip.GetInputTypeDescription(curDevice, true, 1, false);
            string centerTargetText = iaCenterTarget.GetInputTypeDescription(curDeviceNoMouse, true, 1, false);
            string moveText = ActiveSpaceShip != null ? ActiveSpaceShip.GetInputDescription(curDevice) : "";
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Light;
            
            if (spaceShips.Count <= 1)
            {
                string textBottom = $"{moveText} | Add Ship {addShipText} | Center Mode {centerTargetActive} {centerTargetText}";
                textFont.DrawTextWrapNone(textBottom, rect, new(0.5f));
                // font.DrawText(textBottom, rect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
            }
            else
            {
                string textBottom = $"{moveText} | Add Ship {addShipText} | Next Ship {nextShipText} | Center Mode {centerTargetActive} {centerTargetText}";
                textFont.DrawTextWrapNone(textBottom, rect, new(0.5f));
                // font.DrawText(textBottom, rect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
            }
            
            
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
       
        
    }

}
