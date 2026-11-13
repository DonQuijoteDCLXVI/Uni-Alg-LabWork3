using PracticalWork3.DataStructures;
using PracticalWork3.Part1;
using PracticalWork3.Part2;
using PracticalWork3.Part3;
using PracticalWork3.Part4;

namespace PracticalWork3;

/// <summary>Встроенные самопроверки (без внешних пакетов): dotnet run -- selftest.</summary>
public static class SelfTest
{
    private static int _passed, _failed;

    private static void Check(string name, Func<bool> test)
    {
        bool ok;
        try { ok = test(); }
        catch (Exception ex) { ok = false; Console.WriteLine($"   исключение: {ex.Message}"); }
        Console.WriteLine($"[{(ok ? "OK  " : "FAIL")}] {name}");
        if (ok) _passed++; else _failed++;
    }

    private static string[] Lines(Action<Action<string>> run)
    {
        var list = new List<string>();
        run(list.Add);
        return list.ToArray();
    }

    private static double Ev(string infix) => PostfixEvaluator.Evaluate(InfixToPostfix.Convert(infix));
    private static bool Near(double a, double b) => Math.Abs(a - b) < 1e-9;

    private static bool Throws<T>(Action a) where T : Exception
    {
        try { a(); return false; } catch (T) { return true; }
    }

    private static string L(MyLinkedList<int> l) => l.Join(" ");

    public static int Run()
    {
        Console.WriteLine("=== САМОПРОВЕРКА ===");

        // ---- Часть 1: стек ----
        Check("Стек: пример из задания", () =>
        {
            var o = Lines(w => StackRunner.Run(OperationParser.Parse("3 4 1,56 1,7 1,cat 2 5 4"), w));
            return o.Length == 8 && o[0].Contains("пуст") && o[1] == "isEmpty(): true"
                && o[5] == "Pop(): удалён элемент cat" && o[6].EndsWith("[7, 56]") && o[7] == "isEmpty(): false";
        });
        Check("Стек: LIFO-порядок", () =>
        {
            var s = new LinkedStack<int>();
            s.Push(1); s.Push(2); s.Push(3);
            return s.Pop() == 3 && s.Top() == 2 && s.Pop() == 2 && s.Pop() == 1 && s.IsEmpty();
        });
        Check("Стек: Pop на пустом — исключение", () => Throws<InvalidOperationException>(() => new LinkedStack<int>().Pop()));
        Check("Парсер: ошибочные токены не ломают выполнение", () =>
        {
            var o = Lines(w => StackRunner.Run(OperationParser.Parse("1 7 1,5 9 2"), w));
            return o.Length == 5 && o[0].StartsWith("Ошибка") && o[1].StartsWith("Ошибка") && o[2].StartsWith("Push(5)") && o[3].StartsWith("Ошибка") && o[4].Contains("5");
        });

        // ---- Часть 1: постфикс ----
        Check("Постфикс: 7 8 + 3 2 + / = 3 (рисунок из задания)", () => Near(PostfixEvaluator.Evaluate("7 8 + 3 2 + /"), 3));
        Check("Постфикс: все операции", () =>
            Near(PostfixEvaluator.Evaluate("2 3 ^ 1 -"), 7) &&
            Near(PostfixEvaluator.Evaluate("10 4 :"), 2.5) &&
            Near(PostfixEvaluator.Evaluate("16 sqrt 1 ln +"), 4) &&
            Near(PostfixEvaluator.Evaluate("0 cos 0 sin +"), 1) &&
            Near(PostfixEvaluator.Evaluate("5 ~ 2 *"), -10));
        Check("Постфикс: ошибки (деление на 0, нехватка операндов, ln(0), мусор)", () =>
            Throws<ExpressionException>(() => PostfixEvaluator.Evaluate("1 0 /")) &&
            Throws<ExpressionException>(() => PostfixEvaluator.Evaluate("1 +")) &&
            Throws<ExpressionException>(() => PostfixEvaluator.Evaluate("0 ln")) &&
            Throws<ExpressionException>(() => PostfixEvaluator.Evaluate("1 2")) &&
            Throws<ExpressionException>(() => PostfixEvaluator.Evaluate("1 foo")));

        // ---- Часть 1: инфикс → постфикс ----
        Check("Инфикс: 2+2 → 2 2 +", () => InfixToPostfix.Convert("2+2") == "2 2 +");
        Check("Инфикс: классический пример Дейкстры", () =>
            InfixToPostfix.Convert("3+4*2/(1-5)^2") == "3 4 2 * 1 5 - 2 ^ / +");
        Check("Инфикс: ^ правоассоциативна (2^3^2 = 512)", () => Near(Ev("2^3^2"), 512));
        Check("Инфикс: унарный минус (-2^2 = -4, 2*-3 = -6, -(1+2) = -3)", () =>
            Near(Ev("-2^2"), -4) && Near(Ev("2*-3"), -6) && Near(Ev("-(1+2)"), -3));
        Check("Инфикс: функции и скобки", () =>
            Near(Ev("sqrt(16)+ln(1)"), 4) && Near(Ev("(7+8)/(3+2)"), 3) && Near(Ev("2*(3+4)*5"), 70));
        Check("Инфикс: ошибки (скобки, пропущенный операнд, символ)", () =>
            Throws<ExpressionException>(() => InfixToPostfix.Convert("(2+3")) &&
            Throws<ExpressionException>(() => InfixToPostfix.Convert("2+3)")) &&
            Throws<ExpressionException>(() => InfixToPostfix.Convert("2+*3")) &&
            Throws<ExpressionException>(() => InfixToPostfix.Convert("2 3")) &&
            Throws<ExpressionException>(() => InfixToPostfix.Convert("2$3")) &&
            Throws<ExpressionException>(() => InfixToPostfix.Convert("2+")));

        // ---- Часть 2: очередь ----
        Check("Очередь (оба вида): FIFO", () =>
        {
            foreach (IMyQueue<int>? _ in new IMyQueue<int>?[] { null }) { }
            var a = new LinkedQueue<int>(); var b = new StdQueue<int>();
            foreach (var q in new IMyQueue<int>[] { a, b })
            {
                q.Enqueue(1); q.Enqueue(2); q.Enqueue(3);
                if (q.Peek() != 1 || q.Dequeue() != 1 || q.Dequeue() != 2 || q.Dequeue() != 3 || !q.IsEmpty()) return false;
            }
            return true;
        });
        Check("Очередь: обе реализации дают ИДЕНТИЧНЫЙ вывод на случайном наборе", () =>
        {
            var rnd = new Random(7);
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < 5000; i++) sb.Append(Benchmarks.BenchCases.Weighted_ForTests(rnd)).Append(' ');
            var ops = OperationParser.Parse(sb.ToString());
            var o1 = Lines(w => QueueRunner.Run(new LinkedQueue<string>(), ops, w));
            var o2 = Lines(w => QueueRunner.Run(new StdQueue<string>(), ops, w));
            return o1.SequenceEqual(o2) && o1.Length == 5000;
        });
        Check("Очередь: пример из задания", () =>
        {
            var o = Lines(w => QueueRunner.Run(new LinkedQueue<string>(), OperationParser.Parse("3 4 1,56 1,7 1,cat 2 5 4"), w));
            return o[5] == "Dequeue(): удалён элемент 56" && o[6].EndsWith("[7, cat]");
        });

        // ---- Часть 3 ----
        Check("Иосиф Флавий: N=7,k=3 → 3 6 2 7 5 1, остаётся 4", () =>
        {
            var (order, s) = Josephus.Solve(7, 3);
            return string.Join(" ", order) == "3 6 2 7 5 1" && s == 4;
        });
        Check("Иосиф Флавий: совпадение с формулой на 200 парах (N,k)", () =>
        {
            for (int n = 1; n <= 20; n++)
                for (int k = 1; k <= 10; k++)
                    if (Josephus.Solve(n, k).Survivor != Josephus.SurvivorByFormula(n, k)) return false;
            return true;
        });
        Check("История браузера: back/forward/сброс ветки", () =>
        {
            var h = new BrowserHistory();
            h.Visit("a"); h.Visit("b"); h.Visit("c");
            h.Back(); h.Back();                       // на a
            bool f = h.Forward() && h.Current == "b"; // вперёд на b
            h.Visit("d");                             // ветка c потеряна
            return f && h.Current == "d" && !h.Forward() && h.Back() && h.Current == "b";
        });
        Check("Round-Robin: Gantt и метрики на примере", () =>
        {
            var (t, r) = RoundRobinScheduler.Simulate(new[] { new ProcessInfo("A", 5), new ProcessInfo("B", 3) }, 2);
            // A[0-2] B[2-4] A[4-6] B[6-7] A[7-8]
            return string.Join(" ", t.Select(s => $"{s.Name}{s.Start}-{s.End}")) == "A0-2 B2-4 A4-6 B6-7 A7-8"
                && r.First(x => x.Name == "B").Completion == 7 && r.First(x => x.Name == "A").Waiting == 3;
        });
        Check("Хаффман: кодирование/декодирование и префиксность", () =>
        {
            foreach (var text in new[] { "abracadabra", "aaaa", "hello world", "а б в а б а" })
            {
                var h = new Huffman(text);
                if (h.Decode(h.Encode(text)) != text) return false;
                var codes = h.Codes.Values.ToList();
                foreach (var c1 in codes)
                    foreach (var c2 in codes)
                        if (!ReferenceEquals(c1, c2) && c1 != c2 && c2.StartsWith(c1)) return false;
            }
            return new Huffman("abracadabra").Encode("abracadabra").Length == 23;   // оптимум для этой строки
        });

        // ---- Часть 4: списки ----
        Check("Список: Reverse", () =>
        {
            var l = ListAlgorithms.ParseList("1 2 3 4 5"); ListAlgorithms.Reverse(l);
            var e = new MyLinkedList<int>(); ListAlgorithms.Reverse(e);
            var one = ListAlgorithms.ParseList("9"); ListAlgorithms.Reverse(one);
            return L(l) == "5 4 3 2 1" && Demos.Verify(l).Contains("OK") && e.IsEmpty && L(one) == "9";
        });
        Check("Список: MoveLastToFront / MoveFirstToBack", () =>
        {
            var a = ListAlgorithms.ParseList("1 2 3 4"); ListAlgorithms.MoveLastToFront(a);
            var b = ListAlgorithms.ParseList("1 2 3 4"); ListAlgorithms.MoveFirstToBack(b);
            var c = ListAlgorithms.ParseList("7"); ListAlgorithms.MoveLastToFront(c); ListAlgorithms.MoveFirstToBack(c);
            return L(a) == "4 1 2 3" && L(b) == "2 3 4 1" && L(c) == "7"
                && Demos.Verify(a).Contains("OK") && Demos.Verify(b).Contains("OK")
                && Throws<InvalidOperationException>(() => ListAlgorithms.MoveLastToFront(new MyLinkedList<int>()));
        });
        Check("Список: CountDistinct", () =>
            ListAlgorithms.CountDistinct(ListAlgorithms.ParseList("5 3 8 3 1 5 9")) == 5 &&
            ListAlgorithms.CountDistinct(new MyLinkedList<int>()) == 0);
        Check("Список: RemoveNonUnique / RemoveDuplicatesKeepFirst", () =>
        {
            var a = ListAlgorithms.ParseList("5 3 8 3 1 5 9 3 7"); ListAlgorithms.RemoveNonUnique(a);
            var b = ListAlgorithms.ParseList("5 3 8 3 1 5 9 3 7"); ListAlgorithms.RemoveDuplicatesKeepFirst(b);
            var c = ListAlgorithms.ParseList("2 2 2"); ListAlgorithms.RemoveNonUnique(c);
            return L(a) == "8 1 9 7" && L(b) == "5 3 8 1 9 7" && c.IsEmpty && c.Tail is null && c.Count == 0
                && Demos.Verify(a).Contains("OK") && Demos.Verify(b).Contains("OK");
        });

        // ---- Часть 4: дерево ----
        Check("Дерево с рисунка: обходы, характеристики, сериализация", () =>
        {
            var t = BinaryTree.FromPreorder("ABD*G***CE**FH**J**");
            return t.PreOrder() == "ABDGCEFHJ" && t.InOrder() == "DGBAECHFJ" && t.PostOrder() == "GDBEHJFCA"
                && t.LevelOrder() == "ABCDEFGHJ" && t.CountNodes() == 9 && t.CountLeaves() == 4 && t.Height() == 4
                && t.Serialize() == "ABD*G***CE**FH**J**"
                && t.Root!.Left!.Right is null && t.Root.Left.Left!.Left is null && t.Root.Left.Left.Right!.Value == 'G';
        });
        Check("Дерево: некорректные строки отвергаются", () =>
            Throws<FormatException>(() => BinaryTree.FromPreorder("AB*")) &&
            Throws<FormatException>(() => BinaryTree.FromPreorder("A**B")) &&
            Throws<FormatException>(() => BinaryTree.FromPreorder("")));

        Console.WriteLine($"\nИтого: успешно {_passed}, провалено {_failed}");
        return _failed == 0 ? 0 : 1;
    }
}
