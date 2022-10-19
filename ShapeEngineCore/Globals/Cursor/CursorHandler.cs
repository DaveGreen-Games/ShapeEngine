using System.Numerics;

namespace ShapeEngineCore.Globals.Cursor
{
    public static class CursorHandler
    {
        private static Dictionary<string, CursorBasic> cursors = new();
        private static CursorBasic nullCursor = new CursorNull();
        private static CursorBasic curCursor = nullCursor;
        private static bool hidden = false;

        public static void Initialize()
        {
            Add("ui", new CursorBasic(0.02f, RED));
            Add("game", new CursorGame(0.02f, RED));
            Switch("ui");
            Hide();
        }

        public static void Draw(Vector2 uiSize, Vector2 mousePos)
        {
            if (hidden) return;
            curCursor.Draw(uiSize, mousePos);
        }
        public static void Close()
        {
            cursors.Clear();
            curCursor = nullCursor;
        }


        public static void Hide()
        {
            if (hidden) return;
            hidden = true;
        }
        public static void Show()
        {
            if (!hidden) return;
            hidden = false;
        }
        public static void Switch(string name)
        {
            if (!cursors.ContainsKey(name)) return;
            curCursor = cursors[name];
        }
        public static void Remove(string name)
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
        public static void Add(string name, CursorBasic cursor)
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
