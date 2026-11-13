using System.Globalization;
using PracticalWork3.DataStructures;

namespace PracticalWork3.Part1;

/// <summary>
/// Перевод инфиксной записи («2+2») в постфиксную («2 2 +») алгоритмом сортировочной станции Дейкстры.
/// Поддерживаются: числа (в т.ч. дробные через точку), + - * : / ^, скобки,
/// унарный минус (в постфиксе обозначается «~»), функции ln cos sin sqrt (аргумент — в скобках).
/// Приоритеты: + - (1) &lt; * / : (2) &lt; унарный минус (3) &lt; ^ (4, правоассоциативна) &lt; функции (5).
/// </summary>
public static class InfixToPostfix
{
    public static string Convert(string infix, Action<string>? trace = null)
    {
        var output = new List<string>();
        var ops = new LinkedStack<string>();   // стек операций — тоже на своём списке
        bool expectOperand = true;             // true в начале, после '(' и после оператора
        int i = 0, n = infix.Length;

        void Log(string token) =>
            trace?.Invoke($"  токен {token,-5} | стек операций (вершина слева): {ops.Print(),-22} | вывод: {string.Join(' ', output)}");

        while (i < n)
        {
            char c = infix[i];
            if (char.IsWhiteSpace(c)) { i++; continue; }

            if (char.IsDigit(c) || c == '.')
            {
                if (!expectOperand) throw Err($"пропущен оператор перед числом (позиция {i + 1})");
                int start = i;
                while (i < n && (char.IsDigit(infix[i]) || infix[i] == '.')) i++;
                string num = infix[start..i];
                if (!double.TryParse(num, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                    throw Err($"некорректное число '{num}'");
                output.Add(num);
                expectOperand = false;
                Log(num);
                continue;
            }

            if (char.IsLetter(c))
            {
                if (!expectOperand) throw Err($"пропущен оператор перед функцией (позиция {i + 1})");
                int start = i;
                while (i < n && char.IsLetter(infix[i])) i++;
                string name = infix[start..i].ToLowerInvariant();
                if (!IsFunction(name)) throw Err($"неизвестная функция '{name}'");
                int j = i;
                while (j < n && char.IsWhiteSpace(infix[j])) j++;
                if (j >= n || infix[j] != '(') throw Err($"после функции '{name}' ожидается '('");
                ops.Push(name);   // expectOperand остаётся true: дальше идёт '('
                Log(name);
                continue;
            }

            switch (c)
            {
                case '(':
                    if (!expectOperand) throw Err($"пропущен оператор перед '(' (позиция {i + 1})");
                    ops.Push("(");
                    break;

                case ')':
                    if (expectOperand) throw Err($"пустые скобки или пропущен операнд перед ')' (позиция {i + 1})");
                    while (!ops.IsEmpty() && ops.Top() != "(") output.Add(ops.Pop());
                    if (ops.IsEmpty()) throw Err($"лишняя закрывающая скобка (позиция {i + 1})");
                    ops.Pop();   // выбросить '('
                    if (!ops.IsEmpty() && IsFunction(ops.Top())) output.Add(ops.Pop());
                    expectOperand = false;
                    break;

                case '+': case '-': case '*': case '/': case ':': case '^':
                    if (expectOperand)
                    {
                        if (c == '-') ops.Push("~");          // унарный минус: просто кладём, ничего не выталкивая
                        else if (c != '+')                    // унарный плюс игнорируем
                            throw Err($"оператор '{c}' без левого операнда (позиция {i + 1})");
                    }
                    else
                    {
                        string op = c.ToString();
                        while (!ops.IsEmpty() && ops.Top() != "(" && ShouldPop(ops.Top(), op))
                            output.Add(ops.Pop());
                        ops.Push(op);
                        expectOperand = true;
                    }
                    break;

                default:
                    throw Err($"недопустимый символ '{c}' (позиция {i + 1})");
            }
            Log(c.ToString());
            i++;
        }

        if (expectOperand) throw Err("выражение неполное: ожидается операнд");
        while (!ops.IsEmpty())
        {
            string top = ops.Pop();
            if (top == "(") throw Err("не закрыта открывающая скобка");
            output.Add(top);
        }
        return string.Join(' ', output);
    }

    private static ExpressionException Err(string msg) => new("некорректная инфиксная запись — " + msg);

    private static bool IsFunction(string s) => s is "ln" or "cos" or "sin" or "sqrt";

    private static int Precedence(string op) => op switch
    {
        "(" => 0,
        "+" or "-" => 1,
        "*" or "/" or ":" => 2,
        "~" => 3,
        "^" => 4,
        _ => 5, // функции
    };

    /// <summary>Выталкивать ли оператор top из стека перед вставкой op (левая ассоциативность, кроме ^).</summary>
    private static bool ShouldPop(string top, string op)
    {
        int pt = Precedence(top), po = Precedence(op);
        return pt > po || (pt == po && op != "^");
    }
}
