using System.Numerics;
using Raylib_CsLo;
using ShapeCore;
using ShapeScreen;
using ShapeColor;
using ShapeTiming;
using ShapeLib;

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
            Color fadeColor = Demo.PALETTES.C(ColorIDs.Player);
            fadeColor.a = 0;
            Action start = () => { timer = maxTime; started = true; };
            Demo.TIMER.Add(1f, start);
            Action flash = () => ScreenHandler.UI.Flash(0.5f, Demo.PALETTES.C(ColorIDs.Player), fadeColor);
            Demo.TIMER.Add(1f, flash);
            Action action = () => ShapeEngine.GAMELOOP.GoToScene("mainmenu");
            Demo.TIMER.Add(2f, action);
            
            ShapeEngine.GAMELOOP.backgroundColor = Demo.PALETTES.C(ColorIDs.Background1);
        }

        public override void Deactivate(Scene? newScene)
        {
            base.Deactivate(newScene);
            //Demo.CURSOR.Show();
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
        public override void DrawUI(Vector2 uiSize)
        {
            DrawRectangleRec(new Rectangle(0,0,uiSize.X, uiSize.Y), Demo.PALETTES.C(ColorIDs.Background1));
            
            if(started)
            {
                float f = 1.0f - (timer / maxTime);
                Vector2 textSize = SVec.Lerp(new(0f), new Vector2(0.9f, 0.5f) * uiSize, f);
                SDrawing.DrawTextAligned("SHAPE ENGINE", new Vector2(0.5f, 0.5f) * uiSize, textSize, 1, Demo.PALETTES.C(ColorIDs.Header), Demo.FONT.GetFont(Demo.FONT_Huge), new(0.5f));

            }
        }
    }


}
