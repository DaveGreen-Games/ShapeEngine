using Raylib_CsLo;
using System.Numerics;

namespace ShapeEngine.Screen
{
    internal sealed class ShaderFlash
    {
        public event Action<bool, uint>? ChangeShaderStatus;

        private float duration = 0f;
        private float timer = 0f;
        public uint ID { get; private set; }
        private bool shaderEnabled = false;
        
        public ShaderFlash(float dur, uint id)
        {
            this.ID = id;
            duration = dur;
            timer = dur;
        }

        public void Init()
        {
            if (timer > 0f)
            {
                //this.shaderHandler.EnableScreenShader(id);
                ChangeShaderStatus?.Invoke(true, ID);
                shaderEnabled = true;
            }
        }

        //public uint GetShaderID() { return shaderID; }
        public float Percentage()
        {
            if (duration <= 0.0f) return 0f;
            return timer / duration;
        }
        public bool IsFinished() { return timer <= 0f; }
        public void Reset(float dur)
        {
            if (dur <= 0f)
            {
                Stop();
                return;
            }

            duration = dur;
            timer = dur;
            if (!shaderEnabled)
            {
                //shaderHandler.EnableScreenShader(ID);
                ChangeShaderStatus?.Invoke(true, ID);
                shaderEnabled = true;
            }
        }
        public void Stop()
        {
            timer = 0f;
            if (shaderEnabled)
            {
                //shaderHandler.DisableScreenShader(ID);
                ChangeShaderStatus?.Invoke(false, ID);
                shaderEnabled = false;
            }
        }
        public void Restart()
        {
            if (duration <= 0f) return;
            timer = duration;
            if (!shaderEnabled)
            {
                //shaderHandler.EnableScreenShader(ID);
                ChangeShaderStatus?.Invoke(true, ID);
                shaderEnabled = true;
            }
        }
        public void Update(float dt)
        {
            if (timer > 0.0f)
            {
                timer -= dt;
                if (timer <= 0.0f)
                {
                    //shaderHandler.DisableScreenShader(ID);
                    ChangeShaderStatus?.Invoke(false, ID);
                    shaderEnabled = false;
                    timer = 0f;
                }
            }
        }

    }

    public interface IShaderDevice
    {
        public List<ScreenShader> GetCurActiveShaders();
        public void Update(float dt);
        public void Close();
    }

    public sealed class ShaderDeviceBasic : IShaderDevice
    {
        public void Close() { }

        public List<ScreenShader> GetCurActiveShaders() { return new(); }

        public void Update(float dt) { }
    }

    public sealed class ShaderDevice : IShaderDevice
    {
        private Dictionary<uint, ShaderFlash> screenShaderFlashes = new();
        private Dictionary<uint, ScreenShader> screenShaders = new();
        private Dictionary<uint, Shader> shaders = new();
        private bool enabled = true;
        
        public List<ScreenShader> GetCurActiveShaders()
        {
            if (!enabled) return new() { };

            List<ScreenShader> shadersToApply = screenShaders.Values.ToList().FindAll(s => s.IsEnabled());
            shadersToApply.Sort(delegate (ScreenShader a, ScreenShader b)
            {
                if (a.GetOrder() < b.GetOrder()) return -1;
                else if (a.GetOrder() > b.GetOrder()) return 1;
                else return 0;
            });
            return shadersToApply;
        }


        public void Update(float dt)
        {
            UpdateScreenShaderFlashes(dt);
        }
        public void Close()
        {
            foreach (Shader shader in shaders.Values)
            {
                UnloadShader(shader);
            }
            foreach (ScreenShader screenShader in screenShaders.Values)
            {
                screenShader.Unload();
            }
            screenShaders.Clear();
            shaders.Clear();
            screenShaderFlashes.Clear();
        }


        public void ScreenShaderFlash(float duration, params uint[] shaderIDs)
        {
            if (shaderIDs.Length <= 0) return;

            foreach (var id in shaderIDs)
            {
                if (!HasScreenShader(id)) continue;
                if (screenShaderFlashes.ContainsKey(id))
                {
                    screenShaderFlashes[id].Reset(duration);
                }
                else
                {
                    var flash = new ShaderFlash(duration, id);
                    flash.ChangeShaderStatus += OnShaderFlashStatusChange;
                    flash.Init();
                    screenShaderFlashes.Add(id, flash);
                }
            }
        }
        public void StopAllScreenShaderFlashes()
        {
            foreach (ShaderFlash shaderFlash in screenShaderFlashes.Values)
            {
                shaderFlash.Stop();
            }
            screenShaderFlashes.Clear();
        }
        private void UpdateScreenShaderFlashes(float dt)
        {
            List<uint> remove = new();
            foreach (var shaderFlash in screenShaderFlashes.Values)
            {
                shaderFlash.Update(dt);
                if (shaderFlash.IsFinished()) remove.Add(shaderFlash.ID); // screenShaderFlashes.Remove(shaderFlash.ID); //does that work?
            }
            foreach (var id in remove)
            {
                screenShaderFlashes[id].ChangeShaderStatus -= OnShaderFlashStatusChange;
                screenShaderFlashes.Remove(id);
            }
        }
        private void OnShaderFlashStatusChange(bool enable, uint shaderID)
        {
            if(enable)EnableScreenShader(shaderID);
            else DisableScreenShader(shaderID);
        }
        

        public bool HasScreenShader(uint id) { return screenShaders.ContainsKey(id); }
        public void AddScreenShader(uint id, string fileName, bool enabled = true, int order = 0)
        {
            if (fileName == "") return;
            if (screenShaders.ContainsKey(id)) return;
            screenShaders[id] = new ScreenShader(fileName, id, enabled);
        }
        public void AddScreenShader(uint id, Shader shader, bool enabled = true, int order = 0)
        {
            if (screenShaders.ContainsKey(id)) return;
            screenShaders[id] = new ScreenShader(shader, id, enabled);
        }
        public void RemoveScreenShader(uint id)
        {
            //if (!screenShaders.ContainsKey(name)) return;
            screenShaders.Remove(id);
        }
        public ScreenShader? GetScreenShader(uint id)
        {
            if (!screenShaders.ContainsKey(id)) return null;
            return screenShaders[id];
        }

        public void AddShader(uint id, string path)
        {
            if (path == "") return;
            if (shaders.ContainsKey(id)) return;
            shaders[id] = LoadShader("330", path);
        }
        public void RemoveShader(uint id)
        {
            shaders.Remove(id);
        }
        public Shader? GetShader(uint id)
        {
            if (!shaders.ContainsKey(id)) return null;
            return shaders[id];
        }

        public bool IsEnabled() { return enabled; }
        public bool Enable()
        {
            enabled = true;
            return enabled;
        }
        public bool Disable()
        {
            enabled = false;
            return enabled;
        }
        public void SetEnabled(bool value) { enabled = value; }
        public bool ToggleEnabled()
        {
            if (enabled) Disable();
            else Enable();
            return enabled;
        }
        public bool IsScreenShaderEnabled(uint id)
        {
            if (!screenShaders.ContainsKey(id)) return false;
            return screenShaders[id].IsEnabled();
        }
        public void EnableScreenShader(uint id)
        {
            if (!screenShaders.ContainsKey(id)) return;
            screenShaders[id].Enable();
        }
        public void DisableScreenShader(uint id)
        {
            if (!screenShaders.ContainsKey(id)) return;
            screenShaders[id].Disable();
        }
        public void SetScreenShaderValueFloat(uint id, string propertyName, float value)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, value, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        }
        public void SetScreenShaderValueVec(uint id, string propertyName, float[] values, ShaderUniformDataType dataType)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, values, dataType);
        }
        public void SetScreenShaderValueVec(uint id, string propertyName, float v1, float v2)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2 }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        }
        public void SetScreenShaderValueVec(uint id, string propertyName, float v1, float v2, float v3)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3 }, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
        }
        public void SetScreenShaderValueVec(uint id, string propertyName, float v1, float v2, float v3, float v4)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3, v4 }, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        }
        public void SetScreenShaderValueVec(uint id, string propertyName, Vector2 vec)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { vec.X, vec.Y}, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        }
        public void SetScreenShaderValueVec(uint id, string propertyName, Raylib_CsLo.Color color)
        {
            if (!screenShaders.ContainsKey(id)) return;
            Shader shader = screenShaders[id].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] {color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f}, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        }
        public void SetShaderValueFloat(uint id, string propertyName, float value)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, value, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        }
        public void SetShaderValueVec(uint id, string propertyName, float[] values, ShaderUniformDataType dataType)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, values, dataType);
        }

        public void SetShaderValueVec(uint id, string propertyName, float v1, float v2)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2 }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        }
        public void SetShaderValueVec(uint id, string propertyName, float v1, float v2, float v3)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3 }, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
        }
        public void SetShaderValueVec(uint id, string propertyName, float v1, float v2, float v3, float v4)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3, v4 }, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        }
        public void SetShaderValueVec(uint id, string propertyName, Vector2 vec)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { vec.X, vec.Y }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        }
        public void SetShaderValueVec(uint id, string propertyName, Raylib_CsLo.Color color)
        {
            if (!shaders.ContainsKey(id)) return;
            Shader shader = shaders[id];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f }, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        }
    }
}
