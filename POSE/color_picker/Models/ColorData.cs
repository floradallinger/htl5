using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Avalonia.Media;

namespace ColorPicker.Models;

public sealed class ColorData
{
    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }
    public byte Alpha { get; set; }

    // TODO
    public Color Color => new Color(Red, Green, Blue, Alpha);
    public string Hex => $"#{Red:X2}{Green:X2}{Blue:X2}";
    public string Rgba
    {
        get
        {
            var alpha = (Alpha / 255.0).ToString("0.##", CultureInfo.InvariantCulture);
            return $"rgba({Red}, {Green}, {Blue}, {alpha})";
        }
    }

    public static ColorData Default => new ColorData { Red = 255, Green = 255, Blue = 255, Alpha = 255 };
    public static ColorData FromColor(Color color) => new ColorData {Red = color.R, Green = color.G, 
        Blue = color.B, Alpha = color.A};
}
