using System.Numerics;
using Raylib_CsLo;

namespace ShapeEngineCore.Globals.Input
{
    public static class InputMapHandler
    {

        public static InputMap NULL_MAP = new("NULL MAP");
        private static Dictionary<string, InputMap> inputMaps = new();
        private static InputMap currentInputMap = NULL_MAP;
        private static bool disabled = false;
        public static readonly Dictionary<string, InputAction> UI_Default_InputActions = new()
        {
            {"UI Mouse Select", new("UI Mouse Select", InputAction.Keys.MB_LEFT) },
            {"UI Select", new("UI Select", InputAction.Keys.SPACE, InputAction.Keys.GP_BUTTON_RIGHT_FACE_DOWN) },
            {"UI Cancel", new("UI Cancel", InputAction.Keys.ESCAPE, InputAction.Keys.GP_BUTTON_RIGHT_FACE_RIGHT) },
            {"UI Cancel Mouse", new("UI Cancel Mouse", InputAction.Keys.MB_RIGHT) },
            {"UI Up", new("UI Up", InputAction.Keys.W, InputAction.Keys.UP, InputAction.Keys.GP_BUTTON_LEFT_FACE_UP) },
            {"UI Right", new("UI Right", InputAction.Keys.D, InputAction.Keys.RIGHT, InputAction.Keys.GP_BUTTON_LEFT_FACE_RIGHT) },
            {"UI Down", new("UI Down", InputAction.Keys.S, InputAction.Keys.DOWN, InputAction.Keys.GP_BUTTON_LEFT_FACE_DOWN) },
            {"UI Left", new("UI Left", InputAction.Keys.A, InputAction.Keys.LEFT, InputAction.Keys.GP_BUTTON_LEFT_FACE_LEFT) },
        };
        public static void Initialize()
        {
            inputMaps.Clear();
            inputMaps.Add(NULL_MAP.GetName(), NULL_MAP);
            currentInputMap = NULL_MAP;
            disabled = false;
        }

        public static void Close()
        {
            inputMaps.Clear();
        }

        public static string NextInputMap(bool switchMap = true)
        {
            var mapNameList = inputMaps.Keys.ToList();
            int nextIndex = mapNameList.IndexOf(currentInputMap.GetName()) + 1;
            if (nextIndex >= mapNameList.Count) nextIndex = 0;
            string newMapName = mapNameList[nextIndex];
            if (switchMap) SwitchToMap(newMapName);
            return newMapName;
        }
        public static string PreviousInputMap(bool switchMap = true)
        {
            var mapNameList = inputMaps.Keys.ToList();
            int prevIndex = mapNameList.IndexOf(currentInputMap.GetName()) - 1;
            if (prevIndex < 0) prevIndex = mapNameList.Count - 1;
            string newMapName = mapNameList[prevIndex];
            if (switchMap) SwitchToMap(newMapName);
            return newMapName;
        }
        public static void SwitchToMap(string mapName)
        {
            if (!inputMaps.ContainsKey(mapName)) return;
            currentInputMap = inputMaps[mapName];
        }
        public static bool IsDisabled() { return disabled; }
        public static void Enable() { disabled = false; }
        public static void Disable() { disabled = true; }
        public static void AddInputMap(InputMap map)
        {
            if (map == null) return;
            if (inputMaps.ContainsKey(map.GetName()))
            {
                inputMaps[map.GetName()] = map;
            }
            else
            {
                inputMaps.Add(map.GetName(), map);
            }
        }
        public static void AddInputMap(string name, params InputAction[] actions)
        {
            AddInputMap(new InputMap(name, actions));
        }
        public static void AddDefaultUIInputsToMap(string mapName)
        {

            InputMap map = mapName == "" || !inputMaps.ContainsKey(mapName) ? currentInputMap : inputMaps[mapName];
            foreach (var input in UI_Default_InputActions)
            {
                map.AddAction(input.Key, input.Value);
            }
        }
        public static void RemoveInputMap(string name)
        {
            if (!inputMaps.ContainsKey(name)) return;
            if (inputMaps[name] == currentInputMap) currentInputMap = NULL_MAP;
            inputMaps.Remove(name);
        }
        public static void RemoveInputMap(string name, string fallback)
        {
            if (!inputMaps.ContainsKey(fallback))
            {
                RemoveInputMap(name);
                return;
            }
            if (!inputMaps.ContainsKey(name)) return;
            if (inputMaps[name] == currentInputMap) currentInputMap = inputMaps[fallback];
            inputMaps.Remove(name);
        }

        public static InputMap? GetMap(string name)
        {
            if (!inputMaps.ContainsKey(name)) return null;
            return inputMaps[name];
        }

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
        public static void RenameMap(string name, string newName)
        {
            if (!inputMaps.ContainsKey(name)) return;
            inputMaps[name].Rename(newName);
        }

        public static bool IsDown(string actionName, string mapName = "")
        {
            if (IsDisabled()) return false;
            InputMap map = mapName == "" || !inputMaps.ContainsKey(mapName) ? currentInputMap : inputMaps[mapName];
            return map.IsDown(actionName);
        }
        public static bool IsPressed(string actionName, string mapName = "")
        {

            InputMap map = mapName == "" || !inputMaps.ContainsKey(mapName) ? currentInputMap : inputMaps[mapName];
            return map.IsPressed(actionName);
        }
        public static bool IsReleased(string actionName, string mapName = "")
        {
            if (IsDisabled()) return false;
            InputMap map = mapName == "" || !inputMaps.ContainsKey(mapName) ? currentInputMap : inputMaps[mapName];
            return map.IsReleased(actionName);
        }
        public static bool IsUp(string actionName, string mapName = "")
        {
            if (IsDisabled()) return false;
            InputMap map = mapName == "" || !inputMaps.ContainsKey(mapName) ? currentInputMap : inputMaps[mapName];
            return map.IsUp(actionName);
        }

        public static float GetAxis(string negative, string positive, string mapName = "")
        {
            if (IsDisabled()) return 0f;
            InputMap map = mapName == "" || !inputMaps.ContainsKey(mapName) ? currentInputMap : inputMaps[mapName];
            return map.GetAxis(negative, positive);
        }
        public static Vector2 GetAxis(string left, string right, string up, string down, string mapName = "", bool normalized = true)
        {
            if (IsDisabled()) return new(0f, 0f);
            InputMap map = mapName == "" || !inputMaps.ContainsKey(mapName) ? currentInputMap : inputMaps[mapName];
            if (normalized) return Vec.Normalize(map.GetAxis(left, right, up, down));
            else return map.GetAxis(left, right, up, down);
        }
        public static float GetGamepadAxis(string gamepadAxisAction, string mapName = "")
        {
            if (IsDisabled()) return 0f;
            InputMap map = mapName == "" || !inputMaps.ContainsKey(mapName) ? currentInputMap : inputMaps[mapName];
            return map.GetGamepadAxis(gamepadAxisAction);
        }
        public static Vector2 GetGamepadAxis(string gamepadAxisHor, string gamepadAxisVer, string mapName = "", bool normalized = true)
        {
            if (IsDisabled()) return new(0f, 0f);
            InputMap map = mapName == "" || !inputMaps.ContainsKey(mapName) ? currentInputMap : inputMaps[mapName];

            if (normalized) return Vec.Normalize(map.GetGamepadAxis(gamepadAxisHor, gamepadAxisVer));
            else return map.GetGamepadAxis(gamepadAxisHor, gamepadAxisVer);
        }


        public static string GetInputActionKeyName(string inputAction, bool isGamepad = false, string mapName = "")
        {
            InputMap map = mapName == "" || !inputMaps.ContainsKey(mapName) ? currentInputMap : inputMaps[mapName];
            return map.GetInputActionKeyName(inputAction, isGamepad);
        }
        //public static List<string> GetKeyNames(string actionName, string mapName = "", bool shorthand = true)
        //{
        //    InputMap map = (mapName == "" || !inputMaps.ContainsKey(mapName)) ? currentInputMap : inputMaps[mapName];
        //    return map.GetKeyNames(actionName, shorthand);
        //}
    }
}
