global using static Raylib_CsLo.Raylib;
global using static Raylib_CsLo.RayMath;
global using static ShapeEngineCore.ShapeEngine;
//global using static TemplateRaylib.Program;

//global using static Raylib_CsLo.Easings;
//global using static Raylib_CsLo.RayGui;
using ShapeEngineCore;


namespace ShapeEngineDemo
{
    static class Program
    {
        public static void Main(params string[] launchParams)
        {

            ScreenInitInfo screenInitInfo = new ScreenInitInfo(1920, 1080, 0.25f, 2.0f, "Raylib Template", 60, true, false, 0, false);
            DataInitInfo dataInitInfo = new DataInitInfo("data/test-properties.json", new ShapeEngineDemo.DataObjects.DefaultDataResolver(), "asteroids", "player", "guns", "projectiles", "colors", "engines");
            ShapeEngine.Start(new Demo(), screenInitInfo, dataInitInfo);
        }
    }
}

