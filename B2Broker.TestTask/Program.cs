using B2Broker.TestTask;

var msg = new byte[10];

var bus = new BusConnection();

var cfg = new TestConfig() { FlushThreshold = 990 };

var busMsgWriter = new BusMessageWriter(bus, cfg);

var taskArr = new List<Task>();
for (int i = 0; i < 100; i++)
{
    taskArr.Add(WriteMsg());
}

await Task.WhenAll(taskArr);

Console.WriteLine($"Finish!");

Console.ReadLine();



async Task WriteMsg()
{
    for (int i = 0; i < 1000; i++)
    {
        await Task.Delay(1);
        await busMsgWriter.SendMessageAsync(msg);
    }
}


