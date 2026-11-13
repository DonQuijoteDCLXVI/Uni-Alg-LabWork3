using System.Diagnostics;

namespace PracticalWork3.Infrastructure;

/// <summary>Замеры времени на Stopwatch (высокоточный таймер).</summary>
public static class Timing
{
    public static double TicksToMs(long ticks) => ticks * 1000.0 / Stopwatch.Frequency;

    /// <summary>Медиана времени (мс) по нескольким запускам; перед каждым запуском — сборка мусора вне замера.</summary>
    public static double MedianMs(Action action, int runs)
    {
        var results = new double[runs];
        for (int i = 0; i < runs; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            long start = Stopwatch.GetTimestamp();
            action();
            results[i] = TicksToMs(Stopwatch.GetTimestamp() - start);
        }
        Array.Sort(results);
        return results[runs / 2];
    }
}
