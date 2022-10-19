using System.Numerics;
using Raylib_CsLo;
using ShapeEngineCore.Globals;
using ShapeEngineCore.Globals.Screen;
using ShapeEngineCore.Globals.UI;
using ShapeEngineCore.Globals.Timing;

using ShapeEngineCore;

namespace ShapeEngineDemo
{
    public class SplashScreen : Scene
    {
        float timer = 0.0f;
        float maxTime = 0.5f;
        bool started = false;
        public override void Activate(Scene? oldScene)
        {
            //base.Activate(oldScene);
            Color fadeColor = PaletteHandler.C("player");
            fadeColor.a = 0;
            Action start = () => { timer = maxTime; started = true; };
            TimerHandler.Add(1f, start);
            Action flash = () => ScreenHandler.UI.Flash(0.5f, PaletteHandler.C("player"), fadeColor);
            TimerHandler.Add(1f, flash);
            Action action = () => ShapeEngine.GAMELOOP.GoToScene("mainmenu");
            TimerHandler.Add(2f, action);
            
            ShapeEngine.GAMELOOP.backgroundColor = PaletteHandler.C("bg1");
        }

        public override void Deactivate(Scene? newScene)
        {
            base.Deactivate(newScene);
            ShapeEngineCore.Globals.Cursor.CursorHandler.Show();
        }
        public override void Update(float dt)
        {
            if (timer > 0.0f)
            {
                timer -= dt;
                if (timer <= 0.0f)
                {
                    timer = 0.0f;
                }
            }
        }
        public override void DrawUI(Vector2 uiSize, Vector2 stretchFactor)
        {
            DrawRectangleRec(new Rectangle(0,0,uiSize.X * stretchFactor.X, uiSize.Y * stretchFactor.Y), PaletteHandler.C("bg1"));
            
            if(started)
            {
                float f = 1.0f - (timer / maxTime);
                Vector2 textSize = Vec.Lerp(new(0f), new Vector2(0.9f, 0.5f) * uiSize, f);
                UIHandler.DrawTextAligned("SHAPE ENGINE", new Vector2(0.5f, 0.5f) * uiSize, textSize, 1, PaletteHandler.C("header"), "bold", Alignement.CENTER);

            }
        }
    }


}
