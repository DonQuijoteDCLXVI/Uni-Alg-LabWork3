using PracticalWork3.Benchmarks;
using PracticalWork3.DataStructures;
using PracticalWork3.Infrastructure;
using PracticalWork3.Part1;
using PracticalWork3.Part2;
using PracticalWork3.Part3;
using PracticalWork3.Part4;

namespace PracticalWork3;

/// <summary>Консольные сценарии для каждой части задания.</summary>
public static class Demos
{
    private static IEnumerable<string> ReadLines(string path) =>
        File.ReadAllLines(path)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0 && !l.StartsWith('#'));

    // ------------------------------ Часть 1 ------------------------------

    public static void StackFromFile(string file)
    {
        string path = DataPaths.Resolve(file);
        ConsoleIO.Header($"ЧАСТЬ 1.1. Стек на собственном списке. Файл: {path}");
        Console.WriteLine("Вход: " + File.ReadAllText(path).Trim());
        Console.WriteLine();
        var ops = OperationParser.ReadFile(path);
        StackRunner.Run(ops, Console.WriteLine);
    }

    public static void BenchStack()
    {
        ConsoleIO.Header("ЧАСТЬ 1.2. Замеры времени: стек (медиана запусков, вывод отключён)");
        string table = BenchmarkRunner.RunStack();
        File.WriteAllText("benchmark_stack.md", table);
        Console.WriteLine("\nТаблица сохранена в benchmark_stack.md");
    }

    public static void Postfix(string file, bool interactive)
    {
        string path = DataPaths.Resolve(file);
        ConsoleIO.Header($"ЧАСТЬ 1.3. Вычисление постфиксной записи. Файл: {path}");
        foreach (var line in ReadLines(path))
            EvaluateAndPrint(line);

        if (interactive)
        {
            Console.Write("\nВведите своё постфиксное выражение (Enter — пропустить): ");
            var own = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(own)) EvaluateAndPrint(own);
        }
    }

    private static void EvaluateAndPrint(string expr)
    {
        Console.WriteLine($"\nВыражение: {expr}");
        try
        {
            double result = PostfixEvaluator.Evaluate(expr, Console.WriteLine);
            Console.WriteLine($"  Результат = {PostfixEvaluator.Format(result)}");
        }
        catch (ExpressionException ex)
        {
            ConsoleIO.Error(ex.Message);
        }
    }

    public static void Infix(string file, bool interactive)
    {
        string path = DataPaths.Resolve(file);
        ConsoleIO.Header($"ЧАСТЬ 1.4. Инфиксная запись → постфиксная. Файл: {path}");
        foreach (var line in ReadLines(path))
            ConvertAndPrint(line);

        if (interactive)
        {
            Console.Write("\nВведите своё инфиксное выражение (Enter — пропустить): ");
            var own = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(own)) ConvertAndPrint(own);
        }
    }

    private static void ConvertAndPrint(string infix)
    {
        Console.WriteLine($"\nИнфиксная запись:   {infix}");
        try
        {
            string postfix = InfixToPostfix.Convert(infix, Console.WriteLine);
            Console.WriteLine($"Постфиксная запись: {postfix}");
            double value = PostfixEvaluator.Evaluate(postfix);
            Console.WriteLine($"Проверка вычислением: {PostfixEvaluator.Format(value)}");
        }
        catch (ExpressionException ex)
        {
            ConsoleIO.Error(ex.Message);
        }
    }

    public static void BenchPostfix()
    {
        ConsoleIO.Header("ЧАСТЬ 1.3 (оценка сложности). Эмпирическая проверка O(n) для постфиксных вычислений");
        string table = BenchmarkRunner.RunPostfix();
        File.WriteAllText("benchmark_postfix.md", table);
        Console.WriteLine("\nТаблица сохранена в benchmark_postfix.md");
    }

    // ------------------------------ Часть 2 ------------------------------

    public static void QueueFromFile(string file)
    {
        string path = DataPaths.Resolve(file);
        ConsoleIO.Header($"ЧАСТЬ 2.1. Очередь. Файл: {path}");
        Console.WriteLine("Вход: " + File.ReadAllText(path).Trim());
        var ops = OperationParser.ReadFile(path);

        foreach (IMyQueue<string> q in new IMyQueue<string>[] { new LinkedQueue<string>(), new StdQueue<string>() })
        {
            Console.WriteLine($"\n--- {q.Name} ---");
            QueueRunner.Run(q, ops, Console.WriteLine);
        }
    }

    public static void BenchQueue()
    {
        ConsoleIO.Header("ЧАСТЬ 2.2. Замеры времени: очередь на списке и стандартный Queue<T>");
        string table = BenchmarkRunner.RunQueue();
        File.WriteAllText("benchmark_queue.md", table);
        Console.WriteLine("\nТаблица сохранена в benchmark_queue.md");
    }

    // ------------------------------ Часть 3 ------------------------------

    public static void Josephus(bool interactive)
    {
        ConsoleIO.Header("ЧАСТЬ 3 (Список). Задача Иосифа Флавия на циклическом списке");
        int n = 7, k = 3;
        if (interactive)
        {
            int.TryParse(ConsoleIO.Prompt("Число людей N", "7"), out n);
            int.TryParse(ConsoleIO.Prompt("Каждый k-й выбывает, k", "3"), out k);
        }
        var (order, survivor) = Part3.Josephus.Solve(n, k);
        Console.WriteLine($"N = {n}, k = {k}");
        Console.WriteLine("Порядок выбывания: " + string.Join(" → ", order));
        Console.WriteLine($"Остался номер: {survivor} (проверка по формуле: {Part3.Josephus.SurvivorByFormula(n, k)})");
    }

    public static void History(bool interactive)
    {
        ConsoleIO.Header("ЧАСТЬ 3 (Стек). История браузера: «Назад» / «Вперёд» на двух стеках");
        var h = new BrowserHistory();
        void Do(string cmd)
        {
            var parts = cmd.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            string result = parts[0].ToLowerInvariant() switch
            {
                "visit" when parts.Length == 2 => Visit(parts[1]),
                "back" => h.Back() ? "ok" : "нельзя: стек «назад» пуст",
                "forward" => h.Forward() ? "ok" : "нельзя: стек «вперёд» пуст",
                _ => "неизвестная команда (visit <url> | back | forward | exit)",
            };
            Console.WriteLine($"> {cmd,-22} {result}\n    {h.State()}");
        }
        string Visit(string url) { h.Visit(url); return "ok"; }

        foreach (var cmd in new[]
        {
            "visit google.com", "visit wikipedia.org", "visit github.com",
            "back", "back", "forward", "visit stackoverflow.com", "forward", "back", "back", "back",
        })
            Do(cmd);

        if (!interactive) return;
        Console.WriteLine("\nВводите свои команды (visit <url>, back, forward; exit — выход):");
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line is null || line.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
            if (line.Trim().Length > 0) Do(line.Trim());
        }
    }

    public static void Scheduler(string file)
    {
        string path = DataPaths.Resolve(file);
        ConsoleIO.Header($"ЧАСТЬ 3 (Очередь). Планировщик Round-Robin. Файл: {path}");
        var lines = ReadLines(path).ToList();
        int quantum = int.Parse(lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries)[^1]);
        var procs = lines.Skip(1).Select(l =>
        {
            var p = l.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return new ProcessInfo(p[0], int.Parse(p[1]));
        }).ToList();

        Console.WriteLine($"Квант времени: {quantum}");
        Console.WriteLine("Процессы: " + string.Join(", ", procs.Select(p => $"{p.Name}({p.Burst})")));
        var (timeline, results) = RoundRobinScheduler.Simulate(procs, quantum);

        Console.WriteLine("\nДиаграмма Ганта:");
        Console.WriteLine("  " + string.Join(" ", timeline.Select(s => $"{s.Name}[{s.Start}-{s.End}]")));
        Console.WriteLine("\n  Процесс  Время  Завершение  Оборот  Ожидание");
        foreach (var r in results.OrderBy(r => r.Name))
            Console.WriteLine($"  {r.Name,-8}{r.Burst,5}{r.Completion,12}{r.Turnaround,8}{r.Waiting,10}");
        Console.WriteLine($"\n  Среднее время ожидания: {results.Average(r => r.Waiting):0.00}");
    }

    public static void HuffmanDemo(bool interactive)
    {
        ConsoleIO.Header("ЧАСТЬ 3 (Дерево). Кодирование Хаффмана");
        string text = interactive ? ConsoleIO.Prompt("Текст для кодирования", "abracadabra") : "abracadabra";
        var h = new Huffman(text);

        Console.WriteLine("Символ  Частота  Код");
        foreach (var kv in h.Frequencies.OrderByDescending(k => k.Value).ThenBy(k => k.Key))
            Console.WriteLine($"  '{kv.Key}'{kv.Value,8}   {h.Codes[kv.Key]}");

        string bits = h.Encode(text);
        string back = h.Decode(bits);
        Console.WriteLine($"\nЗакодировано: {bits}");
        Console.WriteLine($"Бит: {bits.Length} против {text.Length * 8} (8 бит/символ) — сжатие в {(double)text.Length * 8 / bits.Length:0.00} раза");
        Console.WriteLine($"Декодировано: {back} — {(back == text ? "совпало с исходным" : "ОШИБКА")}");
    }

    // ------------------------------ Часть 4 ------------------------------

    public static void Lists(string file)
    {
        string path = DataPaths.Resolve(file);
        ConsoleIO.Header($"ЧАСТЬ 4.1. Алгоритмы над собственным списком. Файл: {path}");
        var original = ListAlgorithms.ParseList(File.ReadAllText(path));
        Console.WriteLine("Исходный список L: " + original);

        void Show(string title, Action<MyLinkedList<int>> action)
        {
            var copy = original.Clone();
            action(copy);
            Console.WriteLine($"{title,-44} {copy}   {Verify(copy)}");
        }

        Show("1. Reverse (переворот)", ListAlgorithms.Reverse);
        Show("2а. MoveLastToFront (последний → в начало)", ListAlgorithms.MoveLastToFront);
        Show("2б. MoveFirstToBack (первый → в конец)", ListAlgorithms.MoveFirstToBack);
        Console.WriteLine($"{"3. CountDistinct (различных элементов)",-44} {ListAlgorithms.CountDistinct(original)}");
        Show("4. RemoveNonUnique (убрать все повторяющиеся)", ListAlgorithms.RemoveNonUnique);
        Show("4*. RemoveDuplicatesKeepFirst (вариант)", ListAlgorithms.RemoveDuplicatesKeepFirst);
        Console.WriteLine("\n(в скобках — проверка согласованности Head/Tail/Count после перешивания ссылок)");
    }

    /// <summary>Проверяет, что Count и Tail совпадают с реальным содержимым цепочки.</summary>
    public static string Verify(MyLinkedList<int> list)
    {
        int cnt = 0;
        ListNode<int>? last = null;
        for (var cur = list.Head; cur is not null; cur = cur.Next) { cnt++; last = cur; }
        bool ok = cnt == list.Count && ReferenceEquals(last, list.Tail);
        return ok ? "[Head/Tail/Count: OK]" : "[НАРУШЕНА согласованность!]";
    }

    public static void Tree(string? preorder)
    {
        ConsoleIO.Header("ЧАСТЬ 4.2. Бинарное дерево с рисунка");
        string s = preorder ?? ReadLines(DataPaths.Resolve("tree.txt")).First();
        Console.WriteLine($"Строка прямого обхода (* = NULL): {s}");

        var tree = BinaryTree.FromPreorder(s);
        Console.WriteLine("\nУзлы (как на рисунке: левая и правая ссылки):");
        Console.Write(tree.NodeTable());
        Console.WriteLine("Дерево (повёрнуто на 90°: правое поддерево сверху, левое снизу):\n");
        Console.Write(tree.DrawSideways());

        Console.WriteLine($"\nПрямой обход  (корень-лево-право): {tree.PreOrder()}");
        Console.WriteLine($"Симметричный  (лево-корень-право): {tree.InOrder()}");
        Console.WriteLine($"Обратный      (лево-право-корень): {tree.PostOrder()}");
        Console.WriteLine($"По уровням    (в ширину):          {tree.LevelOrder()}");
        Console.WriteLine($"\nУзлов: {tree.CountNodes()}, листьев: {tree.CountLeaves()}, высота: {tree.Height()}");
        string back = tree.Serialize();
        Console.WriteLine($"Обратная сериализация: {back} — {(back == s.Replace(" ", "") ? "совпала с исходной строкой" : "НЕ СОВПАЛА")}");
    }
}
