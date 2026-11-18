using PracticalWork3.DataStructures;
using PracticalWork3.Part4;

namespace PracticalWork3.Visual;

/// <summary>
/// Запрос от окна: исходный список L, ключ операции и её параметры.
/// X — искомое число (пп. 5, 10); E, F — числа (пп. 6, 7, 8); Other — второй СПИСОК E (п. 9);
/// I, J — индексы (п. 12).
/// </summary>
public sealed record RunRequest(int[]? List, string? Op, int? X, int? E, int? F,
                                int[]? Other = null, int? I = null, int? J = null);

/// <summary>
/// Ответ окну. Before/After — список до и после (для п. 10 After — первая часть, Second — вторая;
/// для п. 9 Input2 — дописываемый список E). *Hl — индексы узлов для подсветки.
/// </summary>
public sealed record RunResult(
    int[] Before, int[] After, int[] BeforeHl, int[] AfterHl,
    string Message, bool Changed, bool Consistent,
    int[]? Input2 = null, int[]? Second = null);

/// <summary>Описание операции для интерфейса: ключ + название по заданию.</summary>
public static class OpKeys
{
    public const string Reverse = "reverse", MoveLast = "moveLast", MoveFirst = "moveFirst",
        Distinct = "distinct", NonUnique = "nonUnique", KeepFirst = "keepFirst",
        InsertSelf = "insertSelf", InsertSorted = "insertSorted", RemoveAll = "removeAll", InsertBefore = "insertBefore",
        Concat = "concat", Split = "split", Double = "double", Swap = "swap";
}

/// <summary>
/// Прослойка между окном (GUI) и алгоритмами: выполняет операцию над копией списка
/// и готовит данные для отрисовки (какие узлы подсвечивать). Сама ничего не рисует.
/// </summary>
public static class ListOps
{
    public const int MaxLength = 40;

    public static RunResult Execute(RunRequest r)
    {
        if (r.List is null) throw new ArgumentException("Список не задан.");
        if (r.List.Length > MaxLength) throw new ArgumentException($"Слишком длинный список (максимум {MaxLength} элементов).");

        var list = MyLinkedList<int>.FromEnumerable(r.List);
        int[] before = list.ToArray();
        int n = before.Length;
        int[] beforeHl = Array.Empty<int>(), afterHl = Array.Empty<int>();
        int[]? input2 = null, second = null;
        var main = list;                       // список, который показываем как «после»
        MyLinkedList<int>? secondList = null;
        string msg;
        bool changed = true;

        switch (r.Op)
        {
            // ---------------- пп. 1–4 ----------------
            case OpKeys.Reverse:
                ListAlgorithms.Reverse(list);
                changed = n > 1;
                msg = n == 0 ? "Список пуст — переворачивать нечего."
                    : "Все ссылки развёрнуты в обратную сторону: бывший хвост стал головой, бывшая голова — хвостом.";
                break;

            case OpKeys.MoveLast:
                ListAlgorithms.MoveLastToFront(list);          // для пустого — InvalidOperationException
                changed = n > 1;
                if (n > 1) { beforeHl = new[] { n - 1 }; afterHl = new[] { 0 }; }
                msg = n > 1 ? $"Последний элемент ({before[^1]}) перенесён в начало списка."
                            : "В списке один элемент — менять нечего.";
                break;

            case OpKeys.MoveFirst:
                ListAlgorithms.MoveFirstToBack(list);
                changed = n > 1;
                if (n > 1) { beforeHl = new[] { 0 }; afterHl = new[] { n - 1 }; }
                msg = n > 1 ? $"Первый элемент ({before[0]}) перенесён в конец списка."
                            : "В списке один элемент — менять нечего.";
                break;

            case OpKeys.Distinct:
            {
                int k = ListAlgorithms.CountDistinct(list);
                var seen = new HashSet<int>();
                var firsts = new List<int>();
                var values = new List<int>();
                for (int i = 0; i < n; i++)
                    if (seen.Add(before[i])) { firsts.Add(i); values.Add(before[i]); }
                beforeHl = firsts.ToArray();
                changed = false;
                msg = $"Различных элементов: {k}" + (n > 0 ? $"  (значения: {string.Join(", ", values)}). Жёлтым отмечено первое вхождение каждого." : ".");
                break;
            }

            case OpKeys.NonUnique:
            {
                var freq = before.GroupBy(v => v).ToDictionary(g => g.Key, g => g.Count());
                beforeHl = Enumerable.Range(0, n).Where(i => freq[before[i]] > 1).ToArray();
                ListAlgorithms.RemoveNonUnique(list);
                changed = beforeHl.Length > 0;
                msg = changed
                    ? $"Удалены все значения, которые встречались больше одного раза ({beforeHl.Length} узл.). Остались только уникальные."
                    : "Повторяющихся значений нет — список не изменился.";
                break;
            }

            case OpKeys.KeepFirst:
            {
                var seen = new HashSet<int>();
                beforeHl = Enumerable.Range(0, n).Where(i => !seen.Add(before[i])).ToArray();
                ListAlgorithms.RemoveDuplicatesKeepFirst(list);
                changed = beforeHl.Length > 0;
                msg = changed
                    ? $"Оставлено по одному (первому) вхождению каждого значения; лишних узлов удалено: {beforeHl.Length}."
                    : "Повторов нет — список не изменился.";
                break;
            }

            // ---------------- пп. 5–8 ----------------
            case OpKeys.InsertSelf:
            {
                int x = Need(r.X, "x");
                int idx = Array.IndexOf(before, x);
                if (!ListAlgorithms.InsertSelfAfterFirst(list, x))
                {
                    msg = $"Число {x} в списке не найдено — список не изменился.";
                    changed = false;
                    break;
                }
                beforeHl = new[] { idx };
                afterHl = Enumerable.Range(idx + 1, n).ToArray();
                msg = $"Первое вхождение {x} — позиция {idx}. Сразу после него вставлена копия всего списка ({n} эл.).";
                break;
            }

            case OpKeys.InsertSorted:
            {
                int e = Need(r.E, "E");
                if (n == 0) throw new ArgumentException("Список должен быть непустым.");
                for (int i = 1; i < n; i++)
                    if (before[i] < before[i - 1])
                        throw new ArgumentException("Для этой операции L должен быть упорядочен по неубыванию, например: 1 3 3 5 8 (поможет кнопка «Отсортировать»).");
                int pos = before.Count(v => v <= e);
                ListAlgorithms.InsertSorted(list, e);
                afterHl = new[] { pos };
                msg = $"Элемент {e} вставлен на позицию {pos} — упорядоченность сохранена.";
                break;
            }

            case OpKeys.RemoveAll:
            {
                int e = Need(r.E, "E");
                beforeHl = Enumerable.Range(0, n).Where(i => before[i] == e).ToArray();
                int removed = ListAlgorithms.RemoveAll(list, e);
                changed = removed > 0;
                msg = removed > 0 ? $"Удалено элементов {e}: {removed}." : $"Элементов {e} в списке нет — список не изменился.";
                break;
            }

            case OpKeys.InsertBefore:
            {
                int e = Need(r.E, "E"), f = Need(r.F, "F");
                int idx = Array.IndexOf(before, e);
                if (!ListAlgorithms.InsertBeforeFirst(list, e, f))
                {
                    msg = $"Элемент {e} в списке не найден — вставка не выполнена.";
                    changed = false;
                    break;
                }
                beforeHl = new[] { idx };
                afterHl = new[] { idx };
                msg = $"Первое вхождение {e} — позиция {idx}. Прямо перед ним вставлен {f}.";
                break;
            }

            // ---------------- пп. 9–12 ----------------
            case OpKeys.Concat:
            {
                int[] other = r.Other ?? Array.Empty<int>();
                if (other.Length > MaxLength) throw new ArgumentException($"Список E слишком длинный (максимум {MaxLength} элементов).");
                var e = MyLinkedList<int>.FromEnumerable(other);
                ListAlgorithms.Concat(list, e);
                input2 = other;
                afterHl = Enumerable.Range(n, other.Length).ToArray();
                changed = other.Length > 0;
                msg = other.Length > 0
                    ? $"Узлы списка E ({other.Length} эл.) прицеплены к концу L без копирования. Сам список E после этого пуст."
                    : "Список E пуст — L не изменился.";
                break;
            }

            case OpKeys.Split:
            {
                int x = Need(r.X, "x");
                int idx = Array.IndexOf(before, x);
                var (first, sec) = ListAlgorithms.SplitAtFirst(list, x);
                main = first;
                secondList = sec;
                second = sec.ToArray();
                if (idx >= 0)
                {
                    beforeHl = new[] { idx };
                    msg = $"Первое вхождение {x} — позиция {idx}. Всё до него — первая часть, всё после — вторая; само {x} ни в одну часть не попало. Исходный L опустел: его узлы разошлись по частям.";
                }
                else
                {
                    msg = $"Числа {x} в списке нет — первая часть это весь список, вторая пуста.";
                }
                break;
            }

            case OpKeys.Double:
                ListAlgorithms.DoubleSelf(list);
                afterHl = Enumerable.Range(n, n).ToArray();
                changed = n > 0;
                msg = n > 0 ? $"К списку приписана его копия: {n} → {2 * n} элементов (зелёным — добавленные)."
                            : "Список пуст — удваивать нечего.";
                break;

            case OpKeys.Swap:
            {
                int i = Need(r.I, "i"), j = Need(r.J, "j");
                if (n == 0) throw new ArgumentException("Список пуст — менять местами нечего.");
                if (i < 0 || i >= n || j < 0 || j >= n)
                    throw new ArgumentException($"Индексы должны быть от 0 до {n - 1} (счёт с нуля: первый элемент имеет индекс 0).");
                ListAlgorithms.SwapElements(list, i, j);
                changed = i != j;
                beforeHl = i == j ? new[] { i } : new[] { i, j };
                afterHl = beforeHl;
                msg = i == j ? "Индексы совпали — список не изменился."
                             : $"Элементы на позициях {i} и {j} ({before[i]} и {before[j]}) поменялись местами.";
                break;
            }

            default:
                throw new ArgumentException("Неизвестная операция.");
        }

        bool consistent = Demos.Verify(main).Contains("OK") && (secondList is null || Demos.Verify(secondList).Contains("OK"));
        return new RunResult(before, main.ToArray(), beforeHl, afterHl, msg, changed, consistent, input2, second);
    }

    private static int Need(int? v, string name) =>
        v ?? throw new ArgumentException($"Не задан параметр {name} (целое число).");
}
