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
        
        // ---- Часть 4: пункты 5–8 ----
        Check("Список п.5: вставка списка в себя после первого x", () =>
        {
            var a = ListAlgorithms.ParseList("1 2 3 4"); bool fa = ListAlgorithms.InsertSelfAfterFirst(a, 2);
            var b = ListAlgorithms.ParseList("1 2 3 4"); bool fb = ListAlgorithms.InsertSelfAfterFirst(b, 4);   // x — хвост
            var c = ListAlgorithms.ParseList("1 2 3 4"); bool fc = ListAlgorithms.InsertSelfAfterFirst(c, 1);   // x — голова
            var d = ListAlgorithms.ParseList("1 2 3");   bool fd = ListAlgorithms.InsertSelfAfterFirst(d, 9);   // x нет
            var e = ListAlgorithms.ParseList("7");       bool fe = ListAlgorithms.InsertSelfAfterFirst(e, 7);
            var f = ListAlgorithms.ParseList("2 5 2");   ListAlgorithms.InsertSelfAfterFirst(f, 2);             // первое вхождение
            var g = new MyLinkedList<int>();             bool fg = ListAlgorithms.InsertSelfAfterFirst(g, 1);
            return fa && L(a) == "1 2 1 2 3 4 3 4" && a.Count == 8
                && fb && L(b) == "1 2 3 4 1 2 3 4" && b.Count == 8
                && fc && L(c) == "1 1 2 3 4 2 3 4" && c.Count == 8
                && !fd && L(d) == "1 2 3" && d.Count == 3
                && fe && L(e) == "7 7" && e.Count == 2
                && L(f) == "2 2 5 2 5 2" && !fg && g.IsEmpty
                && new[] { a, b, c, d, e, f, g }.All(x => Demos.Verify(x).Contains("OK"));
        });
        Check("Список п.6: вставка в упорядоченный список", () =>
        {
            string Ins(string src, int v) { var l = ListAlgorithms.ParseList(src); ListAlgorithms.InsertSorted(l, v);
                                            return L(l) + (Demos.Verify(l).Contains("OK") ? "" : "!"); }
            return Ins("1 3 5", 0) == "0 1 3 5"          // в начало
                && Ins("1 3 5", 9) == "1 3 5 9"          // в конец
                && Ins("1 3 5", 4) == "1 3 4 5"          // в середину
                && Ins("1 3 3 5", 3) == "1 3 3 3 5"      // равные значения
                && Ins("1 3 5", 5) == "1 3 5 5"
                && Ins("4", 2) == "2 4" && Ins("4", 4) == "4 4" && Ins("4", 8) == "4 8"
                && Throws<InvalidOperationException>(() => ListAlgorithms.InsertSorted(new MyLinkedList<int>(), 1));
        });
        Check("Список п.7: удаление всех элементов E", () =>
        {
            var a = ListAlgorithms.ParseList("3 1 3 3 2 3"); int ra = ListAlgorithms.RemoveAll(a, 3);   // голова, середина, хвост
            var b = ListAlgorithms.ParseList("2 2 2");       int rb = ListAlgorithms.RemoveAll(b, 2);   // всё
            var c = ListAlgorithms.ParseList("1 2 3");       int rc = ListAlgorithms.RemoveAll(c, 9);   // нет такого
            var d = new MyLinkedList<int>();                 int rd = ListAlgorithms.RemoveAll(d, 1);
            return L(a) == "1 2" && ra == 4 && a.Count == 2
                && b.IsEmpty && rb == 3 && b.Tail is null && b.Count == 0
                && L(c) == "1 2 3" && rc == 0 && rd == 0
                && Demos.Verify(a).Contains("OK") && Demos.Verify(b).Contains("OK") && Demos.Verify(c).Contains("OK");
        });
        Check("Список п.8: вставка F перед первым вхождением E", () =>
        {
            string Ins(string src, int e, int f, out bool ok) { var l = ListAlgorithms.ParseList(src);
                ok = ListAlgorithms.InsertBeforeFirst(l, e, f);
                return L(l) + (Demos.Verify(l).Contains("OK") ? "" : "!"); }
            string r1 = Ins("1 2 3 2", 2, 9, out bool o1);   // середина, первое из двух
            string r2 = Ins("2 1", 2, 9, out bool o2);       // голова
            string r3 = Ins("1 2", 2, 9, out bool o3);       // хвост
            string r4 = Ins("1 2", 5, 9, out bool o4);       // E нет
            string r5 = Ins("", 5, 9, out bool o5);          // пустой
            return o1 && r1 == "1 9 2 3 2" && o2 && r2 == "9 2 1" && o3 && r3 == "1 9 2"
                && !o4 && r4 == "1 2" && !o5 && r5 == "";
        });
        Check("Визуализатор: Execute() — подсветка и ошибки", () =>
        {
            var r5 = Visual.ListOps.Execute(new Visual.RunRequest(new[] { 1, 2, 3 }, "insertSelf", 2, null, null));
            var r6 = Visual.ListOps.Execute(new Visual.RunRequest(new[] { 1, 3, 5 }, "insertSorted", null, 4, null));
            var r7 = Visual.ListOps.Execute(new Visual.RunRequest(new[] { 3, 1, 3 }, "removeAll", null, 3, null));
            var r8 = Visual.ListOps.Execute(new Visual.RunRequest(new[] { 1, 2 }, "insertBefore", null, 2, 9));
            return r5.After.SequenceEqual(new[] { 1, 2, 1, 2, 3, 3 }) && r5.AfterHl.SequenceEqual(new[] { 2, 3, 4 }) && r5.BeforeHl.SequenceEqual(new[] { 1 })
                && r6.After.SequenceEqual(new[] { 1, 3, 4, 5 }) && r6.AfterHl.SequenceEqual(new[] { 2 })
                && r7.After.SequenceEqual(new[] { 1 }) && r7.BeforeHl.SequenceEqual(new[] { 0, 2 })
                && r8.After.SequenceEqual(new[] { 1, 9, 2 }) && r8.AfterHl.SequenceEqual(new[] { 1 })
                && r5.Consistent && r6.Consistent && r7.Consistent && r8.Consistent
                && Throws<ArgumentException>(() => Visual.ListOps.Execute(new Visual.RunRequest(new[] { 3, 1 }, "insertSorted", null, 2, null)))
                && Throws<ArgumentException>(() => Visual.ListOps.Execute(new Visual.RunRequest(new[] { 1 }, "insertSelf", null, null, null)));
        });
        Check("Визуализатор: все операции пп. 1–12 через Execute()", () =>
        {
            Visual.RunResult Run(string op, string list, int? x = null, int? e = null, int? f = null, string? other = null, int? i = null, int? j = null)
            {
                int[] P(string t) => t.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                return Visual.ListOps.Execute(new Visual.RunRequest(P(list), op, x, e, f, other is null ? null : P(other), i, j));
            }
            string J(int[]? a) => a is null ? "null" : string.Join(" ", a);
            var rev = Run("reverse", "1 2 3");
            var ml = Run("moveLast", "1 2 3");
            var mf = Run("moveFirst", "1 2 3");
            var di = Run("distinct", "5 3 5 1 3");
            var nu = Run("nonUnique", "5 3 8 3 1 5 9 3 7");
            var kf = Run("keepFirst", "5 3 8 3 1 5 9 3 7");
            var cc = Run("concat", "1 2", other: "3 4 5");
            var sp = Run("split", "1 2 3 4 5", x: 3);
            var sp2 = Run("split", "1 2 3", x: 9);
            var db = Run("double", "1 2 3");
            var sw = Run("swap", "1 2 3 4 5", i: 0, j: 4);
            return J(rev.After) == "3 2 1" && J(ml.After) == "3 1 2" && J(mf.After) == "2 3 1"
                && J(di.After) == "5 3 5 1 3" && J(di.BeforeHl) == "0 1 3" && di.Message.Contains("3")
                && J(nu.After) == "8 1 9 7" && J(kf.After) == "5 3 8 1 9 7"
                && J(cc.After) == "1 2 3 4 5" && J(cc.Input2) == "3 4 5" && J(cc.AfterHl) == "2 3 4"
                && J(sp.After) == "1 2" && J(sp.Second) == "4 5" && J(sp.BeforeHl) == "2"
                && J(sp2.After) == "1 2 3" && J(sp2.Second) == ""
                && J(db.After) == "1 2 3 1 2 3" && J(db.AfterHl) == "3 4 5"
                && J(sw.After) == "5 2 3 4 1"
                && new[] { rev, ml, mf, di, nu, kf, cc, sp, sp2, db, sw }.All(x => x.Consistent)
                && Throws<ArgumentException>(() => Run("swap", "1 2", i: 0, j: 2))
                && Throws<InvalidOperationException>(() => Run("moveLast", ""));
        });
        // ---- Часть 4: пункты 9–12 ----
        Check("Список: Concat (дописать E в конец L)", () =>
        {
            var l = ListAlgorithms.ParseList("1 2 3");
            var e = ListAlgorithms.ParseList("4 5");
            ListAlgorithms.Concat(l, e);
            // E после слияния обнуляется
            var emptyL = new MyLinkedList<int>();
            var e2 = ListAlgorithms.ParseList("7 8");
            ListAlgorithms.Concat(emptyL, e2);
            // самослияние: Concat(L, L) не должно зацикливаться
            var self = ListAlgorithms.ParseList("1 2");
            ListAlgorithms.Concat(self, self);
            return L(l) == "1 2 3 4 5" && l.Count == 5 && e.IsEmpty && e.Count == 0
                && L(emptyL) == "7 8" && L(self) == "1 2 1 2"
                && Demos.Verify(l).Contains("OK") && Demos.Verify(emptyL).Contains("OK")
                && Demos.Verify(self).Contains("OK");
        });
        Check("Список: SplitAtFirst (разбиение по первому вхождению)", () =>
        {
            // число в середине
            var (f1, s1) = ListAlgorithms.SplitAtFirst(ListAlgorithms.ParseList("1 2 3 4 5"), 3);
            // числа нет: first = весь список, second пуст
            var (f2, s2) = ListAlgorithms.SplitAtFirst(ListAlgorithms.ParseList("1 2 3"), 9);
            // число — первый элемент: first пуст
            var (f3, s3) = ListAlgorithms.SplitAtFirst(ListAlgorithms.ParseList("3 4 5"), 3);
            // число — последний элемент: second пуст
            var (f4, s4) = ListAlgorithms.SplitAtFirst(ListAlgorithms.ParseList("1 2 3"), 3);
            return L(f1) == "1 2" && L(s1) == "4 5"
                && L(f2) == "1 2 3" && s2.IsEmpty
                && f3.IsEmpty && L(s3) == "4 5"
                && L(f4) == "1 2" && s4.IsEmpty
                && Demos.Verify(f1).Contains("OK") && Demos.Verify(s1).Contains("OK")
                && Demos.Verify(f2).Contains("OK") && Demos.Verify(f3).Contains("OK")
                && Demos.Verify(f4).Contains("OK");
        });
        Check("Список: DoubleSelf (удвоение)", () =>
        {
            var a = ListAlgorithms.ParseList("1 2 3"); ListAlgorithms.DoubleSelf(a);
            var b = ListAlgorithms.ParseList("7"); ListAlgorithms.DoubleSelf(b);
            var c = new MyLinkedList<int>(); ListAlgorithms.DoubleSelf(c);
            return L(a) == "1 2 3 1 2 3" && a.Count == 6
                && L(b) == "7 7" && b.Count == 2
                && c.IsEmpty && c.Count == 0
                && Demos.Verify(a).Contains("OK") && Demos.Verify(b).Contains("OK");
        });
        Check("Список: SwapElements (обмен по индексам)", () =>
        {
            var a = ListAlgorithms.ParseList("1 2 3 4 5"); ListAlgorithms.SwapElements(a, 0, 4);
            var b = ListAlgorithms.ParseList("1 2 3 4 5"); ListAlgorithms.SwapElements(b, 1, 3);
            var c = ListAlgorithms.ParseList("1 2 3"); ListAlgorithms.SwapElements(c, 1, 1); // тот же индекс — без изменений
            return L(a) == "5 2 3 4 1" && L(b) == "1 4 3 2 5" && L(c) == "1 2 3"
                && Demos.Verify(a).Contains("OK") && Demos.Verify(b).Contains("OK")
                && Throws<ArgumentOutOfRangeException>(() => ListAlgorithms.SwapElements(ListAlgorithms.ParseList("1 2"), 0, 2))
                && Throws<ArgumentOutOfRangeException>(() => ListAlgorithms.SwapElements(ListAlgorithms.ParseList("1 2"), -1, 0));
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
