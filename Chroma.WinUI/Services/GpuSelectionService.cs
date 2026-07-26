using System.Runtime.InteropServices;
using Chroma.Models;
using Microsoft.Win32;

namespace Chroma.Services;

public sealed class GpuSelectionService
{
    private const string SettingsKey = @"Software\Chroma";
    private const string PreferenceValue = "GpuPreference";
    private const uint DisplayDeviceActive = 0x00000001;
    private const uint DisplayDevicePrimaryDevice = 0x00000004;

    public GpuPreference GetPreference()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(SettingsKey, writable: false);
        return key?.GetValue(PreferenceValue) is int value &&
               Enum.IsDefined(typeof(GpuPreference), value)
            ? (GpuPreference)value
            : GpuPreference.Automatic;
    }

    public void SetPreference(GpuPreference preference)
    {
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(SettingsKey, writable: true)
            ?? throw new InvalidOperationException("Could not open the Chroma registry key.");
        key.SetValue(PreferenceValue, (int)preference, RegistryValueKind.DWord);
    }

    public IReadOnlyList<GpuSelectionOption> GetOptions()
    {
        IReadOnlyList<DisplayAdapterInfo> adapters = EnumerateActiveAdapters();
        string primaryName = adapters.FirstOrDefault(adapter => adapter.IsPrimary)?.Name
            ?? adapters.FirstOrDefault()?.Name
            ?? "the first compatible GPU";

        var options = new List<GpuSelectionOption>
        {
            new(
                GpuPreference.Automatic,
                $"Automatic — {primaryName}",
                "Follow the primary Windows display and fall back to the first compatible GPU backend.")
        };

        AddVendorOption(options, adapters, GpuPreference.Intel, "Intel");
        AddVendorOption(options, adapters, GpuPreference.Nvidia, "NVIDIA");
        AddVendorOption(options, adapters, GpuPreference.Amd, "AMD");

        GpuPreference savedPreference = GetPreference();
        if (savedPreference != GpuPreference.Automatic &&
            options.All(option => option.Preference != savedPreference))
        {
            string vendor = GetVendorLabel(savedPreference);
            options.Add(new GpuSelectionOption(
                savedPreference,
                $"{vendor} — not currently detected",
                $"Keep the {vendor} backend selected and retry when its driver or display becomes available."));
        }

        return options;
    }

    private static void AddVendorOption(
        ICollection<GpuSelectionOption> options,
        IEnumerable<DisplayAdapterInfo> adapters,
        GpuPreference preference,
        string vendorLabel)
    {
        string[] names = adapters
            .Where(adapter => adapter.Preference == preference)
            .Select(adapter => adapter.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (names.Length == 0)
        {
            return;
        }

        string detectedNames = string.Join(" / ", names);
        options.Add(new GpuSelectionOption(
            preference,
            $"{vendorLabel} — {detectedNames}",
            names.Length == 1
                ? $"Use the {vendorLabel} color backend for {detectedNames}."
                : $"Use the {vendorLabel} color backend. Detected adapters: {detectedNames}."));
    }

    private static IReadOnlyList<DisplayAdapterInfo> EnumerateActiveAdapters()
    {
        var adapters = new List<DisplayAdapterInfo>();

        try
        {
            for (uint index = 0; ; index++)
            {
                var adapter = new DisplayDevice
                {
                    Cb = Marshal.SizeOf<DisplayDevice>()
                };

                if (!EnumDisplayDevices(null, index, ref adapter, 0))
                {
                    break;
                }

                if ((adapter.StateFlags & DisplayDeviceActive) == 0 ||
                    string.IsNullOrWhiteSpace(adapter.DeviceString))
                {
                    continue;
                }

                GpuPreference preference = DetectVendor(adapter.DeviceString, adapter.DeviceId);
                if (preference == GpuPreference.Automatic)
                {
                    continue;
                }

                adapters.Add(new DisplayAdapterInfo(
                    FormatAdapterName(adapter.DeviceString),
                    preference,
                    (adapter.StateFlags & DisplayDevicePrimaryDevice) != 0));
            }
        }
        catch
        {
            // A selector with Automatic only remains usable if Windows adapter
            // enumeration is temporarily unavailable during startup.
        }

        return adapters;
    }

    private static GpuPreference DetectVendor(string name, string deviceId)
    {
        string combined = $"{name} {deviceId}";
        if (combined.Contains("VEN_8086", StringComparison.OrdinalIgnoreCase) ||
            combined.Contains("Intel", StringComparison.OrdinalIgnoreCase))
        {
            return GpuPreference.Intel;
        }

        if (combined.Contains("VEN_10DE", StringComparison.OrdinalIgnoreCase) ||
            combined.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase) ||
            combined.Contains("GeForce", StringComparison.OrdinalIgnoreCase))
        {
            return GpuPreference.Nvidia;
        }

        if (combined.Contains("VEN_1002", StringComparison.OrdinalIgnoreCase) ||
            combined.Contains("AMD", StringComparison.OrdinalIgnoreCase) ||
            combined.Contains("Radeon", StringComparison.OrdinalIgnoreCase))
        {
            return GpuPreference.Amd;
        }

        return GpuPreference.Automatic;
    }

    private static string FormatAdapterName(string name)
    {
        string value = name
            .Replace("(R)", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("(TM)", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();

        while (value.Contains("  ", StringComparison.Ordinal))
        {
            value = value.Replace("  ", " ", StringComparison.Ordinal);
        }

        return value;
    }

    private static string GetVendorLabel(GpuPreference preference) => preference switch
    {
        GpuPreference.Intel => "Intel",
        GpuPreference.Nvidia => "NVIDIA",
        GpuPreference.Amd => "AMD",
        _ => "Automatic"
    };

    private sealed record DisplayAdapterInfo(
        string Name,
        GpuPreference Preference,
        bool IsPrimary);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DisplayDevice
    {
        public int Cb;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;

        public uint StateFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplayDevices(
        string? device,
        uint deviceNumber,
        ref DisplayDevice displayDevice,
        uint flags);
}
