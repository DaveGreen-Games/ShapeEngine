﻿using Raylib_CsLo;
using System.Numerics;
using ShapeColor;

namespace ShapeLib
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

        public int Rand() { return SRNG.randI(min, max); }
        public int Lerp(float f) { return (int)RayMath.Lerp(min, max, f); }
        public float Inverse(int value) { return (value - min) / (max - min); }
        public int Remap(RangeInt to, int value) { return to.Lerp(Inverse(value)); }
        public int Remap(int newMin, int newMax, int value) { return SUtils.LerpInt(newMin, newMax, Inverse(value)); }
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
        public void Sort()
        {
            if (min > max)
            {
                float temp = max;
                max = min;
                min = temp;
            }
        }
        public float Rand() { return SRNG.randF(min, max); }
        public float Lerp(float f) { return RayMath.Lerp(min, max, f); }
        public float Inverse(float value) { return (value - min) / (max - min); }
        public float Remap(RangeFloat to, float value) { return to.Lerp(Inverse(value)); }
        public float Remap(float newMin, float newMax, float value) { return SUtils.LerpFloat(newMin, newMax, Inverse(value)); }
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

        public Vector2 Rand() { return center + SRNG.randVec2(min, max); }
        public Vector2 Lerp(Vector2 end, float f) { return SVec.Lerp(center, end, f); }
    }
    
    
    
    public static class SUtils
    {
        public static bool Blinking(float timer, float interval)
        {
            if (interval <= 0f) return false;
            return (int)(timer / interval) % 2 == 0;
        }
        public static float CalculateFrameIndependentLerpFactor(float lerpPercentage, float dt)
        {
            float rate = 1f - MathF.Pow(1f - lerpPercentage, dt);
            return rate;
        }
        public static Vector2 GetNormal(Vector2 start, Vector2 end, Vector2 intersectionPoint, Vector2 referencePoint)
        {
            Vector2 dir = SVec.Normalize(start - end);
            Vector2 w = referencePoint - intersectionPoint;
            Vector2 n1 = new(dir.Y, -dir.X);
            Vector2 n2 = new(-dir.Y, dir.X);
            
            float d1 = SVec.Dot(w, n1);
            //float d2 = SVec.Dot(w, n2);
            return d1 > 0 ? n1 : n2;
        }
        public static Vector2 GetNormalOpposite(Vector2 start, Vector2 end, Vector2 intersectionPoint, Vector2 referencePoint)
        {
            Vector2 dir = SVec.Normalize(start - end);
            Vector2 w = referencePoint - intersectionPoint;
            Vector2 n1 = new(dir.Y, -dir.X);
            Vector2 n2 = new(-dir.Y, dir.X);

            float d1 = SVec.Dot(w, n1);
            //float d2 = SVec.Dot(w, n2);
            return d1 <= 0 ? n1 : n2;
        }

        public static (int col, int row) TransformIndexToCoordinates(int index, int rows, int cols, bool leftToRight = true)
        {
            if (leftToRight)
            {
                int row = index / cols;
                int col = index % cols;
                return (col, row);
            }
            else
            {
                int col = index / rows;
                int row = index % rows;
                return (col, row);
            }
            
        }

        public static int TransformCoordinatesToIndex(int row, int col, int rows, int cols, bool leftToRight = true)
        {
            if (leftToRight)
            {
                return row * cols + col;
            }
            else
            {
                return col * rows + row;
            }
        }

        public static int AbsInt(int value)
        {
            return (int)MathF.Abs(value);
        }
        public static dynamic LerpDynamic(dynamic a, dynamic b, float f)
        {
            if (a is float) return LerpFloat(a, b, f);
            else if (a is int) return LerpInt(a, b, f);
            else if (a is Vector2) return SVec.Lerp(a, b, f);
            else if (a is Color) return SColor.LerpColor(a, b, f);
            else return a;
        }
        public static float LerpFloat(float a, float b, float f)
        {
            //return (1.0f - f) * a + b * f;
            return RayMath.Lerp(a, b, f);
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
            return (int)RayMath.Lerp(a, b, f);
        }
        public static float LerpInverseInt(int a, int b, int value)
        {
            float cur = (float)(value - a);
            float max = (float)(b - a);
            return cur / max;
        }
        public static int RemapInt(int value, int minOld, int maxOld, int minNew, int maxNew)
        {
            return LerpInt(minNew, maxNew, LerpInverseInt(minOld, maxOld, value));
        }
        public static float WrapAngleRad(float amount)
        {
            return WrapF(amount, 0f, 2.0f * RayMath.PI);
        }
        public static float WrapAngleDeg(float amount)
        {
            return WrapF(amount, 0f, 360f);
        }
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
            if (MathF.Abs(dif) > RayMath.PI)
            {
                if (dif > 0) dif -= 2f * RayMath.PI;
                else if (dif < 0) dif += 2f * RayMath.PI;
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


        
        public static float AimAt(Vector2 pos, Vector2 targetPos, float curAngleRad, float rotSpeedRad, float dt)
        {
            return AimAt(curAngleRad, SVec.AngleRad(targetPos - pos), rotSpeedRad, dt);
        }
        public static float AimAt(float curAngleRad, float targetAngleRad, float rotSpeedRad, float dt)
        {
            float dif = SUtils.GetShortestAngleRad(curAngleRad, targetAngleRad);
            float amount = MathF.Min(rotSpeedRad * dt, MathF.Abs(dif));
            float dir = 1;
            if (dif < 0) dir = -1;
            else if (dir == 0) dir = 0;
            return dir * amount;
        }


        
    }
}
