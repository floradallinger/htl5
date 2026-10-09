using Avalonia.Controls;
using Avalonia.Input;

namespace ColorPicker.Controls;

public sealed partial class ColorPickerControl : UserControl
{
    public ColorPickerControl()
    {
        InitializeComponent();
    }

    private void HandleTextSelection(object? sender, TappedEventArgs e)
    {
        // TODO
        if (sender is not SelectableTextBlock textBlock)
        {
            return;
        }

        textBlock.SelectAll();
    }
}

