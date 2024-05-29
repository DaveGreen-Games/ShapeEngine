using System.Numerics;
using ShapeEngine.Lib;

namespace Examples.PayloadSystem;

public readonly struct PdsInfo
{
    public readonly float CallInTime;
    public readonly float Cooldown;
    public readonly float Duration;
    public readonly int Activations;
    public readonly uint PayloadID;
    
    public float TriggerInterval => Activations <= 0 ? 0f :  Duration / Activations;
    public bool HasDuration => Duration > 0f;
    public bool HasCooldown => Cooldown > 0f;
    public bool HasCallInTime => CallInTime > 0f;

    public PdsInfo(uint payloadId, float callInTime, float cooldown, float duration, int activations)
    {
        this.PayloadID = payloadId;
        this.Cooldown = cooldown;
        this.CallInTime = callInTime;
        this.Duration = duration;
        this.Activations = activations;
    }
}