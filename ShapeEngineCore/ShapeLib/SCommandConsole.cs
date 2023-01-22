using Raylib_CsLo;
using ShapeInput;



namespace ShapeLib
{

    public class CommandConsole
    {

    }

    public static class SCommandConsole
    {

        private static List<string> commandHistory = new();
        private static Color textColor = WHITE;
        private static Color caretColor = RED;
        private static Color boxColor = GRAY;
        private static Rectangle consoleRect = new();
        private static TextEntry textEntry = new("Enter New Command...", -1);

        public static event Action<string, List<dynamic>>? CommandEntered;
        public static event Action? ConsoleOpened;
        public static event Action? ConsoleClosed;

        public static KeyboardKey openKey = KeyboardKey.KEY_F10;
        public static KeyboardKey cancelKey = KeyboardKey.KEY_ESCAPE;


        public static void Update(float dt)
        {

        }


        public static void SetColors(Color text, Color caret, Color box)
        {
            textColor = text;
            caretColor = caret;
            boxColor = box;
        }
        public static void SetRect(Rectangle console)
        {
            consoleRect = console;
        }

        public static void Draw()
        {

        }


    }
}
