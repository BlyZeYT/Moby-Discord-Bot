namespace Moby.Common;

public static class Stats
{
    public static double CpuLoad { get; private set; }

    public static ulong AllocatedMemory { get; private set; }

    public static int FramesSent { get; private set; }

    public static int Players { get; private set; }

    public static TimeSpan Uptime { get; private set; }

    public static Task UpdateAsync(double cpuLoad, ulong allocatedMemory, int framesSent, int players, TimeSpan uptime)
    {
        CpuLoad = cpuLoad;
        AllocatedMemory = allocatedMemory;
        FramesSent = framesSent;
        Players = players;
        Uptime = uptime;

        return Task.CompletedTask;
    }
}