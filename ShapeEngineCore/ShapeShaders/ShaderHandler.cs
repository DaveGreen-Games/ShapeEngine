using Raylib_CsLo;
using ShapeScreen;
using System.Numerics;

namespace ShapeShaders
{

    public class ShaderHandler
    {
        private Dictionary<string, ScreenShader> screenShaders = new Dictionary<string, ScreenShader>();
        private Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();
        private bool enabled = true;
        
        public void Close()
        {
            //foreach (ScreenBuffer screenBuffer in screenBuffers)
            //{
            //    screenBuffer.Unload();
            //}
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
            //screenBuffers = new ScreenBuffer[0];
        }

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

        public bool HasScreenShader(string name) { return screenShaders.ContainsKey(name); }
        public void AddScreenShader(string name, string fileName, bool enabled = true, int order = 0)
        {
            if (name == "" || fileName == "") return;
            if (screenShaders.ContainsKey(name)) return;
            screenShaders[name] = new ScreenShader(fileName, name, enabled);
        }
        public void AddScreenShader(string name, Shader shader, bool enabled = true, int order = 0)
        {
            if (name == "") return;
            if (screenShaders.ContainsKey(name)) return;
            screenShaders[name] = new ScreenShader(shader, name, enabled);
        }
        public void RemoveScreenShader(string name)
        {
            //if (!screenShaders.ContainsKey(name)) return;
            screenShaders.Remove(name);
        }
        public ScreenShader? GetScreenShader(string name)
        {
            if (!screenShaders.ContainsKey(name)) return null;
            return screenShaders[name];
        }

        public void AddShader(string name, string path)
        {
            if (name == "" || path == "") return;
            if (shaders.ContainsKey(name)) return;
            shaders[name] = LoadShader("330", path);
        }
        public void RemoveShader(string name)
        {
            shaders.Remove(name);
        }
        public Shader? GetShader(string name)
        {
            if (!shaders.ContainsKey(name)) return null;
            return shaders[name];
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
        public bool IsScreenShaderEnabled(string name)
        {
            if (!screenShaders.ContainsKey(name)) return false;
            return screenShaders[name].IsEnabled();
        }
        public void EnableScreenShader(string name)
        {
            if (!screenShaders.ContainsKey(name)) return;
            screenShaders[name].Enable();
        }
        public void DisableScreenShader(string name)
        {
            if (!screenShaders.ContainsKey(name)) return;
            screenShaders[name].Disable();
        }
        public void SetScreenShaderValueFloat(string shaderName, string propertyName, float value)
        {
            if (!screenShaders.ContainsKey(shaderName)) return;
            Shader shader = screenShaders[shaderName].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, value, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        }
        public void SetScreenShaderValueVec(string shaderName, string propertyName, float[] values, ShaderUniformDataType dataType)
        {
            if (!screenShaders.ContainsKey(shaderName)) return;
            Shader shader = screenShaders[shaderName].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, values, dataType);
        }
        public void SetScreenShaderValueVec(string shaderName, string propertyName, float v1, float v2)
        {
            if (!screenShaders.ContainsKey(shaderName)) return;
            Shader shader = screenShaders[shaderName].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2 }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        }
        public void SetScreenShaderValueVec(string shaderName, string propertyName, float v1, float v2, float v3)
        {
            if (!screenShaders.ContainsKey(shaderName)) return;
            Shader shader = screenShaders[shaderName].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3 }, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
        }
        public void SetScreenShaderValueVec(string shaderName, string propertyName, float v1, float v2, float v3, float v4)
        {
            if (!screenShaders.ContainsKey(shaderName)) return;
            Shader shader = screenShaders[shaderName].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3, v4 }, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        }
        public void SetScreenShaderValueVec(string shaderName, string propertyName, Vector2 vec)
        {
            if (!screenShaders.ContainsKey(shaderName)) return;
            Shader shader = screenShaders[shaderName].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { vec.X, vec.Y}, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        }
        public void SetScreenShaderValueVec(string shaderName, string propertyName, Color color)
        {
            if (!screenShaders.ContainsKey(shaderName)) return;
            Shader shader = screenShaders[shaderName].GetShader();
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] {color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f}, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        }
        public void SetShaderValueFloat(string shaderName, string propertyName, float value)
        {
            if (!shaders.ContainsKey(shaderName)) return;
            Shader shader = shaders[shaderName];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, value, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        }
        public void SetShaderValueVec(string shaderName, string propertyName, float[] values, ShaderUniformDataType dataType)
        {
            if (!shaders.ContainsKey(shaderName)) return;
            Shader shader = shaders[shaderName];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, values, dataType);
        }

        public void SetShaderValueVec(string shaderName, string propertyName, float v1, float v2)
        {
            if (!shaders.ContainsKey(shaderName)) return;
            Shader shader = shaders[shaderName];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2 }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        }
        public void SetShaderValueVec(string shaderName, string propertyName, float v1, float v2, float v3)
        {
            if (!shaders.ContainsKey(shaderName)) return;
            Shader shader = shaders[shaderName];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3 }, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
        }
        public void SetShaderValueVec(string shaderName, string propertyName, float v1, float v2, float v3, float v4)
        {
            if (!shaders.ContainsKey(shaderName)) return;
            Shader shader = shaders[shaderName];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { v1, v2, v3, v4 }, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        }
        public void SetShaderValueVec(string shaderName, string propertyName, Vector2 vec)
        {
            if (!shaders.ContainsKey(shaderName)) return;
            Shader shader = shaders[shaderName];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { vec.X, vec.Y }, ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        }
        public void SetShaderValueVec(string shaderName, string propertyName, Color color)
        {
            if (!shaders.ContainsKey(shaderName)) return;
            Shader shader = shaders[shaderName];
            int valueLocation = GetShaderLocation(shader, propertyName);
            SetShaderValue(shader, valueLocation, new float[] { color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f }, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        }
    }
}
