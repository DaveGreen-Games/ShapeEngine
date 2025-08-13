using Raylib_cs;
using ShapeEngine.Core;
using ShapeEngine.Screen;
using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Input;
using ShapeEngine.Random;
namespace Examples.Scenes.ExampleScenes
{
    public class CameraAreaDrawExample : ExampleScene
    {
        Font font;
        Vector2 movementDir = new();
        Rect universe = new(new Vector2(0f), new Size(10000f), new AnchorPoint(0.5f));
        List<Star> stars = new();
        private List<Star> drawStars = new();
        
        private Ship ship = new(new Vector2(0f), 30f, Colors.PcCold, Colors.PcHighlight, Colors.PcLight);
        private Ship ship2 = new(new Vector2(100, 0), 30f, Colors.PcCold, Colors.PcSpecial, Colors.PcLight);
        private Ship currentShip;
        private uint prevCameraTweenID = 0;
        private InputAction iaChangeCameraTarget;
        private readonly InputActionTree inputActionTree;

        private readonly ShapeCamera camera;
        private readonly CameraFollowerSingle follower;
        public CameraAreaDrawExample()
        {
            Title = "Camera Area Draw Example";

            font = GameloopExamples.Instance.GetFont(FontIDs.JetBrains);

            camera = new();
            GenerateStars(Rng.Instance.RandI(15000, 30000));
            follower = new(ship.Speed * 1.1f, 200, 400);
            camera.Follower = follower;
            UpdateFollower(GameloopExamples.Instance.UIScreenInfo.Area.Size.Min());
            
            currentShip = ship;

            InputActionSettings defaultSettings = new();
            
            var changeCameraTargetKB = new InputTypeKeyboardButton(ShapeKeyboardButton.B);
            var changeCameraTargetGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
            var changeCameraTargetMB = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            iaChangeCameraTarget = new(defaultSettings,changeCameraTargetKB, changeCameraTargetGP, changeCameraTargetMB);
            inputActionTree = [iaChangeCameraTarget];
        }

        private void UpdateFollower(float size)
        {
            var minBoundary = 0.12f * size;
            var maxBoundary = 0.25f * size;
            var boundary = new Vector2(minBoundary, maxBoundary) * camera.ZoomFactor;
            follower.Speed = ship.Speed * 1.1f;
            follower.BoundaryDis = new(boundary);
        }
        private void GenerateStars(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                Vector2 pos = universe.GetRandomPointInside();

                //ChanceList<float> sizes = new((45, 1.5f), (25, 2f), (15, 2.5f), (10, 3f), (3, 3.5f), (2, 4f));
                float size = Rng.Instance.RandF(1.5f, 3f);// sizes.Next();
                Star star = new(pos, size);
                stars.Add(star);
            }
        }

        protected override void OnActivate(Scene oldScene)
        {
            GameloopExamples.Instance.Camera = camera;
            follower.SetTarget(ship);
            currentShip = ship;
            UpdateFollower(GameloopExamples.Instance.UIScreenInfo.Area.Size.Min());
            // GAMELOOP.UseMouseMovement = false;
        }

        protected override void OnDeactivate()
        {
            GameloopExamples.Instance.ResetCamera();
            // GAMELOOP.UseMouseMovement = true;
        }
        
        public override void Reset()
        {
            GameloopExamples.Instance.ScreenEffectIntensity = 1f;
            camera.Reset();
            ship.Reset(new Vector2(0), 30f);
            ship2.Reset(new Vector2(100, 0), 30f);
            follower.SetTarget(ship);
            currentShip = ship;
            UpdateFollower(GameloopExamples.Instance.UIScreenInfo.Area.Size.Min());
            stars.Clear();
            GenerateStars(Rng.Instance.RandI(15000, 30000));

        }
        protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI)
        {
            if (iaChangeCameraTarget.State.Pressed)
            {
                camera.StopTweenSequence(prevCameraTweenID);
                CameraTweenZoomFactor zoomFactorOut = new(1f, 0.5f, 0.25f, TweenType.LINEAR);
                CameraTweenZoomFactor zoomFactorIn = new(0.5f, 1.25f, 0.75f, TweenType.LINEAR);
                CameraTweenZoomFactor zoomFactorFinal = new(1.25f, 1f, 0.25f, TweenType.LINEAR);
                prevCameraTweenID = camera.StartTweenSequence(zoomFactorOut, zoomFactorIn, zoomFactorFinal);
                
                if (currentShip == ship)
                {
                    currentShip = ship2;
                    follower.ChangeTarget(ship2, 1f);
                }
                else
                {
                    currentShip = ship;
                    follower.ChangeTarget(ship, 1f);
                }
            }

        }

        protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            UpdateFollower(ui.Area.Size.Min());
            
            // GAMELOOP.MouseControlEnabled = GAMELOOP.CurGamepad?.IsDown(ShapeGamepadTriggerAxis.RIGHT, 0.1f) ?? true;
            inputActionTree.CurrentGamepad = Input.GamepadManager.LastUsedGamepad;
            inputActionTree.Update(time.Delta);
            
            currentShip.Update(time.Delta, camera.RotationDeg);

            drawStars.Clear();
            Rect cameraArea = game.Area;
            foreach (var star in stars)
            {
                if(cameraArea.OverlapShape(star.GetBoundingBox())) drawStars.Add(star);
            }
        }

        protected override void OnDrawGameExample(ScreenInfo game)
        {
            foreach (var star in drawStars)
            {
                star.Draw(Colors.Dark);
            }
            ship.Draw();
            ship2.Draw();
            
            // Circle cameraBoundaryMin = new(camera.Position, camera.Follower.BoundaryDis.Min);
            // cameraBoundaryMin.DrawLines(2f, ColorHighlight3);
            // Circle cameraBoundaryMax = new(camera.Position, camera.Follower.BoundaryDis.Max);
            // cameraBoundaryMax.DrawLines(2f, ColorHighlight2);
        }

        protected override void OnDrawUIExample(ScreenInfo ui)
        {
            var rects = GameloopExamples.Instance.UIRects.GetRect("bottom center").SplitV(0.5f);
            DrawStarInfo(rects.top);
            DrawInputDescription(rects.bottom);

            
        }

        private void DrawStarInfo(Rect rect)
        {
            string infoText = $"Total Stars {stars.Count} | Drawn Stars {drawStars.Count} | Camera Size {camera.Area.Size.Round()}";
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Warm;
            textFont.DrawTextWrapNone(infoText, rect, new(0.5f));
            // font.DrawText(infoText, rect, 1f, new Vector2(0.5f, 0.5f), ColorHighlight3);
        }
        private void DrawInputDescription(Rect rect)
        {
            var curDevice = Input.CurrentInputDeviceType;
            // var curDeviceNoMouse = Input.CurrentInputDeviceNoMouse;
            string changeTargetText = iaChangeCameraTarget.GetInputTypeDescription(curDevice, true, 1, false);
            string moveText = ship.GetInputDescription(curDevice);
            string shipInfoText = $"{moveText} | Switch Ship {changeTargetText}";
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Light;
            textFont.DrawTextWrapNone(shipInfoText, rect, new(0.5f));
            // font.DrawText(shipInfoText, rect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }
    }

}
