namespace PracticalWork3.Part1;

/// <summary>Код операции из input.txt: 1 - вставка, 2 - удаление, 3 - просмотр, 4 - проверка на пустоту, 5 - печать.</summary>
public enum OpKind
{
    Invalid = 0,
    Push = 1,
    Pop = 2,
    Top = 3,
    IsEmpty = 4,
    Print = 5,
}

public readonly record struct Operation(OpKind Kind, string? Value, string Raw);

/// <summary>
/// Разбор файла input.txt. Формат: числа 1..5 через пробел,
/// для вставки «1,значение» (значение — число либо слово).
/// Пример: 3 4 1,56 1,7 1,cat 2 5 4
/// </summary>
public static class OperationParser
{
    public static Operation[] ReadFile(string path) => Parse(File.ReadAllText(path));

    public static Operation[] Parse(string text)
    {
        var tokens = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var ops = new Operation[tokens.Length];
        for (int i = 0; i < tokens.Length; i++)
            ops[i] = ParseToken(tokens[i]);
        return ops;
    }

    private static Operation ParseToken(string t)
    {
        if (t[0] == '1')
        {
            // «1,значение»: значение — всё после первой запятой
            if (t.Length > 2 && t[1] == ',')
                return new Operation(OpKind.Push, t.Substring(2), t);
            return new Operation(OpKind.Invalid, null, t);
        }

        if (t.Length == 1)
        {
            switch (t[0])
            {
                case '2': return new Operation(OpKind.Pop, null, t);
                case '3': return new Operation(OpKind.Top, null, t);
                case '4': return new Operation(OpKind.IsEmpty, null, t);
                case '5': return new Operation(OpKind.Print, null, t);
            }
        }
        return new Operation(OpKind.Invalid, null, t);
    }
}
