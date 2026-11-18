using System.Drawing.Drawing2D;

namespace PracticalWork3.Gui;

/// <summary>Чем подсвечивать выделенные узлы раздела.</summary>
public enum HighlightKind { Found, New, Removed }

/// <summary>Один нарисованный список: заголовок, значения, подсвеченные индексы.</summary>
public sealed record ListSection(string Title, int[] Values, int[] Highlight, HighlightKind Kind, bool Animated = false);

/// <summary>
/// Панель, которая рисует несколько односвязных списков друг под другом узлами со стрелками.
/// Узел = [ значение | указатель ]. Длинный список переносится на следующую строку.
/// </summary>
public sealed class ListCanvas : Panel
{
    // размеры в пикселях при 96 DPI; S() подгоняет их под масштаб экрана
    private int S(int v) => (int)Math.Round(v * DeviceDpi / 96.0);
    private int NodeW => S(66);
    private int NodeH => S(40);
    private int ValueW => S(46);
    private int ArrowW => S(30);
    private int Gutter => S(34);
    private int RowGap => S(36);
    private int TitleH => S(30);
    private int SectionGap => S(14);

    private IReadOnlyList<ListSection> _sections = Array.Empty<ListSection>();
    private float _progress = 1f;

    private readonly Font _valueFont = new("Segoe UI", 12f, FontStyle.Bold);
    private readonly Font _smallFont = new("Segoe UI", 8f, FontStyle.Bold);
    private readonly Font _titleFont = new("Segoe UI", 10f, FontStyle.Bold);

    public ListCanvas()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        AutoScroll = true;
        BackColor = Color.White;
    }

    /// <summary>Доля уже «появившихся» узлов в разделах с Animated = true (0..1).</summary>
    public float Progress
    {
        get => _progress;
        set { _progress = Math.Clamp(value, 0f, 1f); Invalidate(); }
    }

    public void SetSections(IReadOnlyList<ListSection> sections, bool animate)
    {
        _sections = sections;
        _progress = animate ? 0f : 1f;
        UpdateScrollSize();
        Invalidate();
    }

    /// <summary>Просто показать один список (предпросмотр при вводе).</summary>
    public void ShowOnly(int[] list) =>
        SetSections(new[] { new ListSection("СПИСОК L", list, Array.Empty<int>(), HighlightKind.Found) }, false);

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        UpdateScrollSize();
    }

    private int PerRow() => Math.Max(1, (ClientSize.Width - 2 * Gutter) / (NodeW + ArrowW));

    private int SectionHeight(int count)
    {
        int rows = count == 0 ? 1 : (count + PerRow() - 1) / PerRow();
        return TitleH + S(16) + rows * (NodeH + RowGap);
    }

    private void UpdateScrollSize()
    {
        int h = S(10);
        foreach (var s in _sections) h += SectionHeight(s.Values.Length) + SectionGap;
        AutoScrollMinSize = new Size(0, h + S(20));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.TranslateTransform(AutoScrollPosition.X, AutoScrollPosition.Y);

        int y = S(10);
        foreach (var s in _sections)
        {
            int visible = s.Animated ? (int)Math.Ceiling(s.Values.Length * _progress) : s.Values.Length;
            y = DrawSection(g, s, visible, y) + SectionGap;
        }
    }

    private int DrawSection(Graphics g, ListSection sec, int visible, int top)
    {
        var arr = sec.Values;
        var hl = new HashSet<int>(sec.Highlight);
        var (hlBorder, hlFill) = sec.Kind switch
        {
            HighlightKind.New => (Color.FromArgb(31, 157, 85), Color.FromArgb(223, 245, 231)),
            HighlightKind.Removed => (Color.FromArgb(214, 69, 69), Color.FromArgb(253, 228, 228)),
            _ => (Color.FromArgb(245, 165, 36), Color.FromArgb(255, 244, 220)),
        };

        g.DrawString(sec.Title, _titleFont, Brushes.DimGray, Gutter, top);
        int y0 = top + TitleH + S(16);

        if (arr.Length == 0)
        {
            g.DrawString("список пуст (Head = null)", _valueFont, Brushes.Gray, Gutter, y0 + S(6));
            return top + SectionHeight(0);
        }

        int perRow = PerRow();
        using var normalPen = new Pen(Color.FromArgb(150, 155, 170), 2f);
        using var hlPen = new Pen(hlBorder, 2.5f);
        using var arrowPen = new Pen(Color.FromArgb(110, 115, 130), 2f) { CustomEndCap = new AdjustableArrowCap(4, 5) };
        using var hlBrush = new SolidBrush(hlFill);
        using var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        int n = Math.Min(visible, arr.Length);
        for (int i = 0; i < n; i++)
        {
            int row = i / perRow, col = i % perRow;
            int x = Gutter + col * (NodeW + ArrowW);
            int y = y0 + row * (NodeH + RowGap);
            bool lit = hl.Contains(i);

            var rect = new Rectangle(x, y, NodeW, NodeH);
            g.FillRectangle(lit ? hlBrush : Brushes.White, rect);
            g.DrawRectangle(lit ? hlPen : normalPen, rect);
            g.DrawLine(lit ? hlPen : normalPen, x + ValueW, y, x + ValueW, y + NodeH);

            g.DrawString(arr[i].ToString(), _valueFont, Brushes.Black, new RectangleF(x, y, ValueW, NodeH), center);
            if (lit && sec.Kind == HighlightKind.Removed)
                g.DrawLine(Pens.Firebrick, x + S(6), y + NodeH / 2, x + ValueW - S(6), y + NodeH / 2);   // «зачёркнуто»

            g.DrawString(i == arr.Length - 1 ? "∅" : "•", _valueFont, Brushes.Gray,
                new RectangleF(x + ValueW, y, NodeW - ValueW, NodeH), center);

            if (i == 0) g.DrawString("HEAD", _smallFont, Brushes.RoyalBlue, x, y - S(15));
            if (i == arr.Length - 1) g.DrawString("TAIL", _smallFont, Brushes.SeaGreen, x, y - S(15));

            if (i < arr.Length - 1 && i + 1 < visible)
            {
                int cy = y + NodeH / 2;
                if (col < perRow - 1)
                {
                    g.DrawLine(arrowPen, x + NodeW, cy, x + NodeW + ArrowW - 2, cy);
                }
                else
                {
                    // перенос на следующую строку: ломаная вниз и влево
                    int midY = y + NodeH + RowGap / 2 + S(4);
                    int nextY = y + NodeH + RowGap + NodeH / 2;
                    var pts = new[]
                    {
                        new Point(x + NodeW, cy),
                        new Point(x + NodeW + S(14), cy),
                        new Point(x + NodeW + S(14), midY),
                        new Point(Gutter - S(16), midY),
                        new Point(Gutter - S(16), nextY),
                        new Point(Gutter - 2, nextY),
                    };
                    g.DrawLines(arrowPen, pts);
                }
            }
        }
        return top + SectionHeight(arr.Length);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _valueFont.Dispose();
            _smallFont.Dispose();
            _titleFont.Dispose();
        }
        base.Dispose(disposing);
    }
}
