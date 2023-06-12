/*
global using static Raylib_CsLo.Raylib;
global using static Raylib_CsLo.RayMath;
global using static ShapeCore.ShapeEngine;
using System.Runtime.InteropServices;

namespace ShapeCore
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
        public static Raylib_CsLo.Color DEBUG_CollisionHandlerBorder = GOLD;
        public static Raylib_CsLo.Color DEBUG_CollisionHandlerFill = new(0, 150, 0, 100);
        public static Raylib_CsLo.Color DEBUG_SpawnAreaLines = RED;
        
        public static OSPlatform OS_PLATFORM { get; private set; } =
           RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? OSPlatform.Windows :
           RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSPlatform.Linux :
           RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OSPlatform.OSX :
                                                                   OSPlatform.FreeBSD;

        public static bool IsWindows() { return OS_PLATFORM == OSPlatform.Windows; }
        public static bool IsLinux() { return OS_PLATFORM == OSPlatform.Linux; }
        public static bool IsOSX() { return OS_PLATFORM == OSPlatform.OSX; }

        public static void Start(GameLoop gameloop, int devWidth, int devHeight, float gameSizeFactor, float uiSizeFactor, string windowName, bool fixedTexture, bool pixelSmoothing, bool hideCursor, params string[] launchParams)
        {
            GAMELOOP = gameloop;

            EDITORMODE = Directory.Exists("resources");
            //Start of Program
            GAMELOOP.Initialize(devWidth, devHeight, gameSizeFactor, uiSizeFactor, windowName, fixedTexture, pixelSmoothing, hideCursor, launchParams);

            GAMELOOP.Run();//runs continously

            bool fullscreen = GAMELOOP.Close();
            if (GAMELOOP.restart)
            {
                if (fullscreen) Start(gameloop, devWidth, devHeight, gameSizeFactor, uiSizeFactor, windowName, fixedTexture, pixelSmoothing, hideCursor, "fullscreen");
                else Start(gameloop, devWidth, devHeight, gameSizeFactor, uiSizeFactor, windowName, fixedTexture, pixelSmoothing, hideCursor);
            }
        }
    }
}
*/

