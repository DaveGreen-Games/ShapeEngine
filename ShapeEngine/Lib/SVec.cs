
using System.Numerics;
using ShapeEngine.Core;

namespace ShapeEngine.Lib
{
    public static class SVec
    {
        public static bool IsSimilar(this Vector2 a, Vector2 b, float tolerance = 0.001f)
        {
            return 
                MathF.Abs(a.X - b.X) <= tolerance &&
                MathF.Abs(a.Y - b.Y) <= tolerance;
        }
        public static Vector2 Flip(this Vector2 v) { return v * -1f; }
        public static bool IsFacingTheSameDirection(this Vector2 a,  Vector2 b) { return a.Dot(b) > 0; }
        public static bool IsFacingTheOppositeDirection(this Vector2 a, Vector2 b) { return a.Dot(b) < 0; }
        public static bool IsNormalFacingOutward(this Vector2 normal, Vector2 outwardDirection) { return normal.IsFacingTheSameDirection(outwardDirection); }
        public static Vector2 GetOutwardFacingNormal(this Vector2 normal, Vector2 outwardDirection)
        {
            if(IsNormalFacingOutward(normal, outwardDirection)) return normal;
            else return -normal;
        }
        public static bool IsColinear(Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 prevCur = a - b;
            Vector2 nextCur = c - b;

            return prevCur.Cross(nextCur) == 0f;
        }

        public static float GetArea(this Vector2 v) { return v.X * v.Y; }

        public static Vector2 DivideSafe(this Vector2 a, Vector2 b)
        {
            return new
            (
                b.X == 0f ? 1f : a.X / b.X,
                b.Y == 0f ? 1f : a.Y / b.Y
            );
        }
        public static bool IsNan(this Vector2 v) { return float.IsNaN(v.X) || float.IsNaN(v.Y); }
        public static Vector2 Right() { return new(1.0f, 0.0f); }
        public static Vector2 Left() { return new(-1.0f, 0.0f); }
        public static Vector2 Up() { return new(0.0f, -1.0f); }
        public static Vector2 Down() { return new(0.0f, 1.0f); }
        public static Vector2 One() { return new(1.0f, 1.0f); }
        public static Vector2 Zero() { return new(0.0f, 0.0f); }

        //Perpendicular & Rotation
        public static Vector2 GetPerpendicularRight(this Vector2 v) { return new(-v.Y, v.X); }
        public static Vector2 GetPerpendicularLeft(this Vector2 v) { return new(v.Y, -v.X); }
        public static Vector2 Rotate90CCW(this Vector2 v) { return GetPerpendicularLeft(v); }
        public static Vector2 Rotate90CW(this Vector2 v) { return GetPerpendicularRight(v); }

        public static Vector2 VecFromAngleRad(float angleRad)
        {
            return new(MathF.Cos(angleRad), MathF.Sin(angleRad));
            //return SVec.Rotate(SVec.Right(), angleRad);
        }
        public static Vector2 VecFromAngleDeg(float angleDeg)
        {
            return VecFromAngleRad(angleDeg * SUtils.DEGTORAD);
        }

        public static Vector2 FindArithmeticMean(IEnumerable<Vector2> vertices)
        {
            float sx = 0f;
            float sy = 0f;
            int count = 0;
            foreach (var v in vertices)
            {
                sx += v.X;
                sy += v.Y;
                count ++;
            }

            float invArrayLen = 1f / (float)count;
            return new Vector2(sx * invArrayLen, sy * invArrayLen);
        }
        

        //Projection
        public static float ProjectionTime(this Vector2 v, Vector2 onto) { return (v.X * onto.X + v.Y * onto.Y) / onto.LengthSquared(); }
        public static Vector2 ProjectionPoint(this Vector2 point, Vector2 v, float t) { return point + v * t; }
        public static Vector2 Project(this Vector2 project, Vector2 onto)
        {
            float d = Vector2.Dot(onto, onto);
            if (d > 0.0f)
            {
                float dp = Vector2.Dot(project, onto);
                return onto * (dp / d);
            }
            return onto;
        }
        public static bool Parallel(this Vector2 a, Vector2 b)
        {
            Vector2 rotated = Rotate90CCW(a);
            return Vector2.Dot(rotated, b) == 0.0f;
        }

        public static Vector2 Align(this Vector2 pos, Vector2 size, Vector2 alignement)
        {
            return pos - size * alignement;
        }

        public static Vector2 Wrap(this Vector2 v, Vector2 min, Vector2 max)
        {
            return new
            (
                SUtils.WrapF(v.X, min.X, max.X),
                SUtils.WrapF(v.Y, min.Y, max.Y)
            );
        }
        public static Vector2 Wrap(this Vector2 v, float min, float max)
        {
            return new
            (
                SUtils.WrapF(v.X, min, max),
                SUtils.WrapF(v.Y, min, max)
            );
        }
        public static float Max(this Vector2 v) { return MathF.Max(v.X, v.Y); }
        public static float Min(this Vector2 v) { return MathF.Min(v.X, v.Y); }
        public static Vector2 LerpDirection(this Vector2 from, Vector2 to, float t)
        {
            float angleA = SVec.AngleRad(from);
            float angle = SUtils.GetShortestAngleRad(angleA, SVec.AngleRad(to));
            return SVec.Rotate(from, SUtils.LerpFloat(0, angle, t));
        }
        public static Vector2 Lerp(this Vector2 from, Vector2 to, float t) { return Vector2.Lerp(from, to, t); } //RayMath.Vector2Lerp(v1, v2, t);
        public static Vector2 MoveTowards(this Vector2 from, Vector2 to, float maxDistance) 
        {
            
            Vector2 result = new();
            float difX = to.X - from.X;
            float difY = to.Y - from.Y;
            float lengthSq = difX * difX + difY * difY;
            if (lengthSq == 0f || (maxDistance >= 0f && lengthSq <= maxDistance * maxDistance))
            {
                return to;
            }

            float length = MathF.Sqrt(lengthSq);
            result.X = from.X + difX / length * maxDistance;
            result.Y = from.Y + difY / length * maxDistance;
            return result;
        }
        public static Vector2 Floor(this Vector2 v) { return new(MathF.Floor(v.X), MathF.Floor(v.Y)); }
        public static Vector2 Ceiling(this Vector2 v) { return new(MathF.Ceiling(v.X), MathF.Ceiling(v.Y)); }
        public static Vector2 Round(this Vector2 v) { return new(MathF.Round(v.X), MathF.Round(v.Y)); }
        public static Vector2 Truncate(this Vector2 v) { return new(MathF.Truncate(v.X), MathF.Truncate(v.Y)); }
        public static Vector2 Abs(this Vector2 v) { return Vector2.Abs(v); }
        public static Vector2 Negate(this Vector2 v) { return Vector2.Negate(v); } //RayMath.Vector2Negate(v);
        public static Vector2 Min(this Vector2 v1, Vector2 v2) { return Vector2.Min(v1, v2); }
        public static Vector2 Max(this Vector2 v1, Vector2 v2) { return Vector2.Max(v1, v2); }
        public static Vector2 Clamp(this Vector2 v, Vector2 min, Vector2 max) { return Vector2.Clamp(v, min, max); }
        public static Vector2 Clamp(this Vector2 v, float min, float max) { return Vector2.Clamp(v, new(min), new(max)); }
        //return value / value.Length(); // RayMath.Vector2Normalize(v); }//return Vector2.Normalize(v); } //Vector2 normalize returns NaN sometimes???
        public static Vector2 Normalize(this Vector2 v) 
        {
            //vector2 with length 0 can not be normalized (division by 0)
            //and vector2 normalize does not check for that...
            if (v.X == 0f && v.Y == 0f) return new Vector2(0f, 0f);
            return Vector2.Normalize(v); 
        } 
        public static Vector2 Reflect(this Vector2 v, Vector2 n) { return Vector2.Reflect(v, n); } //RayMath.Vector2Reflect(v, n);
        //public static Vector2 Scale(this Vector2 v, float amount) { return RayMath.Vector2Scale(v, amount); }
        public static Vector2 ScaleUniform(this Vector2 v, float distance)
        {
            float length = v.Length();
            if (length <= 0) return v;

            float scale = 1f + (distance / v.Length());
            return v * scale; // Scale(v, scale);
        }
        public static Vector2 SquareRoot(this Vector2 v) { return Vector2.SquareRoot(v); }
        public static Vector2 Rotate(this Vector2 v, float angleRad) 
        {
            Vector2 result = new();
            float num = MathF.Cos(angleRad);
            float num2 = MathF.Sin(angleRad);
            result.X = v.X * num - v.Y * num2;
            result.Y = v.X * num2 + v.Y * num;
            return result;

            //return RayMath.Vector2Rotate(v, angleRad); 
        } //radians
        public static Vector2 RotateDeg(this Vector2 v, float angleDeg) { return Rotate(v, angleDeg * SUtils.DEGTORAD); }
        public static float AngleDeg(this Vector2 v1, Vector2 v2) { return AngleRad(v1, v2) * SUtils.RADTODEG; }
        public static float AngleDeg(this Vector2 v) { return AngleRad(v) * SUtils.RADTODEG; }
        public static float AngleRad(this Vector2 v) { return AngleRad(Zero(), v); }
        public static float AngleRad(this Vector2 v1, Vector2 v2) { return MathF.Atan2(v2.Y, v2.X) - MathF.Atan2(v1.Y, v1.X); }// return RayMath.Vector2Angle(v1, v2); }
        public static float Distance(this Vector2 v1, Vector2 v2) { return Vector2.Distance(v1, v2); }// RayMath.Vector2Distance(v1, v2); }
        public static float Dot(this Vector2 v1, Vector2 v2) { return v1.X * v2.X + v1.Y * v2.Y; }// Vector2.Dot(v1, v2); }// RayMath.Vector2DotProduct(v1, v2); }
        public static float Cross(this Vector2 value1, Vector2 value2) { return value1.X * value2.Y - value1.Y * value2.X; }
        
        //public static float Length(Vector2 v) { return v.Length(); } //RayMath.Vector2Length(v);
        //public static float LengthSquared(Vector2 v) { return v.LengthSquared(); } //RayMath.Vector2LengthSqr(v);


    }
}
