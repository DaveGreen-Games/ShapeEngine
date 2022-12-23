using System.Numerics;
using Raylib_CsLo;
using ShapeLib;

namespace ShapeCollision
{
    public struct CastInfo
    {
        public bool overlapping = false;
        public bool collided = false;
        public float time = 0.0f;
        public Vector2 intersectionPoint = new();
        public Vector2 collisionPoint = new();
        public Vector2 reflectVector = new();
        public Vector2 normal = new();
        public ICollidable? self = null;
        public ICollidable? other = null;
        public Vector2 selfVel = new();
        public Vector2 otherVel = new();

        public CastInfo() { overlapping = false; collided = false; }
        public CastInfo(bool overlapping) { this.overlapping = overlapping; collided = false; }
        public CastInfo(bool overlapping, bool collided) { this.overlapping = overlapping; this.collided = collided; }
        public CastInfo(bool overlapping, bool collided, float time, Vector2 intersectionPoint, Vector2 collisionPoint, Vector2 reflectVector, Vector2 normal, Vector2 selfVel, Vector2 otherVel)
        {
            this.overlapping = overlapping;
            this.collided = collided;
            this.time = time;
            this.intersectionPoint = intersectionPoint;
            this.collisionPoint = collisionPoint;
            this.reflectVector = reflectVector;
            this.normal = normal;
            this.selfVel = selfVel;
            this.otherVel = otherVel;
        }
    }


    public static class SCast
    {

        //exact point line, point segment and point point overlap calculations are used if <= 0
        public static readonly float POINT_OVERLAP_EPSILON = 5.0f; //point line and point segment overlap makes more sense when the point is a circle (epsilon = radius)

        //CAST (SemiDynamic - Get Collision Response only for first object - second object can have vel as well)

        private static (bool intersected, Vector2 intersectPoint, float time) IntersectPointCircle(Vector2 point, Vector2 vel, Vector2 circlePos, float radius)
        {
            Vector2 w = circlePos - point;
            float qa = vel.LengthSquared();
            float qb = vel.X * w.X + vel.Y * w.Y;
            float qc = w.LengthSquared() - radius * radius;

            float qd = qb * qb - qa * qc;
            if (qd < 0.0f) return (false, new(0f), 0f);
            float t = (qb - MathF.Sqrt(qd)) / qa;
            if (t < 0.0f || t > 1.0f) return (false, new(0f), 0f);

            Vector2 intersectionPoint = point + vel * t; // new(point.X + vel.X * t, point.Y + vel.Y * t);
            return (true, intersectionPoint, t);
        }

        public static CastInfo CastIntersection(Collider point, CircleCollider circle, float dt)
        {
            bool overlapping = SGeometry.Overlap(point, circle);// Contains(circle.Pos, circle.Radius, point.Pos);
            Vector2 vel = point.Vel - circle.Vel; //-> simple way of making sure second object is static and first is dynamic
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);

            Vector2 w = circle.Pos - point.Pos;
            float qa = vel.LengthSquared();
            float qb = vel.X * w.X + vel.Y * w.Y;
            float qc = w.LengthSquared() - circle.RadiusSquared;
            float qd = qb * qb - qa * qc;

            if (qd < 0.0f) return new();
            float t = (qb - MathF.Sqrt(qd)) / qa;
            if (t < 0.0f || t > 1.0f) return new();

            Vector2 intersectPoint = point.Pos + vel * t;
            Vector2 collisionPoint = intersectPoint;
            Vector2 normal = (intersectPoint - circle.Pos) / circle.Radius;
            float remaining = 1.0f - t;
            float d = 2.0f * (vel.X * normal.X + vel.Y * normal.Y);
            Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
            //Vector2 reflectPoint = intersectPoint + reflectVector;

            return new(false, true, t, intersectPoint, collisionPoint, reflectVector, normal, point.Vel, circle.Vel);
        }
        public static CastInfo CastIntersection(CircleCollider circle, Collider point, float dt)
        {
            bool overlapping = SGeometry.Overlap(circle, point);// Contains(circle.Pos, circle.Radius, point.Pos);
            Vector2 vel = circle.Vel - point.Vel;
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);

            Vector2 w = point.Pos - circle.Pos;

            float qa = vel.LengthSquared();
            float qb = vel.X * w.X + vel.Y * w.Y;
            float qc = w.LengthSquared() - circle.RadiusSquared;
            float qd = qb * qb - qa * qc;
            if (qd < 0) return new();
            float t = (qb - MathF.Sqrt(qd)) / qa;
            if (t < 0 || t > 1) return new();

            Vector2 intersectionPoint = circle.Pos + vel * t;
            Vector2 collisionPoint = point.Pos;
            Vector2 normal = (intersectionPoint - point.Pos) / circle.Radius;
            float remaining = 1.0f - t;
            float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
            Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
            //Vector2 reflectPoint = intersectionPoint + reflectVector;
            return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, point.Vel);
        }
        public static CastInfo CastIntersection(CircleCollider self, CircleCollider other, float dt)
        {
            bool overlapping = SGeometry.Overlap(self, other);
            Vector2 vel = self.Vel - other.Vel;
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);

            float r = self.Radius + other.Radius;
            var intersectionInfo = IntersectPointCircle(self.Pos, vel, other.Pos, r);
            if (!intersectionInfo.intersected) return new();
            float remaining = 1f - intersectionInfo.time;
            Vector2 normal = (intersectionInfo.intersectPoint - other.Pos) / r;
            Vector2 collisionPoint = other.Pos + normal * other.Radius;
            //Vector2 reflectVector = Utils.ElasticCollision2D(self.Pos, self.Vel, self.Mass, other.Pos, other.Vel, other.Mass, 1f);
            if (Vector2.Dot(vel, normal) > 0f) vel *= -1;
            float dot = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
            Vector2 reflectVector = new(remaining * (vel.X - dot * normal.X), remaining * (vel.Y - dot * normal.Y));
            //Vector2 reflectPoint = intersectionInfo.point + reflectVector;
            return new(false, true, intersectionInfo.time, intersectionInfo.intersectPoint, collisionPoint, reflectVector, normal, self.Vel, other.Vel);
        }
        public static CastInfo CastIntersection(Collider self, Collider other, float dt)
        {
            //REAL Point - Point collision basically never happens.... so this is the point - circle cast code!!!
            CircleCollider circle = new(other.Pos, other.Vel, POINT_OVERLAP_EPSILON);

            bool overlapping = SGeometry.Overlap(self, circle);// Contains(circle.Pos, circle.Radius, point.Pos);
            Vector2 vel = self.Vel - circle.Vel; //-> simple way of making sure second object is static and first is dynamic
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);

            Vector2 w = circle.Pos - self.Pos;
            float qa = vel.LengthSquared();
            float qb = vel.X * w.X + vel.Y * w.Y;
            float qc = w.LengthSquared() - circle.RadiusSquared;
            float qd = qb * qb - qa * qc;

            if (qd < 0.0f) return new();
            float t = (qb - MathF.Sqrt(qd)) / qa;
            if (t < 0.0f || t > 1.0f) return new();

            Vector2 intersectPoint = self.Pos + vel * t;
            Vector2 collisionPoint = intersectPoint;
            Vector2 normal = (intersectPoint - circle.Pos) / circle.Radius;
            float remaining = 1.0f - t;
            float d = 2.0f * (vel.X * normal.X + vel.Y * normal.Y);
            Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
            //Vector2 reflectPoint = intersectPoint + reflectVector;
            return new(false, true, t, intersectPoint, collisionPoint, reflectVector, normal, self.Vel, circle.Vel);
            /*
            bool overlapping = Overlap.Simple(self, other);
            Vector2 vel = self.Vel - other.Vel;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new CollisionInfo() { collided = false, overlapping = overlapping };

            Vector2 w = Helper.Floor(other.Pos) - Helper.Floor(self.Pos); //displacement
            float p = w.X * vel.Y - w.Y * vel.X; //perpendicular product
            if(p != 0.0f) return new CollisionInfo { overlapping = false, collided = false };
            float t = (w.X * vel.X + w.Y * vel.Y) / vel.LengthSquared();
            if(t < 0.0f || t > 1.0f) return new CollisionInfo() { overlapping = false, collided=false };

            Vector2 intersectionPoint = other.Pos;
            Vector2 collisionPoint = intersectionPoint;
            float len = vel.Length();
            Vector2 normal = -vel / len;
            float remaining = 1.0f - t;
            float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
            Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (self.Vel.Y - normal.Y * d));
            Vector2 reflectPoint = intersectionPoint + reflectVector;

            return new CollisionInfo 
            { 
                overlapping = false, 
                collided = true,
                self = new CollisionResponse { shape = self, available = true, normal = normal, intersectPoint = intersectionPoint, reflectVector = reflectVector, reflectPoint = reflectPoint },
                other = new CollisionResponse { shape = other, available = false},
                intersectPoint = intersectionPoint,
                collisionPoint = collisionPoint,
                time = t,
                remaining = remaining
            };
            */
        }
        public static CastInfo CastIntersection(Collider point, SegmentCollider segment, float dt)
        {
            //bool overlapping = Overlap.Simple(point, segment);
            Vector2 vel = point.Vel - segment.Vel;
            vel *= dt;
            if (vel.LengthSquared() <= 0.0f) return new();
            Vector2 sv = segment.Dir * segment.Length;
            Vector2 w = segment.Pos - point.Pos;
            float projectionTime = -(w.X * sv.X + w.Y * sv.Y) / sv.LengthSquared();
            if (projectionTime < 0.0f)//behind
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f)//parallel
                {
                    float c = w.X * segment.Dir.Y - w.Y * segment.Dir.X;
                    if (c != 0.0f) return new();
                    float t;
                    if (vel.X == 0.0f) t = w.Y / vel.Y;
                    else t = w.X / vel.X;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = segment.Pos;
                    Vector2 collisionPoint = intersectionPoint;
                    Vector2 normal = segment.Dir * -1.0f;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, segment.Vel);
                }
                else //not parallel
                {
                    float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                    if (ts < 0.0f || ts > 1.0f) return new();
                    float t = (sv.X * w.Y - sv.Y * w.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();
                    if (ts == 0.0f)
                    {
                        Vector2 intersectionPoint = segment.Pos;
                        Vector2 collisionPoint = intersectionPoint;
                        Vector2 normal = segment.Dir * -1.0f;
                        float remaining = 1.0f - t;
                        float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                        Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                        //Vector2 reflectPoint = intersectionPoint + reflectVector;
                        return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, segment.Vel);
                    }
                    else
                    {
                        Vector2 intersectionPoint = point.Pos + vel * t;
                        Vector2 collisionPoint = intersectionPoint;
                        Vector2 normal;
                        if (p < 0) normal = new(-segment.Dir.Y, segment.Dir.X);
                        else normal = new(segment.Dir.Y, -segment.Dir.X);
                        float remaining = 1.0f - t;
                        float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                        Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                        //Vector2 reflectPoint = intersectionPoint + reflectVector;
                        return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, segment.Vel);
                    }
                }
            }
            else if (projectionTime > 1.0f)//ahead
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f) //parallel
                {
                    float c = w.X * segment.Dir.Y - w.Y * segment.Dir.X;
                    if (c != 0.0f) return new();
                    float t = vel.X == 0.0f ? w.Y / vel.Y - 1.0f : w.X / vel.X - 1.0f;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = segment.End;
                    Vector2 collisionPoint = intersectionPoint;
                    Vector2 normal = segment.Dir;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, segment.Vel);
                }
                else // not parallel
                {
                    float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                    if (ts < 0.0f || ts > 1.0f) return new();
                    float t = (sv.X * w.Y - sv.Y * w.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = segment.End;
                    Vector2 collisionPoint = intersectionPoint;
                    Vector2 normal = segment.Dir;

                    if (ts != 1.0f)
                    {
                        intersectionPoint = point.Pos + vel * t;
                        collisionPoint = intersectionPoint;
                        normal = p < 0.0f ? new(-segment.Dir.Y, segment.Dir.X) : new(segment.Dir.Y, -segment.Dir.X);
                    }

                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, segment.Vel);
                }
            }
            else//on
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f) return new();
                float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                if (ts < 0.0f || ts > 1.0f) return new();
                float t = (sv.X * w.Y - sv.Y * w.X) / p;
                if (t < 0.0f || t > 1.0f) return new();

                Vector2 intersectionPoint = point.Pos + vel * t;
                Vector2 collisionPoint = intersectionPoint;
                Vector2 normal = p < 0.0f ? new(-segment.Dir.Y, segment.Dir.X) : new(segment.Dir.Y, -segment.Dir.X);

                float remaining = 1.0f - t;
                float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                //Vector2 reflectPoint = intersectionPoint + reflectVector;
                return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, segment.Vel);
            }
        }
        public static CastInfo CastIntersection(CircleCollider circle, SegmentCollider segment, float dt)
        {
            bool overlapping = SGeometry.Overlap(circle, segment);
            Vector2 vel = circle.Vel - segment.Vel;
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);
            Vector2 sv = segment.Dir * segment.Length;
            float p = sv.X * vel.Y - sv.Y * vel.X;
            if (p < 0.0f)
            {

                Vector2 point = new(segment.Pos.X - segment.Dir.Y * circle.Radius, segment.Pos.Y + segment.Dir.X * circle.Radius);// segment.Pos - segment.Dir * circle.Radius;
                Vector2 w1 = point - circle.Pos;
                float ts = (vel.X * w1.Y - vel.Y * w1.X) / p;
                if (ts < 0.0f)
                {
                    Vector2 w2 = segment.Pos - circle.Pos;
                    float qa = vel.LengthSquared();
                    float qb = vel.X * w2.X + vel.Y * w2.Y;
                    float qc = w2.LengthSquared() - circle.RadiusSquared;
                    float qd = qb * qb - qa * qc;
                    if (qd < 0.0f) return new();
                    float t = (qb - MathF.Sqrt(qd)) / qa;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = segment.Pos;
                    Vector2 normal = (intersectionPoint - segment.Pos) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, segment.Vel);
                }
                else if (ts > 1.0f)
                {
                    Vector2 end = segment.Pos + sv;
                    Vector2 w2 = end - circle.Pos;
                    float qa = vel.LengthSquared();
                    float qb = vel.X * w2.X + vel.Y * w2.Y;
                    float qc = w2.LengthSquared() - circle.RadiusSquared;
                    float qd = qb * qb - qa * qc;
                    if (qd < 0.0f) return new();
                    float t = (qb - MathF.Sqrt(qd)) / qa;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = end;
                    Vector2 normal = (intersectionPoint - end) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, segment.Vel);
                }
                else
                {
                    float t = (sv.X * w1.Y - sv.Y * w1.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = new(intersectionPoint.X + segment.Dir.Y * circle.Radius, intersectionPoint.Y - segment.Dir.X * circle.Radius);
                    Vector2 normal = new(-segment.Dir.Y, segment.Dir.X);
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, segment.Vel);
                }
            }
            else if (p > 0.0f)
            {
                Vector2 p1 = new(segment.Pos.X + segment.Dir.Y * circle.Radius, segment.Pos.Y - segment.Dir.X * circle.Radius);// segment.Pos + segment.Dir * circle.Radius;
                Vector2 w1 = p1 - circle.Pos;
                float ts = (vel.X * w1.Y - vel.Y * w1.X) / p;
                if (ts < 0.0f)
                {
                    Vector2 w2 = segment.Pos - circle.Pos;
                    float qa = vel.LengthSquared();
                    float qb = vel.X * w2.X + vel.Y * w2.Y;
                    float qc = w2.LengthSquared() - circle.RadiusSquared;
                    float qd = qb * qb - qa * qc;
                    if (qd < 0.0f) return new();
                    float t = (qb - MathF.Sqrt(qd)) / qa;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = segment.Pos;
                    Vector2 normal = (intersectionPoint - segment.Pos) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, segment.Vel);
                }
                else if (ts > 1.0f)
                {
                    Vector2 end = segment.Pos + sv;// segment.End;
                    Vector2 w2 = end - circle.Pos;
                    float qa = vel.LengthSquared();
                    float qb = vel.X * w2.X + vel.Y * w2.Y;
                    float qc = w2.LengthSquared() - circle.RadiusSquared;
                    float qd = qb * qb - qa * qc;
                    if (qd < 0.0f) return new();
                    float t = (qb - MathF.Sqrt(qd)) / qa;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = end;
                    Vector2 normal = (intersectionPoint - end) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, segment.Vel);
                }
                else
                {
                    float t = (sv.X * w1.Y - sv.Y * w1.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = new(intersectionPoint.X - segment.Dir.Y * circle.Radius, intersectionPoint.Y + segment.Dir.X * circle.Radius); // intersectionPoint - segment.Dir * circle.Radius;
                    Vector2 normal = new(segment.Dir.Y, -segment.Dir.X);
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, segment.Vel);
                }
            }
            else
            {
                return new(true);
            }
        }
        public static CastInfo CastIntersection(Vector2 pointPos, Vector2 pointVel, Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Vector2 segmentVel, float dt)
        {
            //bool overlapping = Overlap.Simple(point, segment);
            Vector2 vel = pointVel - segmentVel;
            vel *= dt;
            if (vel.LengthSquared() <= 0.0f) return new();
            Vector2 sv = segmentDir * segmentLength;
            Vector2 w = segmentPos - pointPos;
            float projectionTime = -(w.X * sv.X + w.Y * sv.Y) / sv.LengthSquared();
            if (projectionTime < 0.0f)//behind
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f)//parallel
                {
                    float c = w.X * segmentDir.Y - w.Y * segmentDir.X;
                    if (c != 0.0f) return new();
                    float t;
                    if (vel.X == 0.0f) t = w.Y / vel.Y;
                    else t = w.X / vel.X;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = segmentPos;
                    Vector2 collisionPoint = intersectionPoint;
                    Vector2 normal = segmentDir * -1.0f;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, pointVel, segmentVel);
                }
                else //not parallel
                {
                    float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                    if (ts < 0.0f || ts > 1.0f) return new();
                    float t = (sv.X * w.Y - sv.Y * w.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();
                    if (ts == 0.0f)
                    {
                        Vector2 intersectionPoint = segmentPos;
                        Vector2 collisionPoint = intersectionPoint;
                        Vector2 normal = segmentDir * -1.0f;
                        float remaining = 1.0f - t;
                        float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                        Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                        //Vector2 reflectPoint = intersectionPoint + reflectVector;
                        return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, pointVel, segmentVel);
                    }
                    else
                    {
                        Vector2 intersectionPoint = pointPos + vel * t;
                        Vector2 collisionPoint = intersectionPoint;
                        Vector2 normal;
                        if (p < 0) normal = new(-segmentDir.Y, segmentDir.X);
                        else normal = new(segmentDir.Y, -segmentDir.X);
                        float remaining = 1.0f - t;
                        float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                        Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                        //Vector2 reflectPoint = intersectionPoint + reflectVector;
                        return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, pointVel, segmentVel);
                    }
                }
            }
            else if (projectionTime > 1.0f)//ahead
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f) //parallel
                {
                    float c = w.X * segmentDir.Y - w.Y * segmentDir.X;
                    if (c != 0.0f) return new();
                    float t = vel.X == 0.0f ? w.Y / vel.Y - 1.0f : w.X / vel.X - 1.0f;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = segmentPos + segmentDir * segmentLength;
                    Vector2 collisionPoint = intersectionPoint;
                    Vector2 normal = segmentDir;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, pointVel, segmentVel);
                }
                else // not parallel
                {
                    float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                    if (ts < 0.0f || ts > 1.0f) return new();
                    float t = (sv.X * w.Y - sv.Y * w.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = segmentPos + segmentDir * segmentLength; ;
                    Vector2 collisionPoint = intersectionPoint;
                    Vector2 normal = segmentDir;

                    if (ts != 1.0f)
                    {
                        intersectionPoint = pointPos + vel * t;
                        collisionPoint = intersectionPoint;
                        normal = p < 0.0f ? new(-segmentDir.Y, segmentDir.X) : new(segmentDir.Y, -segmentDir.X);
                    }

                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, pointVel, segmentVel);
                }
            }
            else//on
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f) return new();
                float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                if (ts < 0.0f || ts > 1.0f) return new();
                float t = (sv.X * w.Y - sv.Y * w.X) / p;
                if (t < 0.0f || t > 1.0f) return new();

                Vector2 intersectionPoint = pointPos + vel * t;
                Vector2 collisionPoint = intersectionPoint;
                Vector2 normal = p < 0.0f ? new(-segmentDir.Y, segmentDir.X) : new(segmentDir.Y, -segmentDir.X);

                float remaining = 1.0f - t;
                float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                //Vector2 reflectPoint = intersectionPoint + reflectVector;
                return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, pointVel, segmentVel);
            }
        }
        public static CastInfo CastIntersectionPointLine(Collider point, Vector2 linePos, Vector2 lineDir, Vector2 lineVel, float dt)
        {
            //bool overlapping = Overlap.Simple(point, line);
            Vector2 vel = point.Vel - lineVel;
            vel *= dt;
            if (vel.LengthSquared() <= 0.0f) return new();

            Vector2 w = linePos - point.Pos;
            float p = lineDir.X * point.Vel.Y - lineDir.Y * point.Vel.X;
            if (p == 0.0f) return new();
            float t = (lineDir.X * w.Y - lineDir.Y * w.X) / p;
            if (t < 0.0f || t > 1.0f) return new();

            Vector2 intersectionPoint = point.Pos + point.Vel * t;
            Vector2 collisionPoint = intersectionPoint;
            Vector2 n = p < 0.0f ? new(-lineDir.Y, lineDir.X) : new(lineDir.Y, -lineDir.X);
            float remaining = 1.0f - t;
            float d = 2.0f * (point.Vel.X * n.X + point.Vel.Y * n.Y);
            Vector2 reflectVector = new(remaining * (point.Vel.X - n.X * d), remaining * (point.Vel.Y - n.Y * d));
            //Vector2 reflectPoint = intersectionPoint + reflectVector;
            return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, n, point.Vel, lineVel);
        }
        public static CastInfo CastIntersectionPointRay(Collider point, Vector2 rayPos, Vector2 rayDir, Vector2 rayVel, float dt)
        {
            //bool overlapping = Overlap.Simple(point, ray);
            Vector2 vel = point.Vel - rayVel;
            vel *= dt;
            if (vel.LengthSquared() <= 0.0f) return new();

            Vector2 w = rayPos - point.Pos;
            float p = rayDir.X * vel.Y - rayDir.Y * vel.X;
            if (p == 0.0f)
            {
                float c = w.X * rayDir.Y - w.Y * rayDir.X;
                if (c != 0.0f) return new();

                float t;
                if (vel.X == 0.0f) t = w.Y / vel.Y;
                else t = w.X / vel.X;

                if (t < 0.0f || t > 1.0f) return new();

                Vector2 intersectionPoint = rayPos;
                Vector2 collisionPoint = intersectionPoint;
                Vector2 normal = rayDir * -1;
                float remaining = 1.0f - t;
                float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                //Vector2 reflectPoint = intersectionPoint + reflectVector;
                return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, rayVel);
            }
            else
            {
                float t = (rayDir.X * w.Y - rayDir.Y * w.X) / p;
                if (t < 0.0f || t > 1.0f) return new();
                float tr = (vel.X * w.Y - vel.Y * w.X) / p;
                if (tr < 0.0f) return new();

                Vector2 intersectionPoint = point.Pos + vel * t;
                Vector2 collisionPoint = intersectionPoint;
                Vector2 normal;
                if (p < 0) normal = new(-rayDir.Y, rayDir.X);
                else normal = new(rayDir.Y, -rayDir.X);
                float remaining = 1.0f - t;
                float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                //Vector2 reflectPoint = intersectionPoint + reflectVector;
                return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, rayVel);
            }
        }
        public static CastInfo CastIntersectionCircleLine(CircleCollider circle, Vector2 linePos, Vector2 lineDir, Vector2 lineVel, float dt)
        {
            bool overlapping = SGeometry.OverlapCircleLine(circle.Pos, circle.Radius, linePos, lineDir);
            Vector2 vel = circle.Vel - lineVel;
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);

            Vector2 intersectionPoint, normal;
            float t;
            float p = lineDir.X * vel.Y - lineDir.Y * vel.X;
            if (p < 0.0f)
            {
                Vector2 w = linePos - circle.Pos;
                t = (lineDir.X * w.Y - lineDir.Y * w.X + circle.Radius) / p;
                if (t < 0.0f || t > 1.0f) return new();
                intersectionPoint = circle.Pos + vel * t;
                normal = new(-lineDir.Y, lineDir.X);

            }
            else if (p > 0.0f)
            {
                Vector2 w = linePos - circle.Pos;
                t = (lineDir.X * w.Y - lineDir.Y * w.X - circle.Radius) / p;
                if (t < 0.0f || t > 1.0f) return new();
                intersectionPoint = circle.Pos + vel * t;
                normal = new(lineDir.Y, -lineDir.X);
            }
            else
            {
                return new(true);
            }

            Vector2 collisionPoint = intersectionPoint - circle.Radius * normal;
            float remaining = 1.0f - t;
            float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
            Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
            //Vector2 reflectPoint = intersectionPoint + reflectVector;
            return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, lineVel);
        }
        public static CastInfo CastIntersectionCircleRay(CircleCollider circle, Vector2 rayPos, Vector2 rayDir, Vector2 rayVel, float dt)
        {
            bool overlapping = SGeometry.OverlapCircleRay(circle.Pos, circle.Radius, rayPos, rayDir);
            Vector2 vel = circle.Vel - rayVel;
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);

            float p = rayDir.X * vel.Y - rayDir.Y * vel.X;
            if (p < 0.0f)
            {
                Vector2 point = new(rayPos.X - rayDir.Y * circle.Radius, rayPos.Y + rayDir.X * circle.Radius);
                Vector2 w1 = point - circle.Pos;
                float tr = (vel.X * w1.Y - vel.Y * w1.X) / p;
                if (tr < 0.0f)
                {
                    Vector2 w2 = rayPos - circle.Pos;
                    float qa = vel.LengthSquared();
                    float qb = vel.X * w2.X + vel.Y * w2.Y;
                    float qc = w2.LengthSquared() - circle.RadiusSquared;
                    float qd = qb * qb - qa * qc;
                    if (qd < 0.0f) return new();
                    float t = (qb - MathF.Sqrt(qd)) / qa;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = rayPos;
                    Vector2 normal = (intersectionPoint - rayPos) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, rayVel);
                }
                else
                {
                    float t = (rayDir.X * w1.Y - rayDir.Y * w1.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = new(intersectionPoint.X + rayDir.Y * circle.Radius, intersectionPoint.Y - rayDir.X * circle.Radius); // intersectionPoint + ray.Dir * circle.Radius;
                    Vector2 normal = new(-rayDir.Y, rayDir.X);
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, rayVel);
                }
            }
            else if (p > 0.0f)
            {
                Vector2 point = new(rayPos.X + rayDir.Y * circle.Radius, rayPos.Y - rayDir.X * circle.Radius);
                Vector2 w1 = point - circle.Pos;
                float tr = (vel.X * w1.Y - vel.Y * w1.X) / p;
                if (tr < 0.0f)
                {
                    Vector2 w2 = rayPos - circle.Pos;
                    float qa = vel.LengthSquared();
                    float qb = vel.X * w2.X + vel.Y * w2.Y;
                    float qc = w2.LengthSquared() - circle.RadiusSquared;
                    float qd = qb * qb - qa * qc;
                    if (qd < 0.0f) return new();
                    float t = (qb - MathF.Sqrt(qd)) / qa;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = rayPos;
                    Vector2 normal = (intersectionPoint - rayPos) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, rayVel);
                }
                else
                {
                    float t = (rayDir.X * w1.Y - rayDir.Y * w1.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = new(intersectionPoint.X - rayDir.Y * circle.Radius, intersectionPoint.Y + rayDir.X * circle.Radius); ;
                    Vector2 normal = new(rayDir.Y, -rayDir.X);// (intersectionPoint - ray.Pos) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, rayVel);
                }
            }
            else//p == 0
            {
                return new(true);
            }
        }

    }
}
