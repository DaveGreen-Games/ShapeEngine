using System.Numerics;

namespace ShapeEngineCore.Globals.Input
{
    public class InputMap
    {
        private string name = "";
        //private int gamepad = -1;
        private Dictionary<string, InputAction> inputActions = new();
        //private bool disabled = false;

        public InputMap(string name, params InputAction[] actions)
        {
            this.name = name;
            foreach (var action in actions)
            {
                AddAction(action.GetName(), action);
            }
        }

        //public void Update(float dt, int gamepad, bool gamepadOnly)
        //{
        //    foreach (var inputAction in inputActions.Values)
        //    {
        //        inputAction.Update(dt, gamepad, gamepadOnly);
        //    }
        //}
        //public int GamepadIndex { get { return gamepad; } }
        //public bool HasGamepad { get { return gamepad >= 0; } }
        public void Rename(string newName) { name = newName; }
        public string GetName() { return name; }
        public void AddActions(List<InputAction> actions)
        {
            foreach (var inputAction in actions)
            {
                AddAction(inputAction);
            }
        }
        public void AddAction(InputAction action)
        {
            string name = action.GetName();
            if (inputActions.ContainsKey(name))
            {
                inputActions[name] = action;
            }
            else
            {
                inputActions.Add(name, action);
            }
        }
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
            return inputActions[name].GetAllKeyNames();
        }
        //public bool IsDisabled() { return disabled; }
        //public void Enable() { disabled = false; }
        //public void Disable() { disabled = true; }

        public float GetAxis(int gamepad, string negative, string positive)
        {
            if (!inputActions.ContainsKey(negative) || !inputActions.ContainsKey(positive)) return 0f;
            //int gamepadIndex = gamepad < 0 ? Input.GetCurGamepad() : gamepad;
            float p = inputActions[positive].IsDown(gamepad) ? 1f : 0f;
            float n = inputActions[negative].IsDown(gamepad) ? 1f : 0f;
            return p - n;
        }
        public Vector2 GetAxis(int gamepad, string left, string right, string up, string down)
        {
            return new(GetAxis(gamepad, left, right), GetAxis(gamepad, up, down));
        }
        public float GetGamepadAxis(int gamepad, string gamepadAxisAction)
        {
            if (!inputActions.ContainsKey(gamepadAxisAction)) return 0f;
            //int gamepadIndex = gamepad < 0 ? Input.GetCurGamepad() : gamepad;
            return inputActions[gamepadAxisAction].GetGamepadAxis(gamepad);
        }
        public Vector2 GetGamepadAxis(int gamepad,string gamepadAxisHor, string gamepadAxisVer)
        {
            return new(GetGamepadAxis(gamepad, gamepadAxisHor), GetGamepadAxis(gamepad, gamepadAxisVer));
        }

        //public float GetHoldF(int gamepad, string actionName, bool gamepadOnly = false)
        //{
        //    if (inputActions.Count <= 0 || !inputActions.ContainsKey(actionName)) return -1f;
        //    return inputActions[actionName].GetHoldF();
        //}
        public bool IsDown(int gamepad, string actionName, bool gamepadOnly = false)
        {
            if (inputActions.Count <= 0 || !inputActions.ContainsKey(actionName)) return false;
            //int gamepadIndex = gamepad < 0 ? Input.GetCurGamepad() : gamepad;
            return inputActions[actionName].IsDown(gamepad, gamepadOnly);
        }
        public bool IsPressed(int gamepad, string actionName, bool gamepadOnly = false)
        {
            if (inputActions.Count <= 0 || !inputActions.ContainsKey(actionName)) return false;
            //int gamepadIndex = gamepad < 0 ? Input.GetCurGamepad() : gamepad;
            return inputActions[actionName].IsPressed(gamepad, gamepadOnly);
        }
        public bool IsReleased(int gamepad, string actionName, bool gamepadOnly = false)
        {
            if (inputActions.Count <= 0 || !inputActions.ContainsKey(actionName)) return false;
            //int gamepadIndex = gamepad < 0 ? Input.GetCurGamepad() : gamepad;
            return inputActions[actionName].IsReleased(gamepad, gamepadOnly);
        }
        public bool IsUp(int gamepad, string actionName, bool gamepadOnly = false)
        {
            if (inputActions.Count <= 0 || !inputActions.ContainsKey(actionName)) return false;
            //int gamepadIndex = gamepad < 0 ? Input.GetCurGamepad() : gamepad;
            return inputActions[actionName].IsUp(gamepad, gamepadOnly);
        }

        public (string keyboard, string mouse, string gamepad) GetKeyNames(string actionName, bool shorthand = false)
        {
            if (inputActions.Count <= 0 || !inputActions.ContainsKey(actionName)) return ("", "", "");
            return inputActions[actionName].GetKeyNames(shorthand);
        }
        //public List<string> GetKeyNames(string actionName, bool shorthand = true)
        //{
        //    if (inputActions.Count <= 0 || IsDisabled() || !inputActions.ContainsKey(actionName)) return new();
        //    return inputActions[actionName].GetKeyNames(shorthand);
        //}
    }

}
