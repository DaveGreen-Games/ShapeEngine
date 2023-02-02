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

namespace ShapeEngineDemo
{
    public class MainMenu : Scene
    {
        //MazeGenerator.Maze maze;
        //MazeGenerator.MazeDrawer mazeDrawer;
        ButtonLabel level1Button, quitButton, optionsButton;

        //ButtonLabel tb1, tb2, tb3;


        public MainMenu()
        {
            //maze = new(16, 9);
            //mazeDrawer = new(maze, new(SCREEN_AREA.X + SCREEN_AREA.width * 0.1f, SCREEN_AREA.Y + SCREEN_AREA.height * 0.1f), new(SCREEN_AREA.width * 0.8f, SCREEN_AREA.height * 0.8f), 2);
            //maze.GenerateDeadCells(50);
            //maze.Generate(MazeGenerator.Maze.GenerationType.PRIM_SIMPLE, -1, 1);
            var font = Demo.FONT.GetFont("medium");
            level1Button =  new(font, "START");
            optionsButton = new(font, "OPTIONS");
            quitButton =    new(font, "QUIT");
            quitButton.AddShortcut("UI Cancel");

            level1Button.SetStateColors(new(Demo.PALETTES.C("bg2"), Demo.PALETTES.C("energy"), Demo.PALETTES.C("player"), Demo.PALETTES.C("sepcial1"), Demo.PALETTES.C("neutral")));
            level1Button.SetTextStateColors(new(Demo.PALETTES.C("text"), Demo.PALETTES.C("bg2"), Demo.PALETTES.C("player"), Demo.PALETTES.C("b1"), Demo.PALETTES.C("enemy")));

            optionsButton.SetStateColors(new(Demo.PALETTES.C("bg2"), Demo.PALETTES.C("energy"), Demo.PALETTES.C("player"), Demo.PALETTES.C("sepcial1"), Demo.PALETTES.C("neutral")));
            optionsButton.SetTextStateColors(new(Demo.PALETTES.C("text"), Demo.PALETTES.C("bg2"), Demo.PALETTES.C("player"), Demo.PALETTES.C("b1"), Demo.PALETTES.C("enemy")));

            quitButton.SetStateColors(new(Demo.PALETTES.C("bg2"), Demo.PALETTES.C("energy"), Demo.PALETTES.C("player"), Demo.PALETTES.C("sepcial1"), Demo.PALETTES.C("neutral")));
            quitButton.SetTextStateColors(new(Demo.PALETTES.C("text"), Demo.PALETTES.C("bg2"), Demo.PALETTES.C("player"), Demo.PALETTES.C("b1"), Demo.PALETTES.C("enemy")));



            //tb1 = new("TEST 1", "medium");
            //tb1.SetStateColors(new(Demo.PALETTES.C("bg2"), Demo.PALETTES.C("energy"), Demo.PALETTES.C("player"), Demo.PALETTES.C("sepcial1"), Demo.PALETTES.C("neutral")));
            //tb1.SetTextStateColors(new(Demo.PALETTES.C("text"), Demo.PALETTES.C("bg2"), Demo.PALETTES.C("player"), Demo.PALETTES.C("b1"), Demo.PALETTES.C("enemy")));
            //
            //tb2 = new("TEST 2", "medium");
            //tb2.SetStateColors(new(Demo.PALETTES.C("bg2"), Demo.PALETTES.C("energy"), Demo.PALETTES.C("player"), Demo.PALETTES.C("sepcial1"), Demo.PALETTES.C("neutral")));
            //tb2.SetTextStateColors(new(Demo.PALETTES.C("text"), Demo.PALETTES.C("bg2"), Demo.PALETTES.C("player"), Demo.PALETTES.C("b1"), Demo.PALETTES.C("enemy")));
            //
            //tb3 = new("TEST 3", "medium");
            //tb3.SetStateColors(new(Demo.PALETTES.C("bg2"), Demo.PALETTES.C("energy"), Demo.PALETTES.C("player"), Demo.PALETTES.C("sepcial1"), Demo.PALETTES.C("neutral")));
            //tb3.SetTextStateColors(new(Demo.PALETTES.C("text"), Demo.PALETTES.C("bg2"), Demo.PALETTES.C("player"), Demo.PALETTES.C("b1"), Demo.PALETTES.C("enemy")));


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

            //UIHandler.RegisterUIElement(tb1);
            //UIHandler.RegisterUIElement(tb2);
            //UIHandler.RegisterUIElement(tb3);

            Demo.CURSOR.Switch("ui");
            GAMELOOP.backgroundColor = Demo.PALETTES.C("bg2");
            GAMELOOP.RemoveScene("level1");
            //AudioHandler.SwitchPlaylist("menu");
        }
        public override void Deactivate(Scene? newScene)
        {
            base.Deactivate(newScene);
            UIHandler.UnregisterUIElement(level1Button);
            UIHandler.UnregisterUIElement(optionsButton);
            UIHandler.UnregisterUIElement(quitButton);

            //UIHandler.UnregisterUIElement(tb1);
            //UIHandler.UnregisterUIElement(tb2);
            //UIHandler.UnregisterUIElement(tb3);
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

            //tb1.UpdateRect(center + offset * 3, size, new(0.5f));
            //tb2.UpdateRect(center - new Vector2(size.X * 1.7f, 0f), size * 1.2f, new(0.5f));
            //tb3.UpdateRect(center + size * 1.5f, size * 0.5f, new(0.5f));
            //tb1.Update(dt, GAMELOOP.MOUSE_POS_UI);
            //tb2.Update(dt, GAMELOOP.MOUSE_POS_UI);
            //tb3.Update(dt, GAMELOOP.MOUSE_POS_UI);


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
            DrawRectangleRec(uiArea, Demo.PALETTES.C("bg1"));
            
            SDrawing.DrawTextAligned("MAIN MENU", uiSize * new Vector2(0.5f, 0.21f), uiSize * new Vector2(0.5f, 0.5f), 1, Demo.PALETTES.C("bg2"), Demo.FONT.GetFont("huge"), new(0.5f));
            SDrawing.DrawTextAligned("MAIN MENU", uiSize * new Vector2(0.5f, 0.2f), uiSize * new Vector2(0.5f, 0.5f), 1, Demo.PALETTES.C("header"), Demo.FONT.GetFont("huge"), new(0.5f));
            
            
            
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
            
            level1Button.Draw(uiSize, stretchFactor);
            optionsButton.Draw(uiSize, stretchFactor);
            quitButton.Draw(uiSize, stretchFactor);

            //tb1.Draw(uiSize, stretchFactor);
            //tb2.Draw(uiSize, stretchFactor);
            //tb3.Draw(uiSize, stretchFactor);
            
        }
    }


}
