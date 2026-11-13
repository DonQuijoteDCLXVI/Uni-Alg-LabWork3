using System.Text;

namespace PracticalWork3.DataStructures;

/// <summary>Общий контракт очереди, чтобы одинаково прогонять обе реализации.</summary>
public interface IMyQueue<T>
{
    string Name { get; }
    int Count { get; }
    bool IsEmpty();
    void Enqueue(T elem);
    T Dequeue();
    T Peek();
    string Print();
}

/// <summary>Очередь на СОБСТВЕННОМ связном списке: вставка в хвост, удаление из головы — O(1).</summary>
public sealed class LinkedQueue<T> : IMyQueue<T>
{
    private readonly MyLinkedList<T> _list = new();

    public string Name => "Очередь на своём списке";
    public int Count => _list.Count;
    public bool IsEmpty() => _list.IsEmpty;
    public void Enqueue(T elem) => _list.AddLast(elem);

    public T Dequeue() => _list.IsEmpty
        ? throw new InvalidOperationException("Очередь пуста.")
        : _list.RemoveFirst();

    public T Peek() => _list.IsEmpty
        ? throw new InvalidOperationException("Очередь пуста.")
        : _list.First;

    public string Print() => _list.ToString();
}

/// <summary>Обёртка над стандартным System.Collections.Generic.Queue&lt;T&gt; (кольцевой массив).</summary>
public sealed class StdQueue<T> : IMyQueue<T>
{
    private readonly Queue<T> _queue = new();

    public string Name => "Стандартный Queue<T>";
    public int Count => _queue.Count;
    public bool IsEmpty() => _queue.Count == 0;
    public void Enqueue(T elem) => _queue.Enqueue(elem);

    public T Dequeue() => _queue.Count == 0
        ? throw new InvalidOperationException("Очередь пуста.")
        : _queue.Dequeue();

    public T Peek() => _queue.Count == 0
        ? throw new InvalidOperationException("Очередь пуста.")
        : _queue.Peek();

    public string Print()
    {
        var sb = new StringBuilder("[");
        bool first = true;
        foreach (var item in _queue)
        {
            if (!first) sb.Append(", ");
            sb.Append(item);
            first = false;
        }
        return sb.Append(']').ToString();
    }
}
