using System.Globalization;
using System.Text;
using PracticalWork3.Infrastructure;

namespace PracticalWork3;

public static class Program
{
    public static int Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        try
        {
            Console.OutputEncoding = new UTF8Encoding(false);
            Console.InputEncoding = new UTF8Encoding(false);
        }
        catch { /* перенаправленный ввод/вывод — кодировку менять не нужно */ }

        return args.Length > 0 ? RunCommand(args) : RunMenu();
    }

    // ------------------------- режим командной строки -------------------------

    private static int RunCommand(string[] args)
    {
        string cmd = args[0].ToLowerInvariant();
        string? arg = args.Length > 1 ? args[1] : null;
        try
        {
            switch (cmd)
            {
                case "stack":       Demos.StackFromFile(arg ?? "input.txt"); break;
                case "bench-stack": Demos.BenchStack(); break;
                case "postfix":     Demos.Postfix(arg ?? "postfix.txt", false); break;
                case "bench-postfix": Demos.BenchPostfix(); break;
                case "infix":       Demos.Infix(arg ?? "infix.txt", false); break;
                case "queue":       Demos.QueueFromFile(arg ?? "input.txt"); break;
                case "bench-queue": Demos.BenchQueue(); break;
                case "josephus":    Demos.Josephus(false); break;
                case "history":     Demos.History(false); break;
                case "scheduler":   Demos.Scheduler(arg ?? "processes.txt"); break;
                case "huffman":     Demos.HuffmanDemo(false); break;
                case "lists":       Demos.Lists(arg ?? "list.txt"); break;
                case "tree":        Demos.Tree(arg); break;
                case "selftest":    return SelfTest.Run();
                case "all":         RunAll(); break;
                case "help": case "-h": case "--help": PrintHelp(); break;
                default:
                    ConsoleIO.Error($"неизвестная команда '{args[0]}'.");
                    PrintHelp();
                    return 2;
            }
            return 0;
        }
        catch (Exception ex) when (ex is IOException or FormatException or ArgumentException or InvalidOperationException)
        {
            ConsoleIO.Error(ex.Message);
            return 1;
        }
    }

    private static void RunAll()
    {
        Demos.StackFromFile("input.txt");
        Demos.Postfix("postfix.txt", false);
        Demos.Infix("infix.txt", false);
        Demos.QueueFromFile("input.txt");
        Demos.Josephus(false);
        Demos.History(false);
        Demos.Scheduler("processes.txt");
        Demos.HuffmanDemo(false);
        Demos.Lists("list.txt");
        Demos.Tree(null);
    }

    private static void PrintHelp()
    {
        Console.WriteLine(@"
Использование: dotnet run --project src/PracticalWork3 -- <команда> [аргумент]

  (без аргументов)      интерактивное меню
  stack [файл]          часть 1: стек, операции из файла (по умолчанию input.txt)
  bench-stack           часть 1: замеры времени для стека
  postfix [файл]        часть 1: вычисление постфиксных выражений (postfix.txt)
  bench-postfix         часть 1: эмпирическая проверка сложности O(n)
  infix [файл]          часть 1: инфикс -> постфикс (infix.txt)
  queue [файл]          часть 2: очередь (список и Queue<T>), операции из файла
  bench-queue           часть 2: замеры времени для очередей
  josephus | history | scheduler [файл] | huffman     часть 3: примеры применения
  lists [файл]          часть 4: алгоритмы над списком (list.txt)
  tree [строка]         часть 4: дерево с рисунка (по умолчанию ABD*G***CE**FH**J**)
  selftest              встроенные самопроверки
  all                   запустить все демонстрации (без замеров)");
    }

    // ------------------------------- меню -------------------------------

    private static int RunMenu()
    {
        while (true)
        {
            Console.WriteLine(@"
================ ПРАКТИЧЕСКОЕ ЗАНЯТИЕ №3. ДИНАМИЧЕСКИЕ СТРУКТУРЫ ================
 ЧАСТЬ 1  Стек
   1  Стек на своём списке: операции из input.txt
   2  Стек: замеры времени (разные наборы операций)
   3  Постфиксные вычисления из postfix.txt (+ свой ввод)
   4  Инфиксная запись -> постфиксная из infix.txt (+ свой ввод)
   5  Проверка сложности постфиксных вычислений (замеры)
 ЧАСТЬ 2  Очередь
   6  Очередь (свой список и Queue<T>): операции из input.txt
   7  Очередь: замеры времени
 ЧАСТЬ 3  Примеры применения
   8  Список: задача Иосифа Флавия
   9  Стек: история браузера (назад/вперёд)
  10  Очередь: планировщик Round-Robin
  11  Дерево: кодирование Хаффмана
 ЧАСТЬ 4  Алгоритмы
  12  Списки: reverse, перенос, различные, неуникальные (list.txt)
  13  Бинарное дерево с рисунка
 ---
  14  Самопроверка     0  Выход
=================================================================================");
            Console.Write("Ваш выбор: ");
            string? choice = Console.ReadLine();
            if (choice is null) return 0;   // конец ввода

            try
            {
                switch (choice.Trim())
                {
                    case "1": Demos.StackFromFile("input.txt"); break;
                    case "2": Demos.BenchStack(); break;
                    case "3": Demos.Postfix("postfix.txt", true); break;
                    case "4": Demos.Infix("infix.txt", true); break;
                    case "5": Demos.BenchPostfix(); break;
                    case "6": Demos.QueueFromFile("input.txt"); break;
                    case "7": Demos.BenchQueue(); break;
                    case "8": Demos.Josephus(true); break;
                    case "9": Demos.History(true); break;
                    case "10": Demos.Scheduler("processes.txt"); break;
                    case "11": Demos.HuffmanDemo(true); break;
                    case "12": Demos.Lists("list.txt"); break;
                    case "13": Demos.Tree(null); break;
                    case "14": SelfTest.Run(); break;
                    case "0": return 0;
                    default: ConsoleIO.Error("нет такого пункта меню."); break;
                }
            }
            catch (Exception ex) when (ex is IOException or FormatException or ArgumentException or InvalidOperationException)
            {
                ConsoleIO.Error(ex.Message);
            }
        }
    }
}
