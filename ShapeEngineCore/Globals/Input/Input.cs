using System.Numerics;
using Raylib_CsLo;
using ShapeEngineCore.Globals.Cursor;
using Vortice.XInput;

namespace ShapeEngineCore.Globals.Input
{
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

    public static class Input
    {
        public enum InputType
        {
            KEYBOARD_MOUSE = 0,
            GAMEPAD = 2,
            TOUCH = 3
        }

        public static float GAMEPAD_VIBRATION_STRENGTH = 1.0f;

        private static List<GamepadVibration> gamepadVibrationStack = new();


        private static int CUR_GAMEPAD = -1;//-1 non connected
        private static List<int> connectedGamepads = new();
        public static int GetCurGamepad() { return CUR_GAMEPAD; }
        public static ref readonly List<int> GetConnectedGamepads() { return ref connectedGamepads; }
        public static int GetConnectedGamepadCount() { return connectedGamepads.Count; }


        private static InputType CUR_INPUT_TYPE = InputType.KEYBOARD_MOUSE;
        public static InputType GetCurInputType() { return CUR_INPUT_TYPE; }

        private static bool mouseUsed = false;
        private static bool keyboardOnlyMode = false;
        private static int gamepadUsed = -1;
        //STATIC EVENT!!! - everything that subscribes to a static event must unsubscribe before deletion, otherwise memory leaks can happen
        //garbage collector can not collect anything that is subscribed to a static event!!!
        public delegate void InputChanged(InputType newType);
        public static event InputChanged? OnInputChanged;

        public delegate void GamepadConnectionChanged(int gamepad, bool connected, int curGamepad);
        public static event GamepadConnectionChanged? OnGamepadConnectionChanged;

        private static void OnInputTypeChanged(InputType newInputType)
        {
            if (newInputType == InputType.KEYBOARD_MOUSE) { CursorHandler.Show(); }
            else { CursorHandler.Hide(); }
            OnInputChanged?.Invoke(newInputType);
        }
        private static void OnControllerConnectionChanged(int gamepad, bool connected, int curGamepad)
        {
            if (!connected && gamepad == curGamepad) StopVibration(gamepad);
            OnGamepadConnectionChanged?.Invoke(gamepad, connected, curGamepad);
        }


        public static void Initialize()
        {
            if (IsGamepadAvailable(0)) { CUR_GAMEPAD = 0; }
            CUR_INPUT_TYPE = InputType.KEYBOARD_MOUSE;
        }

        public static void AddVibration(float leftMotor, float rightMotor, float duration = -1f, string name = "default")
        {
            if (!IsCurGamepadConnected() || !IsGamepad() || GAMEPAD_VIBRATION_STRENGTH <= 0f) return;
            gamepadVibrationStack.Add(new(name, duration, leftMotor, rightMotor));
        }
        public static void RemoveVibration(string name)
        {
            if (name == "" || gamepadVibrationStack.Count <= 0) return;
            gamepadVibrationStack.RemoveAll(item => item.name == name);
        }
        public static void StopVibration(int gamepad)
        {
            XInput.SetVibration(gamepad, 0f, 0f);
            gamepadVibrationStack.Clear();
        }

        public static bool IsMouse() { return mouseUsed && !keyboardOnlyMode; }
        public static bool IsKeyboardOnly() { return keyboardOnlyMode; }
        public static bool IsKeyboardMouse() { return CUR_INPUT_TYPE == InputType.KEYBOARD_MOUSE; }
        public static bool IsGamepad() { return CUR_INPUT_TYPE == InputType.GAMEPAD; }
        public static bool IsTouch() { return CUR_INPUT_TYPE == InputType.TOUCH; }
        public static void Update(float dt)
        {
            CheckGamepadConnection();
            CheckInputType();
            UpdateVibration(dt);

        }
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
                        if (CUR_GAMEPAD < 0)
                        {
                            CUR_GAMEPAD = i;
                            if (CUR_INPUT_TYPE != InputType.GAMEPAD)
                            {
                                CUR_INPUT_TYPE = InputType.GAMEPAD;
                                OnInputTypeChanged(CUR_INPUT_TYPE);
                            }
                        }
                    }
                }
                else
                {
                    if (contains)
                    {
                        connectedGamepads.Remove(i);
                        OnControllerConnectionChanged(i, false, CUR_GAMEPAD);
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
                }
            }


            gamepadUsed = GetGamepadUsed();
            if (gamepadUsed != CUR_GAMEPAD && gamepadUsed >= 0)
            {
                if (CUR_GAMEPAD >= 0)//change to another connected gamepad
                {
                    XInput.SetVibration(CUR_GAMEPAD, 0f, 0f);
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
                    else mouseUsed = WasMouseUsed();
                    break;

                case InputType.GAMEPAD:
                    mouseUsed = WasMouseUsed();
                    if (mouseUsed || WasKeyboardUsed())
                    {
                        StopVibration(CUR_GAMEPAD);
                        CUR_INPUT_TYPE = InputType.KEYBOARD_MOUSE;
                        OnInputTypeChanged(CUR_INPUT_TYPE);
                    }
                    else if (WasTouchUsed())
                    {
                        StopVibration(CUR_GAMEPAD);
                        CUR_INPUT_TYPE = InputType.TOUCH;
                        OnInputTypeChanged(CUR_INPUT_TYPE);
                    }
                    break;

                case InputType.TOUCH:
                    if (WasMouseUsed() || WasKeyboardUsed())
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
        private static void UpdateVibration(float dt)
        {
            if (IsCurGamepadConnected() && IsGamepad())
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

                XInput.SetVibration(CUR_GAMEPAD, Clamp(maxLeftMotor * GAMEPAD_VIBRATION_STRENGTH, 0f, 1f), Clamp(maxRightMotor * GAMEPAD_VIBRATION_STRENGTH, 0f, 1f));
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
                if (CheckGamepadUsed(gamepad)) return gamepad;
            }
            return -1;
        }
        private static bool CheckGamepadUsed(int gamepad)
        {

            if (!IsGamepadConnected(gamepad)) return false;

            bool button = GetGamepadButtonPressed() > 0;
            if (button) return true;

            bool axis =
                Vector2LengthSqr(GetGamepadAxisLeft(gamepad)) > 0.0f ||
                Vector2LengthSqr(GetGamepadAxisRight(gamepad)) > 0.0f ||
                GetGamepadLeftTrigger(gamepad) > 0.0f ||
                GetGamepadRightTrigger(gamepad) > 0.0f;
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
        private static bool IsGamepadConnected(int gamepadIndex)
        {
            return connectedGamepads.Contains(gamepadIndex);
        }
        private static bool IsCurGamepadConnected()
        {
            return connectedGamepads.Contains(CUR_GAMEPAD);
        }


        public static Vector2 GetGamepadAxisLeft(int gamepad, float deadzone = 0.25f, bool normalized = false)
        {
            if (!IsGamepadConnected(gamepad)) return new(0.0f, 0.0f);

            Vector2 axis = new Vector2(
                GetGamepadAxisMovement(CUR_GAMEPAD, GamepadAxis.GAMEPAD_AXIS_LEFT_X),
                GetGamepadAxisMovement(CUR_GAMEPAD, GamepadAxis.GAMEPAD_AXIS_LEFT_Y));

            if (Vector2LengthSqr(axis) < deadzone * deadzone) return new(0.0f, 0.0f);

            if (normalized) return Vector2Normalize(axis);
            return axis;
        }
        public static Vector2 GetGamepadAxisRight(int gamepad, float deadzone = 0.25f, bool normalized = false)
        {
            if (!IsGamepadConnected(gamepad)) return new(0.0f, 0.0f);

            Vector2 axis = new Vector2(
                GetGamepadAxisMovement(CUR_GAMEPAD, GamepadAxis.GAMEPAD_AXIS_RIGHT_X),
                GetGamepadAxisMovement(CUR_GAMEPAD, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y));

            if (Vector2LengthSqr(axis) < deadzone * deadzone) return new(0.0f, 0.0f);

            if (normalized) return Vector2Normalize(axis);
            return axis;
        }
        public static float GetGamepadLeftTrigger(int gamepad, float deadzone = 0.25f, bool inverted = false)
        {
            if (!IsGamepadConnected(gamepad))
            {
                if (inverted) return 1.0f;
                return 0.0f;
            }
            //raylib axis goes from -1 to 1 -> converted to 0 to 1
            float axis = (GetGamepadAxisMovement(CUR_GAMEPAD, GamepadAxis.GAMEPAD_AXIS_LEFT_TRIGGER) + 1.0f) * 0.5f;
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
            if (!IsGamepadConnected(gamepad))
            {
                if (inverted) return 1.0f;
                return 0.0f;
            }
            //raylib axis goes from -1 to 1 -> converted to 0 to 1
            float axis = (GetGamepadAxisMovement(CUR_GAMEPAD, GamepadAxis.GAMEPAD_AXIS_RIGHT_TRIGGER) + 1.0f) * 0.5f;
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
    }
}