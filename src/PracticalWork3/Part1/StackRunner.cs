using PracticalWork3.DataStructures;

namespace PracticalWork3.Part1;

/// <summary>Выполняет последовательность операций над СТЕКОМ на собственном списке.</summary>
public static class StackRunner
{
    public static void Run(Operation[] ops, Action<string> output)
    {
        var stack = new LinkedStack<string>();
        foreach (var op in ops)
        {
            switch (op.Kind)
            {
                case OpKind.Push:
                    stack.Push(op.Value!);
                    output($"Push({op.Value}): элемент добавлен");
                    break;
                case OpKind.Pop:
                    output(stack.IsEmpty()
                        ? "Pop(): стек пуст, удалять нечего"
                        : $"Pop(): удалён элемент {stack.Pop()}");
                    break;
                case OpKind.Top:
                    output(stack.IsEmpty()
                        ? "Top(): стек пуст"
                        : $"Top(): {stack.Top()}");
                    break;
                case OpKind.IsEmpty:
                    output($"isEmpty(): {(stack.IsEmpty() ? "true" : "false")}");
                    break;
                case OpKind.Print:
                    output(stack.IsEmpty()
                        ? "Print(): стек пуст"
                        : $"Print() (вершина слева): {stack.Print()}");
                    break;
                default:
                    output($"Ошибка: неизвестная или неполная операция '{op.Raw}'");
                    break;
            }
        }
    }
}
