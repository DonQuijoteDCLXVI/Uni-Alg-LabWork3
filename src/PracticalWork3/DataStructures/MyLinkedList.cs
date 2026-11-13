using System.Collections;
using System.Text;

namespace PracticalWork3.DataStructures;

/// <summary>Узел односвязного списка.</summary>
public sealed class ListNode<T>
{
    public T Value;
    public ListNode<T>? Next;

    public ListNode(T value, ListNode<T>? next = null)
    {
        Value = value;
        Next = next;
    }
}

/// <summary>
/// СОБСТВЕННЫЙ односвязный список с указателями на голову и хвост.
/// Не использует System.Collections.Generic.LinkedList / List.
/// На нём построены Стек (часть 1), Очередь (часть 2) и алгоритмы части 4.
/// </summary>
public sealed class MyLinkedList<T> : IEnumerable<T>
{
    // Head/Tail/Count открыты намеренно: алгоритмы части 4 «перешивают» ссылки напрямую.
    public ListNode<T>? Head { get; set; }
    public ListNode<T>? Tail { get; set; }
    public int Count { get; set; }

    public bool IsEmpty => Head is null;

    /// <summary>Вставка в начало — O(1).</summary>
    public void AddFirst(T value)
    {
        var node = new ListNode<T>(value, Head);
        Head = node;
        Tail ??= node;
        Count++;
    }

    /// <summary>Вставка в конец — O(1) благодаря указателю Tail.</summary>
    public void AddLast(T value)
    {
        var node = new ListNode<T>(value);
        if (Tail is null)
        {
            Head = Tail = node;
        }
        else
        {
            Tail.Next = node;
            Tail = node;
        }
        Count++;
    }

    /// <summary>Удаление первого элемента — O(1).</summary>
    public T RemoveFirst()
    {
        if (Head is null) throw new InvalidOperationException("Список пуст.");
        var node = Head;
        Head = node.Next;
        if (Head is null) Tail = null;
        Count--;
        return node.Value;
    }

    public T First => Head is null ? throw new InvalidOperationException("Список пуст.") : Head.Value;

    public void Clear()
    {
        Head = Tail = null;
        Count = 0;
    }

    public static MyLinkedList<T> FromEnumerable(IEnumerable<T> items)
    {
        var list = new MyLinkedList<T>();
        foreach (var item in items) list.AddLast(item);
        return list;
    }

    public MyLinkedList<T> Clone() => FromEnumerable(this);

    public IEnumerator<T> GetEnumerator()
    {
        for (var cur = Head; cur is not null; cur = cur.Next)
            yield return cur.Value;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Строка вида «a, b, c» без квадратных скобок.</summary>
    public string Join(string separator = ", ")
    {
        var sb = new StringBuilder();
        for (var cur = Head; cur is not null; cur = cur.Next)
        {
            if (cur != Head) sb.Append(separator);
            sb.Append(cur.Value);
        }
        return sb.ToString();
    }

    public override string ToString() => "[" + Join() + "]";
}
