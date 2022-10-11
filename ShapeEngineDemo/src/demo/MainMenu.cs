using System.Numerics;
using Raylib_CsLo;
using ShapeEngineCore;
using ShapeEngineCore.Globals.UI;
using ShapeEngineCore.Globals.Screen;
using ShapeEngineCore.Globals;
using ShapeEngineCore.Globals.Cursor;
using ShapeEngineCore.Globals.Audio;

namespace ShapeEngineDemo
{
    public class MainMenu : Scene
    {
        //MazeGenerator.Maze maze;
        //MazeGenerator.MazeDrawer mazeDrawer;
        ButtonLabel level1Button, quitButton, optionsButton;
        public MainMenu()
        {
            //maze = new(16, 9);
            //mazeDrawer = new(maze, new(SCREEN_AREA.X + SCREEN_AREA.width * 0.1f, SCREEN_AREA.Y + SCREEN_AREA.height * 0.1f), new(SCREEN_AREA.width * 0.8f, SCREEN_AREA.height * 0.8f), 2);
            //maze.GenerateDeadCells(50);
            //maze.Generate(MazeGenerator.Maze.GenerationType.PRIM_SIMPLE, -1, 1);
            
            level1Button = new("START", "medium");
            optionsButton = new("OPTIONS", "medium");
            quitButton = new("QUIT", "medium");
            quitButton.AddShortcut("UI Cancel");

            level1Button.SetStateColors(new(PaletteHandler.C("bg2"), PaletteHandler.C("energy"), PaletteHandler.C("player"), PaletteHandler.C("sepcial1"), PaletteHandler.C("neutral")));
            level1Button.SetTextStateColors(new(PaletteHandler.C("text"), PaletteHandler.C("bg2"), PaletteHandler.C("player"), PaletteHandler.C("b1"), PaletteHandler.C("enemy")));

            optionsButton.SetStateColors(new(PaletteHandler.C("bg2"), PaletteHandler.C("energy"), PaletteHandler.C("player"), PaletteHandler.C("sepcial1"), PaletteHandler.C("neutral")));
            optionsButton.SetTextStateColors(new(PaletteHandler.C("text"), PaletteHandler.C("bg2"), PaletteHandler.C("player"), PaletteHandler.C("b1"), PaletteHandler.C("enemy")));

            quitButton.SetStateColors(new(PaletteHandler.C("bg2"), PaletteHandler.C("energy"), PaletteHandler.C("player"), PaletteHandler.C("sepcial1"), PaletteHandler.C("neutral")));
            quitButton.SetTextStateColors(new(PaletteHandler.C("text"), PaletteHandler.C("bg2"), PaletteHandler.C("player"), PaletteHandler.C("b1"), PaletteHandler.C("enemy")));

            optionsButton.Disable();
            //startButton.AddShortcut(KeyboardKey.KEY_Q);
            //startButton.SetNeighbor(quitButton, UINeighbors.NeighborDirection.BOTTOM);
            //startButton.SetNeighbor(quitButton, UINeighbors.NeighborDirection.TOP);
            //quitButton.SetNeighbor(startButton, UINeighbors.NeighborDirection.BOTTOM);
            //quitButton.SetNeighbor(startButton, UINeighbors.NeighborDirection.TOP);
            UIHandler.SelectUIElement(level1Button);
        }

        public override void Activate(Scene? oldScene)
        {
            base.Activate(oldScene);
            UIHandler.RegisterUIElement(level1Button);
            UIHandler.RegisterUIElement(optionsButton);
            UIHandler.RegisterUIElement(quitButton);
            CursorHandler.Switch("ui");
            GAMELOOP.backgroundColor = PaletteHandler.C("bg2");
            GAMELOOP.RemoveScene("level1");
            //AudioHandler.SwitchPlaylist("menu");
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
            level1Button.UpdateRect(center, size);
            optionsButton.UpdateRect(center + offset, size);
            quitButton.UpdateRect(center + offset * 2, size);
            level1Button.Update(dt, GAMELOOP.MOUSE_POS_UI);
            optionsButton.Update(dt, GAMELOOP.MOUSE_POS_UI);
            quitButton.Update(dt, GAMELOOP.MOUSE_POS_UI);
            if (level1Button.Clicked())
            {
                GAMELOOP.AddScene("level1", new Level());
                GAMELOOP.GoToScene("level1");
            }
            //if (optionsButton.Clicked()) GAMELOOP.GoToScene("level2");
            if (quitButton.Clicked()) GAMELOOP.QUIT = true;
        }
        public override void Draw()
        {
            //mazeDrawer.Draw();
        }
        public override void DrawUI(Vector2 uiSize, Vector2 stretchFactor)
        {
            Rectangle uiArea = ScreenHandler.UIArea();
            DrawRectangleRec(uiArea, PaletteHandler.C("bg1"));
            
            UIHandler.DrawTextAligned("MAIN MENU", uiSize * new Vector2(0.5f, 0.21f), uiSize * new Vector2(0.5f, 0.5f), 1, PaletteHandler.C("bg2"), "bold");
            UIHandler.DrawTextAligned("MAIN MENU", uiSize * new Vector2(0.5f, 0.2f), uiSize * new Vector2(0.5f, 0.5f), 1, PaletteHandler.C("header"), "bold");
            
            
            
            UIHandler.DrawTextAligned2(ShapeEngine.EDITORMODE == true ? "EDITOR" : "STANDALONE", 
                uiSize * new Vector2(0.01f, 0.03f), uiSize.X * 0.05f, 1, WHITE, Alignement.LEFTCENTER);

            Vector2 start = new(30, 180);
            Vector2 gap = new(0, 65);
            float fontSize = 60f;
            UIHandler.DrawTextAligned(String.Format("UI Size: {0}", ScreenHandler.UISize()), start, fontSize, 1, WHITE, Alignement.LEFTCENTER);
            UIHandler.DrawTextAligned(String.Format("Dev Res: {0}", ScreenHandler.DEVELOPMENT_RESOLUTION), start + gap, fontSize, 1, WHITE, Alignement.LEFTCENTER);
            UIHandler.DrawTextAligned(String.Format("Target Res: {0}", ScreenHandler.UI.TARGET_RESOLUTION), start + gap * 2, fontSize, 1, WHITE, Alignement.LEFTCENTER);
            UIHandler.DrawTextAligned(String.Format("Win Size: {0}", ScreenHandler.CUR_WINDOW_SIZE), start + gap * 3, fontSize, 1, WHITE, Alignement.LEFTCENTER);
            UIHandler.DrawTextAligned(String.Format("Stretch F: {0}", stretchFactor), start + gap * 4, fontSize, 1, WHITE, Alignement.LEFTCENTER);
            
            //UIHandler.DrawTextAligned(String.Format("Area F: {0}", ScreenHandler.UI.STRETCH_AREA_FACTOR), new Vector2(100f, 420f), 50, 5, WHITE, Alignement.LEFTCENTER);
            //UIHandler.DrawTextAligned(String.Format("Area Side F: {0}", ScreenHandler.UI.STRETCH_AREA_SIDE_FACTOR), new Vector2(100f, 500f), 50, 5, WHITE, Alignement.LEFTCENTER);
            
            //UIHandler.DrawTextAligned(String.Format("Button Pos: {0}", level1Button.GetCenter()), new Vector2(100f, 580f), 60, 5, WHITE, Alignement.LEFTCENTER);
            //UIHandler.DrawTextAligned(String.Format("Button Scaled Pos: {0}", level1Button.GetCenter() * stretchFactor), new Vector2(100f, 660f), 60, 5, WHITE, Alignement.LEFTCENTER);
            
            level1Button.Draw(uiSize, stretchFactor);
            optionsButton.Draw(uiSize, stretchFactor);
            quitButton.Draw(uiSize, stretchFactor);


            //Vector2 barSize = uiSize * new Vector2(0.5f, 0.05f);// * stretchFactor;
            //Vector2 center = uiSize * new Vector2(0.5f, 0.3f); // * stretchFactor;
            //Vector2 topleft = center - barSize / 2;
            //UIHandler.DrawBar(topleft, barSize, RNG.randF(), RED, DARKPURPLE, BarType.LEFTRIGHT);


        }
    }


}
