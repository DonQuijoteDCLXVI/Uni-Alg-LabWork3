using PracticalWork3.Visual;

namespace PracticalWork3.Gui;

/// <summary>Главное окно: все операции над списком (пп. 1–12), ввод данных и наглядная отрисовка результата.</summary>
public sealed class MainForm : Form
{
    /// <summary>Описание операции: что показывать в списке, какие параметры нужны и что подставить по умолчанию.</summary>
    private sealed record OpItem(string Key, string Title, string Description,
        string? LabelA = null, string? LabelB = null, bool UseOtherList = false,
        int DefaultA = 0, int DefaultB = 0, string DefaultOther = "")
    {
        public override string ToString() => Title;
        public string ShortName => Title.Split('.')[0];
    }

    private static readonly OpItem[] Ops =
    {
        new(OpKeys.Reverse, "1. Перевернуть список",
            "Меняет порядок элементов на обратный. Пример: [1, 2, 3] → [3, 2, 1]."),
        new(OpKeys.MoveLast, "2а. Последний элемент → в начало",
            "Переносит последний элемент непустого списка в его начало. Пример: [1, 2, 3] → [3, 1, 2]."),
        new(OpKeys.MoveFirst, "2б. Первый элемент → в конец",
            "Переносит первый элемент непустого списка в его конец. Пример: [1, 2, 3] → [2, 3, 1]."),
        new(OpKeys.Distinct, "3. Количество различных элементов",
            "Считает, сколько разных значений в списке. Пример: [5, 3, 5, 1, 3] → 3 (это 5, 3 и 1). Список не меняется."),
        new(OpKeys.NonUnique, "4. Удалить неуникальные (все повторяющиеся)",
            "Удаляет все значения, которые встречаются больше одного раза, вместе со всеми их вхождениями. Пример: [5, 3, 8, 3, 1, 5, 9, 3, 7] → [8, 1, 9, 7]."),
        new(OpKeys.KeepFirst, "4*. Оставить по одному вхождению (вариант п. 4)",
            "Другое прочтение п. 4: каждое значение остаётся один раз (первое вхождение). Пример: [5, 3, 8, 3, 1, 5, 9, 3, 7] → [5, 3, 8, 1, 9, 7]."),
        new(OpKeys.InsertSelf, "5. Вставить список в себя после первого x",
            "Сразу после первого вхождения числа x вставляется копия всего списка L. Пример: L = [1, 2, 3, 4], x = 2 → [1, 2, 1, 2, 3, 4, 3, 4]. Если x в L нет — список не меняется.",
            LabelA: "x — число, после первого вхождения которого вставляем копию L:", DefaultA: 3),
        new(OpKeys.InsertSorted, "6. Вставить E в упорядоченный список",
            "L должен быть непустым и идти по неубыванию (например 1 3 3 5 8). Число E вставляется так, чтобы порядок сохранился. Пример: L = [1, 3, 5], E = 4 → [1, 3, 4, 5]. Кнопка «Отсортировать» подготовит список.",
            LabelA: "E — число, которое нужно вставить:", DefaultA: 4),
        new(OpKeys.RemoveAll, "7. Удалить все элементы E",
            "Из L удаляются все элементы, равные числу E. Пример: L = [3, 1, 3, 2], E = 3 → [1, 2].",
            LabelA: "E — число, которое нужно удалить (все вхождения):", DefaultA: 3),
        new(OpKeys.InsertBefore, "8. Вставить F перед первым вхождением E",
            "Если число E есть в L, то прямо перед его первым вхождением вставляется число F. Пример: L = [1, 2, 3, 2], E = 2, F = 9 → [1, 9, 2, 3, 2]. Если E нет — список не меняется.",
            LabelA: "E — перед первым вхождением какого числа вставляем:",
            LabelB: "F — какое число вставляем:", DefaultA: 3, DefaultB: 100),
        new(OpKeys.Concat, "9. Дописать список E в конец L",
            "К концу L прицепляются узлы второго списка E (без копирования). Пример: L = [1, 2], E = [3, 4, 5] → L = [1, 2, 3, 4, 5]. После этого E пуст.",
            LabelA: "Список E (числа через пробел), который дописываем в конец L:", UseOtherList: true, DefaultOther: "10 20 30"),
        new(OpKeys.Split, "10. Разбить L по первому вхождению x",
            "L делится на две части: всё до первого x и всё после него. Само x не входит ни в одну часть, а исходный L становится пустым. Пример: L = [1, 2, 3, 4, 5], x = 3 → [1, 2] и [4, 5].",
            LabelA: "x — число, по первому вхождению которого разбиваем:", DefaultA: 3),
        new(OpKeys.Double, "11. Удвоить список",
            "К концу списка приписывается его копия. Пример: [1, 2, 3] → [1, 2, 3, 1, 2, 3]."),
        new(OpKeys.Swap, "12. Поменять местами два элемента по индексам",
            "Меняет местами элементы на позициях i и j. Счёт с нуля: первый элемент — 0. Пример: L = [1, 2, 3, 4, 5], i = 0, j = 4 → [5, 2, 3, 4, 1].",
            LabelA: "i — индекс первого элемента (счёт с 0):", LabelB: "j — индекс второго элемента:", DefaultA: 0, DefaultB: 2),
    };

    private readonly Panel _top = new() { Dock = DockStyle.Top, Height = 262 };
    private readonly ComboBox _op = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _list = new() { Text = "5 3 8 3 1 5 9 3 7" };
    private readonly Label _lblA = new() { AutoSize = true };
    private readonly Label _lblB = new() { AutoSize = true };
    private readonly NumericUpDown _numA = MakeNumber();
    private readonly NumericUpDown _numB = MakeNumber();
    private readonly TextBox _other = new();
    private readonly Label _desc = new() { ForeColor = Color.FromArgb(70, 75, 90), AutoSize = false };
    private readonly Button _run = new() { Text = "Выполнить" };
    private readonly Button _random = new() { Text = "Случайный список" };
    private readonly Button _sort = new() { Text = "Отсортировать" };
    private readonly Button _help = new() { Text = "Справка: что такое L, x, E, F…" };
    private readonly CheckBox _chain = new() { Text = "результат → новый список L", AutoSize = true };
    private readonly Label _status = new();
    private readonly Label _legend = new();
    private readonly ListBox _history = new();
    private readonly ListCanvas _canvas = new();
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 16 };
    private readonly Random _rnd = new();
    private bool _suppressPreview;   // чтобы смена текста программой не стирала результат с экрана

    public MainForm()
    {
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Text = "Односвязный список — визуализатор (пп. 1–12)";
        Font = new Font("Segoe UI", 10f);
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1000, 800);
        MinimumSize = new Size(820, 640);

        BuildLayout();
        WireEvents();

        _op.Items.AddRange(Ops);
        _op.SelectedIndex = 0;                 // вызовет OnOpChanged
        _canvas.ShowOnly(ParseListOrEmpty());
    }

    private static NumericUpDown MakeNumber() => new()
    {
        Minimum = -1_000_000, Maximum = 1_000_000, Value = 0, Width = 90,
    };

    private OpItem CurrentOp => (OpItem)_op.SelectedItem!;

    // ------------------------------------------------------------------ разметка

    private void BuildLayout()
    {
        var lblOp = new Label { Text = "Операция:", AutoSize = true, Location = new Point(12, 17) };
        _op.SetBounds(150, 13, 800, 28);

        var lblList = new Label
        {
            Text = "Список L\n(числа через пробел):",
            AutoSize = false, Location = new Point(12, 48), Size = new Size(136, 44),
        };
        _list.SetBounds(150, 52, 800, 28);

        _lblA.Location = new Point(12, 102);
        _numA.Location = new Point(300, 98);
        _other.Location = new Point(300, 98);
        _lblB.Location = new Point(12, 136);
        _numB.Location = new Point(300, 132);

        _desc.SetBounds(12, 168, 940, 54);

        _run.SetBounds(12, 224, 120, 34);
        _run.BackColor = Color.FromArgb(59, 110, 245);
        _run.ForeColor = Color.White;
        _run.FlatStyle = FlatStyle.Flat;
        _run.FlatAppearance.BorderSize = 0;
        _random.SetBounds(142, 224, 160, 34);
        _sort.SetBounds(312, 224, 130, 34);
        _chain.Location = new Point(456, 231);
        _help.SetBounds(700, 224, 250, 34);

        _top.Controls.AddRange(new Control[]
        {
            lblOp, _op, lblList, _list, _lblA, _numA, _other, _lblB, _numB, _desc,
            _run, _random, _sort, _chain, _help,
        });
        _top.Resize += (_, _) => Relayout();

        _status.Dock = DockStyle.Bottom;
        _status.Height = 40;
        _status.Padding = new Padding(12, 8, 12, 0);
        _status.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        _status.Text = "Выберите операцию и нажмите «Выполнить». Если непонятно, что такое L, x, E, F — нажмите «Справка».";

        _legend.Dock = DockStyle.Bottom;
        _legend.Height = 24;
        _legend.Padding = new Padding(12, 2, 12, 0);
        _legend.ForeColor = Color.DimGray;
        _legend.Text = "Цвета:  жёлтый — найденный/затронутый элемент  ·  зелёный — новый/перемещённый  ·  красный — будет удалён";

        _history.Dock = DockStyle.Bottom;
        _history.Height = 96;
        _history.Font = new Font("Consolas", 9.5f);
        _history.HorizontalScrollbar = true;

        _canvas.Dock = DockStyle.Fill;

        // Fill-элемент добавляется первым, затем остальные (порядок важен для Dock)
        Controls.Add(_canvas);
        Controls.Add(_status);
        Controls.Add(_legend);
        Controls.Add(_history);
        Controls.Add(_top);

        AcceptButton = _run;
        Relayout();
    }

    /// <summary>Подгоняет ширину полей под окно и ставит поля параметров сразу после их подписей.</summary>
    private void Relayout()
    {
        int right = _top.ClientSize.Width - 12;
        _op.Width = Math.Max(200, right - _op.Left);
        _list.Width = Math.Max(200, right - _list.Left);
        _desc.Width = Math.Max(200, right - _desc.Left);
        _help.Left = Math.Max(_chain.Right + 12, right - _help.Width);

        _numA.Left = _lblA.Right + 8;
        _numB.Left = _lblB.Right + 8;
        _other.Left = _lblA.Right + 8;
        _other.Width = Math.Max(120, right - _other.Left);
    }

    // ------------------------------------------------------------------ события

    private void WireEvents()
    {
        _op.SelectedIndexChanged += (_, _) => OnOpChanged();
        _run.Click += (_, _) => RunOperation();
        _help.Click += (_, _) => ShowHelp();
        _random.Click += (_, _) =>
        {
            int n = _rnd.Next(5, 11);
            _list.Text = string.Join(" ", Enumerable.Range(0, n).Select(_ => _rnd.Next(1, 10)));
        };
        _sort.Click += (_, _) =>
        {
            if (!TryParse(_list.Text, out int[] arr, out string err)) { SetStatus(err, true); return; }
            Array.Sort(arr);
            _list.Text = string.Join(" ", arr);
        };
        _list.TextChanged += (_, _) =>
        {
            if (_suppressPreview) return;
            if (TryParse(_list.Text, out int[] arr, out _) && arr.Length <= ListOps.MaxLength)
            {
                _timer.Stop();
                _canvas.ShowOnly(arr);
            }
        };
        _timer.Tick += (_, _) =>
        {
            _canvas.Progress += 0.07f;
            if (_canvas.Progress >= 1f) _timer.Stop();
        };
    }

    private void OnOpChanged()
    {
        var op = CurrentOp;

        _lblA.Visible = op.LabelA is not null;
        _lblA.Text = op.LabelA ?? "";
        _lblB.Visible = op.LabelB is not null;
        _lblB.Text = op.LabelB ?? "";

        _numA.Visible = op.LabelA is not null && !op.UseOtherList;
        _numB.Visible = op.LabelB is not null;
        _other.Visible = op.UseOtherList;

        if (op.LabelA is not null && !op.UseOtherList) _numA.Value = op.DefaultA;
        if (op.LabelB is not null) _numB.Value = op.DefaultB;
        if (op.UseOtherList) _other.Text = op.DefaultOther;

        _desc.Text = op.Description;

        // подписи — по вертикали вровень с полями
        _lblA.Top = _numA.Top + 4;
        _lblB.Top = _numB.Top + 4;
        Relayout();
    }

    // ------------------------------------------------------------------ выполнение

    private static bool TryParse(string text, out int[] result, out string error)
    {
        result = Array.Empty<int>();
        error = "";
        var parts = text.Split(new[] { ' ', ',', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var values = new List<int>();
        foreach (var p in parts)
        {
            if (!int.TryParse(p, out int v)) { error = $"«{p}» — не целое число. Вводите числа через пробел."; return false; }
            values.Add(v);
        }
        result = values.ToArray();
        return true;
    }

    private int[] ParseListOrEmpty() => TryParse(_list.Text, out var arr, out _) ? arr : Array.Empty<int>();

    private void RunOperation()
    {
        if (!TryParse(_list.Text, out int[] arr, out string err)) { SetStatus(err, true); return; }
        var op = CurrentOp;

        int[]? other = null;
        if (op.UseOtherList && !TryParse(_other.Text, out other, out err))
        {
            SetStatus("Список E: " + err, true);
            return;
        }

        try
        {
            int a = (int)_numA.Value, b = (int)_numB.Value;
            // a и b попадают в те параметры, которые нужны выбранной операции: x / E / i и F / j
            var r = ListOps.Execute(new RunRequest(arr, op.Key, X: a, E: a, F: b, Other: other, I: a, J: b));

            _canvas.SetSections(BuildSections(op, r), animate: true);
            _timer.Start();

            SetStatus(r.Message, false);
            string result = $"[{string.Join(" ", r.After)}]" + (r.Second is null ? "" : $" и [{string.Join(" ", r.Second)}]");
            _history.Items.Insert(0,
                $"п.{op.ShortName}:  [{string.Join(" ", r.Before)}]  →  {result}"
                + (r.Consistent ? "" : "   (!) нарушена согласованность Head/Tail/Count"));

            if (_chain.Checked && r.Changed && r.Second is null)
            {
                _suppressPreview = true;
                _list.Text = string.Join(" ", r.After);
                _suppressPreview = false;
            }
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            SetStatus(ex.Message, true);
        }
    }

    /// <summary>Какие списки и как рисовать для данной операции.</summary>
    private static IReadOnlyList<ListSection> BuildSections(OpItem op, RunResult r)
    {
        var none = Array.Empty<int>();
        bool removal = op.Key is OpKeys.RemoveAll or OpKeys.NonUnique or OpKeys.KeepFirst;
        var sections = new List<ListSection>
        {
            new("СПИСОК L — ДО", r.Before, r.BeforeHl, removal ? HighlightKind.Removed : HighlightKind.Found),
        };

        switch (op.Key)
        {
            case OpKeys.Distinct:
                sections[0] = sections[0] with { Title = "СПИСОК L (жёлтым — первое вхождение каждого значения)" };
                break;

            case OpKeys.Concat:
                sections.Add(new("СПИСОК E — будет дописан в конец L", r.Input2 ?? none, none, HighlightKind.Found));
                sections.Add(new("СПИСОК L — ПОСЛЕ (зелёным — узлы, пришедшие из E)", r.After, r.AfterHl, HighlightKind.New, true));
                break;

            case OpKeys.Split:
                sections.Add(new("ПЕРВАЯ ЧАСТЬ (всё до x)", r.After, none, HighlightKind.New, true));
                sections.Add(new("ВТОРАЯ ЧАСТЬ (всё после x)", r.Second ?? none, none, HighlightKind.New, true));
                sections.Add(new("ИСХОДНЫЙ L ПОСЛЕ РАЗБИЕНИЯ", none, none, HighlightKind.Found));
                break;

            default:
                sections.Add(new("СПИСОК L — ПОСЛЕ", r.After, r.AfterHl, HighlightKind.New, true));
                break;
        }
        return sections;
    }

    private void SetStatus(string text, bool error)
    {
        _status.Text = text;
        _status.ForeColor = error ? Color.Firebrick : Color.FromArgb(31, 120, 70);
    }

    private void ShowHelp()
    {
        using var dlg = new Form
        {
            Text = "Справка: обозначения и операции",
            StartPosition = FormStartPosition.CenterParent,
            ClientSize = new Size(780, 660),
            MinimizeBox = false,
            MaximizeBox = false,
            ShowInTaskbar = false,
        };
        var box = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 10.5f),
            Text = HelpText.Full.Replace("\r\n", "\n").Replace("\n", "\r\n"),
        };
        dlg.Controls.Add(box);
        dlg.Shown += (_, _) => { box.SelectionStart = 0; box.SelectionLength = 0; };
        dlg.ShowDialog(this);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _timer.Dispose();
        base.Dispose(disposing);
    }
}
