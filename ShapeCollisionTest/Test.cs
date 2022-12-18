using ShapeEngineCore;
using System.Numerics;
using Raylib_CsLo;
using ShapeEngineCore.Globals;
using ShapeEngineCore.Globals.UI;
using ShapeCollision;
using ShapeLib;
using System.Runtime.InteropServices;

namespace ShapeCollisionTest
{
    public class Test
    {
        public virtual void Update(float dt, Vector2 mousePos) { }
        public virtual void Draw(Vector2 mousePos) 
        { 
        }
        public virtual void Close() { }
        public virtual void Start() { }
    }

    public class TestStart : Test
    {
        public override void Update(float dt, Vector2 mousePos)
        {
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_SPACE))
            {
                Program.ChangeTest(new Test4());
            }
        }
        public override void Draw(Vector2 mousePos)
        {
            UIHandler.DrawTextAligned(String.Format("{0}", Raylib.GetFPS()), new(5, 5, 75, 50), 10, Raylib.GREEN, Alignement.TOPLEFT);
            UIHandler.DrawTextAligned("Press Space", new(1920/2, 1080/2, 1000, 500), 15, Raylib.WHITE, Alignement.CENTER);
        }
    }

    //closest point test
    public class Test1 : Test
    {
        Collider dPoint = new();
        CircleCollider dCircle = new(0, 0, 100);
        SegmentCollider dSegment = new(new Vector2(0, 0), SRNG.randVec2(), 500);
        RectCollider dRect = new(new Vector2(0f), new Vector2(100, 100), new Vector2(0.5f, 0.5f));
        PolyCollider dPoly = new(0, 0, new() { new Vector2(1, 0) * 100f, new Vector2(-0.5f, -0.5f) * 100f, new Vector2(-0.5f, 0.5f) * 100f });
        List<Collider> dynamicColliders = new();
        int dynIndex = 0;

        Collider sPoint = new();
        CircleCollider sCircle = new(0, 0, 100);
        SegmentCollider sSegment = new(new Vector2(0, 0), SRNG.randVec2(), 500);
        RectCollider sRect = new(new Vector2(0f), new Vector2(100, 100), new Vector2(0.5f, 0.5f));
        PolyCollider sPoly = new(0, 0, new() { new Vector2(1, 0) * 100f, new Vector2(-0.5f, -0.5f) * 100f, new Vector2(-0.5f, 0.5f) * 100f });
        List<Collider> staticColliders = new();
        int staIndex = 0;

        public Test1()
        {
            dPoint.Pos = RandPos();
            dynamicColliders.Add(dPoint);
            dCircle.Pos = RandPos();
            dynamicColliders.Add(dCircle);
            dSegment.Pos = RandPos();
            dynamicColliders.Add(dSegment);
            dRect.Pos = RandPos();
            dynamicColliders.Add(dRect);
            dPoly.Pos = RandPos();
            dynamicColliders.Add(dPoly);

            sPoint.Pos = RandPos();
            staticColliders.Add(sPoint);
            sCircle.Pos = RandPos();
            staticColliders.Add(sCircle);
            sSegment.Pos = RandPos();
            staticColliders.Add(sSegment);
            sRect.Pos = RandPos();
            staticColliders.Add(sRect);
            sPoly.Pos = RandPos();
            staticColliders.Add(sPoly);
        }

        public override void Update(float dt, Vector2 mousePos)
        {
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                dynamicColliders[dynIndex].Pos = mousePos;
            }
            else if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                staticColliders[staIndex].Pos = mousePos;
            }

            if (Raylib.IsKeyReleased(KeyboardKey.KEY_Q))
            {
                staIndex += 1;
                if (staIndex >= staticColliders.Count) staIndex = 0;
            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_E))
            {
                dynIndex += 1;
                if (dynIndex >= dynamicColliders.Count) dynIndex = 0;
            }
        }
        public override void Draw(Vector2 mousePos)
        {
            UIHandler.DrawTextAligned(String.Format("{0}", Raylib.GetFPS()), new(5, 5, 75, 50), 10, Raylib.GREEN, Alignement.TOPLEFT);
            //Raylib.DrawCircleV(mousePos, 150, Raylib.WHITE);

            staticColliders[staIndex].DebugDrawShape(Raylib.YELLOW);
            dynamicColliders[dynIndex].DebugDrawShape(Raylib.GREEN);
            Vector2 p = SGeometry.ClosestPoint(staticColliders[staIndex], dynamicColliders[dynIndex]);
            Raylib.DrawCircleV(p, 10f, new Color(255, 0, 0, 150));
        }

        private Vector2 RandPos()
        {
            return SRNG.randPoint(new Rectangle(0, 0, 1920, 1080));
        }
    }
    
    //intersection test
    public class Test2 : Test
    {
        Collider dPoint = new();
        CircleCollider dCircle = new(0, 0, SRNG.randF(50, 150));
        SegmentCollider dSegment = new(new Vector2(0, 0), SRNG.randVec2(), 500);
        RectCollider dRect = new(new Vector2(0f), new Vector2(100, 100), new Vector2(0.5f, 0.5f));
        PolyCollider dPoly = new(0, 0, SPoly.GeneratePolygon(12, new(0f), 50, 250));
        List<Collider> dynamicColliders = new();
        int dynIndex = 0;

        Collider sPoint = new();
        CircleCollider sCircle = new(0, 0, SRNG.randF(200, 300));
        SegmentCollider sSegment = new(new Vector2(0, 0), SRNG.randVec2(), 500);
        RectCollider sRect = new(new Vector2(0f), new Vector2(100, 100), new Vector2(0.5f, 0.5f));
        PolyCollider sPoly = new(0, 0, new() { new Vector2(1, 0) * 100f, new Vector2(-0.5f, -0.5f) * 100f, new Vector2(-0.5f, 0.5f) * 100f });
        List<Collider> staticColliders = new();
        int staIndex = 0;

        public Test2()
        {
            dPoint.Pos = RandPos();
            dynamicColliders.Add(dPoint);
            dCircle.Pos = RandPos();
            dynamicColliders.Add(dCircle);
            dSegment.Pos = RandPos();
            dynamicColliders.Add(dSegment);
            dRect.Pos = RandPos();
            dynamicColliders.Add(dRect);
            dPoly.Pos = RandPos();
            dynamicColliders.Add(dPoly);

            sPoint.Pos = RandPos();
            staticColliders.Add(sPoint);
            sCircle.Pos = RandPos();
            staticColliders.Add(sCircle);
            sSegment.Pos = RandPos();
            staticColliders.Add(sSegment);
            sRect.Pos = RandPos();
            staticColliders.Add(sRect);
            sPoly.Pos = RandPos();
            staticColliders.Add(sPoly);
        }

        public override void Update(float dt, Vector2 mousePos)
        {
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                dynamicColliders[dynIndex].Pos = mousePos;
            }
            else if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                staticColliders[staIndex].Pos = mousePos;
            }

            if (Raylib.IsKeyReleased(KeyboardKey.KEY_Q))
            {
                staIndex += 1;
                if (staIndex >= staticColliders.Count) staIndex = 0;
            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_E))
            {
                dynIndex += 1;
                if (dynIndex >= dynamicColliders.Count) dynIndex = 0;
            }
        }
        public override void Draw(Vector2 mousePos)
        {
            UIHandler.DrawTextAligned(String.Format("{0}", Raylib.GetFPS()), new(5, 5, 75, 50), 10, Raylib.GREEN, Alignement.TOPLEFT);
            //Raylib.DrawCircleV(mousePos, 150, Raylib.WHITE);

            staticColliders[staIndex].DebugDrawShape(Raylib.YELLOW);
            dynamicColliders[dynIndex].DebugDrawShape(Raylib.GREEN);
            var points = SGeometry.Intersect(staticColliders[staIndex], dynamicColliders[dynIndex]);
            foreach (var p in points)
            {
                Raylib.DrawCircleV(p, 10f, new Color(255, 0, 0, 150));
            }
            
        }

        private Vector2 RandPos()
        {
            return SRNG.randPoint(new Rectangle(0, 0, 1920, 1080));
        }
    }
    
    //contains test
    public class Test3 : Test
    {
        Collider dPoint = new();
        CircleCollider dCircle = new(0, 0, SRNG.randF(50, 100));
        SegmentCollider dSegment = new(new Vector2(0, 0), SRNG.randVec2(), 500);
        RectCollider dRect = new(new Vector2(0f), new Vector2(100, 100), new Vector2(0.5f, 0.5f));
        PolyCollider dPoly = new(0, 0, SPoly.GeneratePolygon(12, new(0f), 50, 150));
        List<Collider> dynamicColliders = new();
        int dynIndex = 0;

        Collider sPoint = new();
        CircleCollider sCircle = new(0, 0, SRNG.randF(200, 300));
        SegmentCollider sSegment = new(new Vector2(0, 0), SRNG.randVec2(), 500);
        RectCollider sRect = new(new Vector2(0f), new Vector2(500, 500), new Vector2(0.5f, 0.5f));
        PolyCollider sPoly = new(0, 0, SPoly.GeneratePolygon(12, new(0f), 250, 400));
        List<Collider> staticColliders = new();
        int staIndex = 0;

        public Test3()
        {
            dPoint.Pos = RandPos();
            dynamicColliders.Add(dPoint);
            dCircle.Pos = RandPos();
            dynamicColliders.Add(dCircle);
            dSegment.Pos = RandPos();
            dynamicColliders.Add(dSegment);
            dRect.Pos = RandPos();
            dynamicColliders.Add(dRect);
            dPoly.Pos = RandPos();
            dynamicColliders.Add(dPoly);

            sPoint.Pos = RandPos();
            staticColliders.Add(sPoint);
            sCircle.Pos = RandPos();
            staticColliders.Add(sCircle);
            sSegment.Pos = RandPos();
            staticColliders.Add(sSegment);
            sRect.Pos = RandPos();
            staticColliders.Add(sRect);
            sPoly.Pos = RandPos();
            staticColliders.Add(sPoly);
        }

        public override void Update(float dt, Vector2 mousePos)
        {
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                dynamicColliders[dynIndex].Pos = mousePos;
            }
            else if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                staticColliders[staIndex].Pos = mousePos;
            }

            if (Raylib.IsKeyReleased(KeyboardKey.KEY_Q))
            {
                staIndex += 1;
                if (staIndex >= staticColliders.Count) staIndex = 0;
            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_E))
            {
                dynIndex += 1;
                if (dynIndex >= dynamicColliders.Count) dynIndex = 0;
            }
        }
        public override void Draw(Vector2 mousePos)
        {
            UIHandler.DrawTextAligned(String.Format("{0}", Raylib.GetFPS()), new(5, 5, 75, 50), 10, Raylib.GREEN, Alignement.TOPLEFT);
            //Raylib.DrawCircleV(mousePos, 150, Raylib.WHITE);

            bool contains = SGeometry.Contains(staticColliders[staIndex], dynamicColliders[dynIndex]);
            staticColliders[staIndex].DebugDrawShape(Raylib.YELLOW);
            dynamicColliders[dynIndex].DebugDrawShape(contains ? Raylib.RED : Raylib.GREEN);

        }

        private Vector2 RandPos()
        {
            return SRNG.randPoint(new Rectangle(0, 0, 1920, 1080));
        }
    }

    //overlap test
    public class Test4 : Test
    {
        internal class Collidable : ICollidable
        {
            Collider collider;
            public Collidable(Collider collider)
            {
                this.collider = collider;
            }

            public Collider GetCollider()
            {
                return collider;
            }

            public string GetCollisionLayer()
            {
                return "all";
            }

            public string[] GetCollisionMask()
            {
                return new string[] { "all" };
            }

            public string GetID()
            {
                return "test";
            }

            public Vector2 GetPos()
            {
                return collider.Pos;
            }
            public void Overlap(OverlapInfo info)
            {
                return;
            }
        }

        Collider dPoint = new();
        CircleCollider dCircle = new(0, 0, SRNG.randF(50, 100));
        SegmentCollider dSegment = new(new Vector2(0, 0), SRNG.randVec2(), 500);
        RectCollider dRect = new(new Vector2(0f), new Vector2(100, 100), new Vector2(0.5f, 0.5f));
        PolyCollider dPoly = new(0, 0, SPoly.GeneratePolygon(12, new(0f), 50, 150));
        List<Collider> dynamicColliders = new();
        int dynIndex = 0;

        Collider sPoint = new();
        CircleCollider sCircle = new(0, 0, SRNG.randF(200, 300));
        SegmentCollider sSegment = new(new Vector2(0, 0), SRNG.randVec2(), 500);
        RectCollider sRect = new(new Vector2(0f), new Vector2(500, 500), new Vector2(0.5f, 0.5f));
        PolyCollider sPoly = new(0, 0, SPoly.GeneratePolygon(12, new(0f), 250, 400));
        List<Collider> staticColliders = new();
        int staIndex = 0;

        public Test4()
        {
            dPoint.Pos = RandPos();
            dynamicColliders.Add(dPoint);
            dCircle.Pos = RandPos();
            dynamicColliders.Add(dCircle);
            dSegment.Pos = RandPos();
            dynamicColliders.Add(dSegment);
            dRect.Pos = RandPos();
            dynamicColliders.Add(dRect);
            dPoly.Pos = RandPos();
            dynamicColliders.Add(dPoly);

            sPoint.Pos = RandPos();
            staticColliders.Add(sPoint);
            sCircle.Pos = RandPos();
            staticColliders.Add(sCircle);
            sSegment.Pos = RandPos();
            staticColliders.Add(sSegment);
            sRect.Pos = RandPos();
            staticColliders.Add(sRect);
            sPoly.Pos = RandPos();
            staticColliders.Add(sPoly);
        }

        public override void Update(float dt, Vector2 mousePos)
        {
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
            {
                dynamicColliders[dynIndex].Pos = mousePos;
            }
            else if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                staticColliders[staIndex].Pos = mousePos;
            }

            if (Raylib.IsKeyReleased(KeyboardKey.KEY_Q))
            {
                staIndex += 1;
                if (staIndex >= staticColliders.Count) staIndex = 0;
            }
            if (Raylib.IsKeyReleased(KeyboardKey.KEY_E))
            {
                dynIndex += 1;
                if (dynIndex >= dynamicColliders.Count) dynIndex = 0;
            }
        }
        public override void Draw(Vector2 mousePos)
        {
            UIHandler.DrawTextAligned(String.Format("{0}", Raylib.GetFPS()), new(5, 5, 75, 50), 10, Raylib.GREEN, Alignement.TOPLEFT);
            //Raylib.DrawCircleV(mousePos, 150, Raylib.WHITE);
            Collidable a = new(staticColliders[staIndex]);
            Collidable b = new(dynamicColliders[dynIndex]);
            var info = SGeometry.GetOverlapInfo(a, b, true, true);
            staticColliders[staIndex].DebugDrawShape(Raylib.BLUE);

            Color color = Raylib.GREEN;
            if(info.containsSelfOther) color = Raylib.YELLOW;
            else if(info.overlapping) color = Raylib.ORANGE;
            dynamicColliders[dynIndex].DebugDrawShape(color);

            foreach (var p in info.intersectionPoints)
            {
                Raylib.DrawCircleV(p, 5f, Raylib.RED);
            }

        }

        private Vector2 RandPos()
        {
            return SRNG.randPoint(new Rectangle(0, 0, 1920, 1080));
        }
    }
    public class TestCircleCircleIntersection : Test
    {
        Vector2 start = SRNG.randPoint(new Rectangle(0, 0, 1920, 1080));
        Vector2 end = SRNG.randPoint(new Rectangle(0, 0, 1920, 1080));
        public TestCircleCircleIntersection()
        {

        }

        public override void Update(float dt, Vector2 mousePos)
        {

        }
        public override void Draw(Vector2 mousePos)
        {
            Vector2 aPos = new Vector2(1920, 1080) / 2;
            float aR = 250f;
            Vector2 bPos = mousePos;
            float bR = 100f;
            Drawing.DrawCircleLines(aPos, aR, 5f, Raylib.WHITE, 8f);
            Drawing.DrawCircleLines(bPos, bR, 5f, Raylib.GREEN, 8f);
            //Raylib.DrawLineEx(start, end, 4f, Raylib.GREEN);
            var intersections = SGeometry.IntersectCircleCircle(aPos, aR, bPos, bR);
            foreach (var point in intersections)
            {
                Raylib.DrawCircleV(point, 20f, Raylib.RED);
            }
        }
    }
}
