using System.Numerics;

namespace ShapeCursor
{
    public class CursorHandler
    {
        private Dictionary<string, CursorBasic> cursors = new();
        private CursorBasic nullCursor;
        private CursorBasic curCursor;
        private bool hidden = false;

        public CursorHandler(bool hidden = false) 
        {
            this.nullCursor = new CursorNull();
            this.curCursor = nullCursor;
            this.hidden = hidden;
        }

        //public void Initialize()
        //{
        //    Add("ui", new CursorBasic(0.02f, RED));
        //    Add("game", new CursorGame(0.02f, RED));
        //    Switch("ui");
        //    Hide();
        //}

        public void Draw(Vector2 uiSize, Vector2 mousePos)
        {
            if (hidden) return;
            curCursor.Draw(uiSize, mousePos);
        }
        public void Close()
        {
            cursors.Clear();
            curCursor = nullCursor;
        }


        public void Hide()
        {
            if (hidden) return;
            hidden = true;
        }
        public void Show()
        {
            if (!hidden) return;
            hidden = false;
        }
        public void Switch(string name)
        {
            if (!cursors.ContainsKey(name)) return;
            curCursor = cursors[name];
        }
        public void Remove(string name)
        {
            if (name == "ui" || name == "game") return;
            if (!cursors.ContainsKey(name)) return;
            if (cursors[name] == curCursor)
            {
                curCursor = nullCursor;
                cursors.Remove(name);
            }
            else
            {
                cursors.Remove(name);
            }
        }
        public void Add(string name, CursorBasic cursor)
        {
            curCursor.Name = name;
            if (cursors.ContainsKey(name))
            {

                cursors[name] = cursor;
            }
            else
            {
                cursors.Add(name, cursor);
            }
        }
    }
}
