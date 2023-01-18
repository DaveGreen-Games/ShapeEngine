global using static Raylib_CsLo.Raylib;
global using static Raylib_CsLo.RayMath;
global using static ShapeCore.ShapeEngine;
using ShapeCore;

//either copy into project file or download those 2 nuget packages.
//< ItemGroup >
//    < PackageReference Include = "Newtonsoft.Json" Version = "13.0.1" />
//    < PackageReference Include = "Raylib-CsLo" Version = "4.2.0.3" />
//    < PackageReference Include = "Vortice.XInput" Version = "2.1.41" />
//  </ ItemGroup >


namespace ShapeEngineDemo
{
    static class Program
    {
        public static void Main(params string[] launchParams)
        {
            //Uncomment to generate a new resources.txt file
            //ShapeEngineCore.Globals.Persistent.ResourceManager.Generate("resources", "", "resources.txt");
            
            
            //START
            ScreenInitInfo screenInitInfo = new ScreenInitInfo(1920, 1080, 0.5f, 1.0f, "Raylib Template", false, true);
            ResourceInitInfo resourceInitInfo = new("", "resources.txt");
            GameInitInfo gameInitInfo = new("solobytegames", "shape-engine-demo");
            ShapeEngine.Start(new Demo(), gameInitInfo, resourceInitInfo, screenInitInfo);
        }
    }
}

