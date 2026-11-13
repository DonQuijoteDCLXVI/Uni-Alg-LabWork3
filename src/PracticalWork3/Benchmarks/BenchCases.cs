namespace PracticalWork3.Benchmarks;

/// <summary>Описание тестового набора операций для input.txt.</summary>
public sealed record BenchCase(string Name, int Count, Func<int, Random, string> Token);

/// <summary>Генераторы наборов операций разного размера и состава (коды 1..5, «1,значение»).</summary>
public static class BenchCases
{
    public static string Value(Random r) => r.Next(2) == 0 ? r.Next(1000).ToString() : "w" + r.Next(1000);
    public static string Insert(Random r) => "1," + Value(r);

    /// <summary>Один случайный токен (для самопроверок): вставка 40 %, остальные операции по 15–30 %.</summary>
    public static string Weighted_ForTests(Random r) => Weighted(40, 30, 10, 10, 10)(0, r);

    /// <summary>Случайный выбор операции с заданными весами (в процентах).</summary>
    private static Func<int, Random, string> Weighted(double ins, double rem, double peek, double empty, double print)
    {
        double total = ins + rem + peek + empty + print;
        return (_, r) =>
        {
            double x = r.NextDouble() * total;
            if ((x -= ins) < 0) return Insert(r);
            if ((x -= rem) < 0) return "2";
            if ((x -= peek) < 0) return "3";
            if ((x -= empty) < 0) return "4";
            return "5";
        };
    }

    /// <summary>Наборы РАЗНОГО размера, одинаковый «типовой» состав операций.</summary>
    public static IReadOnlyList<BenchCase> SizeCases()
    {
        var mix = Weighted(40, 30, 14.99, 15, 0.01);
        return new[] { 100, 1_000, 10_000, 100_000, 1_000_000 }
            .Select(n => new BenchCase($"Случайная смесь, {n:N0} оп.", n, mix))
            .ToArray();
    }

    /// <summary>Наборы ОДИНАКОВОЙ длины, но РАЗНОГО состава операций.</summary>
    public static IReadOnlyList<BenchCase> CompositionCases(int n, string ins, string rem)
    {
        return new[]
        {
            new BenchCase($"Только {ins}", n, (_, r) => Insert(r)),
            new BenchCase($"Только {rem} (пустая структура)", n, (_, _) => "2"),
            new BenchCase($"{ins}/{rem} чередуются", n, (i, r) => i % 2 == 0 ? Insert(r) : "2"),
            new BenchCase($"{ins}×3 : {rem}×1 (рост)", n, (i, r) => i % 4 == 3 ? "2" : Insert(r)),
            new BenchCase($"Пачки: 1000 {ins}, 1000 {rem}", n, (i, r) => (i / 1000) % 2 == 0 ? Insert(r) : "2"),
            new BenchCase($"1000 {ins}, затем просмотр/isEmpty", n, (i, r) => i < 1000 ? Insert(r) : (i % 2 == 0 ? "3" : "4")),
            new BenchCase($"{ins} + Print каждые 2000 оп.", n, (i, r) => i % 2000 == 1999 ? "5" : Insert(r)),
        };
    }

    /// <summary>Генерирует файл набора и возвращает путь к нему.</summary>
    public static string WriteFile(BenchCase c, string path, int seed = 42)
    {
        var rnd = new Random(seed);
        using var w = new StreamWriter(path, false, new System.Text.UTF8Encoding(false), 1 << 16);
        for (int i = 0; i < c.Count; i++)
        {
            if (i > 0) w.Write(' ');
            w.Write(c.Token(i, rnd));
        }
        return path;
    }
}
