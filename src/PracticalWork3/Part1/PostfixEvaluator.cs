using System.Globalization;
using PracticalWork3.DataStructures;

namespace PracticalWork3.Part1;

public sealed class ExpressionException : Exception
{
    public ExpressionException(string message) : base(message) { }
}

/// <summary>
/// Вычисление выражения в ПОСТФИКСНОЙ записи (обратная польская запись) с помощью стека.
/// Операции: бинарные + - * : (или /) ^ ; унарные ln cos sin sqrt и ~ (унарный минус).
/// Один проход по токенам, каждый токен — O(1) операций со стеком => O(n) по времени, O(n) по памяти.
/// </summary>
public static class PostfixEvaluator
{
    public static double Evaluate(string expression, Action<string>? trace = null)
    {
        var stack = new LinkedStack<double>();
        var tokens = expression.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0) throw new ExpressionException("Пустое выражение.");

        foreach (var token in tokens)
        {
            if (IsNumber(token, out double number))
            {
                stack.Push(number);
            }
            else
            {
                string name = char.IsLetter(token[0]) ? token.ToLowerInvariant() : token;
                if (IsBinary(name))
                {
                    if (stack.Count < 2)
                        throw new ExpressionException($"Недостаточно операндов для операции '{token}'.");
                    double b = stack.Pop();   // правый операнд снимается первым
                    double a = stack.Pop();
                    stack.Push(Check(ApplyBinary(name, a, b), token));
                }
                else if (IsUnary(name))
                {
                    if (stack.Count < 1)
                        throw new ExpressionException($"Недостаточно операндов для операции '{token}'.");
                    double a = stack.Pop();
                    stack.Push(Check(ApplyUnary(name, a), token));
                }
                else
                {
                    throw new ExpressionException($"Неизвестный токен '{token}'.");
                }
            }

            trace?.Invoke($"  токен {token,-5} -> стек (основание → вершина): {Snapshot(stack)}");
        }

        if (stack.Count != 1)
            throw new ExpressionException(
                $"В стеке осталось {stack.Count} значений вместо одного: не хватает операций.");
        return stack.Pop();
    }

    public static string Format(double value) => value.ToString("G12", CultureInfo.InvariantCulture);

    private static string Snapshot(LinkedStack<double> stack)
    {
        var items = stack.TopToBottom.Select(Format).Reverse();
        return "[" + string.Join(", ", items) + "]";
    }

    private static bool IsNumber(string t, out double value)
    {
        value = 0;
        char c = t[0];
        bool looksNumeric = char.IsDigit(c) || c == '.' ||
            ((c == '-' || c == '+') && t.Length > 1 && (char.IsDigit(t[1]) || t[1] == '.'));
        return looksNumeric &&
               double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static bool IsBinary(string t) => t is "+" or "-" or "*" or ":" or "/" or "^";
    private static bool IsUnary(string t) => t is "ln" or "cos" or "sin" or "sqrt" or "~";

    private static double ApplyBinary(string op, double a, double b)
    {
        switch (op)
        {
            case "+": return a + b;
            case "-": return a - b;
            case "*": return a * b;
            case ":":
            case "/":
                if (b == 0) throw new ExpressionException("Деление на ноль.");
                return a / b;
            case "^": return Math.Pow(a, b);
            default: throw new ExpressionException($"Неизвестная операция '{op}'.");
        }
    }

    private static double ApplyUnary(string op, double a)
    {
        switch (op)
        {
            case "ln":
                if (a <= 0) throw new ExpressionException("ln определён только для x > 0.");
                return Math.Log(a);
            case "cos": return Math.Cos(a);
            case "sin": return Math.Sin(a);
            case "sqrt":
                if (a < 0) throw new ExpressionException("sqrt определён только для x >= 0.");
                return Math.Sqrt(a);
            case "~": return -a;
            default: throw new ExpressionException($"Неизвестная операция '{op}'.");
        }
    }

    private static double Check(double result, string op)
    {
        if (double.IsNaN(result) || double.IsInfinity(result))
            throw new ExpressionException($"Операция '{op}' дала недопустимый результат ({result}).");
        return result;
    }
}
