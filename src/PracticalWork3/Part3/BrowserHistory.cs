using PracticalWork3.DataStructures;

namespace PracticalWork3.Part3;

/// <summary>
/// СТЕК: история браузера (кнопки «Назад» и «Вперёд») на двух стеках.
/// Назад-стек хранит посещённые страницы, Вперёд-стек — «отменённые» возвраты.
/// Переход по новой ссылке очищает Вперёд-стек — как в настоящем браузере.
/// </summary>
public sealed class BrowserHistory
{
    private LinkedStack<string> _back = new();
    private LinkedStack<string> _forward = new();

    public string? Current { get; private set; }
    public int BackCount => _back.Count;
    public int ForwardCount => _forward.Count;

    public void Visit(string url)
    {
        if (Current is not null) _back.Push(Current);
        Current = url;
        _forward = new LinkedStack<string>();   // ветка «вперёд» теряется
    }

    public bool Back()
    {
        if (_back.IsEmpty() || Current is null) return false;
        _forward.Push(Current);
        Current = _back.Pop();
        return true;
    }

    public bool Forward()
    {
        if (_forward.IsEmpty() || Current is null) return false;
        _back.Push(Current);
        Current = _forward.Pop();
        return true;
    }

    public string State() =>
        $"текущая: {Current ?? "—"} | назад: {_back.Print()} | вперёд: {_forward.Print()}";
}
