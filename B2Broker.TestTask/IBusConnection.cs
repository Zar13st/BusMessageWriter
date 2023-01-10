namespace B2Broker.TestTask;

public interface IBusConnection
{
    Task PublishAsync(byte[] msg);
}