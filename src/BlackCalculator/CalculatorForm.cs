using System.Globalization;
using BlackCalculator.Core;

namespace BlackCalculator;

public sealed class CalculatorForm : Form
{
    private static readonly Color Accent = Color.FromArgb(255, 159, 67);
    private readonly CalculatorEngine _engine = new(CultureInfo.CurrentCulture);
    private readonly Label _display;
    private readonly Label _status;

    public CalculatorForm()
    {
        Text = "Калькулятор";
        BackColor = Color.Black;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 11);
        ClientSize = new Size(380, 560);
        MinimumSize = new Size(350, 540);
        StartPosition = FormStartPosition.CenterScreen;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Color.Black
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(layout);

        layout.Controls.Add(new Label
        {
            Text = "КАЛЬКУЛЯТОР",
            ForeColor = Accent,
            Font = new Font("Segoe UI Semibold", 11),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);

        _display = new Label
        {
            Text = "0",
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            BackColor = Color.Black,
            Font = new Font("Segoe UI", 34),
            TextAlign = ContentAlignment.MiddleRight,
            AccessibleName = "Результат",
            UseMnemonic = false
        };
        _display.SizeChanged += (_, _) => FitDisplayFont();
        layout.Controls.Add(_display, 0, 1);

        _status = new Label
        {
            Text = "Enter — результат · Esc — сброс",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(160, 160, 160),
            Font = new Font("Segoe UI", 9),
            TextAlign = ContentAlignment.MiddleRight
        };
        layout.Controls.Add(_status, 0, 2);

        var keypad = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 5,
            Margin = new Padding(0, 10, 0, 0),
            BackColor = Color.Black
        };
        for (var i = 0; i < 4; i++)
            keypad.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        for (var i = 0; i < 5; i++)
            keypad.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        layout.Controls.Add(keypad, 0, 3);

        AddButton(keypad, "AC", 0, 0, _engine.Clear, "Сбросить всё", utility: true);
        AddButton(keypad, "C", 1, 0, _engine.ClearEntry, "Очистить текущее число", utility: true);
        AddButton(keypad, "⌫", 2, 0, _engine.Backspace, "Удалить последнюю цифру", utility: true);
        AddButton(keypad, "÷", 3, 0, () => _engine.SetOperation(Operation.Divide), "Разделить", utility: true);

        var digits = new[] { 7, 8, 9, 4, 5, 6, 1, 2, 3 };
        for (var i = 0; i < digits.Length; i++)
        {
            var digit = digits[i];
            AddButton(keypad, digit.ToString(CultureInfo.InvariantCulture), i % 3, i / 3 + 1,
                () => _engine.InputDigit(digit), digit.ToString(CultureInfo.InvariantCulture));
        }

        AddButton(keypad, "×", 3, 1, () => _engine.SetOperation(Operation.Multiply), "Умножить", utility: true);
        AddButton(keypad, "−", 3, 2, () => _engine.SetOperation(Operation.Subtract), "Вычесть", utility: true);
        AddButton(keypad, "+", 3, 3, () => _engine.SetOperation(Operation.Add), "Сложить", utility: true);
        AddButton(keypad, "±", 0, 4, _engine.ToggleSign, "Изменить знак");
        AddButton(keypad, "0", 1, 4, () => _engine.InputDigit(0), "Ноль");
        AddButton(keypad, CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, 2, 4,
            _engine.InputDecimalSeparator, "Десятичный разделитель");
        AddButton(keypad, "=", 3, 4, _engine.Calculate, "Вычислить результат", equals: true);
    }

    private void AddButton(TableLayoutPanel keypad, string text, int column, int row,
        Action action, string accessibleName, bool utility = false, bool equals = false)
    {
        var button = new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            Margin = new Padding(4),
            FlatStyle = FlatStyle.Flat,
            BackColor = equals ? Accent : utility ? Color.FromArgb(38, 38, 38) : Color.FromArgb(24, 24, 24),
            ForeColor = equals ? Color.Black : utility ? Accent : Color.White,
            Font = new Font("Segoe UI", text.Length > 1 ? 15 : 21),
            Cursor = Cursors.Hand,
            AccessibleName = accessibleName,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = equals
            ? Color.FromArgb(255, 183, 112) : Color.FromArgb(58, 58, 58);
        button.FlatAppearance.MouseDownBackColor = equals
            ? Color.FromArgb(224, 130, 40) : Color.FromArgb(75, 75, 75);
        button.Click += (_, _) => Execute(action);
        keypad.Controls.Add(button, column, row);
    }

    private void Execute(Action action)
    {
        action();
        _display.Text = _engine.Display;
        FitDisplayFont();
        _display.ForeColor = _engine.HasError ? Accent : Color.White;
        _status.Text = _engine.ErrorMessage ?? "Enter — результат · Esc — сброс";
    }

    private void FitDisplayFont()
    {
        var size = 34f;
        while (size > 10)
        {
            using var candidate = new Font("Segoe UI", size);
            var measured = TextRenderer.MeasureText(_display.Text, candidate,
                Size.Empty, TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            if (measured.Width <= _display.ClientSize.Width - 8)
                break;
            size--;
        }

        if (_display.Font.Size != size)
        {
            var previous = _display.Font;
            _display.Font = new Font("Segoe UI", size);
            previous.Dispose();
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        var key = keyData & Keys.KeyCode;
        var modifiers = keyData & Keys.Modifiers;
        if (modifiers == Keys.Control && key == Keys.C)
        {
            Clipboard.SetText(_engine.Display);
            return true;
        }
        if (modifiers is not (Keys.None or Keys.Shift))
            return base.ProcessCmdKey(ref msg, keyData);

        Action? action = key switch
        {
            Keys.D8 when modifiers == Keys.Shift => () => _engine.SetOperation(Operation.Multiply),
            Keys.Oemplus when modifiers == Keys.Shift => () => _engine.SetOperation(Operation.Add),
            >= Keys.D0 and <= Keys.D9 when modifiers == Keys.None => () => _engine.InputDigit(key - Keys.D0),
            >= Keys.NumPad0 and <= Keys.NumPad9 => () => _engine.InputDigit(key - Keys.NumPad0),
            Keys.Add => () => _engine.SetOperation(Operation.Add),
            Keys.Subtract or Keys.OemMinus => () => _engine.SetOperation(Operation.Subtract),
            Keys.Multiply => () => _engine.SetOperation(Operation.Multiply),
            Keys.Divide or Keys.OemQuestion => () => _engine.SetOperation(Operation.Divide),
            Keys.Decimal or Keys.OemPeriod or Keys.Oemcomma => _engine.InputDecimalSeparator,
            Keys.Enter or Keys.Oemplus => _engine.Calculate,
            Keys.Escape => _engine.Clear,
            Keys.Delete => _engine.ClearEntry,
            Keys.Back => _engine.Backspace,
            Keys.F9 => _engine.ToggleSign,
            _ => null
        };
        if (action is null)
            return base.ProcessCmdKey(ref msg, keyData);

        Execute(action);
        return true;
    }
}
