using Raylib_CsLo;
using System.Numerics;

namespace ShapeEase
{
    public class EaseHandler
    {
        Dictionary<string, EaseChain> chains = new();

        public EaseHandler() { }

        public delegate void EaseChainFinished(string name, dynamic lastValue);
        public event EaseChainFinished? OnEaseChainFinished;

        public bool HasChain(string name) { return chains.ContainsKey(name); }
        public void AddChain(string name, dynamic start, EaseOrder change, params EaseOrder[] changes)
        {
            if (chains.ContainsKey(name)) chains[name] = new(start, change, changes);
            else chains.Add(name, new(start, change, changes));
        }
        public dynamic? Get(string name)
        {
            if (!chains.ContainsKey(name)) return null;
            return chains[name].GetValue();
        }
        public float GetFloat(string name)
        {
            if (!chains.ContainsKey(name)) return 0f;
            dynamic value = chains[name].GetValue();
            if (value is float) return value;
            else return 0f;
        }
        public int GetInt(string name)
        {
            if (!chains.ContainsKey(name)) return 0;
            dynamic value = chains[name].GetValue();
            if (value is int) return value;
            else return 0;
        }
        public Vector2 GetVector2(string name)
        {
            if (!chains.ContainsKey(name)) return new(0f);
            dynamic value = chains[name].GetValue();
            if (value is Vector2) return value;
            else return new(0f);
        }
        public Color GetColor(string name)
        {
            if (!chains.ContainsKey(name)) return new(0, 0, 0, 0);
            dynamic value = chains[name].GetValue();
            if (value is Color) return value;
            else return new(0, 0, 0, 0);
        }

        public void Update(float dt)
        {
            var remove = chains.Where(kvp => kvp.Value.IsFinished());
            foreach (var chain in remove)
            {
                OnEaseChainFinished?.Invoke(chain.Key, chain.Value.GetValue());
                chains.Remove(chain.Key);
            }
            foreach (var chain in chains.Values)
            {
                chain.Update(dt);
            }
        }

        public void Close()
        {
            chains.Clear();
        }
    }

}
