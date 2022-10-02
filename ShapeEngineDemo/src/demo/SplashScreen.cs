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
        public override void Activate(Scene? oldScene)
        {
            //base.Activate(oldScene);
            Color fadeColor = PaletteHandler.C("player");
            fadeColor.a = 0;
            Action flash = () => ScreenHandler.UI.Flash(0.5f, PaletteHandler.C("player"), fadeColor);
            TimerHandler.Add(0.5f, flash);
            Action action = () => ShapeEngine.GAMELOOP.GoToScene("mainmenu");
            TimerHandler.Add(1.5f, action);
            timer = maxTime;
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
        public override void DrawUI()
        {
            DrawRectangleRec(ScreenHandler.UIArea(), PaletteHandler.C("bg1"));
            float f = 1.0f - (timer / maxTime);
            float fontSize = Lerp(0, 800, f);
            Vector2 fontPos = new
                (
                    ScreenHandler.UIArea().x + ScreenHandler.UIArea().width / 2,
                    ScreenHandler.UIArea().y + ScreenHandler.UIArea().height / 2
                );

            //float fontSpacing = 10.0f;
            //string text = "SHAPE ENGINE";
            //Font font = GetFontDefault();
            //Vector2 fontDimensions = MeasureTextEx(font, text, fontSize, fontSpacing);
            //DrawTextEx(font, text, fontPos - fontDimensions * 0.5f, fontSize, fontSpacing, WHITE);

            UIHandler.DrawTextAligned("SHAPE ENGINE", fontPos, fontSize, UIHandler.Scale(5), PaletteHandler.C("header"), "bold", Alignement.CENTER);
        }
    }


}
