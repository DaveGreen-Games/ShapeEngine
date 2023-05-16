using ShapeCore;
using System.Numerics;

namespace ShapeLib
{
    public static class SSegment
    {
        public static List<Segment> Split(this Segment l, float f)
        {
            Vector2 p = GetPoint(l, f);
            return new() { new(l.start, p), new(p, l.end) };
        }
        public static Vector2 GetPoint(this Segment l, float f) { return l.start.Lerp(l.end, f); }
        public static Vector2 GetRandomPoint(this Segment l) { return GetPoint(l, SRNG.randF()); }
        public static Segment Rotate(this Segment l, float pivot, float rad)
        {
            Vector2 p = GetPoint(l, pivot);
            Vector2 s = l.start - p;
            Vector2 e = l.end - p;
            return new Segment(p + s.Rotate(rad), p + e.Rotate(rad));


            //float len = l.Length;
            //Vector2 d = l.Dir;
            //
            //float startLength = len * pivot;
            //float endLength = len * (1f - pivot);
            //
            //Vector2 p = l.start + d * startLength;
            //Vector2 newStart = p - (d * startLength).Rotate(rad);
            //Vector2 newEnd = p + (d * endLength).Rotate(rad);
            //return new Line(newStart, newEnd);
        }

        public static Segment Scale(this Segment l, float scale) { return new(l.start * scale, l.end * scale); }
        public static Segment Scale(this Segment l, Vector2 scale) { return new(l.start * scale, l.end * scale); }
        public static Segment Scale(this Segment l, float startScale, float endScale) { return new(l.start * startScale, l.end * endScale); }
        public static Segment ScaleF(this Segment l, float scale, float f)
        {
            Vector2 p = GetPoint(l, f);
            Vector2 s = l.start - p;
            Vector2 e = l.end - p;
            return new Segment(p + s * scale, p + e * scale);

            //float len = l.Length;
            //Vector2 d = l.Dir;
            //
            //float startLength = len * f;
            //float endLength = len * (1f - f);
            //
            //Vector2 p = l.start + d * startLength;
            //Vector2 newStart = p - (d * startLength * scale);
            //Vector2 newEnd = p + (d * endLength * scale);
            //return new Line(newStart, newEnd);
        }
        public static Segment ScaleF(this Segment l, Vector2 scale, float f)
        {
            Vector2 p = GetPoint(l, f);
            Vector2 s = l.start - p;
            Vector2 e = l.end - p;
            return new Segment(p + s * scale, p + e * scale);
        }
        public static Segment Move(this Segment l, Vector2 offset, float f) { return new(l.start + (offset * (1f - f)), l.end + (offset * (f))); }
        public static Segment Move(this Segment l, Vector2 offset) { return new(l.start + offset, l.end + offset); }
        public static Segment Move(this Segment l, float x, float y) { return Move(l, new Vector2(x, y)); }
        public static List<Vector2> GetPoints(this Segment s) { return new() { s.start, s.end }; }

        public static Vector2 GetClosestPoint(this Segment s, Vector2 p)
        {
            var w = s.Displacement;
            float t = (p - s.start).Dot(w) / w.LengthSquared();
            if (t < 0f) return s.start;
            else if(t > 1f) return s.end;
            else return s.start + w * t;
        }


    }
}
