using System.Numerics;

namespace ShapeEngine.Lib
{
    public static class ShapeUtils
    {
        public const float FloatComparisonTolerance = 0.001f;


        public static bool IsEqual<T>(List<T>? a, List<T>? b) where T : IEquatable<T>
        {
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;
            for (var i = 0; i < a.Count; i++)
            {
                if (!a[i].Equals(b[i])) return false;
            }
            return true;
        }
        public static int GetHashCode<T>(IEnumerable<T> collection)
        {
            HashCode hash = new();
            foreach (var element in collection)
            {
                hash.Add(element);
            }
            return hash.ToHashCode();
        }
        public static bool IsSimilar(float a, float b, float tolerance = FloatComparisonTolerance) => MathF.Abs(a - b) <= tolerance;
        public static float GetFactor(float cur, float min, float max)
        {
            return (cur - min) / (max - min);
        }
        public static T GetItem<T>(List<T> collection, int index)
        {
            int i = ShapeMath.WrapIndex(collection.Count, index);
            return collection[i];
        }
        public static bool Blinking(float timer, float interval)
        {
            if (interval <= 0f) return false;
            return (int)(timer / interval) % 2 == 0;
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

        public static float AimAt(Vector2 pos, Vector2 targetPos, float curAngleRad, float rotSpeedRad, float dt)
        {
            return AimAt(curAngleRad, ShapeVec.AngleRad(targetPos - pos), rotSpeedRad, dt);
        }
        public static float AimAt(float curAngleRad, float targetAngleRad, float rotSpeedRad, float dt)
        {
            float dif = ShapeMath.GetShortestAngleRad(curAngleRad, targetAngleRad);
            float amount = MathF.Min(rotSpeedRad * dt, MathF.Abs(dif));
            float dir = 1;
            if (dif < 0) dir = -1;
            else if (dir == 0) dir = 0;
            return dir * amount;
        }

        
        
        
        
    }

}

/*
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
        */