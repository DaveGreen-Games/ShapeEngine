using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;

namespace Examples.PayloadSystem;

// public interface IPayloadTimingSystem
// {
//     public float GetTriggerInterval(int curActivation, int maxActivations, float duration, float remainingDuration);
// }






public abstract class PayloadDeliverySystem
{

    public event Action<IPayload, int, int>? OnPayloadLaunched;
    
    private float callInTimer = 0f;
    private float cooldownTimer = 0f;
    private float activeTimer = 0f;
    private int remainingActivations = 0;
    private float triggerTimer = 0f;
    
    public bool IsReady => cooldownTimer <= 0f && callInTimer <= 0f && activeTimer <= 0f && curMarker == null;
    public float CallInF => BasicInfo.CallInTime <= 0f ? 0f : callInTimer / BasicInfo.CallInTime;
    public float CooldownF => BasicInfo.Cooldown <= 0f ? 0f : cooldownTimer / BasicInfo.Cooldown;
    public float ActiveF => BasicInfo.Duration <= 0f ? 0f : activeTimer / BasicInfo.Duration;
    public float ActivationF => BasicInfo.Activations <= 0f ? 0f : (float)remainingActivations / (float)BasicInfo.Activations;
    public float TriggerF => BasicInfo.TriggerInterval <= 0f ? 0f : 1f - (triggerTimer / BasicInfo.TriggerInterval);

    public readonly PdsInfo BasicInfo;
    public readonly Vector2 Position;
    private readonly IPayloadConstructor payloadConstructor;
    protected readonly IPayloadTargetingSystem TargetingSystem;

    protected PayloadMarker? curMarker { get; private set; } = null;

    private readonly List<IPayload> payloads = new(32);
    // private bool payloadMarkerActive = false;
    
    protected PayloadDeliverySystem(PdsInfo info, IPayloadTargetingSystem targetingSystem, IPayloadConstructor constructor, Vector2 position)
    {
        BasicInfo = info;
        TargetingSystem = targetingSystem;
        Position = position;
        payloadConstructor = constructor;
    }

    private void OnPayloadMarkerLocationReached()
    {
        // payloadMarkerActive = false;
        CallIn();
    }


    public void Update(float dt)
    {
        for (int i = payloads.Count - 1; i >= 0; i--)
        {
            var payload = payloads[i];
            payload.Update(dt);
            if(payload.IsFinished()) payloads.RemoveAt(i);
        }
        
        
        if (curMarker != null && curMarker.Launched)
        {
            curMarker.Update(dt);
        }
        
        if (callInTimer > 0f)
        {
            callInTimer -= dt;
            if (callInTimer <= 0f)
            {
                StartDeployment();
            }
        }
        if (activeTimer > 0f)
        {
            activeTimer -= dt;
            if (activeTimer <= 0f)
            {
                EndDeployment();
            }
        }
        if (triggerTimer > 0f)
        {
            triggerTimer -= dt;
            if (triggerTimer <= 0f)
            {
                LaunchPayload(BasicInfo.Activations - remainingActivations, BasicInfo.Activations);
                remainingActivations--;
                if (remainingActivations > 0) triggerTimer = BasicInfo.TriggerInterval;

            }
        }
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= dt;
            if (cooldownTimer <= 0f)
            {
                CooldownHasFinished();
            }
        }
        
        WasUpdated(dt);
    }

    public void Draw()
    {
        WasDrawn();
        
        foreach (var payload in payloads)
        {
            payload.Draw();
        }
        
        curMarker?.Draw();
        
    }

    public abstract void DrawUI(Rect rect);
    
    public void Reset()
    {
        cooldownTimer = 0f;
        callInTimer = 0f;
        activeTimer = 0f;
        remainingActivations = 0;
        triggerTimer = 0f;
        DismissMarker();
        WasReset();
    }
    public bool Cancel()
    {
        if (IsReady) return false;
        Reset();
        cooldownTimer = BasicInfo.Cooldown;
        WasCanceled();
        return true;
    }
    
    public bool RequestPayload(PayloadMarker marker)
    {
        if (!IsReady) return false;
        if (!marker.Launched) return false;

        curMarker = marker;
        curMarker.OnTargetReached += OnPayloadMarkerLocationReached;
        WasRequested();
        return true;
    }
    
    private void CallIn()
    {
        if(curMarker != null) TargetingSystem.Activate(Position, curMarker.Location, curMarker.Direction);
        callInTimer = BasicInfo.CallInTime;
        WasCalledIn();
    }
    private void StartDeployment()
    {
        if (BasicInfo.Duration > 0f)
        {
            activeTimer = BasicInfo.Duration;
            if (BasicInfo.Activations > 0)
            {
                LaunchPayload(0, BasicInfo.Activations);
                remainingActivations = BasicInfo.Activations - 1;
                if(remainingActivations > 0) triggerTimer = BasicInfo.TriggerInterval;
            }
            else LaunchPayload(0, 0);
            
            DeploymentHasStarted();
        }
        else
        {
            if (BasicInfo.Activations > 0)
            {
                for (int i = 0; i < BasicInfo.Activations; i++)
                {
                    LaunchPayload(i, BasicInfo.Activations);
                }
            }
            else LaunchPayload(0, 0);

            DeploymentHasStarted();
            EndDeployment();
        }
        
    }
    private void EndDeployment()
    {
        DismissMarker();
        cooldownTimer = BasicInfo.Cooldown;
        DeploymentHasEnded();
    }
    private void LaunchPayload(int cur, int max)
    {
        if (curMarker == null || !curMarker.Launched) return;
        
        var payload = payloadConstructor.Create(BasicInfo.PayloadID);
        if (payload == null)
        {
            Cancel();
            return;
        }

        var targetLocation = TargetingSystem.GetTargetPosition(cur, max); // BasicInfo.GetDeploymentPosition(curMarker.Location);
        
        payload.Launch(Position, targetLocation, curMarker.Location, curMarker.Direction);
        payloads.Add(payload);
        
        OnPayloadLaunched?.Invoke(payload, cur, max);
        PayloadWasLaunched(payload, cur, max);
    }
    private void DismissMarker()
    {
        if (curMarker == null) return;

        curMarker.Dismiss();
        curMarker.OnTargetReached -= OnPayloadMarkerLocationReached;
        curMarker = null;
    }
    
    protected virtual void PayloadWasLaunched(IPayload payload, int cur, int max) { }
    protected virtual void WasReset() { }
    protected virtual void WasCanceled() { }
    protected virtual void WasRequested() { }
    protected virtual void WasCalledIn() { }
    protected virtual void DeploymentHasStarted() { }
    protected virtual void DeploymentHasEnded() { }
    protected virtual void CooldownHasFinished() { }
    protected virtual void WasUpdated(float dt) { }
    protected virtual void WasDrawn() { }
    
    // {
    //     if (CooldownF > 0f)
    //     {
    //         var marginRect = rect.ApplyMargins(0f, CooldownF, 0f, 0f);
    //         marginRect.Draw(Colors.Warm.ChangeBrightness(-0.5f));
    //         rect.DrawLines(2f, Colors.Warm);
    //     }
    //     else if(CallInF > 0f)
    //     {
    //         
    //         float f = 1f - CallInF;
    //         var marginRect = rect.ApplyMargins(0f, f, 0f, 0f);
    //         marginRect.Draw(Colors.Special.ChangeBrightness(-0.5f));
    //         rect.DrawLines(2f, Colors.Special);
    //     }
    //     else if (ActiveF > 0f)
    //     {
    //         float f = 1f - ActiveF;
    //         var marginRect = rect.ApplyMargins(0f, f, 0f, 0f);
    //         marginRect.Draw(Colors.Highlight.ChangeBrightness(-0.5f));
    //         rect.DrawLines(2f, Colors.Highlight);
    //     }
    //     else
    //     {
    //         rect.Draw(Colors.Cold.ChangeBrightness(-0.5f));
    //         rect.DrawLines(2f, Colors.Cold);
    //     }
    // }

}