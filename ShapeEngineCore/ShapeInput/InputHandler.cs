using ShapeLib;
using System.Numerics;
using Raylib_CsLo;
using Vortice.XInput;
using ShapeCore;

namespace ShapeInput
{
    public enum InputType
    {
        KEYBOARD_MOUSE = 0,
        GAMEPAD = 2,
        TOUCH = 3
    }
    public class GamepadVibration
    {
        public string name = "";
        public float duration = 0f;
        public float timer = 0f;
        public float leftMotor = 0f;
        public float rightMotor = 0f;
        public GamepadVibration(string name, float duration, float leftMotor, float rightMotor)
        {
            this.name = name;
            this.duration = duration;
            timer = duration;
            this.leftMotor = leftMotor;
            this.rightMotor = rightMotor;
        }

    }
    public  class InputSlot
    {
        public int gamepadIndex = -1;
        public InputMap curInputMap;
        public bool disabled = false;

        private List<GamepadVibration> gamepadVibrationStack = new();
        public InputSlot(int gamepadIndex)
        {
            curInputMap = new(-1, "Empty");
            this.gamepadIndex = gamepadIndex;
        }
        //public void Update(float dt, bool gamepadOnly)
        //{
        //    curInputMap.Update(dt, gamepadIndex, gamepadOnly);
        //    UpdateVibration(dt);
        //}
        //
        //public float GetHoldF(string actionName, bool gamepadOnly)
        //{
        //    if (disabled) return -1f;
        //    return curInputMap.GetHoldF(gamepadIndex, actionName, gamepadOnly);
        //}
        public bool IsDown(int actionID, bool gamepadOnly)
        {
            if (disabled) return false;
            return curInputMap.IsDown(gamepadIndex, actionID, gamepadOnly);
        }
        public bool IsPressed(int actionID, bool gamepadOnly)
        {
            if (disabled) return false;
            return curInputMap.IsPressed(gamepadIndex, actionID, gamepadOnly);
        }
        public bool IsReleased(int actionID, bool gamepadOnly)
        {
            if (disabled) return false;
            return curInputMap.IsReleased(gamepadIndex, actionID, gamepadOnly);
        }
        public bool IsUp(int actionID, bool gamepadOnly)
        {
            if (disabled) return false;
            return curInputMap.IsUp(gamepadIndex, actionID, gamepadOnly);
        }

        public float GetAxis(int negativeID, int positiveID)
        {
            if (disabled) return 0f;
            return curInputMap.GetAxis(gamepadIndex, negativeID, positiveID);
        }
        public Vector2 GetAxis(int leftID, int rightID, int upID, int downID, bool normalized = true)
        {
            if (disabled) return new(0f, 0f);
            if (normalized) return SVec.Normalize(curInputMap.GetAxis(gamepadIndex,leftID, rightID, upID, downID));
            else return curInputMap.GetAxis(gamepadIndex, leftID, rightID, upID, downID);
        }
        public float GetGamepadAxis(int gamepadAxisActionID)
        {
            if (disabled) return 0f;
            return curInputMap.GetGamepadAxis(gamepadIndex, gamepadAxisActionID);
        }
        public Vector2 GetGamepadAxis(int gamepadAxisHorID, int gamepadAxisVerID, bool normalized = true)
        {
            if (disabled) return new(0f, 0f);

            if (normalized) return SVec.Normalize(curInputMap.GetGamepadAxis(gamepadIndex, gamepadAxisHorID, gamepadAxisVerID));
            else return curInputMap.GetGamepadAxis(gamepadIndex, gamepadAxisHorID, gamepadAxisVerID);
        }

        public (string keyboard, string mouse, string gamepad) GetInputActionKeyNames(int inputActionID, bool shorthand = false)
        {
            return curInputMap.GetKeyNames(inputActionID, shorthand);
        }


        public void AddVibration(float leftMotor, float rightMotor, float duration = -1f, string name = "default")
        {
            if (!InputHandler.IsGamepadConnected(gamepadIndex) || InputHandler.GAMEPAD_VIBRATION_STRENGTH <= 0f) return;
            gamepadVibrationStack.Add(new(name, duration, leftMotor, rightMotor));
        }
        public void RemoveVibration(string name)
        {
            if (name == "" || gamepadVibrationStack.Count <= 0) return;
            gamepadVibrationStack.RemoveAll(item => item.name == name);
        }
        public void StopVibration()
        {
            if(ShapeEngine.IsWindows()) XInput.SetVibration(gamepadIndex, 0f, 0f);

            gamepadVibrationStack.Clear();
        }
        public void UpdateVibration(float dt)
        {
            if (InputHandler.IsGamepadConnected(gamepadIndex))
            {
                float maxLeftMotor = 0f;
                float maxRightMotor = 0f;
                for (int i = gamepadVibrationStack.Count - 1; i >= 0; i--)
                {
                    var stack = gamepadVibrationStack[i];
                    if (stack.duration > 0)
                    {
                        if (stack.timer > 0)
                        {
                            stack.timer -= dt;
                            maxLeftMotor += stack.leftMotor;
                            maxRightMotor += stack.rightMotor;
                        }
                        else gamepadVibrationStack.RemoveAt(i);
                    }
                    else
                    {
                        maxLeftMotor += stack.leftMotor;
                        maxRightMotor += stack.rightMotor;
                    }
                }

                if (ShapeEngine.IsWindows()) 
                    XInput.SetVibration(gamepadIndex, Clamp(maxLeftMotor * InputHandler.GAMEPAD_VIBRATION_STRENGTH, 0f, 1f), Clamp(maxRightMotor * InputHandler.GAMEPAD_VIBRATION_STRENGTH, 0f, 1f));
            }
        }
    }
    public static class InputHandler
    {
        private static Dictionary<int, InputSlot> inputSlots = new();
        private static Dictionary<int, InputMap> inputMaps = new();
        //private static bool disabled = false;
        public const int INPUTMAP_Basic = 100;
        
        //public const int UI_Select = 10;
        //public const int UI_SelectMouse = 11;
        //public const int UI_Cancel = 12;
        //public const int UI_CancelMouse = 13;
        //public const int UI_Up = 14;
        //public const int UI_Down = 15;
        //public const int UI_Left = 16;
        //public const int UI_Right = 17;
        //
        //
        //public static readonly List<InputAction> UI_Default_InputActions = new()
        //{
        //    {new(UI_SelectMouse, InputAction.Keys.MB_LEFT) },
        //    {new(UI_Select, InputAction.Keys.SPACE, InputAction.Keys.GP_BUTTON_RIGHT_FACE_DOWN) },
        //    {new(UI_Cancel, InputAction.Keys.ESCAPE, InputAction.Keys.GP_BUTTON_RIGHT_FACE_RIGHT) },
        //    {new(UI_CancelMouse, InputAction.Keys.MB_RIGHT) },
        //    {new(UI_Up, InputAction.Keys.W, InputAction.Keys.UP, InputAction.Keys.GP_BUTTON_LEFT_FACE_UP) },
        //    {new(UI_Right,InputAction.Keys.D, InputAction.Keys.RIGHT, InputAction.Keys.GP_BUTTON_LEFT_FACE_RIGHT) },
        //    {new(UI_Down, InputAction.Keys.S, InputAction.Keys.DOWN, InputAction.Keys.GP_BUTTON_LEFT_FACE_DOWN) },
        //    {new(UI_Left,InputAction.Keys.A, InputAction.Keys.LEFT, InputAction.Keys.GP_BUTTON_LEFT_FACE_LEFT) },
        //};




        public static float GAMEPAD_VIBRATION_STRENGTH = 1.0f;

        //private static int CUR_GAMEPAD = -1;
        private static List<int> connectedGamepads = new();
        public static int CUR_GAMEPAD {
            get { return inputSlots[0].gamepadIndex; }
            set { inputSlots[0].gamepadIndex = value; } }
        public static ref readonly List<int> GetConnectedGamepads() { return ref connectedGamepads; }
        public static int GetConnectedGamepadCount() { return connectedGamepads.Count; }



        private static InputType CUR_INPUT_TYPE = InputType.KEYBOARD_MOUSE;
        public static InputType GetCurInputType() { return CUR_INPUT_TYPE; }

        private static bool mouseUsed = false;
        private static bool keyboardUsed = false;
        private static bool keyboardOnlyMode = false;
        public static int gamepadUsed = -1;
        //STATIC EVENT!!! - everything that subscribes to a static event must unsubscribe before deletion, otherwise memory leaks can happen
        //garbage collector can not collect anything that is subscribed to a static event!!!
        
        //public delegate void InputChanged(InputType newType);
        public static event Action<InputType>? OnInputChanged;

        public delegate void GamepadConnectionChanged(int gamepad, bool connected, int curGamepad);
        public static event GamepadConnectionChanged? OnGamepadConnectionChanged;

        //public static OperatingSystem OS { get; private set; } = System.Environment.OSVersion;
        //public static bool IsLinux { get; private set; } = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
       
        //public static KeyboardKey quitKey = KeyboardKey.KEY_ESCAPE;
        //public static bool QuitPressed()
        //{
        //    if(disabled) return false;
        //    return IsKeyDown(quitKey);
        //}

        private static bool inputDisabled = false;
        public static void DisableInput()
        {
            if (inputDisabled) return;
            inputDisabled = true;
            foreach (var slot in inputSlots.Keys)
            {
                StopVibration(slot);
            }
        }
        public static void EnableInput()
        {
            if(!inputDisabled) return;
            inputDisabled = false;
        }

        private static void OnInputTypeChanged(InputType newInputType)
        {
            //if (newInputType == InputType.KEYBOARD_MOUSE) { CursorHandler.Show(); }
            //else { CursorHandler.Hide(); }
            OnInputChanged?.Invoke(newInputType);
        }
        private static void OnControllerConnectionChanged(int gamepad, bool connected, int curGamepad)
        {
            if (!connected && gamepad == curGamepad) StopVibration(gamepad);
            OnGamepadConnectionChanged?.Invoke(gamepad, connected, curGamepad);
        }


        public static void Initialize()
        {
            CUR_INPUT_TYPE = InputType.KEYBOARD_MOUSE;
            inputMaps.Clear();
            InputMap basicMap = new(INPUTMAP_Basic, "Basic");
            //foreach (var action in UI_Default_InputActions)
            //{
            //    basicMap.AddAction(action.GetID(), action);
            //}
            inputMaps.Add(basicMap.GetID(), basicMap);
            AddInputSlot(-1);
            if (IsGamepadAvailable(0)) { inputSlots[0].gamepadIndex = 0; }
        }
        public static void Update(float dt)
        {
            CheckGamepadConnection();
            CheckInputType();

            foreach (var inputSlot in inputSlots.Values)
            {
                inputSlot.UpdateVibration(dt);
            }
        }

        public static void Close()
        {
            inputSlots.Clear();
            inputMaps.Clear();
        }


        public static void AddInputSlot(int gamepadIndex, int inputMap = INPUTMAP_Basic)
        {
            if (!inputMaps.ContainsKey(inputMap)) return;
            for (int i = 0; i < inputSlots.Count + 1; i++)
            {
                if (!inputSlots.ContainsKey(i))
                {
                    inputSlots.Add(i, new(gamepadIndex));
                    SwitchToMap(inputMap, i);
                    return;
                }
            }
        }
        public static void RemoveInputSlot(int index)
        {
            if (index <= 0) return;
            if (!inputSlots.ContainsKey(index)) return;
            inputSlots.Remove(index);
        }
        private static InputSlot? GetInputSlot(int index)
        {
            if (index < 0 || index >= inputSlots.Count) return null;
            return inputSlots[index];
        }
        //private static void AddDefaultUIInputsToMap(string mapName)
        //{
        //    if (!inputMaps.ContainsKey(mapName)) return;
        //    AddDefaultUIInputsToMap(inputMaps[mapName]);
        //}
        //private static void AddDefaultUIInputsToMap(InputMap map)
        //{
        //    foreach (var input in UI_Default_InputActions)
        //    {
        //        map.AddAction(input.Key, input.Value);
        //    }
        //}

        //public static void AddInputActionsToInputMap(InputMap map, List<InputAction> inputActions)
        //{
        //    foreach (var input in inputActions)
        //    {
        //        map.AddAction(input);
        //    }
        //}
        public static int NextInputMap(int playerSlot = 0, bool switchMap = true)
        {
            var slot = GetInputSlot(playerSlot);
            if (slot == null) return -1;
            var mapIDs = inputMaps.Keys.ToList();
            int nextIndex = mapIDs.IndexOf(slot.curInputMap.GetID()) + 1;
            if (nextIndex >= mapIDs.Count) nextIndex = 0;
            int nextMapID = mapIDs[nextIndex];
            if (switchMap) SwitchToMap(nextMapID, playerSlot);
            return nextMapID;
        }
        public static int PreviousInputMap(int playerSlot = 0, bool switchMap = true)
        {
            var slot = GetInputSlot(playerSlot);
            if (slot == null) return -1;
            var mapIDs = inputMaps.Keys.ToList();
            int prevIndex = mapIDs.IndexOf(slot.curInputMap.GetID()) - 1;
            if (prevIndex < 0) prevIndex = mapIDs.Count - 1;
            int prevMapID = mapIDs[prevIndex];
            if (switchMap) SwitchToMap(prevMapID, playerSlot);
            return prevMapID;
        }
        public static void SwitchToMap(int mapID, int playerSlot = 0)
        {
            if (!inputMaps.ContainsKey(mapID)) return;
            var slot = GetInputSlot(playerSlot);
            if (slot == null) return;
            slot.curInputMap = inputMaps[mapID];
        }
        public static bool IsDisabled(int playerSlot)
        {
            var slot = GetInputSlot(playerSlot);
            if (slot == null) return false;
            else return slot.disabled;
        }
        public static void Enable(int playerSlot = 0)
        {
            var slot = GetInputSlot(playerSlot);
            if (slot == null) return;
            slot.disabled = false;
        }
        public static void Disable(int playerSlot = 0)
        {
            var slot = GetInputSlot(playerSlot);
            if (slot == null) return;
            slot.disabled = true;
        }
        public static void AddInputMap(InputMap map)
        {
            if (map == null) return;
            if (inputMaps.ContainsKey(map.GetID()))
            {
                inputMaps[map.GetID()] = map;
            }
            else
            {
                inputMaps.Add(map.GetID(), map);
            }
            //if(addUIInputs) AddDefaultUIInputsToMap(map);
        }
        public static void AddInputMap(int id, string displayName, params InputAction[] actions)
        {
            AddInputMap(new InputMap(id, displayName, actions));
        }
        
        public static void RemoveInputMap(int id)
        {
            if (!inputMaps.ContainsKey(id)) return;
            foreach (var slot in inputSlots.Values)
            {
                if(slot.curInputMap.GetID() == id)
                {
                    slot.curInputMap = inputMaps[INPUTMAP_Basic];
                }
            }
            inputMaps.Remove(id);
        }
        public static void RemoveInputMap(int id, int fallbackID)
        {
            if (!inputMaps.ContainsKey(fallbackID))
            {
                RemoveInputMap(id);
                return;
            }
            if (!inputMaps.ContainsKey(id)) return;
            foreach (var slot in inputSlots.Values)
            {
                if (slot.curInputMap.GetID() == id)
                {
                    slot.curInputMap = inputMaps[fallbackID];
                }
            }
            inputMaps.Remove(id);
        }

        public static InputMap? GetMap(int id)
        {
            if (!inputMaps.ContainsKey(id)) return null;
            return inputMaps[id];
        }

        public static string SelectInputActionKeyName((string keyboard, string mouse, string gamepad) selection)
        {
            if (IsKeyboard()) return selection.keyboard;
            else if(IsMouse()) return selection.mouse;
            else if(IsGamepad()) return selection.gamepad;
            else return "";
        }
        public static (string keyboard, string mouse, string gamepad) GetInputActionKeyNames(int playerSlot, int inputActionID, bool shorthand = false)
        {
            var slot = GetInputSlot(playerSlot);
            if (slot == null) return new();
            return slot.GetInputActionKeyNames(inputActionID, shorthand);
        }
        /*
        public static void EnableMap(string name)
        {
            if (!inputMaps.ContainsKey(name)) return;
            inputMaps[name].Enable();
        }
        public static void DisableMap(string name)
        {
            if (!inputMaps.ContainsKey(name)) return;
            inputMaps[name].Disable();
        }
        */
        //public static void RenameMap(int id, string newName)
        //{
        //    if (!inputMaps.ContainsKey(id)) return;
        //    inputMaps[id].Rename(newName);
        //}

        //public static float GetHoldF(int playerSlot, string actionName)
        //{
        //    if (playerSlot < 0)
        //    {
        //        for (int i = 0; i < inputSlots.Count; i++)
        //        {
        //            var slot = inputSlots[i];
        //            float f = slot.GetHoldF(actionName, i > 0);
        //            if (f >= 0f) return f;
        //        }
        //        return -1f;
        //    }
        //    else
        //    {
        //        var slot = GetInputSlot(playerSlot);
        //        if (slot == null) return -1f;
        //        return slot.GetHoldF(actionName, playerSlot > 0);
        //    }
        //}
        public static bool IsDown(int playerSlot, int actionID)
        {
            if (inputDisabled) return false;
            if (playerSlot < 0)
            {
                for (int i = 0; i < inputSlots.Count; i++)
                {
                    var slot = inputSlots[i];
                    if (slot.IsDown(actionID, i > 0)) return true;
                }
                return false;
            }
            else
            {
                var slot = GetInputSlot(playerSlot);
                if (slot == null) return false;
                return slot.IsDown(actionID, playerSlot > 0);
            }
        }
        public static bool IsPressed(int playerSlot, int actionID)
        {
            if (inputDisabled) return false;
            if (playerSlot < 0)
            {
                for (int i = 0; i < inputSlots.Count; i++)
                {
                    var slot = inputSlots[i];
                    if (slot.IsPressed(actionID, i > 0)) return true;
                }
                return false;
            }
            else
            {
                var slot = GetInputSlot(playerSlot);
                if (slot == null) return false;
                return slot.IsPressed(actionID, playerSlot > 0);
            }
        }
        public static bool IsReleased(int playerSlot, int actionID)
        {
            if (inputDisabled) return false;
            if (playerSlot < 0)
            {
                for (int i = 0; i < inputSlots.Count; i++)
                {
                    var slot = inputSlots[i];
                    if (slot.IsReleased(actionID, i > 0)) return true;
                }
                return false;
            }
            else
            {
                var slot = GetInputSlot(playerSlot);
                if (slot == null) return false;
                return slot.IsReleased(actionID, playerSlot > 0);
            }
        }
        public static bool IsUp(int playerSlot, int actionID)
        {
            if (inputDisabled) return false;
            if (playerSlot < 0)
            {
                for (int i = 0; i < inputSlots.Count; i++)
                {
                    var slot = inputSlots[i];
                    if (slot.IsUp(actionID, i > 0)) return true;
                }
                return false;
            }
            else
            {
                var slot = GetInputSlot(playerSlot);
                if (slot == null) return false;
                return slot.IsUp(actionID, playerSlot > 0);
            }
        }

        public static float GetAxis(int playerSlot, int negativeID, int positiveID)
        {
            if (inputDisabled) return 0f;
            var slot = GetInputSlot(playerSlot);
            if (slot == null) return 0f;
            return slot.GetAxis(negativeID, positiveID);
        }
        public static Vector2 GetAxis(int playerSlot, int leftID, int rightID, int upID, int downID, bool normalized = true)
        {
            if (inputDisabled) return new(0f);
            var slot = GetInputSlot(playerSlot);
            if (slot == null) return new(0f, 0f);
            return slot.GetAxis(leftID, rightID, upID, downID, normalized);
        }
        public static float GetGamepadAxis(int playerSlot, int gamepadAxisActionID)
        {
            if (inputDisabled) return 0f;
            var slot = GetInputSlot(playerSlot);
            if (slot == null) return 0f;
            return slot.GetGamepadAxis(gamepadAxisActionID);
        }
        public static Vector2 GetGamepadAxis(int playerSlot, int gamepadAxisHorID, int gamepadAxisVerID, bool normalized = true)
        {
            if (inputDisabled) return new(0f);
            var slot = GetInputSlot(playerSlot);
            if (slot == null) return new(0f, 0f);

            return slot.GetGamepadAxis(gamepadAxisHorID, gamepadAxisVerID, normalized);
        }

        






        public static void AddVibration(int playerSlot, float leftMotor, float rightMotor, float duration = -1f, string name = "default")
        {
            if (inputDisabled) return;
            if (playerSlot == 0 && !IsGamepad()) return;
            
            var slot = GetInputSlot(playerSlot);
            if (slot != null) slot.AddVibration(leftMotor, rightMotor, duration, name);
        }
        public static void RemoveVibration(int playerSlot, string name)
        {
            var slot = GetInputSlot(playerSlot);
            if (slot != null) slot.RemoveVibration(name);
        }
        public static void StopVibration(int playerSlot)
        {
            var slot = GetInputSlot(playerSlot);
            if (slot != null) slot.StopVibration();
        }

        public static bool IsMouse() { return mouseUsed && !keyboardOnlyMode; }
        public static bool IsKeyboardOnlyMode() { return keyboardOnlyMode; }
        public static bool IsKeyboard() { return keyboardUsed; }
        public static bool IsKeyboardMouse() { return CUR_INPUT_TYPE == InputType.KEYBOARD_MOUSE; }
        public static bool IsGamepad() { return CUR_INPUT_TYPE == InputType.GAMEPAD; }
        public static bool IsTouch() { return CUR_INPUT_TYPE == InputType.TOUCH; }
        
        private static void CheckGamepadConnection()
        {
            for (int i = 0; i < 4; i++)
            {
                bool contains = connectedGamepads.Contains(i);
                if (IsGamepadAvailable(i))
                {
                    if (!contains)
                    {
                        connectedGamepads.Add(i);
                        OnControllerConnectionChanged(i, true, CUR_GAMEPAD);
                        if (CUR_INPUT_TYPE != InputType.GAMEPAD)
                        {
                            CUR_INPUT_TYPE = InputType.GAMEPAD;
                            OnInputTypeChanged(CUR_INPUT_TYPE);
                        }
                        if (inputSlots.Count == 1 && CUR_GAMEPAD < 0) CUR_GAMEPAD = i;
                    }
                }
                else
                {
                    if (contains)
                    {
                        connectedGamepads.Remove(i);
                        OnControllerConnectionChanged(i, false, CUR_GAMEPAD);

                        if(inputSlots.Count == 1)
                        {
                            if (connectedGamepads.Count <= 0)
                            {
                                CUR_GAMEPAD = -1;
                                if (CUR_INPUT_TYPE == InputType.GAMEPAD)
                                {
                                    CUR_INPUT_TYPE = InputType.KEYBOARD_MOUSE;
                                    OnInputTypeChanged(CUR_INPUT_TYPE);
                                }
                            }
                            else//other gamepads remaining
                            {
                                CUR_GAMEPAD = connectedGamepads[0];
                                if (CUR_INPUT_TYPE == InputType.GAMEPAD)
                                {
                                    CUR_INPUT_TYPE = InputType.KEYBOARD_MOUSE;
                                    OnInputTypeChanged(CUR_INPUT_TYPE);
                                }
                            }
                        }
                        else
                        {
                            if (CUR_GAMEPAD == i) CUR_GAMEPAD = -1;
                        }
                    }
                }
            }

            if(inputSlots.Count == 1)
            {
                gamepadUsed = GetGamepadUsed();
                if (gamepadUsed >= 0 && gamepadUsed != CUR_GAMEPAD)
                {
                    if (ShapeEngine.IsWindows()) XInput.SetVibration(CUR_GAMEPAD, 0f, 0f);
                    CUR_GAMEPAD = gamepadUsed;
                    if (CUR_INPUT_TYPE != InputType.GAMEPAD)
                    {
                        CUR_INPUT_TYPE = InputType.GAMEPAD;
                        OnInputTypeChanged(CUR_INPUT_TYPE);
                    }
                }
            }
        }
        private static void CheckInputType()
        {
            mouseUsed = false;
            keyboardUsed = false;
            //check if new input type was used and raise event
            switch (CUR_INPUT_TYPE)
            {
                case InputType.KEYBOARD_MOUSE:

                    if (WasAnyGamepadUsed())
                    {
                        CUR_INPUT_TYPE = InputType.GAMEPAD;
                        OnInputTypeChanged(CUR_INPUT_TYPE);
                    }
                    else if (WasTouchUsed())
                    {
                        CUR_INPUT_TYPE = InputType.TOUCH;
                        OnInputTypeChanged(CUR_INPUT_TYPE);
                    }
                    else
                    {
                        mouseUsed = WasMouseUsed();
                        keyboardUsed = WasKeyboardUsed();
                    }
                    break;

                case InputType.GAMEPAD:
                    mouseUsed = WasMouseUsed();
                    keyboardUsed = WasKeyboardUsed();
                    if (mouseUsed || keyboardUsed)
                    {
                        StopVibration(0);
                        CUR_INPUT_TYPE = InputType.KEYBOARD_MOUSE;
                        OnInputTypeChanged(CUR_INPUT_TYPE);
                    }
                    else if (WasTouchUsed())
                    {
                        StopVibration(0);
                        CUR_INPUT_TYPE = InputType.TOUCH;
                        OnInputTypeChanged(CUR_INPUT_TYPE);
                    }
                    break;

                case InputType.TOUCH:
                    mouseUsed = WasMouseUsed();
                    keyboardUsed = WasKeyboardUsed();
                    if (mouseUsed || keyboardUsed)
                    {
                        CUR_INPUT_TYPE = InputType.KEYBOARD_MOUSE;
                        OnInputTypeChanged(CUR_INPUT_TYPE);
                    }
                    else if (WasAnyGamepadUsed())
                    {
                        CUR_INPUT_TYPE = InputType.GAMEPAD;
                        OnInputTypeChanged(CUR_INPUT_TYPE);
                    }
                    break;

                default:
                    break;
            }

        }
        
        private static bool WasKeyboardUsed()
        {
            return GetKeyPressed() > 0;
        }
        private static bool WasMouseUsed()
        {
            bool mouseMovement = Vector2LengthSqr(GetMouseMovement()) > 0.0f;
            if (mouseMovement) return true;

            bool wheelMovement = MathF.Abs(GetMouseWheelMovement()) > 0;
            if (wheelMovement) return true;

            bool mouseButton =
                IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT) ||
                IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT) ||
                IsMouseButtonDown(MouseButton.MOUSE_BUTTON_MIDDLE) ||
                IsMouseButtonDown(MouseButton.MOUSE_BUTTON_SIDE) ||
                IsMouseButtonDown(MouseButton.MOUSE_BUTTON_EXTRA) ||
                IsMouseButtonDown(MouseButton.MOUSE_BUTTON_FORWARD) ||
                IsMouseButtonDown(MouseButton.MOUSE_BUTTON_BACK);

            return mouseButton;
        }

        private static int GetGamepadUsed()
        {
            if (connectedGamepads.Count <= 0) return -1;
            foreach (int gamepad in connectedGamepads)
            {
                //if (gamepad == CUR_GAMEPAD) continue;
                if (CheckGamepadUsed(gamepad)) return gamepad;
            }
            return -1; // CUR_GAMEPAD;
        }
        private static bool CheckGamepadUsed(int gamepad)
        {
            if (!IsGamepadConnected(gamepad)) return false;

            if (GetGamepadButton(gamepad)) return true;

            bool axis =
                Vector2LengthSqr(GetGamepadAxisLeft(gamepad, 0.5f, false)) > 0.0f ||
                Vector2LengthSqr(GetGamepadAxisRight(gamepad, 0.5f, false)) > 0.0f ||
                GetGamepadLeftTrigger(gamepad, 0.5f, false) > 0.0f ||
                GetGamepadRightTrigger(gamepad, 0.5f, false) > 0.0f;
            return axis;
        }
        private static bool WasAnyGamepadUsed()
        {
            return gamepadUsed >= 0;
        }
        private static bool WasTouchUsed()
        {
            return GetTouchPointCount() > 0;
        }
        public static bool IsGamepadConnected(int gamepadIndex)
        {
            return connectedGamepads.Contains(gamepadIndex);
        }
        public static bool IsCurGamepadConnected()
        {
            return connectedGamepads.Contains(CUR_GAMEPAD);
        }

        public static bool GetGamepadButton(int gamepad)
        {
            foreach (var button in Enum.GetValues(typeof(GamepadButton)).Cast<int>())
            {
                if (IsGamepadButtonDown(gamepad, button)) return true;
            }
            return false;
        }
        public static Vector2 GetGamepadAxisLeft(int gamepad, float deadzone = 0.25f, bool normalized = false)
        {
            if (inputDisabled) return new(0f);
            if (!IsGamepadConnected(gamepad)) return new(0.0f, 0.0f);

            Vector2 axis = new Vector2(
                GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_X),
                GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_Y));

            if (Vector2LengthSqr(axis) < deadzone * deadzone) return new(0.0f, 0.0f);

            if (normalized) return Vector2Normalize(axis);
            return axis;
        }
        public static Vector2 GetGamepadAxisRight(int gamepad, float deadzone = 0.25f, bool normalized = false)
        {
            if (inputDisabled) return new(0f);
            if (!IsGamepadConnected(gamepad)) return new(0.0f, 0.0f);

            Vector2 axis = new Vector2(
                GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_X),
                GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y));

            if (Vector2LengthSqr(axis) < deadzone * deadzone) return new(0.0f, 0.0f);

            if (normalized) return Vector2Normalize(axis);
            return axis;
        }
        public static float GetGamepadLeftTrigger(int gamepad, float deadzone = 0.25f, bool inverted = false)
        {
            if (inputDisabled) return inverted ? 1f : 0f;
            if (!IsGamepadConnected(gamepad))
            {
                if (inverted) return 1.0f;
                return 0.0f;
            }
            //raylib axis goes from -1 to 1 -> converted to 0 to 1
            float axis = (GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_TRIGGER) + 1.0f) * 0.5f;
            if (MathF.Abs(axis) < deadzone)
            {
                if (inverted) return 1.0f;
                return 0.0f;
            }

            if (inverted) return 1.0f - axis;
            return axis;
        }
        public static float GetGamepadRightTrigger(int gamepad, float deadzone = 0.25f, bool inverted = false)
        {
            if (inputDisabled) return inverted ? 1f : 0f;
            if (!IsGamepadConnected(gamepad))
            {
                if (inverted) return 1.0f;
                return 0.0f;
            }
            //raylib axis goes from -1 to 1 -> converted to 0 to 1
            float axis = (GetGamepadAxisMovement(gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_TRIGGER) + 1.0f) * 0.5f;
            if (MathF.Abs(axis) < deadzone)
            {
                if (inverted) return 1.0f;
                return 0.0f;
            }

            if (inverted) return 1.0f - axis;
            return axis;
        }


        public static Vector2 GetMouseMovement(float deadzone = 5.0f, bool normalized = false)
        {
            var movement = GetMouseDelta();
            if (Vector2LengthSqr(movement) < deadzone * deadzone) return new(0.0f, 0.0f);
            if (normalized) return Vector2Normalize(movement);
            return movement;
        }
        public static float GetMouseWheelMovement(bool inverted = false)
        {
            float movement = GetMouseWheelMove();

            if (inverted) return -movement;
            return movement;
        }
        public static Vector2 GetMouseWheelMovementV(bool inverted = false)
        {
            Vector2 movement = GetMouseWheelMoveV();

            if (inverted) return -movement;
            return movement;
        }
    
    }
}