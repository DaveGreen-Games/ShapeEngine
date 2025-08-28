using ShapeEngine.Core;
using ShapeEngine.StaticLib;
using ShapeEngine.Random;
using ShapeEngine.Screen;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using Raylib_cs;
using ShapeEngine.Core.GameDef;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.RectDef;

namespace Examples.Scenes.ExampleScenes
{
    public class ShipInputExample : ExampleScene
    {
        internal class ColorScheme
        {
            public readonly PaletteColor PcHull;
            public readonly PaletteColor PcOutline;
            public readonly PaletteColor PcCockpit;
            public ColorRgba Hull => PcHull.ColorRgba;
            public ColorRgba Outline => PcOutline.ColorRgba;
            public ColorRgba Cockpit => PcCockpit.ColorRgba;

            public ColorScheme(PaletteColor hull, PaletteColor outline, PaletteColor cockpit)
            {
                this.PcHull = hull;
                this.PcOutline = outline;
                this.PcCockpit = cockpit;
            }
        }
        internal class SpaceShip : ICameraFollowTarget
        {
            public const float Speed = 500f;
            public const float Size = 30f;
            public const float MaxDistance = 2500f;
            public const float MinDistance = 250f;

            public static readonly ColorScheme[] ColorSchemes = new[]
            {
                new ColorScheme(Colors.PcDark, Colors.PcMedium, Colors.PcCold),
                new ColorScheme(Colors.PcDark, Colors.PcMedium, Colors.PcWarm),
                new ColorScheme(Colors.PcDark, Colors.PcMedium, Colors.PcHighlight),
                new ColorScheme(Colors.PcDark, Colors.PcMedium, Colors.PcSpecial),
                new ColorScheme(Colors.PcDark, Colors.PcMedium, Colors.PcSpecial2),
                new ColorScheme(Colors.PcDark, Colors.PcMedium, Colors.PcLight),
                new ColorScheme(Colors.PcDark, Colors.PcMedium, Colors.PcText),
                new ColorScheme(Colors.PcDark, Colors.PcMedium, Colors.PcDark),

            };
            
            private Circle hull;
            private Vector2 movementDir;
            
            private readonly InputAction iaMoveHor;
            private readonly InputAction iaMoveVer;
            public readonly InputAction iaRemove;
            private readonly InputActionTree inputActionTree;
            private readonly ColorScheme colorScheme;
            public readonly GamepadDevice Gamepad;

            public SpaceShip(Vector2 pos, GamepadDevice gamepad)
            {
                hull = new(pos, Size);
                movementDir = Rng.Instance.RandVec2();
                
                InputActionSettings defaultSettings = new();
                
                // var moveHorKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
                var moveHorGP = new InputTypeGamepadJoyAxis(ShapeGamepadJoyAxis.LEFT_X, 0.1f);
                // var moveHorMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.HORIZONTAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
                iaMoveHor = new(defaultSettings,moveHorGP)
                {
                    Gamepad = gamepad
                };

                // var moveVerKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
                var moveVerGP = new InputTypeGamepadJoyAxis(ShapeGamepadJoyAxis.LEFT_Y, 0.1f);
                // var moveVerMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
                iaMoveVer = new(defaultSettings,moveVerGP)
                {
                    Gamepad = gamepad
                };

                Gamepad = gamepad;
                colorScheme = ColorSchemes[gamepad.Index];
                
                inputActionTree = new InputActionTree
                {
                    CurrentGamepad = gamepad
                };
                inputActionTree.Add(iaMoveHor);
                inputActionTree.Add(iaMoveVer);
                
                var removeShipInputType = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
                iaRemove = new(defaultSettings,removeShipInputType)
                {
                    Gamepad = gamepad
                };
                inputActionTree.Add(iaRemove);
            }

            public bool Overlap(SpaceShip other)
            {
                // if (invisibleTimer > 0f) return false;
                
                return hull.OverlapShape(other.hull);
            }
            public string GetInputDescription(InputDeviceType inputDeviceType)
            {
                string hor = iaMoveHor.GetInputTypeDescription(inputDeviceType, true, 1, false, false);
                string ver = iaMoveVer.GetInputTypeDescription(inputDeviceType, true, 1, false, false);
                return $"Move Horizontal [{hor}] Vertical [{ver}]";
            }
            
            public void Destroy()
            {
                
            }
            public bool Update(float dt)
            {
                inputActionTree.Update(dt);
                Vector2 dir = new(iaMoveHor.State.AxisRaw, iaMoveVer.State.AxisRaw);
                    
                float lsq = dir.LengthSquared();
                if (lsq > 0f)
                {
                    movementDir = dir.Normalize();
                    var movement = movementDir * Speed * dt;
                    hull = new Circle(hull.Center + movement, Size);
                }
                else
                {
                    hull = new Circle(hull.Center, Size);
                }

                return iaRemove.State.Pressed;
            }

            public Vector2 GetRandomSpawnPosition()
            {
                return GetPosition() + Rng.Instance.RandVec2(0, hull.Radius * 2);
            }
            public void Draw()
            {
                var rightThruster = movementDir.RotateDeg(-25);
                var leftThruster = movementDir.RotateDeg(25);

                var outlineColor = colorScheme.Outline;
                var hullColor = colorScheme.Hull;
                var cockpitColor = colorScheme.Cockpit;
                
                CircleDrawing.DrawCircle(hull.Center - rightThruster * hull.Radius, hull.Radius / 6, outlineColor, 12);
                CircleDrawing.DrawCircle(hull.Center - leftThruster * hull.Radius, hull.Radius / 6, outlineColor, 12);
                hull.Draw(hullColor);
                CircleDrawing.DrawCircle(hull.Center + movementDir * hull.Radius * 0.66f, hull.Radius * 0.33f, cockpitColor, 12);

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

        private readonly Font font;
        private readonly Rect universe = new(new Vector2(0f), new Size(10000f), new AnchorPoint(0.5f));
        private readonly List<Star> stars = [];

        private readonly CameraFollowerMulti cameraFollower = new();
        private readonly ShapeCamera camera = new();
        private readonly List<SpaceShip> spaceShips = [];

        // private readonly List<InputActionHelper> inputActionHelpers = [];

        public ShipInputExample()
        {
            Title = "Multiple Gamepads Example";

            font = GameloopExamples.Instance.GetFont(FontIDs.JetBrains);
                
            GenerateStars(2500);
            camera.Follower = cameraFollower;
        }

       
        private void AddShip(GamepadDevice gamepad)
        {
            var ship = new SpaceShip(new(), gamepad);
            spaceShips.Add(ship);
            cameraFollower.AddTarget(ship);
            
            UpdateCursorValues();
        }
        private void RemoveShip(GamepadDevice gamepad)
        {
            gamepad.Free();
            for (int i = spaceShips.Count - 1; i >= 0 ; i--)
            {
                var ship = spaceShips[i];
                if (ship.Gamepad == gamepad)
                {
                    spaceShips.RemoveAt(i);
                    cameraFollower.RemoveTarget(ship);
                    break;
                }
            }

            UpdateCursorValues();
        }

        private void RemoveShip(int index)
        {
            if (index < 0 || index >= spaceShips.Count) return;
            var ship = spaceShips[index];
            var gamepad = spaceShips[index].Gamepad;
            spaceShips.RemoveAt(index);
            cameraFollower.RemoveTarget(ship);
            gamepad.Free();

            UpdateCursorValues();
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
            GameloopExamples.Instance.Camera = camera;
            BitFlag mask = new(GameloopExamples.Instance.SceneAccessTag);
            mask = mask.Add(GameloopExamples.Instance.GamepadMouseMovementTag);
            InputSystem.LockBlacklist(mask);

            UpdateShipsWithClaimedGamepads();

            UpdateCursorValues();
        }

        private void UpdateCursorValues()
        {
            var cursorActive = spaceShips.Count <= 0;
            GameloopExamples.Instance.MouseControlEnabled = cursorActive;
            GameloopExamples.Instance.DrawCursor = cursorActive;
        }
        protected override void OnDeactivate()
        {
            GameloopExamples.Instance.MouseControlEnabled = true;
            GameloopExamples.Instance.DrawCursor = true;
            GameloopExamples.Instance.ResetCamera();
            InputSystem.Unlock();
        }

        private void UpdateShipsWithClaimedGamepads()
        {
            var claimedGamepads = Game.Instance.Input.GamepadManager.GetClaimedGamepads();

            for (int i = spaceShips.Count - 1; i >= 0; i--)
            {
                var ship = spaceShips[i];
                var gamepad = ship.Gamepad;
                if (!gamepad.Claimed)
                {
                    RemoveShip(i);
                }
                else
                {
                    claimedGamepads.Remove(gamepad);
                }
            }

            if (claimedGamepads.Count > 0)
            {
                //all claimed gamepads that do not have a ship yet

                foreach (var gamepad in claimedGamepads)
                {
                    AddShip(gamepad);
                }
            }
        }
        
        public override void Reset()
        {
            GameloopExamples.Instance.ScreenEffectIntensity = 1f;
            stars.Clear();
            GenerateStars(2500);
            
            camera.Reset();
            cameraFollower.Reset();
            foreach (var ship in spaceShips)
            {
                ship.Gamepad.Free();
            }
            spaceShips.Clear();
            
            GameloopExamples.Instance.MouseControlEnabled = true;
            GameloopExamples.Instance.DrawCursor = true;
        }

        protected override void OnGamepadClaimed(GamepadDevice gamepad)
        {
            AddShip(gamepad);
        }

        protected override void OnGamepadDisconnected(GamepadDevice gamepad)
        {
            RemoveShip(gamepad);
        }
        
        protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            for (int i = spaceShips.Count - 1; i >= 0; i--)
            {
                var ship = spaceShips[i];
                if (ship.Update(time.Delta))
                {
                    RemoveShip(i);
                }
            }
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
            
            // cameraFollower.Draw();
            
        }
        protected override void OnDrawGameUIExample(ScreenInfo gameUi)
        {
            
        }
        protected override void OnDrawUIExample(ScreenInfo ui)
        {
            var rects = GameloopExamples.Instance.UIRects.GetRect("bottom center").SplitV(0.35f);
            DrawDescription(rects.top);
            DrawInputDescription(rects.bottom);
           
            DrawGamepadInfo(GameloopExamples.Instance.UIRects.GetRect("bottom right"));
        }

        private void DrawGamepadInfo(Rect rect)
        {
            var gamepadManger = Input.GamepadManager;
            var text = $"Connected {gamepadManger.GetConnectedGamepads().Count} | Available {gamepadManger.GetAvailableGamepads().Count}";
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Warm;
            textFont.DrawTextWrapNone(text, rect, new(0.5f));
            // font.DrawText(text, rect, 1f, new Vector2(0.5f, 0.5f), ColorHighlight3);
        }
        private void DrawDescription(Rect rect)
        {
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Medium;
            textFont.DrawTextWrapNone("Use multiple gamepads!", rect, new(0.5f));
            // font.DrawText("Use multiple gamepads!", rect, 1f, new Vector2(0.5f, 0.5f), ColorMedium);
        }
        private void DrawInputDescription(Rect rect)
        {
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Light;
            
            var connectedGamepads = Game.Instance.Input.GamepadManager.GetConnectedGamepads().Count;
            var claimedGamepads = Game.Instance.Input.GamepadManager.GetClaimedGamepads().Count;

            if (connectedGamepads <= 0)
            {
                textFont.DrawTextWrapNone("No gamepads connected. Connect a gamepad to start.", rect, new(0.5f));
            }
            else if (claimedGamepads <= 0)
            {
                textFont.DrawTextWrapNone("No gamepads claimed. Claim Gamepad: [A]", rect, new(0.5f));
            }
            else
            {
                textFont.DrawTextWrapNone($"Free Gamepad: [Y] | Claim Gamepad: [A]", rect, new(0.5f));
            }
        }
        
    }
}
