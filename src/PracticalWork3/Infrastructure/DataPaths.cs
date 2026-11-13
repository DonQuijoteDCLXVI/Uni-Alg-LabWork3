namespace PracticalWork3.Infrastructure;

/// <summary>Поиск входных файлов: путь как есть → ./data → data рядом с exe.</summary>
public static class DataPaths
{
    public static string Resolve(string nameOrPath)
    {
        var candidates = new[]
        {
            nameOrPath,
            Path.Combine(Directory.GetCurrentDirectory(), "data", nameOrPath),
            Path.Combine(AppContext.BaseDirectory, "data", nameOrPath),
        };
        foreach (var c in candidates)
            if (File.Exists(c)) return c;
        throw new FileNotFoundException(
            $"Файл '{nameOrPath}' не найден. Искал: {string.Join("; ", candidates)}");
    }
}
