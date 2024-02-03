// using Raylib_CsLo;
//
// namespace ShapeEngine.UI
// {
//     public class TextEntry
//     {
//         public List<char> characters = new();
//         public string Text { get; protected set; } = "";
//         public int CaretPosition { get; protected set; } = 0;
//         public bool Active { get; protected set; } = false;
//
//         private int maxCharacters = -1;
//
//         public event Action<string>? TextEntered;
//         public event Action? TextEntryStarted;
//         public event Action? TextEntryCanceled;
//
//         public TextEntry() { }
//         public TextEntry(int maxCharacters = -1)
//         {
//             this.maxCharacters = maxCharacters;
//         }
//
//         
//
//         public void Update(float dt)
//         {
//             if (Active && (maxCharacters < 0 || characters.Count <= maxCharacters))
//             {
//                 int unicode = Raylib.GetCharPressed();
//                 while (unicode != 0)
//                 {
//                     var c = (char)unicode;
//                     characters.Insert(CaretPosition, c);
//                     ActualizeText();
//                     CaretPosition++;
//                     unicode = Raylib.GetCharPressed();
//                 }
//             }
//         }
//
//
//
//         public void Start()
//         {
//             if (Active) return;
//             Active = true;
//             TextEntryStarted?.Invoke();
//         }
//         public string Enter()
//         {
//             if (!Active || characters.Count <= 0) return "";
//             TextEntered?.Invoke(Text);
//             return Text;
//         }
//         public void Cancel()
//         {
//             if (!Active) return;
//             Active = false;
//             ClearText();
//             TextEntryCanceled?.Invoke();
//         }
//         public void ClearText()
//         {
//             Text = "";
//             characters.Clear();
//             CaretPosition = 0;
//         }
//         public void Backspace()
//         {
//             if (characters.Count > 0)
//             {
//                 characters.RemoveAt(CaretPosition - 1);
//                 ActualizeText();
//                 MoveCaretLeft();
//             }
//         }
//         public void Del()
//         {
//             if (characters.Count > 0 && CaretPosition < characters.Count)
//             {
//                 characters.RemoveAt(CaretPosition);
//                 ActualizeText();
//             }
//         }
//         public int MoveCaretLeft()
//         {
//             CaretPosition--;
//             if (CaretPosition < 0) CaretPosition = 0; // Text.Length - 1;
//             return CaretPosition;
//         }
//
//         public int MoveCaretRight()
//         {
//             CaretPosition++;
//             if (CaretPosition >= characters.Count) CaretPosition = characters.Count; // 0;
//             return CaretPosition;
//         }
//
//         private void ActualizeText() { Text = String.Concat(characters); }
//
//         public void SetText(string newText)
//         {
//             Text = newText;
//             characters = newText.ToCharArray().ToList();
//             CaretPosition = characters.Count;
//         }
//     }
//
// }
