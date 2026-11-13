using PracticalWork3.DataStructures;

namespace PracticalWork3.Part3;

public sealed record ProcessInfo(string Name, int Burst);

public sealed record Segment(string Name, int Start, int End);

public sealed record ProcessResult(string Name, int Burst, int Completion, int Turnaround, int Waiting);

/// <summary>
/// ОЧЕРЕДЬ: планировщик процессов Round-Robin (карусель).
/// Процесс получает квант времени, и если не завершился — становится в конец очереди.
/// Все процессы прибывают в момент 0.
/// </summary>
public static class RoundRobinScheduler
{
    private sealed class Job
    {
        public readonly ProcessInfo Info;
        public int Remaining;
        public Job(ProcessInfo info) { Info = info; Remaining = info.Burst; }
    }

    public static (List<Segment> Timeline, List<ProcessResult> Results) Simulate(
        IEnumerable<ProcessInfo> processes, int quantum)
    {
        if (quantum < 1) throw new ArgumentException("Квант должен быть >= 1.");

        var queue = new LinkedQueue<Job>();
        foreach (var p in processes)
        {
            if (p.Burst < 1) throw new ArgumentException($"Время процесса {p.Name} должно быть >= 1.");
            queue.Enqueue(new Job(p));
        }

        var timeline = new List<Segment>();
        var results = new List<ProcessResult>();
        int time = 0;

        while (!queue.IsEmpty())
        {
            var task = queue.Dequeue();
            int slice = Math.Min(quantum, task.Remaining);
            timeline.Add(new Segment(task.Info.Name, time, time + slice));
            time += slice;
            task.Remaining -= slice;

            if (task.Remaining > 0)
                queue.Enqueue(task);         // не доработал — в конец очереди
            else
                results.Add(new ProcessResult(task.Info.Name, task.Info.Burst,
                    Completion: time, Turnaround: time, Waiting: time - task.Info.Burst));
        }
        return (timeline, results);
    }
}
