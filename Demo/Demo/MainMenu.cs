using System.Numerics;
using Raylib_CsLo;
using UI;
using Screen;
using ShapeEngine;
using Lib;
using Input;

namespace Demo
{
    public class ButtonBasic : UIElement
    {
        public string Text { get; set; } = "Button";
        private Font font;
        private uint shortcutID = 0;
        public ButtonBasic(string text, Font font)
        {
            this.Text = text;
            this.font = font;
            this.DisabledSelection = false;
        }
        public ButtonBasic(string text, Font font, uint shortcutID)
        {
            this.Text = text;
            this.font = font;
            this.shortcutID = shortcutID;
            this.DisabledSelection = false;
        }
        protected override bool CheckPressed() { return Demo.INPUT.GetActionState(InputIDs.UI_Pressed).down; }
        protected override bool CheckMousePressed() { return Demo.INPUT.GetActionState(InputIDs.UI_MousePressed).down; }
        protected override bool CheckShortcutPressed()
        {
            if (shortcutID == 0) return false;
            else return Demo.INPUT.GetActionState(shortcutID).down;
        }
        public override void Draw()
        {
            Rectangle r = GetRect();

            if (DisabledSelection)
            {
                //DrawRectangleLinesEx(r, 4f, MAROON);
                SDrawing.DrawRectangleCorners(r, 4f, MAROON, 25);
                SDrawing.DrawTextAligned(Text, r, 1f, MAROON, font, new(0.5f));
            }
            else
            {
                if (Pressed)
                {
                    DrawRectangleLinesEx(r, 4f, LIME);
                    SDrawing.DrawTextAligned(Text, r, 1f, LIME, font, new(0.5f));
                }
                else if (Selected)
                {
                    //DrawRectangleRec(r, LIGHTGRAY);
                    SDrawing.DrawRectangleCorners(r, 4f, WHITE, 25);
                    SDrawing.DrawTextAligned(Text, r, 1f, WHITE, font, new(0.5f));
                }
                else
                {
                    SDrawing.DrawRectangleCorners(r, 4f, GRAY, 25);
                    SDrawing.DrawTextAligned(Text, r, 1f, GRAY, font, new(0.5f));
                }

                
            }
        }
    }

    
    public class MainMenu : Scene
    {
        ButtonBasic startButton, optionsButton, quitButton;
        BoxContainer buttonContainer;

        public MainMenu()
        {
            var font = Demo.FONT.GetFont(Demo.FONT_Medium);

            startButton = new("START", font);
            optionsButton = new("OPTIONS", font);
            optionsButton.DisabledSelection = true;
            quitButton = new("QUIT", font, InputIDs.UI_Cancel);
            buttonContainer = new(startButton, optionsButton, quitButton);
            buttonContainer.GapRelative = 0.1f;
            buttonContainer.Select();
        }


        public override void Start()
        {

        }
        public override void Activate(Scene? oldScene)
        {
            base.Activate(oldScene);
            Demo.CURSOR.Switch(Demo.CURSOR_UI);
            GAMELOOP.BackgroundColor = Demo.PALETTES.C(ColorIDs.Background2);
            
            Demo.INPUT.SetGroupDisabled(InputIDs.GROUP_Player, true);
            Demo.INPUT.SetGroupDisabled(InputIDs.GROUP_Level, true);
            Demo.INPUT.SetGroupDisabled(InputIDs.GROUP_Debug, true);
            Demo.INPUT.SetGroupDisabled(InputIDs.GROUP_UI, false);
            Demo.INPUT.SetGroupDisabled(InputIDs.GROUP_Settings, false);
            GAMELOOP.RemoveScene("level1");

        }
        public override void Deactivate(Scene? newScene)
        {
            base.Deactivate(newScene);
        }
        public override void Update(float dt)
        {
            Vector2 uiSize = GraphicsDevice.UISize();
            Rectangle r = SRect.ConstructRect(uiSize * 0.5f, uiSize * new Vector2(0.3f, 0.4f), new Vector2(0.5f, 0.25f));
            buttonContainer.UpdateRect(r);
            buttonContainer.Update(dt, Demo.MousePosUI);

            UINeighbors.NeighborDirection dir = UINeighbors.NeighborDirection.NONE;
            if (Demo.INPUT.GetActionState(InputIDs.UI_Down).down) dir = UINeighbors.NeighborDirection.BOTTOM;
            else if (Demo.INPUT.GetActionState(InputIDs.UI_Up).down) dir = UINeighbors.NeighborDirection.TOP;
            //else if (InputHandler.IsDown(0, InputIDs.UI_Left))  dir = UINeighbors.NeighborDirection.LEFT;
            //else if (InputHandler.IsDown(0, InputIDs.UI_Right))  dir = UINeighbors.NeighborDirection.RIGHT;
            buttonContainer.Navigate(dir);
            if (startButton.Released)
            {
                GAMELOOP.AddScene("level1", new Level());
                GAMELOOP.GoToScene("level1");
            }
            if (quitButton.Released)
            {
                GAMELOOP.quit = true;
            }
        }
        public override void Draw()
        {
            //mazeDrawer.Draw();
        }
        public override void DrawUI(Vector2 uiSize)
        {

            Rectangle uiArea = GraphicsDevice.UIArea();
            DrawRectangleRec(uiArea, Demo.PALETTES.C(ColorIDs.Background1));
            SDrawing.DrawTextAligned("MAIN MENU", uiSize * new Vector2(0.5f, 0.21f), uiSize * new Vector2(0.5f, 0.5f), 1, Demo.PALETTES.C(ColorIDs.Background2), Demo.FONT.GetFont(Demo.FONT_Huge), new(0.5f));
            SDrawing.DrawTextAligned("MAIN MENU", uiSize * new Vector2(0.5f, 0.2f), uiSize * new Vector2(0.5f, 0.5f), 1, Demo.PALETTES.C(ColorIDs.Header), Demo.FONT.GetFont(Demo.FONT_Huge), new(0.5f));
            
            
            
            SDrawing.DrawTextAligned2(ShapeEngine.EDITORMODE == true ? "EDITOR" : "STANDALONE", 
                uiSize * new Vector2(0.01f, 0.03f), uiSize.X * 0.05f, 1, WHITE, Demo.FONT.GetFont(Demo.FONT_Medium), new(0,0.5f));

            Vector2 start = uiSize * new Vector2(0.01f, 0.08f);
            Vector2 gap = uiSize * new Vector2(0, 0.04f);
            Vector2 textSize = uiSize * new Vector2(0.2f, 0.05f);
            var font = Demo.FONT.GetFont(Demo.FONT_Medium);
            SDrawing.DrawTextAligned(String.Format("UI Size: {0}", GraphicsDevice.UISize()), start, textSize, 1, WHITE, font, new(0, 0.5f));
            SDrawing.DrawTextAligned(String.Format("Dev Res: {0}", GraphicsDevice.DevResolution), start + gap, textSize, 1, WHITE, font, new(0, 0.5f));
            SDrawing.DrawTextAligned(String.Format("Target Res: {0}", GraphicsDevice.UITexture.TARGET_RESOLUTION), start + gap * 2, textSize, 1, WHITE, font, new(0, 0.5f));
            SDrawing.DrawTextAligned(String.Format("Win Size: {0}", GraphicsDevice.CurWindowSize), start + gap * 3, textSize, 1, WHITE, font, new(0, 0.5f));
            //SDrawing.DrawTextAligned(String.Format("Stretch F: {0}", stretchFactor), start + gap * 4, textSize, 1, WHITE, font, new(0, 0.5f));

            if(ShapeEngine.IsWindows()) SDrawing.DrawTextAligned("Windows", start + gap * 5, textSize, 1, WHITE, font, new(0, 0.5f));
            if (ShapeEngine.IsLinux()) SDrawing.DrawTextAligned("Linux", start + gap * 5, textSize, 1, WHITE, font, new(0, 0.5f));
            if (ShapeEngine.IsOSX()) SDrawing.DrawTextAligned("OSX", start + gap * 5, textSize, 1, WHITE, font, new(0, 0.5f));
            SDrawing.DrawTextAligned(String.Format("FPS: {0}", GetFPS()), start + gap * 6, textSize, 1, GREEN, font, new(0, 0.5f));
            buttonContainer.Draw();
        }
    }


}
