using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: Chroma.IconBuilder <source.png> <destination.ico>");
    return 2;
}

string sourcePath = Path.GetFullPath(args[0]);
string destinationPath = Path.GetFullPath(args[1]);

if (!File.Exists(sourcePath))
{
    Console.Error.WriteLine($"Icon source was not found: {sourcePath}");
    return 3;
}

using Image<Rgba32> source = Image.Load<Rgba32>(sourcePath);
if (source.Width != source.Height || source.Width < 256)
{
    Console.Error.WriteLine(
        $"Icon source must be square and at least 256 px; received {source.Width}x{source.Height}.");
    return 4;
}

int[] sizes = [16, 20, 24, 32, 40, 48, 64, 128, 256];
var entries = new List<IconEntry>(sizes.Length);
var pngEncoder = new PngEncoder
{
    ColorType = PngColorType.RgbWithAlpha
};

foreach (int size in sizes)
{
    using Image<Rgba32> resized = source.Clone(context => context.Resize(
        new ResizeOptions
        {
            Size = new Size(size, size),
            Mode = ResizeMode.Stretch,
            Sampler = KnownResamplers.Lanczos3
        }));

    using var stream = new MemoryStream();
    resized.Save(stream, pngEncoder);
    entries.Add(new IconEntry(size, stream.ToArray()));
}

string? destinationDirectory = Path.GetDirectoryName(destinationPath);
if (!string.IsNullOrWhiteSpace(destinationDirectory))
{
    Directory.CreateDirectory(destinationDirectory);
}

string temporaryPath = destinationPath + ".tmp";
try
{
    using (var file = new FileStream(
        temporaryPath,
        FileMode.Create,
        FileAccess.Write,
        FileShare.None))
    using (var writer = new BinaryWriter(file))
    {
        writer.Write((ushort)0); // Reserved.
        writer.Write((ushort)1); // Icon resource.
        writer.Write((ushort)entries.Count);

        int dataOffset = 6 + (16 * entries.Count);
        foreach (IconEntry entry in entries)
        {
            writer.Write((byte)(entry.Size == 256 ? 0 : entry.Size));
            writer.Write((byte)(entry.Size == 256 ? 0 : entry.Size));
            writer.Write((byte)0); // Color palette count.
            writer.Write((byte)0); // Reserved.
            writer.Write((ushort)1); // Color planes.
            writer.Write((ushort)32); // Bits per pixel.
            writer.Write((uint)entry.PngBytes.Length);
            writer.Write((uint)dataOffset);
            dataOffset += entry.PngBytes.Length;
        }

        foreach (IconEntry entry in entries)
        {
            writer.Write(entry.PngBytes);
        }
    }

    File.Move(temporaryPath, destinationPath, overwrite: true);
}
finally
{
    if (File.Exists(temporaryPath))
    {
        File.Delete(temporaryPath);
    }
}

Console.WriteLine(
    $"Generated {destinationPath} from {sourcePath} with sizes: {string.Join(", ", sizes)} px.");
return 0;

internal sealed record IconEntry(int Size, byte[] PngBytes);
