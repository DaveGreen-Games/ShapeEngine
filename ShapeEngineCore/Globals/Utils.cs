using Raylib_CsLo;
using System.Numerics;
//using ShapeEngineCore.Globals;


namespace ShapeEngineCore.Globals
{

    public class RangeInt
    {
        public int min;
        public int max;

        public RangeInt() { min = 0; max = 100; }
        public RangeInt(int min, int max)
        {
            if (min > max)
            {
                this.max = min;
                this.min = max;
            }
            else
            {
                this.min = min;
                this.max = max;
            }
        }
        public RangeInt(int max)
        {
            if (max < 0)
            {
                min = max;
                this.max = 0;
            }
            else
            {
                min = 0;
                this.max = max;
            }
        }

        public int Rand() { return RNG.randI(min, max); }
        public int Lerp(float f) { return (int)RayMath.Lerp(min, max, f); }
        public float Inverse(int value) { return (value - min) / (max - min); }
        public int Remap(RangeInt to, int value) { return to.Lerp(Inverse(value)); }
        public int Remap(int newMin, int newMax, int value) { return Utils.LerpInt(newMin, newMax, Inverse(value)); }
    }
    public class RangeFloat
    {
        public float min;
        public float max;

        public RangeFloat() { min = 0.0f; max = 1.0f; }
        public RangeFloat(float min, float max)
        {
            if (min > max)
            {
                this.max = min;
                this.min = max;
            }
            else
            {
                this.min = min;
                this.max = max;
            }
        }
        public RangeFloat(float max)
        {
            if (max < 0.0f)
            {
                min = max;
                this.max = 0.0f;
            }
            else
            {
                min = 0.0f;
                this.max = max;
            }
        }

        public float Rand() { return RNG.randF(min, max); }
        public float Lerp(float f) { return RayMath.Lerp(min, max, f); }
        public float Inverse(float value) { return (value - min) / (max - min); }
        public float Remap(RangeFloat to, float value) { return to.Lerp(Inverse(value)); }
        public float Remap(float newMin, float newMax, float value) { return Utils.LerpFloat(newMin, newMax, Inverse(value)); }
    }
    public class RangeVector2
    {
        public float min;
        public float max;
        public Vector2 center;
        public RangeVector2() { center = new(); min = 0.0f; max = 1.0f; }
        public RangeVector2(Vector2 center) { this.center = center; min = 0f; max = 1f; }
        public RangeVector2(float min, float max)
        {
            center = new();
            if (min > max)
            {
                this.max = min;
                this.min = max;
            }
            else
            {
                this.min = min;
                this.max = max;
            }
        }
        public RangeVector2(Vector2 center, float min, float max)
        {
            this.center = center;
            if (min > max)
            {
                this.max = min;
                this.min = max;
            }
            else
            {
                this.min = min;
                this.max = max;
            }
        }
        public RangeVector2(float max)
        {
            center = new();
            if (max < 0.0f)
            {
                min = max;
                this.max = 0.0f;
            }
            else
            {
                min = 0.0f;
                this.max = max;
            }
        }
        public RangeVector2(Vector2 center, float max)
        {
            this.center = center;
            if (max < 0.0f)
            {
                min = max;
                this.max = 0.0f;
            }
            else
            {
                min = 0.0f;
                this.max = max;
            }
        }

        public Vector2 Rand() { return center + RNG.randVec2(min, max); }
        public Vector2 Lerp(Vector2 end, float f) { return Vec.Lerp(center, end, f); }
    }
    public static class Utils
    {
        public static Rectangle EnlargeRectangle(Rectangle source, Vector2 point)
        {
            Vector2 newPos = new
                (
                    MathF.Min(source.X, point.X),
                    MathF.Min(source.Y, point.Y)
                );
            Vector2 newSize = new
                (
                    MathF.Max(source.X + source.width, point.X),
                    MathF.Max(source.Y + source.height, point.Y)
                );

            return new(newPos.X, newPos.Y, newSize.X, newSize.Y);
        }
        public static Rectangle ScaleRectangle(Rectangle rect, float scale, Vector2 pivot)
        {
            float newWidth = rect.width * scale;
            float newHeight = rect.height * scale;
            return new Rectangle(rect.x - (newWidth - rect.width) * pivot.X, rect.y - (newHeight - rect.height) * pivot.Y, newWidth, newHeight);
        }
        public static Rectangle ScaleRectangle(Rectangle rect, float scale)
        {
            float newWidth = rect.width * scale;
            float newHeight = rect.height * scale;
            return new Rectangle(rect.x - (newWidth - rect.width) / 2, rect.y - (newHeight - rect.height) / 2, newWidth, newHeight);
        }

        public static Vector2 Attraction(Vector2 center, Vector2 otherPos, Vector2 otherVel, float r, float strength, float friction)
        {
            Vector2 w = center - otherPos;
            float disSq = w.LengthSquared();
            float f = 1.0f - disSq / (r * r);
            Vector2 force = Vec.Normalize(w) * strength;// * f;
            Vector2 stop = -otherVel * friction * f;
            return force + stop;
        }

        public static Vector2 ElasticCollision1D(Vector2 vel1, float mass1, Vector2 vel2, float mass2)
        {
            float totalMass = mass1 + mass2;
            return vel1 * ((mass1 - mass2) / totalMass) + vel2 * (2f * mass2 / totalMass);
        }
        public static Vector2 ElasticCollision2D(Vector2 p1, Vector2 v1, float m1, Vector2 p2, Vector2 v2, float m2, float R)
        {
            float totalMass = m1 + m2;

            float mf = m2 / m1;
            Vector2 posDif = p2 - p1;
            Vector2 velDif = v2 - v1;

            Vector2 velCM = (m1 * v1 + m2 * v2) / totalMass;

            float a = posDif.Y / posDif.X;
            float dvx2 = -2f * (velDif.X + a * velDif.Y) / ((1 + a * a) * (1 + mf));
            //Vector2 newOtherVel = new(v2.X + dvx2, v2.Y + a * dvx2);
            Vector2 newSelfVel = new(v1.X - mf * dvx2, v1.Y - a * mf * dvx2);

            newSelfVel = (newSelfVel - velCM) * R + velCM;
            //newOtherVel = (newOtherVel - velCM) * R + velCM;

            return newSelfVel;
        }
        public static int AbsInt(int value)
        {
            return (int)MathF.Abs(value);
        }


        public static dynamic LerpDynamic(dynamic a, dynamic b, float f)
        {
            if (a is float) return LerpFloat(a, b, f);
            else if (a is int) return LerpInt(a, b, f);
            else if (a is Vector2) return Vec.Lerp(a, b, f);
            else if (a is Color) return LerpColor(a, b, f);
            else return a;
        }
        public static float LerpFloat(float a, float b, float f)
        {
            //return (1.0f - f) * a + b * f;
            return Lerp(a, b, f);
        }
        public static float LerpInverseFloat(float a, float b, float value)
        {
            return (value - a) / (b - a);
        }
        public static float RemapFloat(float value, float minOld, float maxOld, float minNew, float maxNew)
        {
            return LerpFloat(minNew, maxNew, LerpInverseFloat(minOld, maxOld, value));
        }

        public static int LerpInt(int a, int b, float f)
        {
            return (int)Lerp(a, b, f);
        }
        public static float LerpInverseInt(int a, int b, int value)
        {
            return (value - a) / (b - a);
        }
        public static int RemapInt(int value, int minOld, int maxOld, int minNew, int maxNew)
        {
            return LerpInt(minNew, maxNew, LerpInverseInt(minOld, maxOld, value));
        }

        public static Color LerpColor(Color a, Color b, float f)
        {
            return new Color(
                (int)Lerp(a.r, b.r, f),
                (int)Lerp(a.g, b.g, f),
                (int)Lerp(a.b, b.b, f),
                (int)Lerp(a.a, b.a, f));
        }

        public static float WrapAngleRad(float amount)
        {
            return WrapF(amount, 0f, 2.0f * PI);
        }
        public static float WrapAngleDeg(float amount)
        {
            return WrapF(amount, 0f, 360f);
        }

        //RMAPI float Wrap(float value, float min, float max)
        //{
        //    float result = min + fmodf(value - min, max - min);
        //
        //    return result;
        //}
        public static float WrapF(float value, float min, float max)
        {
            float range = max - min;
            return range == 0 ? min : value - range * MathF.Floor((value - min) / range);
        }
        public static int WrapI(int value, int min, int max)
        {
            int range = max - min;
            return range == 0 ? min : value - range * (int)MathF.Floor((value - min) / range);
        }

        public static float LerpAngleRad(float from, float to, float f)
        {
            return from + GetShortestAngleRad(from, to) * f;
        }
        public static float LerpAngleDeg(float from, float to, float f)
        {
            return from + GetShortestAngleDeg(from, to) * f;
        }
        public static float GetShortestAngleRad(float from, float to)
        {
            //from = WrapAngleRad(from);
            //to = WrapAngleRad(to);
            float dif = to - from;
            if (MathF.Abs(dif) > PI)
            {
                if (dif > 0) dif -= 2f * PI;
                else if (dif < 0) dif += 2f * PI;
            }
            return dif;

            //return WrapF(to - from, 0f, PI * 2f);
        }
        public static float GetShortestAngleDeg(float from, float to)
        {
            float dif = to - from;
            if (MathF.Abs(dif) > 180f)
            {
                if (dif > 0) dif -= 360f;
                else if (dif < 0) dif += 360f;
            }
            return dif;
        }

    }
}
