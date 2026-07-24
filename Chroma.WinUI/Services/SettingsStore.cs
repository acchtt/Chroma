using Chroma.Models;
using Microsoft.Win32;

namespace Chroma.Services;

public sealed class SettingsStore
{
    private const string SettingsKey = @"Software\Chroma";
    private const string LegacySettingsKey = @"Software\ArcVibrance";
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string StartupApprovedRunKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
    private const string AutoStartValue = "Chroma";
    private const string LegacyAutoStartValue = "ArcVibrance";

    public SettingsStore()
    {
        MigrateLegacySettings();
    }

    public ThemeMode GetThemeMode() => (ThemeMode)ReadDword("ThemeMode", (int)ThemeMode.Dark, 0, 2);

    public void SetThemeMode(ThemeMode mode) => WriteDword("ThemeMode", (int)mode);

    public CloseBehavior GetCloseBehavior() =>
        (CloseBehavior)ReadDword("CloseBehavior", (int)CloseBehavior.MinimizeToTray, 0, 1);

    public void SetCloseBehavior(CloseBehavior behavior) => WriteDword("CloseBehavior", (int)behavior);

    public bool IsAutoStartEnabled()
    {
        string agentPath = GetAgentPath();
        if (!File.Exists(agentPath))
        {
            return false;
        }

        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKey, false);
        return key?.GetValue(AutoStartValue) is string command &&
               CommandTargetsExecutable(command, agentPath) &&
               !IsStartupApprovedDisabled(AutoStartValue);
    }

    public void SetAutoStartEnabled(bool enabled)
    {
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKey, true)
            ?? throw new InvalidOperationException("Could not open the Windows startup registry key.");

        if (!enabled)
        {
            key.DeleteValue(AutoStartValue, false);
            key.DeleteValue(LegacyAutoStartValue, false);
            DeleteStartupApprovalState();
            return;
        }

        string agentPath = GetAgentPath();
        if (!File.Exists(agentPath))
        {
            throw new FileNotFoundException(
                "Chroma.Agent.exe must be beside Chroma.exe before startup can be enabled.",
                agentPath);
        }

        // Launch the lightweight tray agent directly. It owns monitoring and can
        // reopen the WinUI settings window from its tray menu when needed.
        key.SetValue(
            AutoStartValue,
            $"\"{agentPath}\" --startup",
            RegistryValueKind.String);

        // Windows remembers disabled Run entries under StartupApproved. When the
        // user explicitly turns this switch back on, remove a stale disabled state
        // so the newly written Run entry is allowed to execute at next sign-in.
        DeleteStartupApprovalState();
    }

    private static string GetAgentPath() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Chroma.Agent.exe"));

    private static bool CommandTargetsExecutable(string command, string expectedPath)
    {
        string value = command.Trim();
        if (value.Length == 0)
        {
            return false;
        }

        string commandPath;
        if (value[0] == '"')
        {
            int closingQuote = value.IndexOf('"', 1);
            if (closingQuote <= 1)
            {
                return false;
            }
            commandPath = value[1..closingQuote];
        }
        else
        {
            int separator = value.IndexOfAny([' ', '\t']);
            commandPath = separator < 0 ? value : value[..separator];
        }

        try
        {
            return string.Equals(
                Path.GetFullPath(commandPath),
                Path.GetFullPath(expectedPath),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }


    private static bool IsStartupApprovedDisabled(string valueName)
    {
        using RegistryKey? approved = Registry.CurrentUser.OpenSubKey(StartupApprovedRunKey, writable: false);
        if (approved?.GetValue(valueName) is not byte[] state || state.Length == 0)
        {
            return false;
        }

        // Explorer currently uses 0x03 and 0x07 for disabled startup entries.
        // Unknown values are treated as enabled so a future Windows revision does
        // not incorrectly switch the Chroma setting off.
        return state[0] is 0x03 or 0x07;
    }

    private static void DeleteStartupApprovalState()
    {
        using RegistryKey? approved = Registry.CurrentUser.OpenSubKey(StartupApprovedRunKey, writable: true);
        approved?.DeleteValue(AutoStartValue, false);
        approved?.DeleteValue(LegacyAutoStartValue, false);
    }

    private static int ReadDword(string name, int fallback, int minimum, int maximum)
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(SettingsKey, false);
        object? value = key?.GetValue(name);
        return value is int number && number >= minimum && number <= maximum ? number : fallback;
    }

    private static void WriteDword(string name, int value)
    {
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(SettingsKey, true)
            ?? throw new InvalidOperationException("Could not open the Chroma registry key.");
        key.SetValue(name, value, RegistryValueKind.DWord);
    }

    private static void MigrateLegacySettings()
    {
        using RegistryKey? legacySettings = Registry.CurrentUser.OpenSubKey(LegacySettingsKey, false);
        if (legacySettings is not null)
        {
            using RegistryKey currentSettings = Registry.CurrentUser.CreateSubKey(SettingsKey, true)
                ?? throw new InvalidOperationException("Could not open the Chroma registry key.");
            CopyDwordIfMissing(legacySettings, currentSettings, "ThemeMode");
            CopyDwordIfMissing(legacySettings, currentSettings, "CloseBehavior");
        }

        using RegistryKey runKey = Registry.CurrentUser.CreateSubKey(RunKey, true)
            ?? throw new InvalidOperationException("Could not open the Windows startup registry key.");
        bool hasCurrentEntry = runKey.GetValue(AutoStartValue) is string;
        bool legacyWasEnabled =
            runKey.GetValue(LegacyAutoStartValue) is string &&
            !IsStartupApprovedDisabled(LegacyAutoStartValue);

        string agentPath = GetAgentPath();
        if (!hasCurrentEntry && legacyWasEnabled && File.Exists(agentPath))
        {
            runKey.SetValue(
                AutoStartValue,
                $"\"{agentPath}\" --startup",
                RegistryValueKind.String);
        }

        runKey.DeleteValue(LegacyAutoStartValue, false);
        using RegistryKey? approved = Registry.CurrentUser.OpenSubKey(StartupApprovedRunKey, writable: true);
        approved?.DeleteValue(LegacyAutoStartValue, false);
    }

    private static void CopyDwordIfMissing(
        RegistryKey source,
        RegistryKey destination,
        string valueName)
    {
        if (destination.GetValue(valueName) is null &&
            source.GetValue(valueName) is int value)
        {
            destination.SetValue(valueName, value, RegistryValueKind.DWord);
        }
    }
}
