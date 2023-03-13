using System.Numerics;
using Raylib_CsLo;
using ShapeUI;
using ShapeScreen;
using ShapeCore;
using ShapeCursor;
using ShapeLib;
using ShapeColor;
using static ShapeInput.InputAction;
using ShapeInput;
using static System.Net.Mime.MediaTypeNames;
using System.Resources;
using ShapeAudio;

namespace ShapeEngineDemo
{
    public class MainMenu : Scene
    {
        ButtonLabel level1Button, quitButton, optionsButton;
        public MainMenu()
        {
            var font = Demo.FONT.GetFont("medium");
            level1Button =  new(font, "START");
            optionsButton = new(font, "OPTIONS");
            quitButton =    new(font, "QUIT");
            quitButton.AddShortcut("UI Cancel");

            level1Button.SetStateColors(new(Demo.PALETTES.C(ColorIDs.Background2), Demo.PALETTES.C(ColorIDs.Energy), Demo.PALETTES.C(ColorIDs.Player), Demo.PALETTES.C(ColorIDs.Special1), Demo.PALETTES.C(ColorIDs.Neutral)));
            level1Button.SetTextStateColors(new(Demo.PALETTES.C(ColorIDs.Text), Demo.PALETTES.C(ColorIDs.Background2), Demo.PALETTES.C(ColorIDs.Player), Demo.PALETTES.C(ColorIDs.Background1), Demo.PALETTES.C(ColorIDs.Enemy)));

            optionsButton.SetStateColors(new(Demo.PALETTES.C(ColorIDs.Background2), Demo.PALETTES.C(ColorIDs.Energy), Demo.PALETTES.C(ColorIDs.Player), Demo.PALETTES.C(ColorIDs.Special1), Demo.PALETTES.C(ColorIDs.Neutral)));
            optionsButton.SetTextStateColors(new(Demo.PALETTES.C(ColorIDs.Text), Demo.PALETTES.C(ColorIDs.Background2), Demo.PALETTES.C(ColorIDs.Player), Demo.PALETTES.C(ColorIDs.Background1), Demo.PALETTES.C(ColorIDs.Enemy)));

            quitButton.SetStateColors(new(Demo.PALETTES.C(ColorIDs.Background2), Demo.PALETTES.C(ColorIDs.Energy), Demo.PALETTES.C(ColorIDs.Player), Demo.PALETTES.C(ColorIDs.Special1), Demo.PALETTES.C(ColorIDs.Neutral)));
            quitButton.SetTextStateColors(new(Demo.PALETTES.C(ColorIDs.Text), Demo.PALETTES.C(ColorIDs.Background2), Demo.PALETTES.C(ColorIDs.Player), Demo.PALETTES.C(ColorIDs.Background1), Demo.PALETTES.C(ColorIDs.Enemy)));

            optionsButton.Disable();
        }


        public override void Start()
        {
            UIHandler.SelectUIElement(level1Button);
        }
        public override void Activate(Scene? oldScene)
        {
            base.Activate(oldScene);
            UIHandler.RegisterUIElement(level1Button);
            UIHandler.RegisterUIElement(optionsButton);
            UIHandler.RegisterUIElement(quitButton);
            Demo.CURSOR.Switch("ui");
            GAMELOOP.backgroundColor = Demo.PALETTES.C(ColorIDs.Background2);
            GAMELOOP.RemoveScene("level1");
        }
        public override void Deactivate(Scene? newScene)
        {
            base.Deactivate(newScene);
            UIHandler.UnregisterUIElement(level1Button);
            UIHandler.UnregisterUIElement(optionsButton);
            UIHandler.UnregisterUIElement(quitButton);
        }
        public override void Update(float dt)
        {
            Vector2 uiSize = ScreenHandler.UISize();
            Vector2 center = uiSize * 0.5f;
            Vector2 size = uiSize * new Vector2(0.2f, 0.1f);
            Vector2 offset = new Vector2(0, size.Y * 1.1f);
            level1Button.UpdateRect(center, size, new(0.5f));
            optionsButton.UpdateRect(center + offset, size, new(0.5f));
            quitButton.UpdateRect(center + offset * 2, size, new(0.5f));
            level1Button.Update(dt, GAMELOOP.MOUSE_POS_UI);
            optionsButton.Update(dt, GAMELOOP.MOUSE_POS_UI);
            quitButton.Update(dt, GAMELOOP.MOUSE_POS_UI);

            if (level1Button.Clicked())
            {
                GAMELOOP.AddScene("level1", new Level());
                GAMELOOP.GoToScene("level1");
            }
            if (quitButton.Clicked()) GAMELOOP.QUIT = true;

        }
        public override void Draw()
        {
            //mazeDrawer.Draw();
        }
        public override void DrawUI(Vector2 uiSize, Vector2 stretchFactor)
        {
            Rectangle uiArea = ScreenHandler.UIArea();
            DrawRectangleRec(uiArea, Demo.PALETTES.C(ColorIDs.Background1));
            
            SDrawing.DrawTextAligned("MAIN MENU", uiSize * new Vector2(0.5f, 0.21f), uiSize * new Vector2(0.5f, 0.5f), 1, Demo.PALETTES.C(ColorIDs.Background2), Demo.FONT.GetFont("huge"), new(0.5f));
            SDrawing.DrawTextAligned("MAIN MENU", uiSize * new Vector2(0.5f, 0.2f), uiSize * new Vector2(0.5f, 0.5f), 1, Demo.PALETTES.C(ColorIDs.Header), Demo.FONT.GetFont("huge"), new(0.5f));
            
            
            
            SDrawing.DrawTextAligned2(ShapeEngine.EDITORMODE == true ? "EDITOR" : "STANDALONE", 
                uiSize * new Vector2(0.01f, 0.03f), uiSize.X * 0.05f, 1, WHITE, Demo.FONT.GetFont(), new(0,0.5f));

            Vector2 start = uiSize * new Vector2(0.01f, 0.08f);
            Vector2 gap = uiSize * new Vector2(0, 0.04f);
            Vector2 textSize = uiSize * new Vector2(0.2f, 0.05f);
            var font = Demo.FONT.GetFont("medium");
            SDrawing.DrawTextAligned(String.Format("UI Size: {0}", ScreenHandler.UISize()), start, textSize, 1, WHITE, font, new(0, 0.5f));
            SDrawing.DrawTextAligned(String.Format("Dev Res: {0}", ScreenHandler.DEVELOPMENT_RESOLUTION), start + gap, textSize, 1, WHITE, font, new(0, 0.5f));
            SDrawing.DrawTextAligned(String.Format("Target Res: {0}", ScreenHandler.UI.TARGET_RESOLUTION), start + gap * 2, textSize, 1, WHITE, font, new(0, 0.5f));
            SDrawing.DrawTextAligned(String.Format("Win Size: {0}", ScreenHandler.CUR_WINDOW_SIZE), start + gap * 3, textSize, 1, WHITE, font, new(0, 0.5f));
            SDrawing.DrawTextAligned(String.Format("Stretch F: {0}", stretchFactor), start + gap * 4, textSize, 1, WHITE, font, new(0, 0.5f));

            if(ShapeEngine.IsWindows()) SDrawing.DrawTextAligned("Windows", start + gap * 5, textSize, 1, WHITE, font, new(0, 0.5f));
            if (ShapeEngine.IsLinux()) SDrawing.DrawTextAligned("Linux", start + gap * 5, textSize, 1, WHITE, font, new(0, 0.5f));
            if (ShapeEngine.IsOSX()) SDrawing.DrawTextAligned("OSX", start + gap * 5, textSize, 1, WHITE, font, new(0, 0.5f));


            level1Button.Draw(uiSize, stretchFactor);
            optionsButton.Draw(uiSize, stretchFactor);
            quitButton.Draw(uiSize, stretchFactor);
        }
    }


}
