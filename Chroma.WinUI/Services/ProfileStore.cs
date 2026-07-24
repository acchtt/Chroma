using System.Text;
using System.Text.RegularExpressions;
using Chroma.Models;

namespace Chroma.Services;

public sealed partial class ProfileStore
{
    private static readonly string LegacyProfilesPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ArcVibrance",
        "profiles.txt");

    public string ProfilesPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Chroma",
        "profiles.txt");

    public async Task<IReadOnlyList<GameProfile>> LoadAsync(CancellationToken cancellationToken = default)
    {
        await MigrateLegacyProfilesAsync(cancellationToken);

        if (!File.Exists(ProfilesPath))
        {
            return Array.Empty<GameProfile>();
        }

        string[] lines = await File.ReadAllLinesAsync(ProfilesPath, cancellationToken);
        if (lines.Length == 0)
        {
            throw new InvalidDataException("The profiles file is empty.");
        }

        Match header = HeaderRegex().Match(lines[0]);
        if (!header.Success)
        {
            throw new InvalidDataException("Unsupported Chroma profile format.");
        }

        int version = int.Parse(header.Groups[1].Value);
        if (version is not 1 and not 2)
        {
            throw new InvalidDataException($"Unsupported profile format version {version}.");
        }

        var profiles = new List<GameProfile>();
        for (int index = 1; index < lines.Length; index++)
        {
            if (string.IsNullOrWhiteSpace(lines[index]))
            {
                continue;
            }

            Match match = ProfileRegex().Match(lines[index]);
            if (!match.Success)
            {
                throw new InvalidDataException($"Invalid profile entry on line {index + 1}.");
            }

            string path = UnescapeQuoted(match.Groups[1].Value);
            if (path.Length == 0)
            {
                continue;
            }

            int saturation = Math.Clamp(int.Parse(match.Groups[2].Value), 0, 300);
            bool enabled = version < 2 || !match.Groups[3].Success || match.Groups[3].Value != "0";
            profiles.Add(new GameProfile
            {
                ExecutablePath = path,
                SaturationPercent = saturation,
                Enabled = enabled
            });
        }

        return profiles;
    }

    public async Task SaveAsync(IEnumerable<GameProfile> profiles, CancellationToken cancellationToken = default)
    {
        string? directory = Path.GetDirectoryName(ProfilesPath);
        if (directory is null)
        {
            throw new InvalidOperationException("Could not resolve the Chroma settings directory.");
        }

        Directory.CreateDirectory(directory);
        string temporaryPath = ProfilesPath + ".tmp";
        var output = new StringBuilder("ChromaProfiles 2\n");

        foreach (GameProfile profile in profiles)
        {
            output.Append('"')
                .Append(EscapeQuoted(profile.ExecutablePath))
                .Append("\" ")
                .Append(Math.Clamp(profile.SaturationPercent, 0, 300))
                .Append(' ')
                .Append(profile.Enabled ? '1' : '0')
                .Append('\n');
        }

        await File.WriteAllTextAsync(temporaryPath, output.ToString(), new UTF8Encoding(false), cancellationToken);
        File.Move(temporaryPath, ProfilesPath, true);
    }

    private async Task MigrateLegacyProfilesAsync(CancellationToken cancellationToken)
    {
        if (File.Exists(ProfilesPath) || !File.Exists(LegacyProfilesPath))
        {
            return;
        }

        string? directory = Path.GetDirectoryName(ProfilesPath);
        if (directory is null)
        {
            return;
        }

        Directory.CreateDirectory(directory);
        try
        {
            await using FileStream source = File.OpenRead(LegacyProfilesPath);
            await using FileStream destination = new(
                ProfilesPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);
            await source.CopyToAsync(destination, cancellationToken);
        }
        catch (IOException) when (File.Exists(ProfilesPath))
        {
            // The native agent may complete the same one-time migration first.
        }
    }

    private static string EscapeQuoted(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

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

    [GeneratedRegex(@"^\s*(?:ChromaProfiles|ArcVibranceProfiles)\s+(\d+)\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex HeaderRegex();

    [GeneratedRegex("^\\s*\\\"((?:\\\\.|[^\\\"])*)\\\"\\s+(\\d+)(?:\\s+([01]))?\\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex ProfileRegex();
}
