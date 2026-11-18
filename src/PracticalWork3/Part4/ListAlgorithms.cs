using PracticalWork3.DataStructures;

namespace PracticalWork3.Part4;

/// <summary>
/// Часть 4. Алгоритмы над СОБСТВЕННЫМ односвязным списком целых чисел.
/// Все функции изменяют ссылки узлов «на месте» (без создания копии списка),
/// поддерживая согласованность Head, Tail и Count.
/// </summary>
public static class ListAlgorithms
{
    /// <summary>1. Переворот списка: меняем направление каждой ссылки. O(n) времени, O(1) памяти.</summary>
    public static void Reverse(MyLinkedList<int> list)
    {
        ListNode<int>? prev = null;
        var cur = list.Head;
        list.Tail = list.Head;               // старая голова станет хвостом
        while (cur is not null)
        {
            var next = cur.Next;
            cur.Next = prev;                 // разворачиваем ссылку
            prev = cur;
            cur = next;
        }
        list.Head = prev;                    // старый хвост стал головой
    }

    /// <summary>2а. Переносит ПОСЛЕДНИЙ элемент непустого списка в начало. O(n) (поиск предпоследнего).</summary>
    public static void MoveLastToFront(MyLinkedList<int> list)
    {
        RequireNotEmpty(list);
        if (list.Count == 1) return;

        var beforeLast = list.Head!;
        while (beforeLast.Next != list.Tail) beforeLast = beforeLast.Next!;

        var last = list.Tail!;
        beforeLast.Next = null;              // отцепляем последний
        last.Next = list.Head;               // подцепляем перед головой
        list.Head = last;
        list.Tail = beforeLast;
    }

    /// <summary>2б. Переносит ПЕРВЫЙ элемент непустого списка в конец. O(1) благодаря указателю Tail.</summary>
    public static void MoveFirstToBack(MyLinkedList<int> list)
    {
        RequireNotEmpty(list);
        if (list.Count == 1) return;

        var first = list.Head!;
        list.Head = first.Next;
        first.Next = null;
        list.Tail!.Next = first;
        list.Tail = first;
    }

    /// <summary>3. Количество различных элементов. O(n) времени и O(k) памяти (HashSet).</summary>
    public static int CountDistinct(MyLinkedList<int> list)
    {
        var seen = new HashSet<int>();
        for (var cur = list.Head; cur is not null; cur = cur.Next)
            seen.Add(cur.Value);
        return seen.Count;
    }

    /// <summary>
    /// 4. Удаляет из списка НЕУНИКАЛЬНЫЕ элементы — все значения, встречающиеся более одного раза
    /// (все их вхождения). Остаются только значения, встречавшиеся ровно один раз.
    /// Два прохода: подсчёт частот, затем выбрасывание узлов. O(n) времени, O(k) памяти.
    /// </summary>
    public static void RemoveNonUnique(MyLinkedList<int> list)
    {
        var freq = new Dictionary<int, int>();
        for (var cur = list.Head; cur is not null; cur = cur.Next)
            freq[cur.Value] = freq.GetValueOrDefault(cur.Value) + 1;

        RemoveWhere(list, v => freq[v] > 1);
    }

    /// <summary>Вариант толкования п.4: оставить по одному (первому) вхождению каждого значения.</summary>
    public static void RemoveDuplicatesKeepFirst(MyLinkedList<int> list)
    {
        var seen = new HashSet<int>();
        RemoveWhere(list, v => !seen.Add(v));
    }

    /// <summary>Общий «выбрасыватель» узлов по предикату; пересчитывает Tail и Count.</summary>
    private static void RemoveWhere(MyLinkedList<int> list, Func<int, bool> shouldRemove)
    {
        ListNode<int>? prev = null;
        var cur = list.Head;
        while (cur is not null)
        {
            if (shouldRemove(cur.Value))
            {
                if (prev is null) list.Head = cur.Next;
                else prev.Next = cur.Next;
                list.Count--;
            }
            else
            {
                prev = cur;
            }
            cur = cur.Next;
        }
        list.Tail = prev;                    // последний НЕ удалённый узел (или null)
    }

    // ======================= Пункты 5–8 =======================

    /// <summary>
    /// 5. Вставляет список САМ В СЕБЯ вслед за первым вхождением числа x:
    /// [1,2,3,4], x = 2  →  [1,2, 1,2,3,4, 3,4].
    /// Вставляется КОПИЯ (иначе получился бы цикл). Возвращает false, если x в списке нет (список не меняется).
    /// O(n) времени и памяти.
    /// </summary>
    public static bool InsertSelfAfterFirst(MyLinkedList<int> list, int x)
    {
        var node = list.Head;
        while (node is not null && node.Value != x) node = node.Next;
        if (node is null) return false;

        var copy = list.Clone();                         // копия до любых изменений; непуста, т.к. x найден
        copy.Tail!.Next = node.Next;                     // хвост копии → то, что шло после x
        node.Next = copy.Head;                           // x → голова копии
        if (ReferenceEquals(node, list.Tail)) list.Tail = copy.Tail;   // x был последним — хвост сменился
        list.Count += copy.Count;
        return true;
    }

    /// <summary>
    /// 6. Вставляет E в НЕПУСТОЙ список, упорядоченный по неубыванию, сохраняя порядок.
    /// E ставится после всех элементов ≤ E (стабильно). O(n); вставка в конец — O(1) благодаря Tail.
    /// </summary>
    public static void InsertSorted(MyLinkedList<int> list, int e)
    {
        RequireNotEmpty(list);
        if (e < list.Head!.Value) { list.AddFirst(e); return; }
        if (e >= list.Tail!.Value) { list.AddLast(e); return; }

        var cur = list.Head;                             // здесь Head.Value ≤ e < Tail.Value
        while (cur.Next is not null && cur.Next.Value <= e) cur = cur.Next;
        cur.Next = new ListNode<int>(e, cur.Next);       // cur не хвост, значит Tail не меняется
        list.Count++;
    }

    /// <summary>7. Удаляет из списка ВСЕ элементы, равные e. Возвращает число удалённых. O(n), O(1) памяти.</summary>
    public static int RemoveAll(MyLinkedList<int> list, int e)
    {
        int before = list.Count;
        RemoveWhere(list, v => v == e);
        return before - list.Count;
    }

    /// <summary>
    /// 8. Вставляет f ПЕРЕД первым вхождением e. Возвращает false, если e в списке нет.
    /// Идём с указателем «предыдущий», чтобы не нужен был двусвязный список. O(n).
    /// </summary>
    public static bool InsertBeforeFirst(MyLinkedList<int> list, int e, int f)
    {
        if (list.Head is null) return false;
        if (list.Head.Value == e) { list.AddFirst(f); return true; }

        var prev = list.Head;
        while (prev.Next is not null && prev.Next.Value != e) prev = prev.Next;
        if (prev.Next is null) return false;

        prev.Next = new ListNode<int>(f, prev.Next);     // вставили перед существующим узлом: Tail не меняется
        list.Count++;
        return true;
    }

    // ======================= Пункты 9–12 =======================

    /// <summary>9. Дописывает список E в конец L перецеплением узлов (E после операции пуст). O(1); Concat(L, L) — O(n).</summary>
    public static void Concat(MyLinkedList<int> l, MyLinkedList<int> e)
    {
        if (ReferenceEquals(l, e)) e = l.Clone();        // иначе получился бы цикл
        if (e.IsEmpty) return;
        if (l.IsEmpty) { l.Head = e.Head; l.Tail = e.Tail; l.Count = e.Count; }
        else { l.Tail!.Next = e.Head; l.Tail = e.Tail; l.Count += e.Count; }
        e.Clear();
    }

    /// <summary>
    /// 10. Разбивает L по первому вхождению value на две части (само value не входит ни в одну).
    /// Если value нет — first = весь список, second пуст. Исходный L после операции пуст.
    /// </summary>
    public static (MyLinkedList<int> first, MyLinkedList<int> second) SplitAtFirst(MyLinkedList<int> list, int value)
    {
        var first = new MyLinkedList<int>();
        var second = new MyLinkedList<int>();
        var oldTail = list.Tail;
        int total = list.Count;

        var cur = list.Head;
        while (cur is not null && cur.Value != value)
        {
            first.AddLast(cur.Value);
            cur = cur.Next;
        }
        if (cur?.Next is not null)
        {
            second.Head = cur.Next;                      // хвост перецепляем без копирования
            second.Tail = oldTail;
            second.Count = total - first.Count - 1;
        }
        list.Clear();
        return (first, second);
    }

    /// <summary>11. Приписывает к списку его копию: [1,2,3] → [1,2,3,1,2,3].</summary>
    public static void DoubleSelf(MyLinkedList<int> list) => Concat(list, list.Clone());

    /// <summary>12. Меняет местами элементы с индексами i и j (меняются значения, ссылки не трогаем).</summary>
    public static void SwapElements(MyLinkedList<int> list, int i, int j)
    {
        if (i < 0 || i >= list.Count) throw new ArgumentOutOfRangeException(nameof(i));
        if (j < 0 || j >= list.Count) throw new ArgumentOutOfRangeException(nameof(j));
        if (i == j) return;
        var a = NodeAt(list, i);
        var b = NodeAt(list, j);
        (a.Value, b.Value) = (b.Value, a.Value);
    }

    private static ListNode<int> NodeAt(MyLinkedList<int> list, int index)
    {
        var cur = list.Head!;
        for (int k = 0; k < index; k++) cur = cur.Next!;
        return cur;
    }

    private static void RequireNotEmpty(MyLinkedList<int> list)
    {
        if (list.IsEmpty) throw new InvalidOperationException("Список пуст: операция определена для непустого списка.");
    }

    /// <summary>Чтение целых чисел (через пробел/запятую/перевод строки) из текста.</summary>
    public static MyLinkedList<int> ParseList(string text)
    {
        var list = new MyLinkedList<int>();
        var parts = text.Split(new[] { ' ', ',', ';', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parts)
        {
            if (!int.TryParse(p, out int v))
                throw new FormatException($"'{p}' не является целым числом.");
            list.AddLast(v);
        }
        return list;
    }
}
