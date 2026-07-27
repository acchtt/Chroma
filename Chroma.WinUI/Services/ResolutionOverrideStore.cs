using System.Text;
using System.Text.RegularExpressions;

namespace Chroma.Services;

public readonly record struct ResolutionOverride(int Width, int Height);

public sealed class ResolutionOverrideStore
{
    public const int MinimumWidth = 640;
    public const int MaximumWidth = 16384;
    public const int MinimumHeight = 480;
    public const int MaximumHeight = 8640;

    private static readonly Regex EntryRegex = new(
        "^\\s*\\\"((?:\\\\.|[^\\\"])*)\\\"\\s+(\\d+)\\s+(\\d+)\\s*$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public string FilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Chroma",
        "resolutions.txt");

    public Dictionary<string, ResolutionOverride> Load()
    {
        var overrides = new Dictionary<string, ResolutionOverride>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(FilePath))
        {
            return overrides;
        }

        string[] lines = File.ReadAllLines(FilePath);
        if (lines.Length == 0 ||
            !string.Equals(lines[0].Trim(), "ChromaResolutions 1", StringComparison.Ordinal))
        {
            return overrides;
        }

        foreach (string line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            Match match = EntryRegex.Match(line);
            if (!match.Success)
            {
                continue;
            }

            string executablePath = UnescapeQuoted(match.Groups[1].Value);
            if (int.TryParse(match.Groups[2].Value, out int width) &&
                int.TryParse(match.Groups[3].Value, out int height) &&
                IsValid(width, height))
            {
                overrides[executablePath] = new ResolutionOverride(width, height);
            }
        }

        return overrides;
    }

    public async Task SaveAsync(
        IReadOnlyDictionary<string, ResolutionOverride> overrides,
        CancellationToken cancellationToken = default)
    {
        string? directory = Path.GetDirectoryName(FilePath);
        if (directory is null)
        {
            throw new InvalidOperationException("Could not resolve the CHROMA settings directory.");
        }

        Directory.CreateDirectory(directory);
        string temporaryPath = FilePath + ".tmp";
        var output = new StringBuilder("ChromaResolutions 1\n");

        foreach ((string executablePath, ResolutionOverride resolution) in
                 overrides.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase))
        {
            output.Append('"')
                .Append(EscapeQuoted(executablePath))
                .Append("\" ")
                .Append(resolution.Width)
                .Append(' ')
                .Append(resolution.Height)
                .Append('\n');
        }

        await File.WriteAllTextAsync(
            temporaryPath,
            output.ToString(),
            new UTF8Encoding(false),
            cancellationToken);
        File.Move(temporaryPath, FilePath, true);
    }

    public static bool IsValid(int width, int height) =>
        width is >= MinimumWidth and <= MaximumWidth &&
        height is >= MinimumHeight and <= MaximumHeight;

    private static string EscapeQuoted(string value) =>
        value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string UnescapeQuoted(string value)
    {
        var output = new StringBuilder(value.Length);
        bool escaped = false;
        foreach (char character in value)
        {
            if (escaped)
            {
                output.Append(character);
                escaped = false;
            }
            else if (character == '\\')
            {
                escaped = true;
            }
            else
            {
                output.Append(character);
            }
        }

        if (escaped)
        {
            output.Append('\\');
        }

        return output.ToString();
    }
}
