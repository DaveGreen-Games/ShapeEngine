using ShapeCore;
using ShapeLib;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks.Dataflow;

namespace ShapeInput
{
    public enum SKeyboardButton
    {
        APOSTROPHE = 39,
        COMMA = 44,
        MINUS = 45,
        PERIOD = 46,
        SLASH = 47,
        ZERO = 48,
        ONE = 49,
        TWO = 50,
        THREE = 51,
        FOUR = 52,
        FIVE = 53,
        SIX = 54,
        SEVEN = 55,
        EIGHT = 56,
        NINE = 57,
        SEMICOLON = 59,
        EQUAL = 61,
        A = 65,
        B = 66,
        C = 67,
        D = 68,
        E = 69,
        F = 70,
        G = 71,
        H = 72,
        I = 73,
        J = 74,
        K = 75,
        L = 76,
        M = 77,
        N = 78,
        O = 79,
        P = 80,
        Q = 81,
        R = 82,
        S = 83,
        T = 84,
        U = 85,
        V = 86,
        W = 87,
        X = 88,
        Y = 89,
        Z = 90,
        LEFT_BRACKET = 91,
        BACKSLASH = 92,
        RIGHT_BRACKET = 93,
        GRAVE = 96,
        SPACE = 0x20,
        ESCAPE = 0x100,
        ENTER = 257,
        TAB = 258,
        BACKSPACE = 259,
        INSERT = 260,
        DELETE = 261,
        RIGHT = 262,
        LEFT = 263,
        DOWN = 264,
        UP = 265,
        PAGE_UP = 266,
        PAGE_DOWN = 267,
        HOME = 268,
        END = 269,
        CAPS_LOCK = 280,
        SCROLL_LOCK = 281,
        NUM_LOCK = 282,
        PRINT_SCREEN = 283,
        PAUSE = 284,
        F1 = 290,
        F2 = 291,
        F3 = 292,
        F4 = 293,
        F5 = 294,
        F6 = 295,
        F7 = 296,
        F8 = 297,
        F9 = 298,
        F10 = 299,
        F11 = 300,
        F12 = 301,
        LEFT_SHIFT = 340,
        LEFT_CONTROL = 341,
        LEFT_ALT = 342,
        LEFT_SUPER = 343,
        RIGHT_SHIFT = 344,
        RIGHT_CONTROL = 345,
        RIGHT_ALT = 346,
        RIGHT_SUPER = 347,
        KB_MENU = 348,
        KP_0 = 320,
        KP_1 = 321,
        KP_2 = 322,
        KP_3 = 323,
        KP_4 = 324,
        KP_5 = 325,
        KP_6 = 326,
        KP_7 = 327,
        KP_8 = 328,
        KP_9 = 329,
        KP_DECIMAL = 330,
        KP_DIVIDE = 331,
        KP_MULTIPLY = 332,
        KP_SUBTRACT = 333,
        KP_ADD = 334,
        KP_ENTER = 335,
        KP_EQUAL = 336,
        VOLUME_UP = 24,
        VOLUME_DOWN = 25,
        BACK = 7,
        NULL = 8,
        MENU = 9,
    }
    public enum SMouseButton
    {
        LEFT = 0,
        RIGHT = 1,
        MIDDLE = 2,
        SIDE = 3,
        EXTRA = 4,
        FORWARD = 5,
        BACK = 6,
        MW_UP = 10,
        MW_DOWN = 11,
        MW_LEFT = 12,
        MW_RIGHT = 13,
    }
    public enum SGamepadButton
    {
        UNKNOWN = 0,
        LEFT_FACE_UP = 1,
        LEFT_FACE_RIGHT = 2,
        LEFT_FACE_DOWN = 3,
        LEFT_FACE_LEFT = 4,
        RIGHT_FACE_UP = 5,
        RIGHT_FACE_RIGHT = 6,
        RIGHT_FACE_DOWN = 7,
        RIGHT_FACE_LEFT = 8,
        LEFT_TRIGGER_TOP = 9,
        LEFT_TRIGGER_BOTTOM = 10,
        RIGHT_TRIGGER_TOP = 11,
        RIGHT_TRIGGER_BOTTOM = 12,
        MIDDLE_LEFT = 13,
        MIDDLE = 14,
        MIDDLE_RIGHT = 15,
        LEFT_THUMB = 16,
        RIGHT_THUMB = 17,

        LEFT_STICK_RIGHT = 30,
        LEFT_STICK_LEFT = 40,
        LEFT_STICK_DOWN = 31,
        LEFT_STICK_UP = 41,

        RIGHT_STICK_RIGHT = 32,
        RIGHT_STICK_LEFT = 42,
        RIGHT_STICK_DOWN = 33,
        RIGHT_STICK_UP = 43,
    }
    public enum SMouseWheelAxis
    {
        HORIZONTAL = 0,
        VERTICAL = 1,
    }
    public enum SGamepadAxis
    {
        LEFT_X = 0,
        LEFT_Y = 1,
        RIGHT_X = 2,
        RIGHT_Y = 3,
        LEFT_TRIGGER = 4,
        RIGHT_TRIGGER = 5,
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
    public class InputMap
    {
        public event Action? OnInputTypeChanged;
        public event Action? OnGamepadConnectionChanged;
        public event Action? OnGamepadIndexChanged;
        public event Action? OnMouseMoved;

        private Dictionary<uint, InputActionGroup> actionGroups = new();
        private List<GamepadVibration> gamepadVibrationStack = new();
        private List<int> connectedGamepads = new();

        public bool Disabled { get; set; } = false;
        public float VibrationStrength { get; set; } = 1.0f;
        public int GamepadIndex { get; protected set; } = -1;
        public int MaxGamepads { get; set; } = 4;
        public uint ID { get; protected set; }
        public Vector2 MousePos
        {
            get
            {
                if (IsGamepad) return new(-1);
                else return mousePos;
            }
        }

        private Vector2 mousePos = new(-1);
        private Vector2 prevMousePos = new(-1);

        /// <summary>
        /// The input map is either using a gamepad or keyboard/mouse for input. If the gamepad index is bigger than 0 IsGamepad is always true. (only -1 / 0 index can be keyboard/mouse as well)
        /// </summary>
        public bool IsGamepad { get; protected set; } = false; //Input Type
        public bool IsGamepadConnected { get; protected set; } = false;
        private bool autoAssignGamepad = false;
        
        public InputMap(uint id, int gamepadIndex)//, params InputActionGroup[] actions)
        {
            //SetInputActionsGroups(actions);
            GamepadSetup();
            this.autoAssignGamepad = false;
            this.GamepadIndex = gamepadIndex;
            if (gamepadIndex <= 0) IsGamepad = false;
            else if (gamepadIndex > 0) IsGamepad = true;
            this.IsGamepadConnected = CheckGamepad(this.GamepadIndex);
            this.ID = id;
        }
        public InputMap(uint id)//, params InputActionGroup[] actions)
        {
            //SetInputActionsGroups(actions);
            GamepadSetup();
            this.autoAssignGamepad = true;
            this.GamepadIndex = GetNextGamepad();
            if (this.GamepadIndex < 0) IsGamepad = false;
            this.IsGamepadConnected = CheckGamepad(this.GamepadIndex);
            this.ID = id;
        }
        public InputMap Copy(uint id, int gamepadIndex)
        {
            var newMap = new InputMap(id, gamepadIndex);
            foreach (var group in actionGroups.Values)
            {
                newMap.AddGroup(group.Copy());
            }
            return newMap;
        }
        internal void AddGroup(InputActionGroup group)
        {
            if (actionGroups.ContainsKey(group.ID))
            {
                actionGroups[group.ID] = group;
            }
            else actionGroups.Add(group.ID, group);
        }
        public void AddGroup(uint id, params InputAction[] actions)
        {
            if (actionGroups.ContainsKey(id))
            {
                actionGroups[id] = new(id, actions);
            }
            else actionGroups.Add(id, new(id, actions));
        }
        //public void SetInputActionsGroups(params InputActionGroup[] newGroups)
        //{
        //    this.actionGroups.Clear();
        //    foreach (var group in newGroups)
        //    {
        //        if (this.actionGroups.ContainsKey(group.ID)) continue;
        //        this.actionGroups.Add(group.ID, group);
        //    }
        //}
        public void Update(float dt)
        {
            if (!Disabled)
            {
                if(GamepadIndex <= 0)
                {
                    prevMousePos = mousePos;
                    mousePos = GetMousePos();
                    if (WasMouseMoved()) OnMouseMoved?.Invoke();
                }

                bool gamepadUsed = false;
                bool keyboardUsed = false;
                foreach (var group in actionGroups.Values)
                {
                    if (group.Disabled) continue;
                    foreach (var action in group.Actions.Values)
                    {
                        action.Update(GamepadIndex, dt);
                        var state = action.State;
                        gamepadUsed = gamepadUsed || action.State.gamepadUsed;
                        keyboardUsed = keyboardUsed || action.State.keyboardUsed;
                    }
                }
                
                CheckGamepadConnection();
                CheckInputType(keyboardUsed, gamepadUsed);
                UpdateVibration(dt);
            }
        }
        public InputActionState GetActionState(uint groupID, uint actionID)
        {
            if (!Disabled)
            {
                if(actionGroups.ContainsKey(groupID))
                {
                    return actionGroups[groupID].GetState(actionID);
                }
            }
            return new();
        }
        public InputActionState GetActionState(uint actionID)
        {
            foreach (var group in actionGroups.Values)
            {
                if(group.Actions.ContainsKey(actionID)) 
                    return group.Disabled ? new() : group.GetState(actionID);

            }
            return new();
        }
        public string GetInputName(uint actionID,bool shorthand = true)
        {
            foreach (var group in actionGroups.Values)
            {
                if (group.Actions.ContainsKey(actionID))
                    return group.Disabled ? "" : group.GetInputName(actionID, shorthand);

            }
            return "";
        }
        public string GetInputName(uint groupID, uint actionID, bool shorthand = true)
        {
            if (!actionGroups.ContainsKey(groupID)) return "";
            return actionGroups[groupID].GetInputName(actionID, shorthand);
        }
        public void SetGroupDisabled(uint groupID, bool disabled) { if(actionGroups.ContainsKey(groupID)) actionGroups[groupID].Disabled = disabled; }
        public void AddVibration(float leftMotor, float rightMotor, float duration = -1f, string name = "default")
        {
            if(!Disabled && IsGamepad && IsGamepadConnected && VibrationStrength > 0f)
            {
                gamepadVibrationStack.Add(new(name, duration, leftMotor, rightMotor));
            }
        }
        public void RemoveVibration(string name)
        {
            if (name == "" || gamepadVibrationStack.Count <= 0) return;
            gamepadVibrationStack.RemoveAll(item => item.name == name);
        }
        public void StopVibration()
        {
            //if (ShapeEngine.IsWindows()) XInput.SetVibration(GamepadIndex, 0f, 0f);

            gamepadVibrationStack.Clear();
        }
        private void UpdateVibration(float dt)
        {
            if (IsGamepad && IsGamepadConnected)
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

                //if (ShapeEngine.IsWindows())
                //    XInput.SetVibration(GamepadIndex, Clamp(maxLeftMotor * VibrationStrength, 0f, 1f), Clamp(maxRightMotor * VibrationStrength, 0f, 1f));
            }
        }

        private void GamepadSetup()
        {
            connectedGamepads.Clear();
            for (int i = 0; i < MaxGamepads; i++)
            {
                if (IsGamepadAvailable(i)) connectedGamepads.Add(i);
            }
        }
        private bool CheckGamepad(int gamepadIndex)
        {
            if (gamepadIndex < 0) return false;
            else return connectedGamepads.Contains(gamepadIndex);
        }
        private int GetNextGamepad()
        {
            if (connectedGamepads.Count <= 0) return -1;
            else return connectedGamepads[0];
        }
        private void CheckGamepadConnection()
        {
            for (int i = 0; i < MaxGamepads; i++)
            {
                bool contains = connectedGamepads.Contains(i);
                if (IsGamepadAvailable(i))
                {
                    if (!contains)
                    {
                        connectedGamepads.Add(i);
                    }
                }
                else
                {
                    if (contains)
                    {
                        connectedGamepads.Remove(i);
                    }
                }
            }

            bool prevIsGamepad = IsGamepad;
            bool prevIsGamepadConnected = IsGamepadConnected;
            int prevGamepadIndex = GamepadIndex;
            if (autoAssignGamepad)
            {
                if (!connectedGamepads.Contains(GamepadIndex))
                {
                    GamepadIndex = GetNextGamepad();
                }
            }

            IsGamepadConnected = CheckGamepad(GamepadIndex);
            if(!IsGamepadConnected && GamepadIndex <= 0)
            {
                IsGamepad = false;
            }

            if (prevIsGamepad != IsGamepad) InputTypeChanged();
            if (prevIsGamepadConnected != IsGamepadConnected) GamepadConnectionChanged();
            if (prevGamepadIndex != GamepadIndex) GamepadIndexChanged();
        }
        private void CheckInputType(bool keyboardUsed, bool gamepadUsed)
        {
            if(GamepadIndex <= 0 || autoAssignGamepad)
            {
                bool prevIsGamepad = IsGamepad;

                if (IsGamepad)
                {
                    if (!gamepadUsed)
                    {
                        if (WasMouseMoved() || keyboardUsed) IsGamepad = false;
                    }
                    
                }
                else
                {
                    if (!keyboardUsed)
                    {
                        if (gamepadUsed) 
                            IsGamepad = true;
                    }
                }

                if(IsGamepad != prevIsGamepad)
                {
                    InputTypeChanged();
                }
            }
        }
        
        private void GamepadConnectionChanged() { OnGamepadConnectionChanged?.Invoke(); }
        private void GamepadIndexChanged() { OnGamepadIndexChanged?.Invoke(); }
        private void InputTypeChanged() 
        {
            if (!IsGamepad) StopVibration();
            OnInputTypeChanged?.Invoke(); 
        }
        

        
        public Vector2 GetMouseDelta(float deadzone = 0f, bool normalized = false)
        {
            if (IsGamepad) return new(0f);
            Vector2 d = mousePos - prevMousePos;
            if (Vector2LengthSqr(d) < deadzone * deadzone) return new(0f);
            else return normalized ? Vector2Normalize(d) : d;
        }
        private Vector2 GetMousePos()
        {
            var pos = GetMousePosition();
            if (float.IsNaN(pos.X) || float.IsNaN(pos.Y))
            {
                return prevMousePos;
            }
            else return pos;
        }
        private bool WasMouseMoved(float deadzone = 5f)
        {
            Vector2 d = mousePos - prevMousePos;
            float lSq = Vector2LengthSqr(d);
            if (lSq < deadzone * deadzone) lSq = 0f;
            return lSq > 0.0f;
        }
        
        //private Vector2 GetMouseMovement(float deadzone = 5.0f, bool normalized = false)
        //{
        //    var movement = Raylib_CsLo.Raylib.GetMouseDelta();
        //    if (Vector2LengthSqr(movement) < deadzone * deadzone) return new(0.0f, 0.0f);
        //    if (normalized) return Vector2Normalize(movement);
        //    return movement;
        //}

    }
   
}

    /*
    //IInputDevice interface?
    //input device class -> keyboard/mouse/gamepad
    //gamepad device supports vibration
    //last used input device = cur input device
    public class GamepadSlot
    {
        public int gamepadIndex = -1;
        public bool disabled = false;

        private List<GamepadVibration> gamepadVibrationStack = new();
        public GamepadSlot(int gamepadIndex)
        {
            this.gamepadIndex = gamepadIndex;
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
            if (ShapeEngine.IsWindows()) XInput.SetVibration(gamepadIndex, 0f, 0f);

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
    */