using System.Net;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Chroma.Services;

public sealed class UpdateService
{
    private const string LatestReleaseEndpoint =
        "https://api.github.com/repos/acchtt/Chroma/releases/latest";

    public static string CurrentVersionTag { get; } = GetCurrentVersionTag();

    private static readonly HttpClient Client = CreateHttpClient();

    public static async Task CleanupStaleUpdateFilesAsync()
    {
        // Give the external updater time to finish copying, verification,
        // rollback cleanup, and its final log write before removing artifacts.
        await Task.Delay(TimeSpan.FromSeconds(15)).ConfigureAwait(false);

        string updatesRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Chroma",
            "Updates");
        if (!Directory.Exists(updatesRoot))
        {
            return;
        }

        try
        {
            string retainedLogPath = Path.Combine(updatesRoot, "last-update.log");
            string? latestLogPath = Directory
                .EnumerateFiles(updatesRoot, "update.log", SearchOption.AllDirectories)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(latestLogPath) &&
                File.Exists(latestLogPath))
            {
                File.Copy(latestLogPath, retainedLogPath, overwrite: true);
            }

            foreach (string directory in Directory.EnumerateDirectories(updatesRoot))
            {
                try
                {
                    Directory.Delete(directory, recursive: true);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }

            foreach (string file in Directory.EnumerateFiles(updatesRoot))
            {
                if (string.Equals(
                        Path.GetFullPath(file),
                        Path.GetFullPath(retainedLogPath),
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    File.Delete(file);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    public async Task<UpdateCheckResult> CheckForUpdatesAsync(
        CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, LatestReleaseEndpoint);
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

        using HttpResponseMessage response = await Client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.Forbidden &&
            response.Headers.TryGetValues("X-RateLimit-Remaining", out IEnumerable<string>? remaining) &&
            remaining.Contains("0", StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                "GitHub's update-check limit has been reached. Please try again later.");
        }

        response.EnsureSuccessStatusCode();

        await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        GitHubRelease? release = await JsonSerializer.DeserializeAsync<GitHubRelease>(
            stream,
            cancellationToken: cancellationToken);

        if (release is null ||
            release.Draft ||
            release.Prerelease ||
            string.IsNullOrWhiteSpace(release.TagName))
        {
            throw new InvalidDataException(
                "GitHub did not return a valid stable Chroma release.");
        }

        Version currentVersion = ParseVersion(CurrentVersionTag);
        Version latestVersion = ParseVersion(release.TagName);
        Uri releaseUri = GetTrustedRepositoryUri(release.HtmlUrl, "release page");

        string expectedAssetName = $"Chroma-{release.TagName}-win-x64.zip";
        GitHubReleaseAsset? asset = release.Assets.FirstOrDefault(item =>
            string.Equals(item.Name, expectedAssetName, StringComparison.OrdinalIgnoreCase));
        GitHubReleaseAsset? checksumAsset = release.Assets.FirstOrDefault(item =>
            string.Equals(
                item.Name,
                $"{expectedAssetName}.sha256",
                StringComparison.OrdinalIgnoreCase));

        Uri? downloadUri = asset is null
            ? null
            : GetTrustedRepositoryUri(asset.DownloadUrl, "release download");
        Uri? checksumDownloadUri = checksumAsset is null
            ? null
            : GetTrustedRepositoryUri(checksumAsset.DownloadUrl, "release checksum");

        return new UpdateCheckResult(
            IsUpdateAvailable: latestVersion > currentVersion,
            CurrentVersionTag,
            LatestVersionTag: NormalizeTag(release.TagName),
            ReleaseName: string.IsNullOrWhiteSpace(release.Name)
                ? $"Chroma {NormalizeTag(release.TagName)}"
                : release.Name.Trim(),
            ReleaseNotes: release.Body?.Trim() ?? string.Empty,
            releaseUri,
            downloadUri,
            checksumDownloadUri,
            AssetName: asset?.Name,
            AssetSizeBytes: asset?.Size ?? 0,
            release.PublishedAt);
    }

    public async Task<PreparedUpdate> PrepareUpdateAsync(
        UpdateCheckResult update,
        IProgress<UpdateProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!update.IsUpdateAvailable)
        {
            throw new InvalidOperationException("The selected release is not newer than this installation.");
        }

        if (update.DownloadUri is null ||
            update.ChecksumDownloadUri is null ||
            string.IsNullOrWhiteSpace(update.AssetName))
        {
            throw new InvalidDataException(
                "This release does not include both the Windows archive and its SHA-256 checksum.");
        }

        string updatesRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Chroma",
            "Updates");
        string updateRoot = Path.Combine(
            updatesRoot,
            SanitizePathComponent(update.LatestVersionTag));
        string archivePath = Path.Combine(updateRoot, update.AssetName);
        string stagingDirectory = Path.Combine(updateRoot, "staged");

        if (Directory.Exists(updateRoot))
        {
            Directory.Delete(updateRoot, recursive: true);
        }

        Directory.CreateDirectory(updateRoot);

        progress?.Report(new UpdateProgress("Downloading update…", 0));
        await DownloadFileAsync(
            update.DownloadUri,
            archivePath,
            update.AssetSizeBytes,
            progress,
            cancellationToken);

        progress?.Report(new UpdateProgress("Verifying SHA-256 checksum…", null));
        string checksumText = await Client.GetStringAsync(
            update.ChecksumDownloadUri,
            cancellationToken);
        string expectedHash = ParseChecksum(checksumText, update.AssetName);
        string actualHash = await ComputeSha256Async(archivePath, cancellationToken);
        if (!string.Equals(expectedHash, actualHash, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                "The downloaded update failed SHA-256 verification and was not installed.");
        }

        progress?.Report(new UpdateProgress("Preparing update files…", null));
        ExtractArchiveSafely(archivePath, stagingDirectory);
        ValidateStagedUpdate(stagingDirectory);
        string applicationPath = GetRunningApplicationPath();
        string installationDirectory = GetVerifiedInstallationDirectory(applicationPath);
        EnsureInstallDirectoryWritable(installationDirectory);

        string installerScriptPath = Path.Combine(updatesRoot, "Chroma.Update.ps1");
        Directory.CreateDirectory(updatesRoot);
        await File.WriteAllTextAsync(
            installerScriptPath,
            InstallerScript,
            cancellationToken);

        progress?.Report(new UpdateProgress("Ready to restart", 100));
        return new PreparedUpdate(
            update.LatestVersionTag,
            stagingDirectory,
            installerScriptPath,
            applicationPath);
    }

    public static void LaunchPreparedUpdate(PreparedUpdate update)
    {
        string applicationPath = Path.GetFullPath(update.ApplicationPath);
        _ = GetVerifiedInstallationDirectory(applicationPath);
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("-NoLogo");
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-NonInteractive");
        startInfo.ArgumentList.Add("-ExecutionPolicy");
        startInfo.ArgumentList.Add("Bypass");
        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(update.InstallerScriptPath);
        startInfo.ArgumentList.Add("-ApplicationPath");
        startInfo.ArgumentList.Add(applicationPath);
        startInfo.ArgumentList.Add("-StagingDirectory");
        startInfo.ArgumentList.Add(update.StagingDirectory);
        startInfo.ArgumentList.Add("-ApplicationPid");
        startInfo.ArgumentList.Add(Environment.ProcessId.ToString());

        Process? process = Process.Start(startInfo);
        if (process is null)
        {
            throw new InvalidOperationException("Windows could not start the Chroma updater.");
        }
    }

    internal static Version ParseVersion(string value)
    {
        string normalized = value.Trim();
        if (normalized.StartsWith('v') || normalized.StartsWith('V'))
        {
            normalized = normalized[1..];
        }

        int suffixIndex = normalized.IndexOfAny(['-', '+']);
        if (suffixIndex >= 0)
        {
            normalized = normalized[..suffixIndex];
        }

        string[] components = normalized.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (components.Length is < 1 or > 4)
        {
            throw new FormatException($"Unsupported Chroma version: {value}");
        }

        int[] numbers = new int[4];
        for (int index = 0; index < components.Length; index++)
        {
            if (!int.TryParse(components[index], out numbers[index]) || numbers[index] < 0)
            {
                throw new FormatException($"Unsupported Chroma version: {value}");
            }
        }

        return new Version(numbers[0], numbers[1], numbers[2], numbers[3]);
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(5)
        };
        client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("Chroma", CurrentVersionTag.TrimStart('v', 'V')));
        return client;
    }

    private static async Task DownloadFileAsync(
        Uri downloadUri,
        string destinationPath,
        long expectedSize,
        IProgress<UpdateProgress>? progress,
        CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, downloadUri);
        using HttpResponseMessage response = await Client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        long? contentLength = response.Content.Headers.ContentLength;
        if (expectedSize > 0 && contentLength is long responseSize && responseSize != expectedSize)
        {
            throw new InvalidDataException(
                "GitHub returned an update with an unexpected download size.");
        }

        await using Stream source = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using FileStream destination = new(
            destinationPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        byte[] buffer = new byte[81920];
        long bytesRead = 0;
        while (true)
        {
            int count = await source.ReadAsync(buffer, cancellationToken);
            if (count == 0)
            {
                break;
            }

            await destination.WriteAsync(buffer.AsMemory(0, count), cancellationToken);
            bytesRead += count;

            long total = expectedSize > 0 ? expectedSize : contentLength ?? 0;
            double? percent = total > 0
                ? Math.Clamp(bytesRead * 100d / total, 0d, 100d)
                : null;
            progress?.Report(new UpdateProgress("Downloading update…", percent));
        }

        if (expectedSize > 0 && bytesRead != expectedSize)
        {
            throw new InvalidDataException(
                "The update download was incomplete and was not installed.");
        }
    }

    private static string ParseChecksum(string content, string expectedFileName)
    {
        Match match = Regex.Match(
            content.Trim(),
            @"\A(?<hash>[a-fA-F0-9]{64})(?:\s+\*?(?<name>[^\r\n]+))?\z",
            RegexOptions.CultureInvariant);
        if (!match.Success)
        {
            throw new InvalidDataException("The release checksum file has an invalid format.");
        }

        string suppliedName = match.Groups["name"].Value.Trim();
        if (suppliedName.Length > 0 &&
            !string.Equals(
                Path.GetFileName(suppliedName),
                expectedFileName,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                "The release checksum does not belong to the downloaded update.");
        }

        return match.Groups["hash"].Value.ToLowerInvariant();
    }

    private static async Task<string> ComputeSha256Async(
        string path,
        CancellationToken cancellationToken)
    {
        await using FileStream stream = new(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);
        byte[] hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static void ExtractArchiveSafely(string archivePath, string destinationDirectory)
    {
        Directory.CreateDirectory(destinationDirectory);
        string destinationRoot = Path.GetFullPath(destinationDirectory)
            .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        using ZipArchive archive = ZipFile.OpenRead(archivePath);
        if (archive.Entries.Count == 0)
        {
            throw new InvalidDataException("The update archive is empty.");
        }

        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            string destinationPath = Path.GetFullPath(
                Path.Combine(destinationRoot, entry.FullName));
            if (!destinationPath.StartsWith(destinationRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "The update archive contains an unsafe file path.");
            }

            if (string.IsNullOrEmpty(entry.Name))
            {
                Directory.CreateDirectory(destinationPath);
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            entry.ExtractToFile(destinationPath, overwrite: false);
        }
    }

    private static void ValidateStagedUpdate(string stagingDirectory)
    {
        string[] requiredFiles =
        [
            "Chroma.exe",
            "Chroma.Agent.exe"
        ];

        foreach (string relativePath in requiredFiles)
        {
            string fullPath = Path.Combine(stagingDirectory, relativePath);
            if (!File.Exists(fullPath) || new FileInfo(fullPath).Length <= 0)
            {
                throw new InvalidDataException(
                    $"The update archive is missing the required file '{relativePath}'.");
            }
        }
    }

    private static string GetRunningApplicationPath()
    {
        string? processPath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(processPath))
        {
            using Process process = Process.GetCurrentProcess();
            processPath = process.MainModule?.FileName;
        }

        if (string.IsNullOrWhiteSpace(processPath))
        {
            throw new InvalidOperationException(
                "Chroma could not determine the path of the running executable.");
        }

        return Path.GetFullPath(processPath);
    }

    private static string GetVerifiedInstallationDirectory(string applicationPath)
    {
        string fullApplicationPath = Path.GetFullPath(applicationPath);
        if (!string.Equals(
                Path.GetFileName(fullApplicationPath),
                "Chroma.exe",
                StringComparison.OrdinalIgnoreCase) ||
            !File.Exists(fullApplicationPath))
        {
            throw new InvalidOperationException(
                "Chroma could not verify the currently running installation.");
        }

        string? installationDirectory = Path.GetDirectoryName(fullApplicationPath);
        if (string.IsNullOrWhiteSpace(installationDirectory))
        {
            throw new InvalidOperationException(
                "Chroma could not determine the installation directory.");
        }

        installationDirectory = Path.GetFullPath(installationDirectory);
        string agentPath = Path.Combine(installationDirectory, "Chroma.Agent.exe");
        if (!File.Exists(agentPath))
        {
            throw new InvalidOperationException(
                "Chroma.Agent.exe is not beside the running Chroma.exe. " +
                "The updater will not replace files in an unverified folder.");
        }

        string updatesDirectory = Path.GetFullPath(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Chroma",
            "Updates"))
            .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        string installationRoot = installationDirectory
            .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (installationRoot.StartsWith(
                updatesDirectory,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Chroma is running from its temporary update folder. " +
                "Launch the installed copy before updating.");
        }

        return installationDirectory;
    }

    private static void EnsureInstallDirectoryWritable(string installationDirectory)
    {
        string probePath = Path.Combine(
            installationDirectory,
            $".chroma-update-{Guid.NewGuid():N}.tmp");
        try
        {
            using FileStream _ = new(
                probePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                1,
                FileOptions.DeleteOnClose);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new InvalidOperationException(
                "Chroma cannot update this installation without write permission. " +
                "Move it to a user-writable folder or run it as administrator.",
                exception);
        }
    }

    private static string SanitizePathComponent(string value)
    {
        char[] invalidCharacters = Path.GetInvalidFileNameChars();
        return string.Concat(value.Select(character =>
            invalidCharacters.Contains(character) ? '_' : character));
    }

    private static string GetCurrentVersionTag()
    {
        Assembly assembly = Assembly.GetEntryAssembly() ?? typeof(UpdateService).Assembly;
        string? informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
            .Split('+', 2)[0]
            .Trim();

        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            return NormalizeTag(informationalVersion);
        }

        Version version = assembly.GetName().Version ?? new Version(1, 0);
        return $"v{version.Major}.{version.Minor}" +
               (version.Build > 0 ? $".{version.Build}" : string.Empty);
    }

    private static string NormalizeTag(string value)
    {
        string trimmed = value.Trim();
        return trimmed.StartsWith('v') || trimmed.StartsWith('V')
            ? $"v{trimmed[1..]}"
            : $"v{trimmed}";
    }

    private static Uri GetTrustedRepositoryUri(string? value, string description)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) ||
            uri.Scheme != Uri.UriSchemeHttps ||
            !string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase) ||
            !uri.AbsolutePath.StartsWith(
                "/acchtt/Chroma/",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                $"GitHub returned an unexpected Chroma {description} URL.");
        }

        return uri;
    }

    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; init; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("body")]
        public string? Body { get; init; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; init; } = string.Empty;

        [JsonPropertyName("draft")]
        public bool Draft { get; init; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; init; }

        [JsonPropertyName("published_at")]
        public DateTimeOffset? PublishedAt { get; init; }

        [JsonPropertyName("assets")]
        public List<GitHubReleaseAsset> Assets { get; init; } = [];
    }

    private sealed class GitHubReleaseAsset
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("browser_download_url")]
        public string DownloadUrl { get; init; } = string.Empty;

        [JsonPropertyName("size")]
        public long Size { get; init; }
    }

    private const string InstallerScript =
        """
        param(
            [Parameter(Mandatory=$true)][string]$ApplicationPath,
            [Parameter(Mandatory=$true)][string]$StagingDirectory,
            [Parameter(Mandatory=$true)][int]$ApplicationPid
        )

        $ErrorActionPreference = 'Stop'
        $application = [IO.Path]::GetFullPath($ApplicationPath)
        if (-not [IO.Path]::GetFileName($application).Equals(
                'Chroma.exe',
                [StringComparison]::OrdinalIgnoreCase)) {
            throw "Unexpected application target: $application"
        }
        $install = Split-Path -Parent $application
        $staging = [IO.Path]::GetFullPath($StagingDirectory)
        $updateRoot = Split-Path -Parent $staging
        $backup = Join-Path $updateRoot 'backup'
        $log = Join-Path $updateRoot 'update.log'
        $createdFiles = [Collections.Generic.List[string]]::new()

        try {
            "Install target: $application" | Set-Content $log
            "Staging source: $staging" | Add-Content $log
            if (-not (Test-Path $application -PathType Leaf)) {
                throw "The running Chroma executable no longer exists at $application"
            }
            if (-not (Test-Path (Join-Path $install 'Chroma.Agent.exe') -PathType Leaf)) {
                throw "Chroma.Agent.exe is not present in the verified installation folder."
            }
            "Waiting for Chroma PID $ApplicationPid to exit." | Add-Content $log
            try { Wait-Process -Id $ApplicationPid -Timeout 60 -ErrorAction Stop } catch {
                Stop-Process -Id $ApplicationPid -Force -ErrorAction SilentlyContinue
                Start-Sleep -Milliseconds 500
            }

            $agentPath = Join-Path $install 'Chroma.Agent.exe'
            Get-CimInstance Win32_Process -Filter "Name='Chroma.Agent.exe'" -ErrorAction SilentlyContinue |
                Where-Object {
                    -not [string]::IsNullOrWhiteSpace($_.ExecutablePath) -and
                    [IO.Path]::GetFullPath($_.ExecutablePath).Equals(
                        $agentPath,
                        [StringComparison]::OrdinalIgnoreCase)
                } |
                ForEach-Object {
                    Stop-Process -Id ([int]$_.ProcessId) -Force -ErrorAction Stop
                    Wait-Process -Id ([int]$_.ProcessId) -Timeout 15 -ErrorAction SilentlyContinue
                }

            if (Test-Path $backup) { Remove-Item $backup -Recurse -Force }
            New-Item -ItemType Directory -Path $backup | Out-Null

            $files = @(Get-ChildItem $staging -File -Recurse)
            foreach ($source in $files) {
                $relative = $source.FullName.Substring($staging.Length).TrimStart('\')
                $destination = Join-Path $install $relative
                $destinationDirectory = Split-Path -Parent $destination
                New-Item -ItemType Directory -Path $destinationDirectory -Force | Out-Null

                if (Test-Path $destination -PathType Leaf) {
                    $backupPath = Join-Path $backup $relative
                    New-Item -ItemType Directory -Path (Split-Path -Parent $backupPath) -Force | Out-Null
                    Copy-Item $destination $backupPath -Force
                } else {
                    $createdFiles.Add($destination)
                }

                Copy-Item $source.FullName $destination -Force
                $sourceHash = (Get-FileHash $source.FullName -Algorithm SHA256).Hash
                $destinationHash = (Get-FileHash $destination -Algorithm SHA256).Hash
                if (-not $sourceHash.Equals(
                        $destinationHash,
                        [StringComparison]::OrdinalIgnoreCase)) {
                    throw "Replacement verification failed for $relative"
                }
            }

            "Installed files replaced and verified in $install." | Add-Content $log
            Start-Process $application -WorkingDirectory $install

            if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
            if (Test-Path $backup) { Remove-Item $backup -Recurse -Force }
            Get-ChildItem $updateRoot -Filter '*.zip' -File -ErrorAction SilentlyContinue |
                Remove-Item -Force -ErrorAction SilentlyContinue

            $updatesRoot = Split-Path -Parent $updateRoot
            Get-ChildItem $updatesRoot -Directory -ErrorAction SilentlyContinue |
                Where-Object {
                    -not $_.FullName.Equals(
                        $updateRoot,
                        [StringComparison]::OrdinalIgnoreCase)
                } |
                Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

            "Update installed successfully. Temporary and past update files removed." | Add-Content $log
        }
        catch {
            "Update failed: $($_.Exception.Message)" | Add-Content $log

            foreach ($createdFile in $createdFiles) {
                Remove-Item $createdFile -Force -ErrorAction SilentlyContinue
            }

            if (Test-Path $backup) {
                Get-ChildItem $backup -File -Recurse | ForEach-Object {
                    $relative = $_.FullName.Substring($backup.Length).TrimStart('\')
                    $destination = Join-Path $install $relative
                    New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
                    Copy-Item $_.FullName $destination -Force
                }
            }

            if (Test-Path (Join-Path $install 'Chroma.exe')) {
                Start-Process (Join-Path $install 'Chroma.exe') -WorkingDirectory $install
            }
            exit 1
        }
        """;
}

public sealed record UpdateCheckResult(
    bool IsUpdateAvailable,
    string CurrentVersionTag,
    string LatestVersionTag,
    string ReleaseName,
    string ReleaseNotes,
    Uri ReleaseUri,
    Uri? DownloadUri,
    Uri? ChecksumDownloadUri,
    string? AssetName,
    long AssetSizeBytes,
    DateTimeOffset? PublishedAt);

public sealed record UpdateProgress(string Message, double? Percentage);

public sealed record PreparedUpdate(
    string VersionTag,
    string StagingDirectory,
    string InstallerScriptPath,
    string ApplicationPath);
