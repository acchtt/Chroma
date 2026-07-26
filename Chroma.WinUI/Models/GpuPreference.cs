namespace Chroma.Models;

public enum GpuPreference
{
    Automatic = 0,
    Intel = 1,
    Nvidia = 2,
    Amd = 3
}

public sealed record GpuSelectionOption(
    GpuPreference Preference,
    string DisplayName,
    string Detail)
{
    public override string ToString() => DisplayName;
}
