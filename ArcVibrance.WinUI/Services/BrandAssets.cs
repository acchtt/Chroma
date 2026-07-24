using System.Reflection;

namespace ArcVibrance.Services;

internal static class BrandAssets
{
    private const string IconResourceName = "ArcVibrance.Brand.Chroma.ico";
    private const string LogoResourceName = "ArcVibrance.Brand.Chroma.png";

    private static readonly object Sync = new();
    private static bool _ready;

    public static string IconPath
    {
        get
        {
            EnsureExtracted();
            return Path.Combine(GetBrandDirectory(), "Chroma.ico");
        }
    }

    public static string LogoPath
    {
        get
        {
            EnsureExtracted();
            return Path.Combine(GetBrandDirectory(), "Chroma.png");
        }
    }

    public static void EnsureExtracted()
    {
        lock (Sync)
        {
            if (_ready)
            {
                return;
            }

            string directory = GetBrandDirectory();
            Directory.CreateDirectory(directory);
            Assembly assembly = typeof(BrandAssets).Assembly;

            ExtractResource(
                assembly,
                IconResourceName,
                Path.Combine(directory, "Chroma.ico"));
            ExtractResource(
                assembly,
                LogoResourceName,
                Path.Combine(directory, "Chroma.png"));

            _ready = true;
        }
    }

    private static string GetBrandDirectory() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ArcVibrance",
            "Brand",
            UpdateService.CurrentVersionTag);

    private static void ExtractResource(
        Assembly assembly,
        string resourceName,
        string destinationPath)
    {
        using Stream source = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Chroma is missing the embedded resource '{resourceName}'.");

        string temporaryPath = $"{destinationPath}.{Environment.ProcessId}.tmp";
        try
        {
            using (FileStream destination = new(
                       temporaryPath,
                       FileMode.Create,
                       FileAccess.Write,
                       FileShare.None))
            {
                source.CopyTo(destination);
                destination.Flush(flushToDisk: true);
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
    }
}
