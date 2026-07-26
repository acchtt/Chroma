using Chroma.Infrastructure;
using Chroma.Models;
using Chroma.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Chroma.ViewModels;

public sealed class ProfileViewModel : ObservableObject
{
    private int _saturationPercent;
    private bool _enabled;
    private bool _isSelected;
    private ImageSource? _icon;
    private string _detectedName;
    private string? _customName;
    private GameNameSource _nameSource = GameNameSource.ExecutableFileName;
    private bool _useLightTheme;

    public ProfileViewModel(GameProfile model, string? customName = null)
    {
        Model = model;
        _saturationPercent = model.SaturationPercent;
        _enabled = model.Enabled;
        _detectedName = Path.GetFileNameWithoutExtension(model.ExecutablePath);
        _customName = string.IsNullOrWhiteSpace(customName) ? null : customName.Trim();
    }

    public GameProfile Model { get; }
    public string ExecutablePath => Model.ExecutablePath;
    public string ExecutableName => Path.GetFileName(Model.ExecutablePath);
    public string DetectedName => _detectedName;
    public string? CustomName => _customName;
    public bool HasCustomName => !string.IsNullOrWhiteSpace(_customName);
    public string DisplayName => HasCustomName ? _customName! : _detectedName;
    public string Initial => string.IsNullOrEmpty(DisplayName) ? "?" : DisplayName[..1].ToUpperInvariant();
    public string ExecutableDetails => $"{ExecutableName}  •  {ExecutablePath}";
    public string NameSourceText => HasCustomName ? "Custom profile name" : new GameNameResult(_detectedName, _nameSource).SourceText;

    public int SaturationPercent
    {
        get => _saturationPercent;
        set
        {
            value = Math.Clamp(value, 0, 300);
            if (SetProperty(ref _saturationPercent, value))
            {
                Model.SaturationPercent = value;
                OnPropertyChanged(nameof(SaturationText));
                OnPropertyChanged(nameof(SaturationBrush));
            }
        }
    }

    public string SaturationText => $"{SaturationPercent}%";

    public Brush SaturationBrush => new SolidColorBrush(
        SaturationPercent >= 150
            ? (_useLightTheme
                ? Windows.UI.Color.FromArgb(255, 169, 54, 130)
                : Windows.UI.Color.FromArgb(255, 232, 120, 199))
            : (_useLightTheme
                ? Windows.UI.Color.FromArgb(255, 23, 125, 145)
                : Windows.UI.Color.FromArgb(255, 101, 220, 229)));

    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (SetProperty(ref _enabled, value))
            {
                Model.Enabled = value;
            }
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetProperty(ref _isSelected, value))
            {
                OnPropertyChanged(nameof(CardBorderBrush));
            }
        }
    }

    public Brush CardBorderBrush
    {
        get
        {
            if (!IsSelected)
            {
                return new SolidColorBrush(_useLightTheme
                    ? Windows.UI.Color.FromArgb(255, 211, 217, 232)
                    : Windows.UI.Color.FromArgb(255, 39, 61, 88));
            }

            var gradient = new LinearGradientBrush
            {
                StartPoint = new Windows.Foundation.Point(0, 0),
                EndPoint = new Windows.Foundation.Point(1, 1)
            };
            gradient.GradientStops.Add(new GradientStop { Color = Windows.UI.Color.FromArgb(255, 88, 236, 245), Offset = 0 });
            gradient.GradientStops.Add(new GradientStop { Color = Windows.UI.Color.FromArgb(255, 110, 156, 255), Offset = 0.28 });
            gradient.GradientStops.Add(new GradientStop { Color = Windows.UI.Color.FromArgb(255, 165, 123, 255), Offset = 0.58 });
            gradient.GradientStops.Add(new GradientStop { Color = Windows.UI.Color.FromArgb(255, 243, 106, 207), Offset = 1 });
            return gradient;
        }
    }

    public void SetLightTheme(bool useLightTheme)
    {
        if (_useLightTheme == useLightTheme)
        {
            return;
        }

        _useLightTheme = useLightTheme;
        OnPropertyChanged(nameof(CardBorderBrush));
        OnPropertyChanged(nameof(SaturationBrush));
    }

    public ImageSource? Icon
    {
        get => _icon;
        set
        {
            if (SetProperty(ref _icon, value))
            {
                OnPropertyChanged(nameof(IconVisibility));
                OnPropertyChanged(nameof(InitialVisibility));
            }
        }
    }

    public Visibility IconVisibility => Icon is null ? Visibility.Collapsed : Visibility.Visible;
    public Visibility InitialVisibility => Icon is null ? Visibility.Visible : Visibility.Collapsed;

    public void ApplyDetectedName(GameNameResult result)
    {
        string normalized = string.IsNullOrWhiteSpace(result.Name) ? ExecutableName : result.Name.Trim();
        bool nameChanged = !string.Equals(_detectedName, normalized, StringComparison.Ordinal);
        bool sourceChanged = _nameSource != result.Source;
        _detectedName = normalized;
        _nameSource = result.Source;

        if (nameChanged)
        {
            OnPropertyChanged(nameof(DetectedName));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(Initial));
        }
        if (nameChanged || sourceChanged)
        {
            OnPropertyChanged(nameof(NameSourceText));
        }
    }

    public void SetCustomName(string? name)
    {
        string? normalized = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        if (string.Equals(_customName, normalized, StringComparison.Ordinal))
        {
            return;
        }

        _customName = normalized;
        OnPropertyChanged(nameof(CustomName));
        OnPropertyChanged(nameof(HasCustomName));
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(Initial));
        OnPropertyChanged(nameof(NameSourceText));
    }
}
