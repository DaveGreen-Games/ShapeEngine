using System.Numerics;

namespace ShapeEngineCore.Globals.Input
{
    public class InputMap
    {
        private string name = "";
        private int gamepad = -1;
        private Dictionary<string, InputAction> inputActions = new();
        private bool disabled = false;

        public InputMap(string name, params InputAction[] actions)
        {
            this.name = name;
            foreach (var action in actions)
            {
                AddAction(action.GetName(), action);
            }
        }
        public InputMap(string name, int gamepad, params InputAction[] actions)
        {
            this.name = name;
            this.gamepad = gamepad;
            foreach (var action in actions)
            {
                AddAction(action.GetName(), action);
            }
        }

        public int GamepadIndex { get { return gamepad; } }
        public bool HasGamepad { get { return gamepad >= 0; } }
        public void Rename(string newName) { name = newName; }
        public string GetName() { return name; }
        public void AddAction(string name, InputAction action)
        {
            if (inputActions.ContainsKey(name))
            {
                inputActions[name] = action;
            }
            else
            {
                inputActions.Add(name, action);
            }
        }
        public void AddAction(string name, params InputAction.Keys[] keys)
        {
            AddAction(name, new InputAction(name, keys));
        }
        public void RemoveAction(string name)
        {
            inputActions.Remove(name);
        }
        public InputAction? GetAction(string name)
        {
            if (!inputActions.ContainsKey(name)) return null;
            return inputActions[name];
        }
        public List<string> GetKeyNames(string name)
        {
            if (!inputActions.ContainsKey(name)) return new();
            return inputActions[name].GetKeyNames();
        }
        public bool IsDisabled() { return disabled; }
        public void Enable() { disabled = false; }
        public void Disable() { disabled = true; }

        public float GetAxis(string negative, string positive)
        {
            if (!inputActions.ContainsKey(negative) || !inputActions.ContainsKey(positive)) return 0f;
            int gamepadIndex = !HasGamepad ? Input.GetCurGamepad() : gamepad;
            float p = inputActions[positive].IsDown(gamepadIndex) ? 1f : 0f;
            float n = inputActions[negative].IsDown(gamepadIndex) ? 1f : 0f;
            return p - n;
        }
        public Vector2 GetAxis(string left, string right, string up, string down)
        {
            return new(GetAxis(left, right), GetAxis(up, down));
        }
        public float GetGamepadAxis(string gamepadAxisAction)
        {
            if (!inputActions.ContainsKey(gamepadAxisAction)) return 0f;
            int gamepadIndex = !HasGamepad ? Input.GetCurGamepad() : gamepad;
            return inputActions[gamepadAxisAction].GetGamepadAxis(gamepadIndex);
        }
        public Vector2 GetGamepadAxis(string gamepadAxisHor, string gamepadAxisVer)
        {
            return new(GetGamepadAxis(gamepadAxisHor), GetGamepadAxis(gamepadAxisVer));
        }


        public bool IsDown(string actionName)
        {
            if (inputActions.Count <= 0 || IsDisabled() || !inputActions.ContainsKey(actionName)) return false;
            int gamepadIndex = !HasGamepad ? Input.GetCurGamepad() : gamepad;
            return inputActions[actionName].IsDown(gamepadIndex);
        }
        public bool IsPressed(string actionName)
        {
            if (inputActions.Count <= 0 || IsDisabled() || !inputActions.ContainsKey(actionName)) return false;
            int gamepadIndex = !HasGamepad ? Input.GetCurGamepad() : gamepad;
            return inputActions[actionName].IsPressed(gamepadIndex);
        }
        public bool IsReleased(string actionName)
        {
            if (inputActions.Count <= 0 || IsDisabled() || !inputActions.ContainsKey(actionName)) return false;
            int gamepadIndex = !HasGamepad ? Input.GetCurGamepad() : gamepad;
            return inputActions[actionName].IsReleased(gamepadIndex);
        }
        public bool IsUp(string actionName)
        {
            if (inputActions.Count <= 0 || IsDisabled() || !inputActions.ContainsKey(actionName)) return false;
            int gamepadIndex = !HasGamepad ? Input.GetCurGamepad() : gamepad;
            return inputActions[actionName].IsUp(gamepadIndex);
        }

        public string GetInputActionKeyName(string actionName, bool isGamepad = false)
        {
            if (inputActions.Count <= 0 || IsDisabled() || !inputActions.ContainsKey(actionName)) return "";
            return inputActions[actionName].GetInputKeyName(isGamepad);
        }
        //public List<string> GetKeyNames(string actionName, bool shorthand = true)
        //{
        //    if (inputActions.Count <= 0 || IsDisabled() || !inputActions.ContainsKey(actionName)) return new();
        //    return inputActions[actionName].GetKeyNames(shorthand);
        //}
    }

}
