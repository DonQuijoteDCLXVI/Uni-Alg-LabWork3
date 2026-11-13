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
