using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.ComponentModel;
using System.Numerics;

namespace Examples.Scenes
{
    public class ExampleScene : IScene
    {

        public static Color ColorDark = SColor.HexToColor("0A131F");
        public static Color ColorDarkB = SColor.HexToColor("#121F2B");
        public static Color ColorMedium = SColor.HexToColor("1F3847");
        public static Color ColorLight = SColor.HexToColor("B6E0E2");
        public static Color ColorHighlight1 = SColor.HexToColor("E5F6DF");
        public static Color ColorHighlight2 = SColor.HexToColor("E94957");
        public static Color ColorHighlight3 = SColor.HexToColor("#FCA311");
        public static Color ColorRustyRed = SColor.HexToColor("#DE3C4B");


        public string Title { get; protected set; } = "Title Goes Here";
        public string Description { get; protected set; } = "No Description Yet.";

        protected Font titleFont = GAMELOOP.FontDefault;

        public virtual void WindowSizeChanged(DimensionConversionFactors conversionFactors)
        {
            
        }
        public virtual void Reset() { }

        
        public virtual void Update(float dt, ScreenInfo game, ScreenInfo ui)
        {
            HandleInput(dt, game.MousePos, ui.MousePos);
        }

        protected virtual void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            if (IsKeyPressed(KeyboardKey.KEY_R))
            {
                Reset();
                //GAMELOOP.Camera.Reset();
            }
            if (IsKeyPressed(KeyboardKey.KEY_ESCAPE)) GAMELOOP.GoToMainScene();
            if (IsKeyPressed(KeyboardKey.KEY_M)) GAMELOOP.Maximized = !GAMELOOP.Maximized;
            if (IsKeyPressed(KeyboardKey.KEY_F)) GAMELOOP.Fullscreen = !GAMELOOP.Fullscreen;
            
            float increment = 0.05f;

            if (IsKeyPressed(KeyboardKey.KEY_NINE)) //zoom out
            {
                GAMELOOP.Camera.Zoom(increment);
            }
            else if (IsKeyPressed(KeyboardKey.KEY_ZERO))//zoom in
            {
                GAMELOOP.Camera.Zoom(-increment);
            }
        }

        public virtual void DrawUI(ScreenInfo ui)
        {
            Vector2 uiSize = ui.Area.Size;
            Segment s = new(uiSize * new Vector2(0f, 0.07f), uiSize * new Vector2(1f, 0.07f));
            s.Draw(2f, ColorLight);

            Rect r = new Rect(uiSize * new Vector2(0.5f, 0.01f), uiSize * new Vector2(0.6f, 0.06f), new Vector2(0.5f, 0f));
            titleFont.DrawText(Title, r, 10f, new(0.5f), ColorLight);

            string backText = "Back [ESC]";
            Rect backRect = new Rect(uiSize * new Vector2(0.02f, 0.06f), uiSize * new Vector2(0.3f, 0.04f), new Vector2(0f, 1f));
            titleFont.DrawText(backText, backRect, 4f, new Vector2(0f, 0.5f), ColorHighlight2);

            string fpsText = $"Fps: {Raylib.GetFPS()}";
            Rect fpsRect = new Rect(uiSize * new Vector2(0.98f, 0.06f), uiSize * new Vector2(0.3f, 0.04f), new Vector2(1f, 1f));
            titleFont.DrawText(fpsText, fpsRect, 4f, new Vector2(1f, 0.5f), ColorHighlight2);
        }

        protected void DrawCross(Vector2 center, float length)
        {
            Color c = ColorLight.ChangeAlpha((byte)125);
            Segment hor = new Segment(center - new Vector2(length / 2, 0f), center + new Vector2(length / 2, 0f));
            Segment ver = new Segment(center - new Vector2(0f, length / 2), center + new Vector2(0f, length / 2));
            hor.Draw(2f, c);
            ver.Draw(2f, c);
        }

        public virtual GameObjectHandler? GetGameObjectHandler()
        {
            return null;
        }

        public virtual void Activate(IScene oldScene)
        {
            GAMELOOP.Camera.Reset();
        }

        public virtual void Deactivate()
        {
            GAMELOOP.Camera.Reset();
            GAMELOOP.ResetCamera();
        }

        public virtual void Close()
        {
            
        }

        

        public virtual void DrawGame(ScreenInfo game)
        {
        }
    }

}
