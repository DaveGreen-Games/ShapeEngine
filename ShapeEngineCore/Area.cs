using Raylib_CsLo;
using System.Numerics;
using ShapeEngineCore.SimpleCollision;
using ShapeEngineCore.Globals.Screen;
using ShapeEngineCore.Globals;

namespace ShapeEngineCore
{
    public class Area
    {
        protected List<GameObject> gameObjects = new();
        protected List<GameObject> uiObjects = new();
        protected Rectangle inner;
        protected Rectangle outer;
        protected Playfield? playfield = null;
        public CollisionHandler colHandler;
        public Area(float x, float y, float w, float h, int rows, int cols)
        {
            colHandler = new(x, y, w, h, rows, cols, true);
            inner = new(x, y, w, h);
            outer = Utils.ScaleRectangle(inner, 2f);
            Start();
        }
        public Area(Vector2 topLeft, Vector2 bottomRight, int rows, int cols)
        {
            float w = bottomRight.X - topLeft.X;
            float h = bottomRight.Y - topLeft.Y;
            colHandler = new(topLeft.X, topLeft.Y, w, h, rows, cols, true);
            inner = new(topLeft.X, topLeft.Y, w, h);
            outer = Utils.ScaleRectangle(inner, 2f);
            Start();
        }
        public Area(Vector2 topLeft, float w, float h, int rows, int cols)
        {
            colHandler = new(topLeft.X, topLeft.Y, w, h, rows, cols, true);
            inner = new(topLeft.X, topLeft.Y, w, h);
            outer = Utils.ScaleRectangle(inner, 2f);
            Start();
        }
        public Area(Rectangle area, int rows, int cols)
        {
            colHandler = new(area.x, area.y, area.width, area.height, rows, cols, true);
            inner = area;
            outer = Utils.ScaleRectangle(inner, 2f);
            Start();
        }

        public Playfield? GetCurPlayfield() { return playfield; }
        public Rectangle GetInnerArea() { return inner; }
        public Rectangle GetOuterArea() { return outer; }
        public Vector2 GetInnerCenter() { return new(inner.x + inner.width / 2, inner.Y + inner.height / 2); }
        public Vector2 GetOuterCenter() { return new(outer.x + outer.width / 2, outer.Y + outer.height / 2); }

        public List<GameObject> GetGameObjects() { return gameObjects; }
        public List<GameObject> GetGameObjects(string group)
        {
            if (group == "") return gameObjects;
            return gameObjects.FindAll(x => x.IsInGroup(group));
        }

        public void ClearGameObjects()
        {
            uiObjects.Clear();
            foreach (var obj in gameObjects)
            {
                obj.Destroy();
            }
            gameObjects.Clear();
        }

        public void AddICollidable(ICollidable obj)
        {
            colHandler.Add(obj);
        }
        public void RemoveICollidable(ICollidable obj)
        {
            colHandler.Remove(obj);
        }

        public void AddGameObject(GameObject obj, bool uiDrawing = false)
        {
            if (obj == null) return;
            //if (gameObjects.Contains(obj)) return; //dont need that i think
            if (obj is ICollidable) colHandler.Add((ICollidable)obj);
            gameObjects.Add(obj);
            if (uiDrawing && !uiObjects.Contains(obj)) uiObjects.Add(obj);
            obj.Spawn();
        }
        public void AddGameObjects(List<GameObject> newObjects, bool uiDrawing = false)
        {
            foreach (GameObject obj in newObjects)
            {
                AddGameObject(obj, uiDrawing);
            }
        }

        public void RemoveGameObjectAt(int index)
        {
            if (index >= gameObjects.Count || gameObjects.Count <= 0) return;
            var obj = gameObjects[index];
            if (obj == null) return;
            if (obj is ICollidable)
            {
                colHandler.Remove((ICollidable)obj);
            }
            uiObjects.Remove(obj);
            gameObjects[index].Destroy();
            gameObjects.RemoveAt(index);

        }
        public void RemoveGameObject(GameObject obj)
        {
            if (obj == null) return;
            if (!gameObjects.Contains(obj)) return;
            if (obj is ICollidable)
            {
                colHandler.Remove((ICollidable)obj);
            }
            uiObjects.Remove(obj);
            obj.Destroy();
            gameObjects.Remove(obj);
        }
        public void RemoveGameObjects(List<GameObject> objs)
        {
            foreach (var obj in objs)
            {
                RemoveGameObject(obj);
            }
        }
        public void RemoveGameObjects(Predicate<GameObject> match)
        {
            var remove = gameObjects.FindAll(match);
            foreach (var obj in remove)
            {
                RemoveGameObject(obj);
            }
        }
        public void RemoveGameObjects(string group)
        {
            if (group == "") return;
            var remove = GetGameObjects(group);
            foreach (var obj in remove)
            {
                RemoveGameObject(obj);
            }
        }

        public virtual void Start() { }
        public virtual void Close()
        {

            colHandler.Close();
            ClearGameObjects();
        }
        public virtual void Draw()
        {
            if (playfield != null) playfield.Draw();
            SortGameObjects();
            foreach (GameObject obj in gameObjects)
            {
                if (Overlap.Simple(outer, obj.GetBoundingBox())) { obj.Draw(); }
                //obj.Draw();
            }
        }
        public virtual void DrawUI()
        {
            foreach (GameObject obj in uiObjects)
            {
                //if (Overlap.Simple(screenArea, obj.GetPosition()))
                obj.DrawUI();
            }
        }
        public virtual void Update(float dt)
        {
            colHandler.Update(dt);
            colHandler.Resolve();

            for (int i = gameObjects.Count - 1; i >= 0; i--)
            {
                GameObject obj = gameObjects[i];
                if (obj == null)
                {
                    gameObjects.RemoveAt(i);
                    return;
                }
                //if (monitorChanged) obj.MonitorHasChanged();

                obj.Update(dt);
                //float disSq = Globals.Vec.LengthSquared(GAMELOOP.GameCenter() - obj.GetPosition());
                bool insideInner = Overlap.Simple(inner, obj.GetBoundingBox());
                bool insideOuter = false;
                if (insideInner) insideOuter = true;
                else insideOuter = Overlap.Simple(outer, obj.GetBoundingBox());
                obj.OnPlayfield(insideInner, insideOuter);

                if (obj.IsDead() || !insideOuter)//!Overlap.Simple(screenArea, obj.GetPosition()))
                {
                    obj.Destroy();
                    gameObjects.RemoveAt(i);
                    if (obj is ICollidable)
                    {
                        colHandler.Remove((ICollidable)obj);
                    }
                }
            }
            //monitorChanged = false;
        }
        public virtual void MonitorHasChanged()
        {
            inner = ScreenHandler.GameArea();
            foreach (var go in gameObjects)
            {
                go.MonitorHasChanged();
            }
            colHandler.UpdateArea(inner);
            //monitorChanged = true;
        }
        protected virtual void SortGameObjects()
        {
            gameObjects.Sort(delegate (GameObject x, GameObject y)
            {
                if (x == null || y == null) return 0;

                if (x.GetDrawOrder() < y.GetDrawOrder()) return -1;
                else if (x.GetDrawOrder() > y.GetDrawOrder()) return 1;
                else return 0;
            });
        }

    }
}
