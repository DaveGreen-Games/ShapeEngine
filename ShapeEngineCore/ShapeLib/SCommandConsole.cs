using Raylib_CsLo;
using ShapeInput;
using ShapeUI;
using System.Numerics;

namespace ShapeLib
{

    public class CommandConsole
    {
        protected List<string> commandHistory = new();
        protected TextEntry textEntry = new(-1);

        private char commandDelimiter = ',';
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

        public void Update(float dt)
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

                if (IsKeyPressed(KeyboardKey.KEY_BACKSPACE)) textEntry.Backspace();

                if (IsKeyPressed(KeyboardKey.KEY_DELETE)) textEntry.Del();


                if (IsKeyPressed(cancelKey)) textEntry.ClearText();

                if (IsKeyPressed(openKey)) Close();
                
                textEntry.Update(dt);
            }
        }

        public void Draw(Rectangle consoleRect, Color textColor, Color caretColor, Color boxColor)
        {
            if (IsActive())
            {
                SDrawing.DrawRectangle(consoleRect, boxColor);
                Rectangle r = SRect.ScaleRectangle(consoleRect, 0.9f, new(0.5f));
                Rectangle historyRect = new(r.x, r.y, r.width, r.height * 0.5f);
                Rectangle textBoxRect = new(r.x, r.y + r.height * 0.5f, r.width, r.height * 0.5f);

                if(commandHistory.Count > 0)
                {
                    SDrawing.DrawTextAligned(commandHistory[commandHistory.Count - 1], historyRect, 1f, caretColor, new(0f, 0.5f));
                }
                SDrawing.DrawTextBox(textBoxRect, "Enter Command...", textEntry.characters, 1f, UIHandler.GetFont(), textColor, true, textEntry.CaretPosition, 2f, caretColor, new(0f, 0.5f));
                SDrawing.DrawRectangeLinesPro(new(consoleRect.x, consoleRect.y), new(consoleRect.width, consoleRect.height), new(0f), new(0f), 0f, 4f, caretColor);
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
        
        public void EnterCommand()
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
                        string name = "";
                        string value = "";
                        if (nameValue.Length <= 0) continue;
                        else if(nameValue.Length == 1)
                        {
                            name = String.Format("Var{0}", i);
                            value = nameValue[0];
                        }
                        else
                        {
                            name = nameValue[0];
                            value = nameValue[1];
                        }

                        char type = value[0];
                        string v = value.Substring(1);
                        if (type == floatPrefix)
                        {
                            float dyn = float.Parse(v);
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

                    commandHistory.Add(input);
                    CommandEntered?.Invoke(command, args);
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

        public void MoveHistoryForward()
        {

        }
        public void MoveHistoryBack()
        {

        }

    }

    public static class SCommandConsole
    {

        

    }
}
