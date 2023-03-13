using System.Numerics;

namespace ShapeInput
{
    public class InputMap
    {
        private int id = -1;
        public string DisplayName { get; private set; }
        //private int gamepad = -1;
        private Dictionary<int, InputAction> inputActions = new();
        //private bool disabled = false;

        public InputMap(int id, string displayName, params InputAction[] actions)
        {
            this.id = id;
            this.DisplayName = displayName;
            foreach (var action in actions)
            {
                AddAction(action.GetID(), action);
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
        //public void Rename(string newName) { id = newName; }
        public int GetID() { return id; }
        public void AddActions(List<InputAction> actions)
        {
            foreach (var inputAction in actions)
            {
                AddAction(inputAction);
            }
        }
        public void AddAction(InputAction action)
        {
            int id = action.GetID();
            if (inputActions.ContainsKey(id))
            {
                inputActions[id] = action;
            }
            else
            {
                inputActions.Add(id, action);
            }
        }
        public void AddAction(int id, InputAction action)
        {
            if (inputActions.ContainsKey(id))
            {
                inputActions[id] = action;
            }
            else
            {
                inputActions.Add(id, action);
            }
        }
        public void AddAction(int id, params InputAction.Keys[] keys)
        {
            AddAction(id, new InputAction(id, keys));
        }
        public void RemoveAction(int id)
        {
            inputActions.Remove(id);
        }
        public InputAction? GetAction(int id)
        {
            if (!inputActions.ContainsKey(id)) return null;
            return inputActions[id];
        }
        public List<string> GetKeyNames(int id)
        {
            if (!inputActions.ContainsKey(id)) return new();
            return inputActions[id].GetAllKeyNames();
        }
        //public bool IsDisabled() { return disabled; }
        //public void Enable() { disabled = false; }
        //public void Disable() { disabled = true; }

        public float GetAxis(int gamepad, int idNegative, int idPositive)
        {
            if (!inputActions.ContainsKey(idNegative) || !inputActions.ContainsKey(idPositive)) return 0f;
            //int gamepadIndex = gamepad < 0 ? Input.GetCurGamepad() : gamepad;
            float p = inputActions[idPositive].IsDown(gamepad) ? 1f : 0f;
            float n = inputActions[idNegative].IsDown(gamepad) ? 1f : 0f;
            return p - n;
        }
        public Vector2 GetAxis(int gamepad, int idLeft, int idRight, int idUp, int idDown)
        {
            return new(GetAxis(gamepad, idLeft, idRight), GetAxis(gamepad, idUp, idDown));
        }
        public float GetGamepadAxis(int gamepad, int id)
        {
            if (!inputActions.ContainsKey(id)) return 0f;
            //int gamepadIndex = gamepad < 0 ? Input.GetCurGamepad() : gamepad;
            return inputActions[id].GetGamepadAxis(gamepad);
        }
        public Vector2 GetGamepadAxis(int gamepad,int gamepadAxisHorID, int gamepadAxisVerID)
        {
            return new(GetGamepadAxis(gamepad, gamepadAxisHorID), GetGamepadAxis(gamepad, gamepadAxisVerID));
        }

        //public float GetHoldF(int gamepad, string actionName, bool gamepadOnly = false)
        //{
        //    if (inputActions.Count <= 0 || !inputActions.ContainsKey(actionName)) return -1f;
        //    return inputActions[actionName].GetHoldF();
        //}
        public bool IsDown(int gamepad, int actionID, bool gamepadOnly = false)
        {
            if (inputActions.Count <= 0 || !inputActions.ContainsKey(actionID)) return false;
            //int gamepadIndex = gamepad < 0 ? Input.GetCurGamepad() : gamepad;
            return inputActions[actionID].IsDown(gamepad, gamepadOnly);
        }
        public bool IsPressed(int gamepad, int actionID, bool gamepadOnly = false)
        {
            if (inputActions.Count <= 0 || !inputActions.ContainsKey(actionID)) return false;
            //int gamepadIndex = gamepad < 0 ? Input.GetCurGamepad() : gamepad;
            return inputActions[actionID].IsPressed(gamepad, gamepadOnly);
        }
        public bool IsReleased(int gamepad, int actionID, bool gamepadOnly = false)
        {
            if (inputActions.Count <= 0 || !inputActions.ContainsKey(actionID)) return false;
            //int gamepadIndex = gamepad < 0 ? Input.GetCurGamepad() : gamepad;
            return inputActions[actionID].IsReleased(gamepad, gamepadOnly);
        }
        public bool IsUp(int gamepad, int actionID, bool gamepadOnly = false)
        {
            if (inputActions.Count <= 0 || !inputActions.ContainsKey(actionID)) return false;
            //int gamepadIndex = gamepad < 0 ? Input.GetCurGamepad() : gamepad;
            return inputActions[actionID].IsUp(gamepad, gamepadOnly);
        }

        public (string keyboard, string mouse, string gamepad) GetKeyNames(int actionID, bool shorthand = false)
        {
            if (inputActions.Count <= 0 || !inputActions.ContainsKey(actionID)) return ("", "", "");
            return inputActions[actionID].GetKeyNames(shorthand);
        }
        //public List<string> GetKeyNames(string actionName, bool shorthand = true)
        //{
        //    if (inputActions.Count <= 0 || IsDisabled() || !inputActions.ContainsKey(actionName)) return new();
        //    return inputActions[actionName].GetKeyNames(shorthand);
        //}
    }

}
