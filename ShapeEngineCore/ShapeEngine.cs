global using static Raylib_CsLo.Raylib;
global using static Raylib_CsLo.RayMath;
global using static ShapeEngineCore.ShapeEngine;

namespace ShapeEngineCore
{
    public static class ShapeEngine
    {
        public static GameLoop GAMELOOP = new GameLoop();
        public static readonly string CURRENT_DIRECTORY = Environment.CurrentDirectory;

        //-----------DEBUG--------------
        private static bool editormode = true;
        private static bool DEBUG_drawColliders = false;
        private static bool DEBUG_drawHelpers = false;
        public static bool EDITORMODE 
        {
            get { return editormode; }
            private set
            {
                if (!value)
                {
                    DEBUG_drawColliders = false;
                    DEBUG_drawHelpers = false;
                }
                editormode = value;
            }
        }
        public static bool DEBUG_DRAWCOLLIDERS
        {
            get { return DEBUG_drawColliders; }
            set
            {
                if (editormode)
                {
                    DEBUG_drawColliders = value;
                }
            }
        }
        public static bool DEBUG_DRAWHELPERS
        {
            get { return DEBUG_drawHelpers; }
            set
            {
                if (editormode)
                {
                    DEBUG_drawHelpers = value;
                }
            }
        }
        public static Raylib_CsLo.Color DEBUG_ColliderColor = new(0, 25, 200, 100);
        public static Raylib_CsLo.Color DEBUG_ColliderDisabledColor = new(200, 25, 0, 100);
        public static Raylib_CsLo.Color DEBUG_HelperColor = new(0, 200, 25, 100);
        public static Raylib_CsLo.Color DEBUG_AreaInnerColor = new(200, 200, 0, 150);
        public static Raylib_CsLo.Color DEBUG_AreaOuterColor = new(0, 200, 200, 150);
        public static Raylib_CsLo.Color DEBUG_CollisionHandlerBorderColor = GOLD;
        public static Raylib_CsLo.Color DEBUG_CollisionHandlerFillColor = new(0, 150, 0, 100);

        public static void Start(GameLoop gameloop, ResourceInitInfo resourceInitInfo, ScreenInitInfo screenInitInfo, params string[] launchParams)
        {
            GAMELOOP = gameloop;

            EDITORMODE = Directory.Exists("resources");

            //Start of Program
            GAMELOOP.Initialize(resourceInitInfo, screenInitInfo, launchParams);
            GAMELOOP.Start();

            GAMELOOP.Run();//runs continously

            //End of Program
            GAMELOOP.End();
            bool fullscreen = GAMELOOP.Close();
            if (GAMELOOP.RESTART)
            {
                if (fullscreen) Start(gameloop, resourceInitInfo, screenInitInfo, "fullscreen");
                else Start(gameloop, resourceInitInfo, screenInitInfo);
            }
        }
    }
}


