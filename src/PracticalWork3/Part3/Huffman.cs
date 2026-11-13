using System.Text;

namespace PracticalWork3.Part3;

/// <summary>
/// ДЕРЕВО: кодирование Хаффмана. Строится бинарное дерево, где частые символы
/// ближе к корню (короткие коды), редкие — глубже (длинные коды).
/// Код символа = путь от корня: влево — «0», вправо — «1». Коды префиксные.
/// </summary>
public sealed class Huffman
{
    public sealed class Node
    {
        public char? Symbol;                 // только у листьев
        public int Freq;
        public Node? Left, Right;
        public bool IsLeaf => Left is null && Right is null;
    }

    public Node Root { get; }
    public Dictionary<char, string> Codes { get; } = new();
    public Dictionary<char, int> Frequencies { get; } = new();

    public Huffman(string text)
    {
        if (string.IsNullOrEmpty(text)) throw new ArgumentException("Текст пуст.");

        foreach (char c in text)
            Frequencies[c] = Frequencies.GetValueOrDefault(c) + 1;

        // Приоритетная очередь: (частота, порядковый номер) — номер делает построение детерминированным.
        var pq = new PriorityQueue<Node, (int Freq, int Order)>();
        int order = 0;
        foreach (var kv in Frequencies.OrderBy(k => k.Key))
            pq.Enqueue(new Node { Symbol = kv.Key, Freq = kv.Value }, (kv.Value, order++));

        while (pq.Count > 1)
        {
            var a = pq.Dequeue();            // два самых редких поддерева
            var b = pq.Dequeue();
            var parent = new Node { Freq = a.Freq + b.Freq, Left = a, Right = b };
            pq.Enqueue(parent, (parent.Freq, order++));
        }
        Root = pq.Dequeue();

        if (Root.IsLeaf) Codes[Root.Symbol!.Value] = "0";   // единственный символ в тексте
        else AssignCodes(Root, "");
    }

    private void AssignCodes(Node node, string prefix)
    {
        if (node.IsLeaf) { Codes[node.Symbol!.Value] = prefix; return; }
        AssignCodes(node.Left!, prefix + "0");
        AssignCodes(node.Right!, prefix + "1");
    }

    public string Encode(string text)
    {
        var sb = new StringBuilder();
        foreach (char c in text) sb.Append(Codes[c]);
        return sb.ToString();
    }

    public string Decode(string bits)
    {
        var sb = new StringBuilder();
        if (Root.IsLeaf)
        {
            foreach (char _ in bits) sb.Append(Root.Symbol!.Value);
            return sb.ToString();
        }

        var cur = Root;
        foreach (char bit in bits)
        {
            cur = bit == '0' ? cur.Left! : cur.Right!;
            if (cur.IsLeaf)
            {
                sb.Append(cur.Symbol!.Value);
                cur = Root;
            }
        }
        return sb.ToString();
    }
}
