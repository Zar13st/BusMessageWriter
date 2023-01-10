namespace B2Broker.TestTask;

public class BusConnection : IBusConnection
{
    private int _sum;

    public Task PublishAsync(byte[] msg)
    {
        _sum += msg.Length;

        Console.WriteLine($"Sum: {_sum}, curr: {msg.Length}");

        return Task.CompletedTask;
    }
}