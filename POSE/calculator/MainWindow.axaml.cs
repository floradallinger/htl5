using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using NCalc;

namespace Calculator;

public partial class MainWindow : Window
{
    public string _calculation = "";
    public List<double> _numbers = new List<double>();
    public List<string> _calcsigns = new List<string>();
    private string _newNumber = "";
    public string _result = "";
    private double? _rawNumericResult = null;
    private bool _commaAllowed = true;
    
    public MainWindow()
    {
        InitializeComponent();
        UpdateCalcDisplay();
        UpdateResultDisplay();
    }

    private void HandleNumberBtnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Content != null)
        {
            string value = btn.Content.ToString()!;
            _newNumber += value;
            _calculation += value;
                
            UpdateCalcDisplay();
        }
    }

    private void HandleCalcSignBtnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Content != null && _newNumber != "")
        {
            string value = btn.Content.ToString()!;

            if (_newNumber.EndsWith("."))
            {
                _newNumber += "0";
            }
            
            _numbers.Add(double.Parse(_newNumber, CultureInfo.InvariantCulture));
            _calcsigns.Add(value);
            _calculation += value;
            _newNumber = "";
            _commaAllowed = true;
            
            UpdateCalcDisplay();
        }
    }

    private void HandleClearBtnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Content != null)
        {
            _calcsigns.Clear();
            _numbers.Clear();
            _calculation = "";
            _result = "";
            UpdateCalcDisplay();
            UpdateResultDisplay();
        }
    }

    private void HandleCommaBtnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Content != null && _commaAllowed)
        {
            _commaAllowed = false;
            var value = "";
            if (_newNumber.Length < 1)
            {
                value = "0.";
            }
            else
            {
                value += ".";
            }

            _newNumber += value;
            _calculation += value;
        }

    }

    private void HandleEqualBtnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Content != null && _calcsigns.Count > 0)
        {
            if (_newNumber != "")
            {
                _numbers.Add(double.Parse(_newNumber));
                _newNumber = "";
            }
            else
            {
                return;
            }

            double result = _numbers[0];
            for (int i = 0; i < _calcsigns.Count; i++)
            {
                var currentOp = _calcsigns[i];
                var nextNum = _numbers[i + 1];

                result = currentOp switch
                {
                    "+" => result + nextNum,
                    "-" => result - nextNum,
                    "*" => result * nextNum,
                    "/" => result != 0 || nextNum != 0 ? result / nextNum : double.NaN,
                    _ => result
                };
            }

            _result = result.ToString();
            UpdateResultDisplay();
        }
    }
    
    private void UpdateCalcDisplay()
    {
        CalcDisplay.Text = $"{_calculation}";
    }
    private void UpdateResultDisplay()
    {
        ResultDisplay.Text = $"{_result}";
    }

    private void OnModeChanged(object? sender, RoutedEventArgs e)
    {
        bool isComplexMode = ModeToggleSwitch.IsChecked ?? false;

        EasyMode.IsVisible = !isComplexMode;
        ComplexMode.IsVisible = isComplexMode;
        
        _result = "";
        _rawNumericResult = null;
        UpdateResultDisplay();
    }

    private void FormatAndDisplayResult()
    {
        if (_rawNumericResult.HasValue)
        {
            int decimals = (int)PrecisionSlider.Value;
            _result = Math.Round(_rawNumericResult.Value, decimals)
                .ToString($"F{decimals}", CultureInfo.InvariantCulture);
            UpdateResultDisplay();
        }
    }

    private void HandleEvaluateBtnClick(object? sender, RoutedEventArgs e)
    {
        string expressionString = ExpressionInput.Text ?? "";
        if (string.IsNullOrWhiteSpace(expressionString)) return;

        try
        {
            var expr = new Expression(expressionString);

            object evalResult = expr.Evaluate();

            if (evalResult != null)
            {
                _rawNumericResult = Convert.ToDouble(evalResult, CultureInfo.InvariantCulture);
                FormatAndDisplayResult();
            }
        }
        catch (Exception)
        {
            _result = "Error";
            UpdateResultDisplay();
        }
    }

    private void OnPrecisionChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        FormatAndDisplayResult();
    }
}