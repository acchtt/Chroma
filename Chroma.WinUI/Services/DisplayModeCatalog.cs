using System.Runtime.InteropServices;

namespace Chroma.Services;

public readonly record struct DisplayResolution(int Width, int Height);

public sealed record DisplayModeSnapshot(
    IReadOnlyList<DisplayResolution> Modes,
    DisplayResolution Preferred);

public static class DisplayModeCatalog
{
    private const uint DisplayDeviceActive = 0x00000001;
    private const uint DisplayDevicePrimaryDevice = 0x00000004;
    private const int EnumCurrentSettings = -1;

    public static DisplayModeSnapshot GetSnapshot()
    {
        var modes = new HashSet<DisplayResolution>();
        DisplayResolution preferred = default;
        bool hasPreferred = false;

        try
        {
            for (uint adapterIndex = 0; ; adapterIndex++)
            {
                var adapter = new DisplayDevice
                {
                    Cb = Marshal.SizeOf<DisplayDevice>()
                };

                if (!EnumDisplayDevices(null, adapterIndex, ref adapter, 0))
                {
                    break;
                }

                if ((adapter.StateFlags & DisplayDeviceActive) == 0 ||
                    string.IsNullOrWhiteSpace(adapter.DeviceName))
                {
                    continue;
                }

                DevMode currentMode = CreateDevMode();
                if (!EnumDisplaySettingsEx(
                        adapter.DeviceName,
                        EnumCurrentSettings,
                        ref currentMode,
                        0))
                {
                    continue;
                }

                DisplayResolution currentResolution = ToResolution(currentMode);
                if (ResolutionOverrideStore.IsValid(
                        currentResolution.Width,
                        currentResolution.Height))
                {
                    modes.Add(currentResolution);
                    bool isPrimary =
                        (adapter.StateFlags & DisplayDevicePrimaryDevice) != 0;
                    if (!hasPreferred || isPrimary)
                    {
                        preferred = currentResolution;
                        hasPreferred = true;
                    }
                }

                for (int modeIndex = 0; ; modeIndex++)
                {
                    DevMode candidate = CreateDevMode();
                    if (!EnumDisplaySettingsEx(
                            adapter.DeviceName,
                            modeIndex,
                            ref candidate,
                            0))
                    {
                        break;
                    }

                    if (candidate.BitsPerPel != currentMode.BitsPerPel ||
                        !RefreshRatesMatch(
                            candidate.DisplayFrequency,
                            currentMode.DisplayFrequency))
                    {
                        continue;
                    }

                    DisplayResolution resolution = ToResolution(candidate);
                    if (ResolutionOverrideStore.IsValid(
                            resolution.Width,
                            resolution.Height))
                    {
                        modes.Add(resolution);
                    }
                }
            }
        }
        catch
        {
            // Keep the editor usable if display enumeration is temporarily
            // unavailable. The native agent validates the selected mode again
            // on the actual game monitor before applying it.
        }

        if (modes.Count == 0)
        {
            DevMode currentMode = CreateDevMode();
            if (EnumDisplaySettingsEx(null, EnumCurrentSettings, ref currentMode, 0))
            {
                DisplayResolution currentResolution = ToResolution(currentMode);
                if (ResolutionOverrideStore.IsValid(
                        currentResolution.Width,
                        currentResolution.Height))
                {
                    modes.Add(currentResolution);
                    preferred = currentResolution;
                    hasPreferred = true;
                }
            }
        }

        if (modes.Count == 0)
        {
            preferred = new DisplayResolution(1920, 1080);
            modes.Add(preferred);
            hasPreferred = true;
        }

        DisplayResolution[] orderedModes = modes
            .OrderByDescending(mode => mode.Width)
            .ThenByDescending(mode => mode.Height)
            .ToArray();

        if (!hasPreferred || !modes.Contains(preferred))
        {
            preferred = orderedModes[0];
        }

        return new DisplayModeSnapshot(orderedModes, preferred);
    }

    private static bool RefreshRatesMatch(uint first, uint second)
    {
        if (first == 0 || second == 0)
        {
            return true;
        }

        return Math.Abs((long)first - second) <= 1;
    }

    private static DisplayResolution ToResolution(DevMode mode) =>
        new(checked((int)mode.PelsWidth), checked((int)mode.PelsHeight));

    private static DevMode CreateDevMode() => new()
    {
        Size = checked((ushort)Marshal.SizeOf<DevMode>())
    };

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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DevMode
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;

        public ushort SpecVersion;
        public ushort DriverVersion;
        public ushort Size;
        public ushort DriverExtra;
        public uint Fields;
        public int PositionX;
        public int PositionY;
        public uint DisplayOrientation;
        public uint DisplayFixedOutput;
        public short Color;
        public short Duplex;
        public short YResolution;
        public short TTOption;
        public short Collate;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string FormName;

        public ushort LogPixels;
        public uint BitsPerPel;
        public uint PelsWidth;
        public uint PelsHeight;
        public uint DisplayFlags;
        public uint DisplayFrequency;
        public uint ICMMethod;
        public uint ICMIntent;
        public uint MediaType;
        public uint DitherType;
        public uint Reserved1;
        public uint Reserved2;
        public uint PanningWidth;
        public uint PanningHeight;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplayDevices(
        string? device,
        uint deviceNumber,
        ref DisplayDevice displayDevice,
        uint flags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplaySettingsEx(
        string? deviceName,
        int modeNumber,
        ref DevMode mode,
        uint flags);
}
