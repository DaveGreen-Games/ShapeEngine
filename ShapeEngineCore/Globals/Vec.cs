using Raylib_CsLo;
using System.Numerics;


namespace ShapeEngineCore.Globals
{
    public static class Vec
    {
        public static Vector2 Right() { return new(1.0f, 0.0f); }
        public static Vector2 Left() { return new(-1.0f, 0.0f); }
        public static Vector2 Up() { return new(0.0f, -1.0f); }
        public static Vector2 Down() { return new(0.0f, 1.0f); }
        public static Vector2 One() { return new(1.0f, 1.0f); }
        public static Vector2 Zero() { return new(0.0f, 0.0f); }

        //Perpendicular & Rotation
        public static Vector2 GetPerpendicularRight(Vector2 v)
        {
            return new(v.Y, -v.X);
        }
        public static Vector2 GetPerpendicularLeft(Vector2 v)
        {
            return new(-v.Y, v.X);
        }
        public static Vector2 Rotate90CCW(Vector2 v)
        {
            return GetPerpendicularLeft(v);
            //return new(-v.Y, v.X);
        }
        public static Vector2 Rotate90CW(Vector2 v)
        {
            return GetPerpendicularRight(v);
        }

        public static Vector2 VecFromAngleRad(float angleRad)
        {
            return Vec.Rotate(Vec.Right(), angleRad);
        }
        public static Vector2 VecFromAngleDeg(float angleDeg)
        {
            return VecFromAngleRad(angleDeg * DEG2RAD);
        }
        //Projection
        public static float ProjectionTime(Vector2 v, Vector2 onto) { return (v.X * onto.X + v.Y * onto.Y) / onto.LengthSquared(); }
        public static Vector2 ProjectionPoint(Vector2 point, Vector2 v, float t) { return point + v * t; }
        public static Vector2 Project(Vector2 project, Vector2 onto)
        {
            float d = Vector2.Dot(onto, onto);
            if (d > 0.0f)
            {
                float dp = Vector2.Dot(project, onto);
                return onto * (dp / d);
            }
            return onto;
        }


        public static bool Parallel(Vector2 a, Vector2 b)
        {
            Vector2 rotated = Rotate90CCW(a);
            return Vector2.Dot(rotated, b) == 0.0f;
        }

        public static float Max(Vector2 v) { return MathF.Max(v.X, v.Y); }
        //public static Vector2 Max(Vector2 a, Vector2 b)
        //{
        //    if (a.LengthSquared() > b.LengthSquared()) return a;
        //    else return b;
        //}
        public static float Min(Vector2 v) { return MathF.Min(v.X, v.Y); }
        // public static Vector2 Min(Vector2 a, Vector2 b)
        // {
        //     if (a.LengthSquared() < b.LengthSquared()) return a;
        //     else return b;
        // }

        public static Vector2 LerpDirection(Vector2 a, Vector2 b, float t)
        {
            float angleA = Vec.AngleRad(a);
            float angle = Utils.GetShortestAngleRad(angleA, Vec.AngleRad(b));
            return Vec.Rotate(a, Utils.LerpFloat(0, angle, t));
        }
        public static Vector2 Lerp(Vector2 v1, Vector2 v2, float t) { return Vector2.Lerp(v1, v2, t); } //RayMath.Vector2Lerp(v1, v2, t);
        public static Vector2 MoveTowards(Vector2 v, Vector2 target, float maxDistance) { return Vector2MoveTowards(v, target, maxDistance); }

        public static Vector2 Floor(Vector2 v) { return new(MathF.Floor(v.X), MathF.Floor(v.Y)); }
        public static Vector2 Ceil(Vector2 v) { return new(MathF.Ceiling(v.X), MathF.Ceiling(v.Y)); }
        public static Vector2 Round(Vector2 v) { return new(MathF.Round(v.X), MathF.Round(v.Y)); }
        public static Vector2 Abs(Vector2 v) { return Vector2.Abs(v); }
        public static Vector2 Negate(Vector2 v) { return Vector2.Negate(v); } //RayMath.Vector2Negate(v);
        public static Vector2 Min(Vector2 v1, Vector2 v2) { return Vector2.Min(v1, v2); }
        public static Vector2 Max(Vector2 v1, Vector2 v2) { return Vector2.Max(v1, v2); }
        public static Vector2 Clamp(Vector2 v, Vector2 min, Vector2 max) { return Vector2.Clamp(v, min, max); }
        public static Vector2 Normalize(Vector2 v) { return Vector2Normalize(v); }//return Vector2.Normalize(v); } //Vector2 normalize returns NaN sometimes???
        public static Vector2 Reflect(Vector2 v, Vector2 n) { return Vector2.Reflect(v, n); } //RayMath.Vector2Reflect(v, n);
        public static Vector2 Scale(Vector2 v, float amount) { return Vector2Scale(v, amount); }
        public static Vector2 ScaleUniform(Vector2 v, float distance)
        {
            float length = v.Length();
            if (length <= 0) return v;

            float scale = 1f + (distance / v.Length());
            return Scale(v, scale);
        }
        public static Vector2 SquareRoot(Vector2 v) { return Vector2.SquareRoot(v); }
        public static Vector2 Rotate(Vector2 v, float angle) { return Vector2Rotate(v, angle); } //radians
        public static float AngleDeg(Vector2 v1, Vector2 v2) { return AngleRad(v1, v2) * RAD2DEG; }
        public static float AngleDeg(Vector2 v)
        {
            return AngleRad(v) * RAD2DEG;
        }
        public static float AngleRad(Vector2 v) { return AngleRad(Zero(), v); }
        public static float AngleRad(Vector2 v1, Vector2 v2) { return Vector2Angle(v1, v2); }
        public static float Distance(Vector2 v1, Vector2 v2) { return Vector2.Distance(v1, v2); }// RayMath.Vector2Distance(v1, v2); }
        public static float Dot(Vector2 v1, Vector2 v2) { return Vector2.Dot(v1, v2); }// RayMath.Vector2DotProduct(v1, v2); }
        public static float Length(Vector2 v) { return v.Length(); } //RayMath.Vector2Length(v);
        public static float LengthSquared(Vector2 v) { return v.LengthSquared(); } //RayMath.Vector2LengthSqr(v);


    }
}
