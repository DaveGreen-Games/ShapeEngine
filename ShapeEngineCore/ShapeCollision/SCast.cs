using System.Numerics;
using Raylib_CsLo;
using ShapeLib;

namespace ShapeCollision
{
    

    public static class SCast
    {

        //exact point line, point segment and point point overlap calculations are used if <= 0
        public static readonly float POINT_RADIUS = 5.0f; //point line and point segment overlap makes more sense when the point is a circle (epsilon = radius)

        //CAST (SemiDynamic - Get Collision Response only for first object - second object can have vel as well)

        
    }
}
