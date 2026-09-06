using Qbs.Domain;
using SkiaSharp;

namespace Qbs.Infrastructure;

/// <summary>Reads LibRaw's binary RGB output; it is converted to a metadata-free PNG for Skia.</summary>
public static class PpmDecoder
{
    public static Stream Decode(Stream input)
    {
        string Token()
        {
            var bytes = new List<byte>();
            int next;
            do
            {
                next = input.ReadByte();
                if (next == '#')
                    while (next >= 0 && next != '\n')
                        next = input.ReadByte();
            } while (next >= 0 && char.IsWhiteSpace((char)next));
            while (next >= 0 && !char.IsWhiteSpace((char)next))
            {
                bytes.Add((byte)next);
                next = input.ReadByte();
            }
            return System.Text.Encoding.ASCII.GetString(bytes.ToArray());
        }

        Rules.Require(Token() == "P6", "RAW converter returned an unsupported image.");
        var width = int.Parse(Token(), System.Globalization.CultureInfo.InvariantCulture);
        var height = int.Parse(Token(), System.Globalization.CultureInfo.InvariantCulture);
        var maximum = int.Parse(Token(), System.Globalization.CultureInfo.InvariantCulture);
        Rules.Require(
            width > 0 && height > 0 && (long)width * height <= 200000000 && maximum is 255 or 65535,
            "RAW output dimensions or depth are unsupported."
        );
        using var bitmap = new SKBitmap(width, height);
        byte Channel()
        {
            var high = input.ReadByte();
            var low = maximum == 65535 ? input.ReadByte() : high;
            Rules.Require(high >= 0 && low >= 0, "RAW output is truncated.");
            return (byte)high;
        }
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            bitmap.SetPixel(x, y, new SKColor(Channel(), Channel(), Channel()));
        using var png = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        return new MemoryStream(png.ToArray());
    }
}
