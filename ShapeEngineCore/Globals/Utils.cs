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

        public static List<Vector2> ScalePolygon(List<Vector2> poly, float scale)
        {
            var points = new List<Vector2>();
            for (int i = 0; i < poly.Count; i++)
            {
                points.Add(Vec.Scale(poly[i], scale));
            }
            return points;
        }

        public static List<Vector2> ScalePolygonUniform(List<Vector2> poly, float distance)
        {
            var points = new List<Vector2>();
            for (int i = 0; i < poly.Count; i++)
            {
                float length = poly[i].Length();
                if (length <= 0f)
                {
                    points.Add(poly[i]);
                    continue;
                }
                float scale = 1f + (distance / length);
                points.Add(Vec.Scale(poly[i], scale));
            }
            return points;
        }

        public static List<Vector2> GeneratePolygon(int pointCount, Vector2 center, float minLength, float maxLength)
        {
            List<Vector2> points = new();
            float angleStep = PI * 2.0f / pointCount;

            for (int i = 0; i < pointCount; i++)
            {
                float randLength = RNG.randF(minLength, maxLength);
                Vector2 p = Vec.Rotate(Vec.Right(), angleStep * i) * randLength;
                p += center;
                points.Add(p);
            }
            return points;
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


        public static Color ChangeAlpha(Color c, byte a)
        {
            c.a = a;
            return c;
        }
        public static Color ChangeAlpha(Color c, int change)
        {
            int newAlpha = c.a - change;
            c.a = (byte)newAlpha;
            return c;
        }
        public static Color ChangeRed(Color c, byte r)
        {
            c.r = r;
            return c;
        }
        public static Color ChangeGreen(Color c, byte g)
        {
            c.g = g;
            return c;
        }
        public static Color ChangeBlue(Color c, byte b)
        {
            c.b = b;
            return c;
        }


        /// <summary>
        /// Creates color with corrected brightness.
        /// </summary>
        /// <param name="color">Color to correct.</param>
        /// <param name="correctionFactor">The brightness correction factor. Must be between -1 and 1. 
        /// Negative values produce darker colors.</param>
        /// <returns>
        /// Corrected <see cref="Color"/> structure.
        /// </returns>
        public static Color ChangeBrightness(Color color, float correctionFactor)
        {
            float red = color.r;
            float green = color.g;
            float blue = color.b;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return new((byte)red, (byte)green, (byte)blue, color.a);
        }

        public static Color ChangeColor(Color source, Color adjust, float p)
        {
            return LerpColor(source, adjust, p);
        }


        public static Color ChangeHUE(Color color, int amount)
        {
            var hvs = ColorToHSV(color);
            return ColorFromHSV((hvs.hue + amount) % 360, hvs.saturation, hvs.value);
        }

        public static (float hue, float saturation, float value) ColorToHSV(Color color)
        {
            int max = Math.Max(color.r, Math.Max(color.g, color.g));
            int min = Math.Min(color.r, Math.Min(color.g, color.b));

            var systemColor = System.Drawing.Color.FromArgb(color.a, color.r, color.g, color.b);

            float hue = systemColor.GetHue();
            float saturation = (max == 0) ? 0 : 1f - (1f * min / max);
            float value = max / 255f;

            return new(hue, saturation, value);
        }

        public static Color ColorFromHSV(float hue, float saturation, float value)
        {
            int hi = Convert.ToInt32(MathF.Floor(hue / 60)) % 6;
            float f = hue / 60 - MathF.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return new(v, t, p, 255);
            else if (hi == 1)
                return new(q, v, p, 255);
            else if (hi == 2)
                return new(p, v, t, 255);
            else if (hi == 3)
                return new(p, q, v, 255);
            else if (hi == 4)
                return new(t, p, v, 255);
            else
                return new(v, p, q, 255);
        }

    }
}
