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
        public static bool DEBUGMODE = true;
        public static bool DEBUG_DrawColliders = false;
        public static bool DEBUG_DrawHelpers = false;
        public static Raylib_CsLo.Color DEBUG_ColliderColor = new(0, 25, 200, 100);
        public static Raylib_CsLo.Color DEBUG_ColliderDisabledColor = new(200, 25, 0, 100);
        public static Raylib_CsLo.Color DEBUG_HelperColor = new(0, 200, 25, 100);

        
        public static void Start(GameLoop gameloop, ResourceInitInfo resourceInitInfo, ScreenInitInfo screenInitInfo, params string[] launchParams)
        {
            GAMELOOP = gameloop;

            if (!DEBUGMODE)
            {
                DEBUG_DrawColliders = false;
                DEBUG_DrawHelpers = false;
            }
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


