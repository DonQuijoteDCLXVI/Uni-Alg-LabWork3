using PracticalWork3.DataStructures;
using PracticalWork3.Part1;

namespace PracticalWork3.Part2;

/// <summary>
/// Выполняет те же коды операций над ОЧЕРЕДЬЮ (любой реализацией IMyQueue):
/// 1 - вставка, 2 - удаление, 3 - просмотр начала, 4 - проверка на пустоту, 5 - печать.
/// </summary>
public static class QueueRunner
{
    public static void Run(IMyQueue<string> queue, Operation[] ops, Action<string> output)
    {
        foreach (var op in ops)
        {
            switch (op.Kind)
            {
                case OpKind.Push:
                    queue.Enqueue(op.Value!);
                    output($"Enqueue({op.Value}): элемент добавлен в конец");
                    break;
                case OpKind.Pop:
                    output(queue.IsEmpty()
                        ? "Dequeue(): очередь пуста, удалять нечего"
                        : $"Dequeue(): удалён элемент {queue.Dequeue()}");
                    break;
                case OpKind.Top:
                    output(queue.IsEmpty()
                        ? "Front(): очередь пуста"
                        : $"Front(): {queue.Peek()}");
                    break;
                case OpKind.IsEmpty:
                    output($"isEmpty(): {(queue.IsEmpty() ? "true" : "false")}");
                    break;
                case OpKind.Print:
                    output(queue.IsEmpty()
                        ? "Print(): очередь пуста"
                        : $"Print() (начало слева): {queue.Print()}");
                    break;
                default:
                    output($"Ошибка: неизвестная или неполная операция '{op.Raw}'");
                    break;
            }
        }
    }
}
