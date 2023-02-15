using Raylib_CsLo;
using System.Numerics;

namespace ShapeCollisionTest
{
    public static class Program
    {
        public static bool QUIT = false;
        //public static Test CUR_TEST = new TestStart();

        public static void Main(string[] args)
        {
            Run();
        }
        //public static void ChangeTest(Test newTest)
        //{
        //    CUR_TEST.Close();
        //    CUR_TEST = newTest;
        //    CUR_TEST.Start();
        //}
        private static void Run()
        {
            Raylib.InitWindow(1920, 1080, "Shape Collision Test");
            //Raylib.HideCursor();
            //Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            Raylib.ClearWindowState(ConfigFlags.FLAG_VSYNC_HINT);
            Raylib.SetTargetFPS(60);
            //Raylib.SetWindowState(ConfigFlags.FLAG_VSYNC_HINT);

           // CUR_TEST.Start();

            while (!Raylib.WindowShouldClose())
            {
                float dt = Raylib.GetFrameTime();
                Vector2 mousePos = Raylib.GetMousePosition();

                //CUR_TEST.Update(dt, mousePos);
                
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Raylib.BLACK);
                //CUR_TEST.Draw(mousePos);
                Raylib.EndDrawing();
            }

            //CUR_TEST.Close();
            Raylib.CloseWindow();
        }
    }
}