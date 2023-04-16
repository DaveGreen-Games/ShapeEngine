using System.Numerics;

namespace ShapeCursor
{
    public class CursorHandler
    {
        private Dictionary<string, CursorBasic> cursors = new();
        private CursorBasic nullCursor;
        private CursorBasic curCursor;
        public bool Hidden { get; protected set; } = false;

        public CursorHandler(bool hidden = false) 
        {
            this.nullCursor = new CursorNull();
            this.curCursor = nullCursor;
            this.Hidden = hidden;
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
            if (Hidden) return;
            curCursor.Draw(uiSize, mousePos);
        }
        public void Close()
        {
            cursors.Clear();
            curCursor = nullCursor;
        }


        public void Hide()
        {
            if (Hidden) return;
            Hidden = true;
        }
        public void Show()
        {
            if (!Hidden) return;
            Hidden = false;
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
