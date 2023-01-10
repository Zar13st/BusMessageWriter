using System.Collections.Concurrent;

namespace B2Broker.TestTask;

public class BusMessageWriter
{
    private readonly IBusConnection _connection;
    private readonly TestConfig _config;

    private readonly ConcurrentQueue<byte[]> _queue = new ConcurrentQueue<byte[]>();
    private readonly SemaphoreSlim _locker = new SemaphoreSlim(1,1);

    private int _currentLength;
    private bool _isFlushing;
    
    public BusMessageWriter(IBusConnection connection, TestConfig config)
    {
        _connection = connection;
        _config = config;
    }

    // how to make this method thread safe?
    public async Task SendMessageAsync(byte[] nextMessage)
    {
        if(nextMessage.Length == 0) return;

        WriteToBuffer(nextMessage);

        if (!IsNeedToFlush()) return;

        var isSuccess = await _locker.WaitAsync(0);
        if(!isSuccess) return;

        try
        {
            if (!IsNeedToFlush()) return;

            _isFlushing = true;

            await FlushBuffer();
        }
        finally
        {
            _locker.Release();
            _isFlushing = false;
        }
    }

    private bool IsNeedToFlush()
    {
        if (_isFlushing) return false;

        return _currentLength > _config.FlushThreshold;
    }

    private void WriteToBuffer(byte[] nextMessage)
    {
        _queue.Enqueue(nextMessage);

        Interlocked.Add(ref _currentLength, nextMessage.Length);
    }

    private async Task FlushBuffer()
    {
        int queueCount = _queue.Count;
        int flushMsgLength = 0;

        MemoryStream buffer = new();
        for (int i = 0; i < queueCount; i++)
        {
            var isSuccess = _queue.TryDequeue(out var item);
            if (!isSuccess) break;

            flushMsgLength += item!.Length;

            buffer.Write(item, 0, item.Length);
        }

        Interlocked.Add(ref _currentLength, -flushMsgLength);

        await _connection.PublishAsync(buffer.ToArray());
    }
}

public record TestConfig
{
    public int FlushThreshold { get; init; }
}