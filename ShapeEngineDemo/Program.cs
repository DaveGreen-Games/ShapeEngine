﻿global using static Raylib_CsLo.Raylib;
global using static Raylib_CsLo.RayMath;
global using static ShapeEngineCore.ShapeEngine;
using ShapeEngineCore;

//either copy into project file or download those 2 nuget packages.
//< ItemGroup >
//    <PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
//    < PackageReference Include = "Raylib-CsLo" Version = "4.0.1" />
//    < PackageReference Include = "Vortice.XInput" Version = "2.1.19" />
//</ ItemGroup >


namespace ShapeEngineDemo
{
    static class Program
    {
        public static void Main(params string[] launchParams)
        {
            //Uncomment to generate a new resources.shp file
            //ShapeEngineCore.Globals.Persistent.ResourceManager.Generate("resources", "");
            
            
            //START
            ScreenInitInfo screenInitInfo = new ScreenInitInfo(1920, 1080, 0.25f, 2.0f, "Raylib Template", 60, true, false, 0, true);
            ShapeEngine.Start(new Demo(), "", screenInitInfo);
        }
    }
}

