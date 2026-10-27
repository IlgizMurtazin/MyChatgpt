using System.Globalization;

namespace BlackCalculator.Core;

public enum Operation { Add, Subtract, Multiply, Divide }

public sealed class CalculatorEngine
{
    private readonly CultureInfo _culture;
    private string _input = "0";
    private decimal? _accumulator;
    private Operation? _pendingOperation;
    private Operation? _lastOperation;
    private decimal? _lastOperand;
    private bool _replaceInput;

    public CalculatorEngine(CultureInfo? culture = null)
    {
        _culture = culture ?? CultureInfo.InvariantCulture;
    }

    public string Display => HasError ? "Ошибка" : _input;
    public bool HasError { get; private set; }
    public string? ErrorMessage { get; private set; }
    private string Separator => _culture.NumberFormat.NumberDecimalSeparator;
    private decimal Value => decimal.Parse(_input, NumberStyles.Number, _culture);

    public void InputDigit(int digit)
    {
        if (digit is < 0 or > 9)
            throw new ArgumentOutOfRangeException(nameof(digit));

        PrepareInput();
        if (_input.Count(char.IsDigit) >= 28)
            return;

        _input = _input switch
        {
            "0" => digit.ToString(CultureInfo.InvariantCulture),
            "-0" => "-" + digit.ToString(CultureInfo.InvariantCulture),
            _ => _input + digit.ToString(CultureInfo.InvariantCulture)
        };
    }

    public void InputDecimalSeparator()
    {
        PrepareInput();
        if (!_input.Contains(Separator, StringComparison.Ordinal))
            _input += Separator;
    }

    public void SetOperation(Operation operation)
    {
        if (HasError)
            return;

        if (_pendingOperation is not null && !_replaceInput)
        {
            if (!TryApply(_accumulator!.Value, Value, _pendingOperation.Value))
                return;
        }

        _accumulator = Value;
        _pendingOperation = operation;
        _lastOperation = null;
        _lastOperand = null;
        _replaceInput = true;
    }

    public void Calculate()
    {
        if (HasError)
            return;

        if (_pendingOperation is not null)
        {
            var operation = _pendingOperation.Value;
            var operand = Value;
            if (!TryApply(_accumulator!.Value, operand, operation))
                return;

            _lastOperation = operation;
            _lastOperand = operand;
            _pendingOperation = null;
            _accumulator = null;
        }
        else if (_lastOperation is not null && _lastOperand is not null)
        {
            if (!TryApply(Value, _lastOperand.Value, _lastOperation.Value))
                return;
        }

        _replaceInput = true;
    }

    public void ToggleSign()
    {
        if (HasError)
            return;

        if (_replaceInput && _pendingOperation is not null)
        {
            _input = "0";
            _replaceInput = false;
        }

        _input = _input.StartsWith('-') ? _input[1..] : "-" + _input;
    }

    public void Backspace()
    {
        if (HasError)
        {
            Clear();
            return;
        }

        if (_replaceInput)
            return;

        _input = _input.Length > 1 ? _input[..^1] : "0";
        if (_input == "-")
            _input = "0";
    }

    public void ClearEntry()
    {
        if (HasError)
        {
            Clear();
            return;
        }

        _input = "0";
        _replaceInput = false;
        _lastOperation = null;
        _lastOperand = null;
    }

    public void Clear()
    {
        _input = "0";
        _accumulator = null;
        _pendingOperation = null;
        _lastOperation = null;
        _lastOperand = null;
        _replaceInput = false;
        HasError = false;
        ErrorMessage = null;
    }

    private void PrepareInput()
    {
        if (HasError)
            Clear();

        if (_replaceInput)
        {
            _input = "0";
            _replaceInput = false;
            _lastOperation = null;
            _lastOperand = null;
        }
    }

    private bool TryApply(decimal left, decimal right, Operation operation)
    {
        try
        {
            var result = operation switch
            {
                Operation.Add => left + right,
                Operation.Subtract => left - right,
                Operation.Multiply => left * right,
                Operation.Divide => left / right,
                _ => throw new ArgumentOutOfRangeException(nameof(operation))
            };
            _input = result.ToString("0.############################", _culture);
            return true;
        }
        catch (DivideByZeroException)
        {
            ErrorMessage = "На ноль делить нельзя";
        }
        catch (OverflowException)
        {
            ErrorMessage = "Число слишком большое";
        }

        HasError = true;
        return false;
    }
}
