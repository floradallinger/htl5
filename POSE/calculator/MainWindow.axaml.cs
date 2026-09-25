using Avalonia.Controls;

namespace Calculator;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InitializeNumbers();
    }

    private void InitializeNumbers()
    {
        for (int i = 0; i < 10; i++)
        {
            Panel.Children.Add(new Button());
        }
    }
}