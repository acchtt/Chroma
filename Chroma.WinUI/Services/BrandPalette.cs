using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Chroma.Services;

/// <summary>
/// Recolors the existing XAML resource objects after the main window has been
/// constructed. Mutating the brushes in place keeps ThemeResource and
/// StaticResource references alive and avoids replacing the application
/// resource dictionaries during startup.
/// </summary>
internal static class BrandPalette
{
    public static void Apply(ResourceDictionary resources)
    {
        try
        {
            ApplyDarkTheme(GetThemeDictionary(resources, "Dark"));
            ApplyLightTheme(GetThemeDictionary(resources, "Light"));
            ApplySharedResources(resources);
        }
        catch
        {
            // Branding must never prevent the application from launching.
        }
    }

    private static ResourceDictionary? GetThemeDictionary(ResourceDictionary resources, string key)
    {
        try
        {
            return resources.ThemeDictionaries[key] as ResourceDictionary;
        }
        catch
        {
            return null;
        }
    }

    private static void ApplyDarkTheme(ResourceDictionary? theme)
    {
        if (theme is null)
        {
            return;
        }

        SetColorAndBrush(theme, "AppBackgroundColor", "AppBackgroundBrush", "#030612");
        SetColorAndBrush(theme, "SidebarColor", "SidebarBrush", "#06091A");
        SetColorAndBrush(theme, "PanelColor", "PanelBrush", "#09162B");
        SetColorAndBrush(theme, "PanelRaisedColor", "PanelRaisedBrush", "#0E1F3B");
        SetColorAndBrush(theme, "PanelHoverColor", "PanelHoverBrush", "#152B4E");
        SetColorAndBrush(theme, "StrokeColor", "StrokeBrush", "#36577E");
        SetColorAndBrush(theme, "TextPrimaryColor", "TextPrimaryBrush", "#FBF9FF");
        SetColorAndBrush(theme, "TextSecondaryColor", "TextSecondaryBrush", "#C2C3D8");
        SetColorAndBrush(theme, "TextMutedColor", "TextMutedBrush", "#8793AB");

        SetBrush(theme, "TitleBarBrush", "#DC030612");
        SetBrush(theme, "ProfileCardBrush", "#0B1930");
        SetBrush(theme, "ProfileIconBrush", "#070B1D");
        SetBrush(theme, "ProfileIconStrokeBrush", "#3A5C84");
        SetBrush(theme, "ProfileMenuIconBrush", "#ECE8F7");
        SetBrush(theme, "DropZoneStrokeBrush", "#5A7198");
        SetBrush(theme, "EditorIconBrush", "#070C1C");
        SetBrush(theme, "InputBrush", "#071023");
        SetBrush(theme, "InputStrokeBrush", "#40638E");
        SetBrush(theme, "DisabledPanelBrush", "#0A172B");
        SetBrush(theme, "DisabledPanelStrokeBrush", "#2F4C70");
        SetBrush(theme, "DisabledBadgeBrush", "#1A2D4A");
        SetBrush(theme, "FooterBrush", "#08152A");
        SetBrush(theme, "FooterStrokeBrush", "#315174");
        SetBrush(theme, "StatusCardBrush", "#09172D");
        SetBrush(theme, "StatusCardStrokeBrush", "#35577D");
        SetBrush(theme, "ButtonHoverOverlayBrush", "#18FFFFFF");
        SetBrush(theme, "ButtonPressedOverlayBrush", "#26FFFFFF");
        SetBrush(theme, "NumericHoverBrush", "#1A3B61");
        SetBrush(theme, "NumericPressedBrush", "#27527D");
        SetBrush(theme, "InactiveNavTextBrush", "#B8BDD1");
        SetBrush(theme, "ProfilesNavIconBrush", "#71F2F8");
        SetBrush(theme, "SettingsNavIconBrush", "#F88AD7");
        SetBrush(theme, "AboutNavIconBrush", "#B69AFF");
        SetBrush(theme, "FooterSecondaryIconBrush", "#9BC7FF");
        SetBrush(theme, "CyanBrush", "#58ECF5");
        SetBrush(theme, "MagentaBrush", "#F36ACF");
        SetBrush(theme, "PositiveBrush", "#8CF5A5");
        SetBrush(theme, "DangerBrush", "#FF7195");
        SetBrush(theme, "PrimaryActionForegroundBrush", "#FFFFFF");
        SetBrush(theme, "LogoTaglinePrimaryBrush", "#58ECF5");
        SetBrush(theme, "LogoTaglineSecondaryBrush", "#F58EDB");
    }

    private static void ApplyLightTheme(ResourceDictionary? theme)
    {
        if (theme is null)
        {
            return;
        }

        SetColorAndBrush(theme, "AppBackgroundColor", "AppBackgroundBrush", "#F8F7FF");
        SetColorAndBrush(theme, "SidebarColor", "SidebarBrush", "#FFFDFE");
        SetColorAndBrush(theme, "PanelColor", "PanelBrush", "#FFFFFF");
        SetColorAndBrush(theme, "PanelRaisedColor", "PanelRaisedBrush", "#F7F2FF");
        SetColorAndBrush(theme, "PanelHoverColor", "PanelHoverBrush", "#EEF7FF");
        SetColorAndBrush(theme, "StrokeColor", "StrokeBrush", "#D1D8EA");
        SetColorAndBrush(theme, "TextPrimaryColor", "TextPrimaryBrush", "#171525");
        SetColorAndBrush(theme, "TextSecondaryColor", "TextSecondaryBrush", "#545B75");
        SetColorAndBrush(theme, "TextMutedColor", "TextMutedBrush", "#7F89A1");

        SetBrush(theme, "TitleBarBrush", "#FCFAFF");
        SetBrush(theme, "ProfileCardBrush", "#FFFFFF");
        SetBrush(theme, "ProfileIconBrush", "#F5F2FF");
        SetBrush(theme, "ProfileIconStrokeBrush", "#CFD6E8");
        SetBrush(theme, "ProfileMenuIconBrush", "#596079");
        SetBrush(theme, "DropZoneStrokeBrush", "#B3BDD2");
        SetBrush(theme, "EditorIconBrush", "#F5F2FF");
        SetBrush(theme, "InputBrush", "#FFFFFF");
        SetBrush(theme, "InputStrokeBrush", "#C0CAE0");
        SetBrush(theme, "DisabledPanelBrush", "#F7F7FC");
        SetBrush(theme, "DisabledPanelStrokeBrush", "#D4DAE8");
        SetBrush(theme, "DisabledBadgeBrush", "#ECEFFA");
        SetBrush(theme, "FooterBrush", "#FFFFFF");
        SetBrush(theme, "FooterStrokeBrush", "#D4DAE8");
        SetBrush(theme, "StatusCardBrush", "#FFFFFF");
        SetBrush(theme, "StatusCardStrokeBrush", "#D4DAE8");
        SetBrush(theme, "ButtonHoverOverlayBrush", "#0C1B2F52");
        SetBrush(theme, "ButtonPressedOverlayBrush", "#151B2F52");
        SetBrush(theme, "NumericHoverBrush", "#105A7EB0");
        SetBrush(theme, "NumericPressedBrush", "#185A7EB0");
        SetBrush(theme, "InactiveNavTextBrush", "#6C748C");
        SetBrush(theme, "ProfilesNavIconBrush", "#087F93");
        SetBrush(theme, "SettingsNavIconBrush", "#B72B91");
        SetBrush(theme, "AboutNavIconBrush", "#7154BF");
        SetBrush(theme, "FooterSecondaryIconBrush", "#4D72AC");
        SetBrush(theme, "CyanBrush", "#087F93");
        SetBrush(theme, "MagentaBrush", "#B72B91");
        SetBrush(theme, "PositiveBrush", "#168453");
        SetBrush(theme, "DangerBrush", "#C84368");
        SetBrush(theme, "PrimaryActionForegroundBrush", "#FFFFFF");
        SetBrush(theme, "LogoTaglinePrimaryBrush", "#087F93");
        SetBrush(theme, "LogoTaglineSecondaryBrush", "#A31A83");
    }

    private static void ApplySharedResources(ResourceDictionary resources)
    {
        SetColor(resources, "CyanColor", "#58ECF5");
        SetColor(resources, "BlueColor", "#6E9CFF");
        SetColor(resources, "VioletColor", "#A57BFF");
        SetColor(resources, "MagentaColor", "#F36ACF");
        SetColor(resources, "PinkColor", "#FF9BCF");

        SetGradient(resources, "NeonGradientBrush",
            ("#58ECF5", 0.00),
            ("#6E9CFF", 0.24),
            ("#A57BFF", 0.50),
            ("#F36ACF", 0.79),
            ("#FF9BCF", 1.00));

        SetGradient(resources, "NavActiveBrush",
            ("#37DDF0", 0.00),
            ("#7C73FF", 0.46),
            ("#ED52C7", 1.00));

        SetGradient(resources, "SliderGradientBrush",
            ("#58ECF5", 0.00),
            ("#6E9CFF", 0.26),
            ("#A57BFF", 0.56),
            ("#F36ACF", 1.00));

        SetGradient(resources, "PrimaryActionGradientBrush",
            ("#39DDF3", 0.00),
            ("#6E9CFF", 0.25),
            ("#A57BFF", 0.52),
            ("#F36ACF", 0.82),
            ("#FF9BCF", 1.00));
    }

    private static void SetColorAndBrush(
        ResourceDictionary dictionary,
        string colorKey,
        string brushKey,
        string value)
    {
        SetColor(dictionary, colorKey, value);
        SetBrush(dictionary, brushKey, value);
    }

    private static void SetColor(ResourceDictionary dictionary, string key, string value)
    {
        try
        {
            dictionary[key] = ParseColor(value);
        }
        catch
        {
        }
    }

    private static void SetBrush(ResourceDictionary dictionary, string key, string value)
    {
        try
        {
            if (dictionary[key] is SolidColorBrush brush)
            {
                brush.Color = ParseColor(value);
            }
        }
        catch
        {
        }
    }

    private static void SetGradient(
        ResourceDictionary dictionary,
        string key,
        params (string Hex, double Offset)[] stops)
    {
        try
        {
            if (dictionary[key] is not LinearGradientBrush brush)
            {
                return;
            }

            brush.GradientStops.Clear();
            foreach ((string hex, double offset) in stops)
            {
                brush.GradientStops.Add(new GradientStop
                {
                    Color = ParseColor(hex),
                    Offset = offset
                });
            }
        }
        catch
        {
        }
    }

    private static Color ParseColor(string value)
    {
        string hex = value.TrimStart('#');
        int index = 0;
        byte alpha = 255;

        if (hex.Length == 8)
        {
            alpha = byte.Parse(hex[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            index = 2;
        }

        byte red = byte.Parse(hex.Substring(index, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        byte green = byte.Parse(hex.Substring(index + 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        byte blue = byte.Parse(hex.Substring(index + 4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return Color.FromArgb(alpha, red, green, blue);
    }
}
