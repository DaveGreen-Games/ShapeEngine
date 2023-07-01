using Raylib_CsLo;
using System.Globalization;
using System.Numerics;
using ShapeEngine.Lib;
using ShapeEngine.Core;
using static System.Net.Mime.MediaTypeNames;

namespace ShapeEngine.UI
{
    public class CommandConsole
    {
        protected List<string> commandHistory = new();
        protected int historyIndex = -1;
        protected TextEntry textEntry = new(-1);

        private char commandDelimiter = '/';
        private char nameDelimiter = '=';

        private char intPrefix = 'i';
        private char floatPrefix = 'f';
        private char stringPrefix = 's';

        //protected Rectangle consoleRect = new();
        //private Color textColor = WHITE;
        //private Color caretColor = RED;
        //private Color boxColor = GRAY;
        
        public KeyboardKey openKey = KeyboardKey.KEY_F10;
        public KeyboardKey cancelKey = KeyboardKey.KEY_ESCAPE;

        
        public event Action<string, Dictionary<string, dynamic>>? CommandEntered;
        public event Action? ConsoleOpened;
        public event Action? ConsoleClosed;


        public CommandConsole() 
        {
            
        }
        public CommandConsole(KeyboardKey openKey, KeyboardKey cancelKey)
        {
            this.openKey = openKey;
            this.cancelKey = cancelKey;
        }

        public CommandConsole(char commandDelimiter, char nameDelimiter, char intPrefix, char floatPrefix, char stringPrefix)
        {
            this.commandDelimiter = commandDelimiter;
            this.nameDelimiter = nameDelimiter;
            this.intPrefix = intPrefix;
            this.floatPrefix = floatPrefix;
            this.stringPrefix = stringPrefix;
        }
        public CommandConsole(KeyboardKey openKey, KeyboardKey cancelKey, char commandDelimiter, char nameDelimiter, char intPrefix, char floatPrefix, char stringPrefix)
        {
            this.openKey = openKey;
            this.cancelKey = cancelKey;
            this.commandDelimiter = commandDelimiter;
            this.nameDelimiter = nameDelimiter;
            this.intPrefix = intPrefix;
            this.floatPrefix = floatPrefix;
            this.stringPrefix = stringPrefix;
        }


        public bool IsActive() { return textEntry.Active; }

        public virtual void Update(float dt)
        {
            if (!IsActive())
            {
                if (IsKeyPressed(openKey)) Open();
            }
            else
            {
                if (IsKeyPressed(KeyboardKey.KEY_LEFT)) textEntry.MoveCaretLeft();
                else if (IsKeyPressed(KeyboardKey.KEY_RIGHT)) textEntry.MoveCaretRight();

                if (IsKeyPressed(KeyboardKey.KEY_UP)) MoveHistoryBack();
                else if (IsKeyPressed(KeyboardKey.KEY_DOWN)) MoveHistoryForward();


                if (IsKeyPressed(KeyboardKey.KEY_ENTER)) EnterCommand();

                if (IsKeyPressed(KeyboardKey.KEY_BACKSPACE))
                {
                    textEntry.Backspace();
                    if (textEntry.characters.Count <= 0) historyIndex = -1;
                }

                if (IsKeyPressed(KeyboardKey.KEY_DELETE))
                {
                    textEntry.Del();
                    if (textEntry.characters.Count <= 0) historyIndex = -1;
                }


                if (IsKeyPressed(cancelKey)) { textEntry.ClearText(); historyIndex = -1; }

                if (IsKeyPressed(openKey)) Close();
                
                textEntry.Update(dt);
            }
        }
        public virtual void Draw(Font font, Rect consoleRect, Vector2 textAlignement, Raylib_CsLo.Color textColor, Raylib_CsLo.Color caretColor, Raylib_CsLo.Color boxColor, params Raylib_CsLo.Color[] extraColors)
        {
            if (IsActive())
            {
                consoleRect.Draw(boxColor);
                Rect r = consoleRect.ScaleSize(0.9f, new(0.5f));
                Rect historyRect = new(r.x, r.y, r.width, r.height * 0.5f);
                Rect textBoxRect = new(r.x, r.y + r.height * 0.5f, r.width, r.height * 0.5f);

                if (commandHistory.Count > 0)
                {
                    font.DrawText(commandHistory[0], historyRect, 1f, textAlignement, caretColor);
                    //commandHistory[0].Draw(historyRect, 1f, caretColor, font, textAlignement);
                }
                //SDrawing.DrawTextBox(textBoxRect, "Enter Command...", textEntry.characters, 1f, font, textColor, true, textEntry.CaretPosition, 2f, caretColor, textAlignement);

                
                string textBoxText = textEntry.Text.Length <= 0 ? "Enter Command..." : textEntry.Text;
                font.DrawText(textBoxText, textBoxRect, 1f, textAlignement, textColor);
                font.DrawCaret(textBoxText, textBoxRect, 1f, textAlignement, textEntry.CaretPosition, 2f, caretColor);


                consoleRect.DrawLines(4f, caretColor);
            }
        }


        public void Open()
        {
            if (IsActive()) return;
            textEntry.Start();
            ConsoleOpened?.Invoke();
        }
        public void Close()
        {
            if(!IsActive()) return;
            textEntry.Cancel();
            ConsoleClosed?.Invoke();
        }
        
        public virtual void EnterCommand()
        {
            if (!IsActive()) return;
            var input = textEntry.Text; // textEntry.Enter();
            textEntry.ClearText();
            if (input != "")
            {
                var substrings = input.Split(commandDelimiter);
                if(substrings.Length > 0)
                {
                    string command = substrings[0];
                    Dictionary<string, dynamic> args = new();
                    for (int i = 1; i < substrings.Length; i++)
                    {
                        var variable = substrings[i];
                        if (variable == "") continue;
                        var nameValue = variable.Split(nameDelimiter);
                        if (nameValue.Length > 1)
                        {
                            string name = nameValue[0];
                            string value = nameValue[1];

                            char type = value[0];
                            string v = value.Substring(1);
                            if (type == floatPrefix)
                            {
                                float dyn = float.Parse(v, CultureInfo.InvariantCulture);
                                args.Add(name, dyn);
                            }
                            else if (type == intPrefix)
                            {
                                int dyn = int.Parse(v);
                                args.Add(name, dyn);
                            }
                            else if (type == stringPrefix)
                            {
                                string dyn = v;
                                args.Add(name, dyn);
                            }
                            else continue;
                        }
                    }

                    commandHistory.Insert(0,input);
                    CommandEntered?.Invoke(command, args);
                    historyIndex = -1;
                }
            }
        }

        //public void SetColors(Color text, Color caret, Color box)
        //{
        //    textColor = text;
        //    caretColor = caret;
        //    boxColor = box;
        //}
        //public void SetRect(Rectangle console)
        //{
        //    consoleRect = console;
        //}
        public void ClearHistory()
        {
            historyIndex = -1;
            commandHistory.Clear();
        }
        public void MoveHistoryForward()
        {
            if (commandHistory.Count <= 0) return;
            if (historyIndex > 0)
            {
                historyIndex -= 1;
                string command = commandHistory[historyIndex];
                textEntry.SetText(command);
            }
            else if (historyIndex == 0)
            {
                historyIndex = -1;
                textEntry.ClearText();
            }
        }
        public void MoveHistoryBack()
        {
            if(commandHistory.Count <= 0) return;
            if(historyIndex < commandHistory.Count - 1)
            {
                historyIndex += 1;
                string command = commandHistory[historyIndex];
                textEntry.SetText(command);
            }
        }

    }




    /*
    public static class CommandConsoleHandler
    {
        private static CommandConsole cc = new();
        private static Rectangle consoleRect = new();
        
        private static Color textColor = WHITE;
        private static Color caretColor = RED;
        private static Color consoleColor = GRAY;
        private static Color[] extraColors = new Color[0];

        public static event Action? OnConsoleOpened;
        public static event Action? OnConsoleClosed;
        public static event Action<string, Dictionary<string, dynamic>>? OnCommandEntered;

        private static bool disabled = false;

        public static bool IsActive()
        {
            if (disabled) return false;
            return cc.IsActive();
        }
        public static void Disable()
        {
            if (disabled) return;
            disabled = true;
            cc.Close();
        }
        public static void Enable() 
        {
            if (!disabled) return;
            disabled = false;

        }
        public static void Initialize() 
        {
            cc.ConsoleOpened += OnCommandConsoleOpened;
            cc.ConsoleClosed += OnCommandConsoleClosed;
            cc.CommandEntered += OnConsoleCommandEntered;
        }
        
        public static void ChangeCommandConsole(CommandConsole newConsole) 
        {
            cc.ConsoleOpened -= OnCommandConsoleOpened;
            cc.ConsoleClosed -= OnCommandConsoleClosed;
            cc.CommandEntered -= OnConsoleCommandEntered;
            
            cc = newConsole;
            
            cc.ConsoleOpened += OnCommandConsoleOpened;
            cc.ConsoleClosed += OnCommandConsoleClosed;
            cc.CommandEntered += OnConsoleCommandEntered;
        }
        
        public static void SetConsoleRect(Rectangle rect) { consoleRect = rect; }
        public static void SetConsoleColors(Color text, Color caret, Color console, params Color[] extra)
        {
            textColor = text;
            caretColor = caret;
            consoleColor = console;
            extraColors = extra;
        }
        public static void Update(float dt) 
        { 
            if(disabled) return;
            cc.Update(dt);
            
        }
        public static void Draw()
        {
            if (disabled) return;
            cc.Draw(consoleRect, new(0f, 0.5f), textColor, caretColor, consoleColor, extraColors);
        }
        public static void Close()
        {
            cc.Close();
            cc.ConsoleOpened -= OnCommandConsoleOpened;
            cc.ConsoleClosed -= OnCommandConsoleClosed;
            cc.CommandEntered -= OnConsoleCommandEntered;
        }

        private static void OnCommandConsoleOpened() 
        {
            OnConsoleOpened?.Invoke();
        }
        private static void OnCommandConsoleClosed() 
        {
            OnConsoleClosed?.Invoke();
        }
        private static void OnConsoleCommandEntered(string command, Dictionary<string, dynamic> args) 
        { 
            OnCommandEntered?.Invoke(command, args);
        }

    }
    */
}
