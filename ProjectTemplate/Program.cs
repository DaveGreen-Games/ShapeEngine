global using static Raylib_CsLo.Raylib;
global using static Raylib_CsLo.RayMath;
global using static ShapeEngineCore.ShapeEngine;
using ShapeEngineCore;


//either copy into project file or download those 2 nuget packages.
//< ItemGroup >
//    <PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
//    < PackageReference Include = "Raylib-CsLo" Version = "4.0.1" />
//    < PackageReference Include = "Vortice.XInput" Version = "2.1.19" />
//</ ItemGroup >


namespace ShapeEngineTemplate
{
    static class Program
    {
        public static void Main(params string[] launchParams)
        {
            //ShapeEngineCore.Globals.Persistent.ResourceManager.Generate("resources", "", "resources.txt");

            ScreenInitInfo screenInitInfo = new ScreenInitInfo(1920, 1080, 1f, 1.0f, "Shape Engine Template", 60, true, false, 0, false);
            ResourceInitInfo resourceInitInfo = new("", "resources.txt");
            GameInitInfo gameInitInfo = new("solobytegames", "shape-engine-demo");
            ShapeEngine.Start(new GameLoop(), gameInitInfo, resourceInitInfo, screenInitInfo);
        }
    }
}