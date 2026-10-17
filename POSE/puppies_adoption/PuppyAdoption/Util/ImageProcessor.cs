using System;
using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Options;
using PuppyAdoption.Shared;

namespace PuppyAdoption.Util;

/// <summary>
///     Provides functionality to process images
///     Bytes of an image go in and it returns the bytes of jpeg with a defined size
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    ///     Resizes and encodes the image
    /// </summary>
    /// <param name="imageData">User provided image data</param>
    /// <returns>A JPG image with max. size as configured</returns>
    public byte[] ProcessImage(byte[] imageData);
}

internal sealed class ImageProcessor(IOptions<Settings> options) : IImageProcessor
{
    private const int JpegQuality = 75;

    private readonly int _maxImageSize = options.Value.MaxImageSizePixel;

    public byte[] ProcessImage(byte[] imageData)
    {
        using var input = new MemoryStream(imageData); // weil Bimap einen steam braucht
        using var decoded = new Bitmap(input);
        var resizedSize = GetResizedSize(decoded.PixelSize);
        // wenn die Größe nicht der resized Größe entspricht wird dementsprechend eine Kopie mit der richtigen Größe
        // angelegt
        using var resized = resizedSize == decoded.PixelSize
            ? null
            : decoded.CreateScaledBitmap(resizedSize, BitmapInterpolationMode.HighQuality);

        using var output = new MemoryStream();
        (resized ?? decoded).Save(output, new JpegBitmapEncoderOptions { Quality = JpegQuality });

        return output.ToArray();
    }

    private PixelSize GetResizedSize(PixelSize size)
    {
        // TODO - ensure largest side is not larger than max size as configured in the application settings
        // Länge * skalierung = max -> skalierung = max / länge
        int longerSide = Math.Max(size.Width, size.Height);
        if (longerSide < _maxImageSize)
        {
            return size;
        }

        // damit das Verhältnis der Bilder beibehalten wird muss man diese mit einem faktor multiplizieren.
        double scale = (double)_maxImageSize / longerSide;

        var newWidth = (int)Math.Round(size.Width * scale);
        var newHeight = (int)Math.Round(size.Height * scale);

        return new PixelSize(newWidth, newHeight);
    }
}
