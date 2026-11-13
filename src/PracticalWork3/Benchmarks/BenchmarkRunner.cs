using System.Globalization;
using System.Text;
using PracticalWork3.DataStructures;
using PracticalWork3.Infrastructure;
using PracticalWork3.Part1;
using PracticalWork3.Part2;

namespace PracticalWork3.Benchmarks;

/// <summary>
/// Замеры: время СЧИТЫВАНИЯ файла (чтение + разбор в массив операций) и время ВЫПОЛНЕНИЯ операций
/// (вывод результатов отбрасывается, чтобы не мерить скорость консоли). Берётся медиана нескольких запусков.
/// </summary>
public static class BenchmarkRunner
{
    private const string Dir = "bench";

    private static int RunsFor(int count) => count >= 500_000 ? 3 : 5;
    private static string F(double v) => v.ToString("0.00", CultureInfo.InvariantCulture);

    public static string RunStack()
    {
        Directory.CreateDirectory(Dir);
        var sb = new StringBuilder();
        sb.AppendLine("| Набор операций | Операций | Размер файла, КБ | Считывание, мс | Выполнение, мс | Итого, мс |");
        sb.AppendLine("|---|---:|---:|---:|---:|---:|");
        Console.WriteLine($"{"Набор",-42}{"Оп.",10}{"КБ",10}{"Чтение,мс",12}{"Выполн.,мс",12}{"Итого,мс",11}");

        // прогрев JIT
        var warm = OperationParser.Parse("1,1 1,2 3 5 2 4");
        StackRunner.Run(warm, _ => { });

        var all = BenchCases.SizeCases().Concat(BenchCases.CompositionCases(200_000, "Push", "Pop")).ToList();
        int idx = 0;
        foreach (var c in all)
        {
            string path = BenchCases.WriteFile(c, Path.Combine(Dir, $"stack_{idx++:00}.txt"));
            long kb = new FileInfo(path).Length / 1024;
            int runs = RunsFor(c.Count);

            Operation[] ops = Array.Empty<Operation>();
            double read = Timing.MedianMs(() => ops = OperationParser.ReadFile(path), runs);
            double exec = Timing.MedianMs(() => StackRunner.Run(ops, _ => { }), runs);

            Console.WriteLine($"{c.Name,-42}{c.Count,10:N0}{kb,10:N0}{F(read),12}{F(exec),12}{F(read + exec),11}");
            sb.AppendLine($"| {c.Name} | {c.Count:N0} | {kb:N0} | {F(read)} | {F(exec)} | {F(read + exec)} |");
        }
        return sb.ToString();
    }

    public static string RunQueue()
    {
        Directory.CreateDirectory(Dir);
        var sb = new StringBuilder();
        sb.AppendLine("| Набор операций | Операций | Считывание, мс | Свой список, мс | Queue&lt;T&gt;, мс | Свой список / Queue&lt;T&gt; |");
        sb.AppendLine("|---|---:|---:|---:|---:|---:|");
        Console.WriteLine($"{"Набор",-42}{"Оп.",10}{"Чтение,мс",12}{"Список,мс",12}{"Queue,мс",11}{"Отношение",11}");

        var warm = OperationParser.Parse("1,1 1,2 3 5 2 4");
        QueueRunner.Run(new LinkedQueue<string>(), warm, _ => { });
        QueueRunner.Run(new StdQueue<string>(), warm, _ => { });

        var sizeCases = BenchCases.SizeCases().Select(c => (Group: "размер", Case: c));
        var compCases = BenchCases.CompositionCases(200_000, "Enqueue", "Dequeue").Select(c => (Group: "состав", Case: c));
        var all = sizeCases.Concat(compCases).ToList();

        string? worstName = null;
        double worstFactor = 1;
        int idx = 0;
        foreach (var (group, c) in all)
        {
            string path = BenchCases.WriteFile(c, Path.Combine(Dir, $"queue_{idx++:00}.txt"));
            int runs = RunsFor(c.Count);

            Operation[] ops = Array.Empty<Operation>();
            double read = Timing.MedianMs(() => ops = OperationParser.ReadFile(path), runs);
            double list = Timing.MedianMs(() => QueueRunner.Run(new LinkedQueue<string>(), ops, _ => { }), runs);
            double std = Timing.MedianMs(() => QueueRunner.Run(new StdQueue<string>(), ops, _ => { }), runs);

            double ratio = list / std;
            double factor = Math.Max(ratio, 1 / ratio);
            if (group == "состав" && factor > worstFactor) { worstFactor = factor; worstName = c.Name; }

            Console.WriteLine($"{c.Name,-42}{c.Count,10:N0}{F(read),12}{F(list),12}{F(std),11}{F(ratio),11}");
            sb.AppendLine($"| {c.Name} | {c.Count:N0} | {F(read)} | {F(list)} | {F(std)} | {F(ratio)} |");
        }

        string summary = worstName is null
            ? "Расхождений нет."
            : $"Максимальное расхождение среди наборов одной длины: «{worstName}» — в {F(worstFactor)} раза.";
        Console.WriteLine(summary);
        sb.AppendLine().AppendLine(summary);
        return sb.ToString();
    }

    /// <summary>Эмпирическая проверка линейности вычисления постфиксной записи: время на токен должно быть ~константой.</summary>
    public static string RunPostfix()
    {
        var sb = new StringBuilder();
        sb.AppendLine("| Токенов | Время (разбор + вычисление), мс | нс на токен |");
        sb.AppendLine("|---:|---:|---:|");
        Console.WriteLine($"{"Токенов",12}{"Время, мс",14}{"нс/токен",12}");

        PostfixEvaluator.Evaluate("1 2 +");   // прогрев
        foreach (int k in new[] { 10_000, 50_000, 100_000, 500_000, 1_000_000 })
        {
            // k единиц, затем k-1 плюсов: стек разрастается до k элементов (худший случай по памяти)
            string expr = string.Join(' ', Enumerable.Repeat("1", k)) + " " + string.Join(' ', Enumerable.Repeat("+", k - 1));
            int tokens = 2 * k - 1;
            double ms = Timing.MedianMs(() => PostfixEvaluator.Evaluate(expr), 5);
            double nsPerToken = ms * 1_000_000 / tokens;
            Console.WriteLine($"{tokens,12:N0}{F(ms),14}{F(nsPerToken),12}");
            sb.AppendLine($"| {tokens:N0} | {F(ms)} | {F(nsPerToken)} |");
        }
        return sb.ToString();
    }
}
