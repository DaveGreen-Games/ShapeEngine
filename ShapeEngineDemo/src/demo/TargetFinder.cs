using System.Numerics;
using ShapeCore;
using ShapeTiming;
using ShapeCollision;
using ShapeLib;
using Raylib_CsLo;
using ShapeColor;

namespace ShapeEngineDemo
{
    public class TargetFinder
    {
        public enum TargetingType
        {
            NEAREST = 0,
            FURTHEST = 1,
            STRONGEST = 2,
            WEAKEST = 3,
            BALANCED = 4
        }

        const float checkInterval = 0.25f; //responsiveness

        Vector2 pos;
        TargetingType targetingType = TargetingType.BALANCED;
        string[] mask = new string[0];
        ICollidable? target = null;
        BasicTimer timer = new();
        bool enabled = false;

        public TargetFinder(TargetingType targetingType, params string[] masks)
        {
            this.targetingType = targetingType;
            this.mask = masks;
        }
        public TargetFinder(params string[] masks)
        {
            this.targetingType = TargetingType.BALANCED;
            this.mask = masks;
        }

        public bool HasTarget() { return target != null && enabled; }
        public ICollidable? GetTarget()
        {
            if (!enabled) return null;
            else return target;
        }

        public void Start()
        {
            if (enabled) return;
            enabled = true;
            timer.Start(checkInterval);
        }
        public void Stop()
        {
            if (!enabled) return;
            enabled = false;
            timer.Stop();
            ClearTarget();
        }
        public void Toggle()
        {
            if (enabled) Stop();
            else Start();
        }
        public void SetTarget(ICollidable newTarget)
        {
            if (newTarget == target) return;
            target = newTarget;
        }
        public void ClearTarget()
        {
            if (target == null) return;
            target = null;
        }
        public void Update(Vector2 pos, float targetingRange, float dt)
        {
            this.pos = pos;
            if (!enabled) return;

            if (target != null)
            {
                var gameObject = target as GameObject;
                if (gameObject != null)
                {
                    if (gameObject.IsDead())
                    {
                        ClearTarget();
                        return;
                    }
                }
                //if (!Overlap.Check(new CircleCollider(this.pos, targetingRange), target.GetCollider()))
                if(!SGeometry.Overlap(new CircleCollider(this.pos, targetingRange), target.GetCollider()))
                {
                    ClearTarget();
                    //target = CheckForTarget();
                }
                //float disSq = Vec.LengthSquared(target.GetPos() - pos) * 0.95f;
                //if (disSq > radiusSq)
                //{
                //    //ClearTarget();
                //    target = CheckForTarget();
                //}
            }
            else
            {
                if (!timer.IsRunning()) timer.Start(checkInterval);

                timer.Update(dt);
                if (timer.IsFinished())
                {
                    target = CheckForTarget(targetingRange);
                }
            }
        }

        public void Draw(float targetingRange)
        {
            if (!enabled) return;
            //Color circleColor = ColorPalette.ChangeAlpha(ColorPalette.Cur.flash, 125);
            Color circleColor = Demo.PALETTES.C(ColorIDs.Flash);
            float lineThickness = 2f;
            if (!HasTarget())
            {
                circleColor.a = 125;
                lineThickness = 1f;
            }

            SDrawing.DrawCircleLines(pos, targetingRange + SRNG.randF(-0.5f, 0.5f), lineThickness, circleColor, 8f);
        }

        public ICollidable? CheckForTarget(float targetingRange)
        {
            if (GAMELOOP.CUR_SCENE == null) return null;
            var area = GAMELOOP.CUR_SCENE.GetCurArea();
            if (area == null) return null;
            var bodies = area.colHandler.CastSpace(this.pos, targetingRange, false, this.mask);
            return Filter(bodies);
        }
        private ICollidable? Filter(List<ICollidable> bodies)
        {
            if (bodies.Count <= 0) return null;
            else if (bodies.Count == 1) return bodies[0];
            else
            {
                Sort(bodies, targetingType);
                return bodies[0];
            }
        }
        private void Sort(List<ICollidable> bodies, TargetingType targetingType)
        {
            bodies.Sort(delegate (ICollidable a, ICollidable b)
            {
                if (a == null && b == null) return 0;
                if (a == null && b != null) return 1;
                if (a != null && b == null) return -1;
                if (targetingType == TargetingType.NEAREST)
                {
                    float aDis = SVec.LengthSquared(a.GetPos() - pos);
                    float bDis = SVec.LengthSquared(b.GetPos() - pos);
                    if (aDis < bDis) return -1;
                    else if (aDis > bDis) return 1;
                    else return 0;
                }
                else if (targetingType == TargetingType.FURTHEST)
                {
                    float aDis = SVec.LengthSquared(a.GetPos() - pos);
                    float bDis = SVec.LengthSquared(b.GetPos() - pos);
                    if (aDis < bDis) return 1;
                    else if (aDis > bDis) return -1;
                    else return 0;
                }
                else if (targetingType == TargetingType.STRONGEST)
                {
                    var aRect = a.GetCollider().GetBoundingRect();
                    var bRect = b.GetCollider().GetBoundingRect();
                    float aSize = aRect.width * aRect.height;
                    float bSize = bRect.width * bRect.height;
                    if (aSize < bSize) return 1;
                    else if (aSize > bSize) return -1;
                    else return 0;
                }
                else if (targetingType == TargetingType.WEAKEST)
                {
                    var aRect = a.GetCollider().GetBoundingRect();
                    var bRect = b.GetCollider().GetBoundingRect();
                    float aSize = aRect.width * aRect.height;
                    float bSize = bRect.width * bRect.height;
                    if (aSize < bSize) return -1;
                    else if (aSize > bSize) return 1;
                    else return 0;
                }
                else //balanced
                {
                    var aRect = a.GetCollider().GetBoundingRect();
                    var bRect = b.GetCollider().GetBoundingRect();
                    float aSize = aRect.width * aRect.height;
                    float bSize = bRect.width * bRect.height;
                    float aDis = SVec.LengthSquared(a.GetPos() - pos);
                    float bDis = SVec.LengthSquared(b.GetPos() - pos);

                    const float disWeight = 1f;
                    const float sizeWeight = 1f;

                    //closer/bigger gets attacked first
                    //-> lower score is better
                    float aScore = (aDis / bDis) * disWeight + (bSize / aSize) * sizeWeight;
                    float bScore = (bDis / aDis) * disWeight + (aSize / bSize) * sizeWeight;

                    if (aScore < bScore) return -1;
                    else if (aScore > bScore) return 1;
                    else return 0;
                }
            });

        }
    }

}
