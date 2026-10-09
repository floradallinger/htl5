using System;
using ColorPicker.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace ColorPicker.ViewModels;

public partial class ColorPickerControlViewModel : ViewModelBase
{
    // TODO
    private ColorData _colorData;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Hex))]
    [NotifyPropertyChangedFor(nameof(Hex))]
    private byte _red;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Hex))]
    [NotifyPropertyChangedFor(nameof(Hex))]
    private byte _green;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Hex))]
    [NotifyPropertyChangedFor(nameof(Hex))]
    private byte _blue;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Hex))]
    [NotifyPropertyChangedFor(nameof(Hex))]
    private byte _alpha;
    
    public ColorDisplayControlViewModel ColorDisplay { get; }
    public string Hex => _colorData?.Hex ?? string.Empty;
    public string RGBA => _colorData.Rgba;

    public ColorPickerControlViewModel(ColorData colorData)
    {
        _colorData = colorData;
        
        _red = colorData.Red;
        _green = colorData.Green;
        _blue = colorData.Blue;
        _alpha = colorData.Alpha;
        
        ColorDisplay = new ColorDisplayControlViewModel(_colorData, false);
    }

    // TODO
    partial void OnRedChanged(byte value)
    {
        _colorData.Red = value;
        NotifyColorChange();
    }
    partial void OnGreenChanged(byte value)
    {
        _colorData.Green = value;
        NotifyColorChange();
    }
    partial void OnBlueChanged(byte value)
    {
        _colorData.Blue = value;
        NotifyColorChange();
    }
    partial void OnAlphaChanged(byte value)
    {
        _colorData.Alpha = value;
        NotifyColorChange();
    }

    private void NotifyColorChange()
    {
        ColorDisplay.Refresh();
        Messenger.Send(new ColorDataChanged());
    }
    
    [RelayCommand]
    private void RandomizeColor()
    {
        Red = (byte) Random.Shared.Next(256);
        Green = (byte) Random.Shared.Next(256);
        Blue = (byte) Random.Shared.Next(256);
        Alpha = (byte) Random.Shared.Next(128, 256); 
    }
}

public sealed record ColorDataChanged;

public sealed class DesignColorPickerControlViewModel() : ColorPickerControlViewModel(ColorData.Default);
