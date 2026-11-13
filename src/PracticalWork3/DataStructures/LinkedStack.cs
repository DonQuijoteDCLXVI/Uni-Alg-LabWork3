namespace PracticalWork3.DataStructures;

/// <summary>
/// Стек на основе СОБСТВЕННОГО связного списка.
/// Вершина стека = голова списка, поэтому Push/Pop/Top работают за O(1).
/// Операции по заданию: Push(elem), Pop(), Top(), isEmpty(), Print().
/// </summary>
public sealed class LinkedStack<T>
{
    private readonly MyLinkedList<T> _list = new();

    public int Count => _list.Count;

    public bool IsEmpty() => _list.IsEmpty;

    public void Push(T elem) => _list.AddFirst(elem);

    public T Pop() => _list.IsEmpty
        ? throw new InvalidOperationException("Стек пуст.")
        : _list.RemoveFirst();

    public T Top() => _list.IsEmpty
        ? throw new InvalidOperationException("Стек пуст.")
        : _list.First;

    /// <summary>Содержимое стека от вершины к основанию: «[cat, 7, 56]».</summary>
    public string Print() => _list.ToString();

    /// <summary>Печать прямо в поток вывода.</summary>
    public void Print(TextWriter writer) => writer.WriteLine(Print());

    /// <summary>Элементы от вершины к основанию.</summary>
    public IEnumerable<T> TopToBottom => _list;
}
