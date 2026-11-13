namespace PracticalWork3.Part3;

/// <summary>
/// СПИСОК: задача Иосифа Флавия на циклическом односвязном списке.
/// N человек стоят по кругу, каждого k-го выводят из круга, пока не останется один.
/// Круговой список даёт удаление узла за O(1) и «бесконечный» обход без проверок на конец.
/// </summary>
public static class Josephus
{
    private sealed class Node
    {
        public readonly int Value;
        public Node? Next;
        public Node(int value) { Value = value; }
    }

    public static (List<int> Order, int Survivor) Solve(int n, int k)
    {
        if (n < 1) throw new ArgumentException("N должно быть >= 1.");
        if (k < 1) throw new ArgumentException("k должно быть >= 1.");

        // Построение кольца 1 → 2 → … → n → 1
        var first = new Node(1);
        var last = first;
        for (int i = 2; i <= n; i++)
        {
            var node = new Node(i);
            last.Next = node;
            last = node;
        }
        last.Next = first;

        var prev = last;                     // узел, стоящий перед «текущим» (текущий = prev.Next)
        var order = new List<int>(n);
        for (int remaining = n; remaining > 1; remaining--)
        {
            for (int step = 1; step < k; step++) prev = prev.Next!;   // k-1 шагов вперёд
            var victim = prev.Next!;
            prev.Next = victim.Next;         // исключаем из кольца
            order.Add(victim.Value);
        }
        return (order, prev.Next!.Value);
    }

    /// <summary>Контроль: рекуррентная формула J(1)=0, J(m)=(J(m-1)+k) mod m.</summary>
    public static int SurvivorByFormula(int n, int k)
    {
        int j = 0;
        for (int m = 2; m <= n; m++) j = (j + k) % m;
        return j + 1;
    }
}
