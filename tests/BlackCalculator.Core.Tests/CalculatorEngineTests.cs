using System.Globalization;
using BlackCalculator.Core;
using Xunit;

namespace BlackCalculator.Core.Tests;

public sealed class CalculatorEngineTests
{
    [Theory]
    [InlineData(Operation.Add, "20")]
    [InlineData(Operation.Subtract, "10")]
    [InlineData(Operation.Multiply, "75")]
    [InlineData(Operation.Divide, "3")]
    public void PerformsArithmetic(Operation operation, string expected)
    {
        var calculator = new CalculatorEngine();
        Enter(calculator, "15");
        calculator.SetOperation(operation);
        Enter(calculator, "5");
        calculator.Calculate();
        Assert.Equal(expected, calculator.Display);
    }

    [Fact]
    public void DecimalArithmeticIsExact()
    {
        var calculator = new CalculatorEngine();
        Enter(calculator, "0.1");
        calculator.SetOperation(Operation.Add);
        Enter(calculator, "0.2");
        calculator.Calculate();
        Assert.Equal("0.3", calculator.Display);
    }

    [Fact]
    public void SupportsRussianDecimalSeparator()
    {
        var calculator = new CalculatorEngine(CultureInfo.GetCultureInfo("ru-RU"));
        Enter(calculator, "1,5");
        calculator.SetOperation(Operation.Multiply);
        Enter(calculator, "2");
        calculator.Calculate();
        Assert.Equal("3", calculator.Display);
        calculator.InputDecimalSeparator();
        Assert.Equal("0,", calculator.Display);
    }

    [Fact]
    public void ChainedOperationsEvaluateFromLeftToRight()
    {
        var calculator = new CalculatorEngine();
        Enter(calculator, "2");
        calculator.SetOperation(Operation.Add);
        Enter(calculator, "3");
        calculator.SetOperation(Operation.Multiply);
        Enter(calculator, "4");
        calculator.Calculate();
        Assert.Equal("20", calculator.Display);
    }

    [Fact]
    public void ConsecutiveOperatorsReplaceThePendingOperator()
    {
        var calculator = new CalculatorEngine();
        Enter(calculator, "8");
        calculator.SetOperation(Operation.Add);
        calculator.SetOperation(Operation.Divide);
        Enter(calculator, "2");
        calculator.Calculate();
        Assert.Equal("4", calculator.Display);
    }

    [Fact]
    public void EqualsRepeatsTheLastOperation()
    {
        var calculator = new CalculatorEngine();
        Enter(calculator, "2");
        calculator.SetOperation(Operation.Add);
        Enter(calculator, "3");
        calculator.Calculate();
        calculator.Calculate();
        Assert.Equal("8", calculator.Display);
    }

    [Fact]
    public void TypingAfterEqualsStartsANewCalculation()
    {
        var calculator = new CalculatorEngine();
        Enter(calculator, "2");
        calculator.SetOperation(Operation.Add);
        Enter(calculator, "3");
        calculator.Calculate();
        Enter(calculator, "7");
        calculator.Calculate();
        Assert.Equal("7", calculator.Display);
    }

    [Fact]
    public void DivisionByZeroShowsAnErrorAndTypingRecovers()
    {
        var calculator = new CalculatorEngine();
        Enter(calculator, "5");
        calculator.SetOperation(Operation.Divide);
        calculator.InputDigit(0);
        calculator.Calculate();
        Assert.True(calculator.HasError);
        Assert.Equal("На ноль делить нельзя", calculator.ErrorMessage);
        calculator.InputDigit(9);
        Assert.Equal("9", calculator.Display);
        Assert.False(calculator.HasError);
        Assert.Null(calculator.ErrorMessage);
    }

    [Fact]
    public void OverflowShowsAnErrorWithoutThrowing()
    {
        var calculator = new CalculatorEngine();
        Enter(calculator, new string('9', 28));
        calculator.SetOperation(Operation.Multiply);
        Enter(calculator, "9");
        calculator.Calculate();
        Assert.True(calculator.HasError);
        Assert.Equal("Число слишком большое", calculator.ErrorMessage);
        calculator.Clear();
        Assert.Equal("0", calculator.Display);
    }

    [Fact]
    public void ClearEntryPreservesThePendingOperation()
    {
        var calculator = new CalculatorEngine();
        Enter(calculator, "12");
        calculator.SetOperation(Operation.Add);
        Enter(calculator, "99");
        calculator.ClearEntry();
        Enter(calculator, "3");
        calculator.Calculate();
        Assert.Equal("15", calculator.Display);
    }

    [Fact]
    public void ClearResetsTheEntireCalculation()
    {
        var calculator = new CalculatorEngine();
        Enter(calculator, "12");
        calculator.SetOperation(Operation.Add);
        calculator.Clear();
        Enter(calculator, "3");
        calculator.Calculate();
        Assert.Equal("3", calculator.Display);
    }

    [Fact]
    public void SignCanBeSetBeforeEnteringTheSecondOperand()
    {
        var calculator = new CalculatorEngine();
        calculator.InputDigit(2);
        calculator.SetOperation(Operation.Add);
        calculator.ToggleSign();
        calculator.InputDigit(3);
        calculator.Calculate();
        Assert.Equal("-1", calculator.Display);
        calculator.ToggleSign();
        Assert.Equal("1", calculator.Display);
    }

    [Fact]
    public void BackspaceRemovesDigitsAndTheDecimalSeparator()
    {
        var calculator = new CalculatorEngine();
        Enter(calculator, "1.5");
        calculator.Backspace();
        Assert.Equal("1.", calculator.Display);
        calculator.Backspace();
        Assert.Equal("1", calculator.Display);
        calculator.ToggleSign();
        calculator.Backspace();
        Assert.Equal("0", calculator.Display);
    }

    [Fact]
    public void DuplicateDecimalSeparatorsAndLeadingZerosAreIgnored()
    {
        var calculator = new CalculatorEngine();
        Enter(calculator, "0001..25");
        Assert.Equal("1.25", calculator.Display);
    }

    [Fact]
    public void InputLengthIsLimitedToDecimalPrecision()
    {
        var calculator = new CalculatorEngine();
        Enter(calculator, new string('9', 40));
        Assert.Equal(new string('9', 28), calculator.Display);
        calculator.Calculate();
        Assert.False(calculator.HasError);
    }

    private static void Enter(CalculatorEngine calculator, string input)
    {
        foreach (var character in input)
        {
            if (character is '.' or ',')
                calculator.InputDecimalSeparator();
            else
                calculator.InputDigit(character - '0');
        }
    }
}
