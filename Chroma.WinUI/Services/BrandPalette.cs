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

        SetColorAndBrush(theme, "AppBackgroundColor", "AppBackgroundBrush", "#02040E");
        SetColorAndBrush(theme, "SidebarColor", "SidebarBrush", "#040617");
        SetColorAndBrush(theme, "PanelColor", "PanelBrush", "#071020");
        SetColorAndBrush(theme, "PanelRaisedColor", "PanelRaisedBrush", "#0B182C");
        SetColorAndBrush(theme, "PanelHoverColor", "PanelHoverBrush", "#10233E");
        SetColorAndBrush(theme, "StrokeColor", "StrokeBrush", "#2A4568");
        SetColorAndBrush(theme, "TextPrimaryColor", "TextPrimaryBrush", "#FAF9FF");
        SetColorAndBrush(theme, "TextSecondaryColor", "TextSecondaryBrush", "#BBC0D2");
        SetColorAndBrush(theme, "TextMutedColor", "TextMutedBrush", "#7F8BA2");

        SetBrush(theme, "TitleBarBrush", "#D902040E");
        SetBrush(theme, "ProfileCardBrush", "#081326");
        SetBrush(theme, "ProfileIconBrush", "#050918");
        SetBrush(theme, "ProfileIconStrokeBrush", "#2B4567");
        SetBrush(theme, "ProfileMenuIconBrush", "#E8EAF4");
        SetBrush(theme, "DropZoneStrokeBrush", "#4A648A");
        SetBrush(theme, "EditorIconBrush", "#050A18");
        SetBrush(theme, "InputBrush", "#060D1D");
        SetBrush(theme, "InputStrokeBrush", "#355579");
        SetBrush(theme, "DisabledPanelBrush", "#081426");
        SetBrush(theme, "DisabledPanelStrokeBrush", "#294564");
        SetBrush(theme, "DisabledBadgeBrush", "#162A46");
        SetBrush(theme, "FooterBrush", "#06101F");
        SetBrush(theme, "FooterStrokeBrush", "#294563");
        SetBrush(theme, "StatusCardBrush", "#071326");
        SetBrush(theme, "StatusCardStrokeBrush", "#2D4B70");
        SetBrush(theme, "ButtonHoverOverlayBrush", "#1C6E9CFF");
        SetBrush(theme, "ButtonPressedOverlayBrush", "#2CA57BFF");
        SetBrush(theme, "NumericHoverBrush", "#173456");
        SetBrush(theme, "NumericPressedBrush", "#224A70");
        SetBrush(theme, "InactiveNavTextBrush", "#B2B7CA");
        SetBrush(theme, "ProfilesNavIconBrush", "#71F2F8");
        SetBrush(theme, "SettingsNavIconBrush", "#F88AD7");
        SetBrush(theme, "AboutNavIconBrush", "#B69AFF");
        SetBrush(theme, "FooterSecondaryIconBrush", "#9BC7FF");
        SetBrush(theme, "CyanBrush", "#58ECF5");
        SetBrush(theme, "MagentaBrush", "#E878C7");
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
        SetBrush(theme, "ButtonHoverOverlayBrush", "#1458A7D8");
        SetBrush(theme, "ButtonPressedOverlayBrush", "#208E6FD0");
        SetBrush(theme, "NumericHoverBrush", "#105A7EB0");
        SetBrush(theme, "NumericPressedBrush", "#185A7EB0");
        SetBrush(theme, "InactiveNavTextBrush", "#6C748C");
        SetBrush(theme, "ProfilesNavIconBrush", "#087F93");
        SetBrush(theme, "SettingsNavIconBrush", "#B72B91");
        SetBrush(theme, "AboutNavIconBrush", "#7154BF");
        SetBrush(theme, "FooterSecondaryIconBrush", "#4D72AC");
        SetBrush(theme, "CyanBrush", "#087F93");
        SetBrush(theme, "MagentaBrush", "#A93682");
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
