using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Calculator;

public partial class MainWindow : Window
{
    public string _calculation = "";
    public List<double> _numbers = new List<double>();
    public List<string> _calcsigns = new List<string>();
    private string _newNumber = "";
    public string _result = "";
    
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
            _numbers.Add(double.Parse(_newNumber));
            _calcsigns.Add(value);
            _calculation += value;
            _newNumber = "";
            
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
            UpdateCalcDisplay();
        }
    }

    private void HandleCommaBtnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
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
}