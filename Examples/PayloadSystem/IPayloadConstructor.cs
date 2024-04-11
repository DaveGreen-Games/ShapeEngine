namespace Examples.PayloadSystem;

public interface IPayloadConstructor
{
    public IPayload? Create(uint payloadId);
}