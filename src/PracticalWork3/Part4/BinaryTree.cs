using System.Text;
using PracticalWork3.DataStructures;

namespace PracticalWork3.Part4;

public sealed class TreeNode
{
    public char Value;
    public TreeNode? Left, Right;
    public TreeNode(char value) { Value = value; }
}

/// <summary>
/// Бинарное дерево, строящееся из строки прямого обхода, где '*' — пустая ссылка (NULL).
/// Строка с рисунка: ABD*G***CE**FH**J**
/// </summary>
public sealed class BinaryTree
{
    public TreeNode? Root { get; private set; }

    /// <summary>Построение дерева из строки прямого обхода с маркерами '*'.</summary>
    public static BinaryTree FromPreorder(string s)
    {
        s = new string(s.Where(ch => !char.IsWhiteSpace(ch)).ToArray());
        if (s.Length == 0) throw new FormatException("Пустая строка.");

        int pos = 0;
        var root = Build(s, ref pos);
        if (pos != s.Length)
            throw new FormatException($"Лишние символы после завершения дерева (с позиции {pos + 1}).");
        return new BinaryTree { Root = root };
    }

    private static TreeNode? Build(string s, ref int pos)
    {
        if (pos >= s.Length)
            throw new FormatException("Строка закончилась раньше, чем дерево: не хватает символов '*'.");
        char c = s[pos++];
        if (c == '*') return null;
        var node = new TreeNode(c);
        node.Left = Build(s, ref pos);       // сначала левое поддерево
        node.Right = Build(s, ref pos);      // затем правое
        return node;
    }

    // ---------- обходы ----------
    public string PreOrder() { var sb = new StringBuilder(); Pre(Root, sb); return sb.ToString(); }
    public string InOrder() { var sb = new StringBuilder(); In(Root, sb); return sb.ToString(); }
    public string PostOrder() { var sb = new StringBuilder(); Post(Root, sb); return sb.ToString(); }

    private static void Pre(TreeNode? n, StringBuilder sb)
    { if (n is null) return; sb.Append(n.Value); Pre(n.Left, sb); Pre(n.Right, sb); }
    private static void In(TreeNode? n, StringBuilder sb)
    { if (n is null) return; In(n.Left, sb); sb.Append(n.Value); In(n.Right, sb); }
    private static void Post(TreeNode? n, StringBuilder sb)
    { if (n is null) return; Post(n.Left, sb); Post(n.Right, sb); sb.Append(n.Value); }

    /// <summary>Обход в ширину (по уровням) — на нашей очереди.</summary>
    public string LevelOrder()
    {
        var sb = new StringBuilder();
        if (Root is null) return "";
        var q = new LinkedQueue<TreeNode>();
        q.Enqueue(Root);
        while (!q.IsEmpty())
        {
            var n = q.Dequeue();
            sb.Append(n.Value);
            if (n.Left is not null) q.Enqueue(n.Left);
            if (n.Right is not null) q.Enqueue(n.Right);
        }
        return sb.ToString();
    }

    // ---------- характеристики ----------
    public int Height() => H(Root);
    private static int H(TreeNode? n) => n is null ? 0 : 1 + Math.Max(H(n.Left), H(n.Right));

    public int CountNodes() => C(Root);
    private static int C(TreeNode? n) => n is null ? 0 : 1 + C(n.Left) + C(n.Right);

    public int CountLeaves() => L(Root);
    private static int L(TreeNode? n) =>
        n is null ? 0 : (n.Left is null && n.Right is null ? 1 : L(n.Left) + L(n.Right));

    /// <summary>Обратная сериализация: должна совпасть с исходной строкой.</summary>
    public string Serialize() { var sb = new StringBuilder(); Ser(Root, sb); return sb.ToString(); }
    private static void Ser(TreeNode? n, StringBuilder sb)
    {
        if (n is null) { sb.Append('*'); return; }
        sb.Append(n.Value);
        Ser(n.Left, sb);
        Ser(n.Right, sb);
    }

    // ---------- вывод ----------
    /// <summary>Таблица узлов в стиле рисунка: у каждого узла левая и правая ссылки (NULL, если пусто).</summary>
    public string NodeTable()
    {
        var sb = new StringBuilder();
        if (Root is null) return "";
        var q = new LinkedQueue<TreeNode>();
        q.Enqueue(Root);
        while (!q.IsEmpty())
        {
            var n = q.Dequeue();
            sb.AppendLine($"  [{n.Value}]  left = {(n.Left?.Value.ToString() ?? "NULL"),-4} right = {(n.Right?.Value.ToString() ?? "NULL")}");
            if (n.Left is not null) q.Enqueue(n.Left);
            if (n.Right is not null) q.Enqueue(n.Right);
        }
        return sb.ToString();
    }

    /// <summary>Дерево, повёрнутое на 90° влево: правое поддерево сверху, левое снизу, корень — слева.</summary>
    public string DrawSideways()
    {
        var sb = new StringBuilder();
        Side(Root, 0, sb);
        return sb.ToString();
    }

    private static void Side(TreeNode? n, int depth, StringBuilder sb)
    {
        if (n is null) return;
        Side(n.Right, depth + 1, sb);
        sb.Append(' ', depth * 6).Append(n.Value).AppendLine();
        Side(n.Left, depth + 1, sb);
    }
}
