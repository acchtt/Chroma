using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.UI;

namespace Chroma.Services;

/// <summary>
/// Applies the rounded Chroma badge palette to both WinUI themes.
/// The visual system follows the logo's deep navy glass, cyan edge light,
/// lavender core, pink glow, and small lime and gold spectrum highlights.
/// </summary>
internal static class BrandPalette
{
    public static void Apply(ResourceDictionary resources)
    {
        if (resources.ThemeDictionaries["Dark"] is ResourceDictionary dark)
        {
            ApplyDarkTheme(dark);
        }

        if (resources.ThemeDictionaries["Light"] is ResourceDictionary light)
        {
            ApplyLightTheme(light);
        }

        resources["CyanColor"] = ParseColor("#58ECF5");
        resources["BlueColor"] = ParseColor("#6E9CFF");
        resources["VioletColor"] = ParseColor("#A57BFF");
        resources["MagentaColor"] = ParseColor("#F36ACF");
        resources["PinkColor"] = ParseColor("#FF9BCF");
        resources["LimeColor"] = ParseColor("#8CF5A5");
        resources["GoldColor"] = ParseColor("#FFD97A");
        resources["LimeBrush"] = Brush("#8CF5A5");
        resources["GoldBrush"] = Brush("#FFD97A");

        resources["NeonGradientBrush"] = Gradient(
            ("#58ECF5", 0.00),
            ("#6E9CFF", 0.24),
            ("#A57BFF", 0.50),
            ("#F36ACF", 0.79),
            ("#FF9BCF", 1.00));

        LinearGradientBrush navigation = Gradient(
            ("#37DDF0", 0.00),
            ("#7C73FF", 0.46),
            ("#ED52C7", 1.00));
        navigation.Opacity = 0.94;
        resources["NavActiveBrush"] = navigation;

        resources["SliderGradientBrush"] = HorizontalGradient(
            ("#58ECF5", 0.00),
            ("#6E9CFF", 0.26),
            ("#A57BFF", 0.56),
            ("#F36ACF", 1.00));

        resources["PrimaryActionGradientBrush"] = HorizontalGradient(
            ("#39DDF3", 0.00),
            ("#6E9CFF", 0.25),
            ("#A57BFF", 0.52),
            ("#F36ACF", 0.82),
            ("#FF9BCF", 1.00));
    }

    private static void ApplyDarkTheme(ResourceDictionary theme)
    {
        SetColor(theme, "AppBackgroundColor", "AppBackgroundBrush", "#030612");
        SetColor(theme, "SidebarColor", "SidebarBrush", "#06091A");
        SetColor(theme, "PanelColor", "PanelBrush", "#09162B");
        SetColor(theme, "PanelRaisedColor", "PanelRaisedBrush", "#0E1F3B");
        SetColor(theme, "PanelHoverColor", "PanelHoverBrush", "#152B4E");
        SetColor(theme, "StrokeColor", "StrokeBrush", "#36577E");
        SetColor(theme, "TextPrimaryColor", "TextPrimaryBrush", "#FBF9FF");
        SetColor(theme, "TextSecondaryColor", "TextSecondaryBrush", "#C2C3D8");
        SetColor(theme, "TextMutedColor", "TextMutedBrush", "#8793AB");

        theme["TitleBarBrush"] = Brush("#DC030612");
        theme["ProfileCardBrush"] = Brush("#0B1930");
        theme["ProfileIconBrush"] = Brush("#070B1D");
        theme["ProfileIconStrokeBrush"] = Brush("#3A5C84");
        theme["ProfileMenuIconBrush"] = Brush("#ECE8F7");
        theme["DropZoneStrokeBrush"] = Brush("#5A7198");
        theme["EditorIconBrush"] = Brush("#070C1C");
        theme["InputBrush"] = Brush("#071023");
        theme["InputStrokeBrush"] = Brush("#40638E");
        theme["DisabledPanelBrush"] = Brush("#0A172B");
        theme["DisabledPanelStrokeBrush"] = Brush("#2F4C70");
        theme["DisabledBadgeBrush"] = Brush("#1A2D4A");
        theme["FooterBrush"] = Brush("#08152A");
        theme["FooterStrokeBrush"] = Brush("#315174");
        theme["StatusCardBrush"] = Brush("#09172D");
        theme["StatusCardStrokeBrush"] = Brush("#35577D");
        theme["ButtonHoverOverlayBrush"] = Brush("#18FFFFFF");
        theme["ButtonPressedOverlayBrush"] = Brush("#26FFFFFF");
        theme["NumericHoverBrush"] = Brush("#1A3B61");
        theme["NumericPressedBrush"] = Brush("#27527D");
        theme["InactiveNavTextBrush"] = Brush("#B8BDD1");
        theme["ProfilesNavIconBrush"] = Brush("#71F2F8");
        theme["SettingsNavIconBrush"] = Brush("#F88AD7");
        theme["AboutNavIconBrush"] = Brush("#B69AFF");
        theme["FooterSecondaryIconBrush"] = Brush("#9BC7FF");
        theme["CyanBrush"] = Brush("#58ECF5");
        theme["MagentaBrush"] = Brush("#F36ACF");
        theme["PositiveBrush"] = Brush("#8CF5A5");
        theme["DangerBrush"] = Brush("#FF7195");
        theme["PrimaryActionForegroundBrush"] = Brush("#FFFFFF");
        theme["LogoTaglinePrimaryBrush"] = Brush("#58ECF5");
        theme["LogoTaglineSecondaryBrush"] = Brush("#F58EDB");
    }

    private static void ApplyLightTheme(ResourceDictionary theme)
    {
        SetColor(theme, "AppBackgroundColor", "AppBackgroundBrush", "#F8F7FF");
        SetColor(theme, "SidebarColor", "SidebarBrush", "#FFFDFE");
        SetColor(theme, "PanelColor", "PanelBrush", "#FFFFFF");
        SetColor(theme, "PanelRaisedColor", "PanelRaisedBrush", "#F7F2FF");
        SetColor(theme, "PanelHoverColor", "PanelHoverBrush", "#EEF7FF");
        SetColor(theme, "StrokeColor", "StrokeBrush", "#D1D8EA");
        SetColor(theme, "TextPrimaryColor", "TextPrimaryBrush", "#171525");
        SetColor(theme, "TextSecondaryColor", "TextSecondaryBrush", "#545B75");
        SetColor(theme, "TextMutedColor", "TextMutedBrush", "#7F89A1");

        theme["TitleBarBrush"] = Brush("#FCFAFF");
        theme["ProfileCardBrush"] = Brush("#FFFFFF");
        theme["ProfileIconBrush"] = Brush("#F5F2FF");
        theme["ProfileIconStrokeBrush"] = Brush("#CFD6E8");
        theme["ProfileMenuIconBrush"] = Brush("#596079");
        theme["DropZoneStrokeBrush"] = Brush("#B3BDD2");
        theme["EditorIconBrush"] = Brush("#F5F2FF");
        theme["InputBrush"] = Brush("#FFFFFF");
        theme["InputStrokeBrush"] = Brush("#C0CAE0");
        theme["DisabledPanelBrush"] = Brush("#F7F7FC");
        theme["DisabledPanelStrokeBrush"] = Brush("#D4DAE8");
        theme["DisabledBadgeBrush"] = Brush("#ECEFFA");
        theme["FooterBrush"] = Brush("#FFFFFF");
        theme["FooterStrokeBrush"] = Brush("#D4DAE8");
        theme["StatusCardBrush"] = Brush("#FFFFFF");
        theme["StatusCardStrokeBrush"] = Brush("#D4DAE8");
        theme["ButtonHoverOverlayBrush"] = Brush("#0C1B2F52");
        theme["ButtonPressedOverlayBrush"] = Brush("#151B2F52");
        theme["NumericHoverBrush"] = Brush("#105A7EB0");
        theme["NumericPressedBrush"] = Brush("#185A7EB0");
        theme["InactiveNavTextBrush"] = Brush("#6C748C");
        theme["ProfilesNavIconBrush"] = Brush("#087F93");
        theme["SettingsNavIconBrush"] = Brush("#B72B91");
        theme["AboutNavIconBrush"] = Brush("#7154BF");
        theme["FooterSecondaryIconBrush"] = Brush("#4D72AC");
        theme["CyanBrush"] = Brush("#087F93");
        theme["MagentaBrush"] = Brush("#B72B91");
        theme["PositiveBrush"] = Brush("#168453");
        theme["DangerBrush"] = Brush("#C84368");
        theme["PrimaryActionForegroundBrush"] = Brush("#FFFFFF");
        theme["LogoTaglinePrimaryBrush"] = Brush("#087F93");
        theme["LogoTaglineSecondaryBrush"] = Brush("#A12A83");
    }

    private static void SetColor(
        ResourceDictionary dictionary,
        string colorKey,
        string brushKey,
        string hex)
    {
        Color color = ParseColor(hex);
        dictionary[colorKey] = color;
        dictionary[brushKey] = new SolidColorBrush(color);
    }

    private static SolidColorBrush Brush(string hex) =>
        new(ParseColor(hex));

    private static LinearGradientBrush Gradient(params (string Hex, double Offset)[] stops) =>
        CreateGradient(new Point(0, 0), new Point(1, 1), stops);

    private static LinearGradientBrush HorizontalGradient(params (string Hex, double Offset)[] stops) =>
        CreateGradient(new Point(0, 0.5), new Point(1, 0.5), stops);

    private static LinearGradientBrush CreateGradient(
        Point start,
        Point end,
        params (string Hex, double Offset)[] stops)
    {
        var brush = new LinearGradientBrush
        {
            StartPoint = start,
            EndPoint = end
        };

        foreach ((string hex, double offset) in stops)
        {
            brush.GradientStops.Add(new GradientStop
            {
                Color = ParseColor(hex),
                Offset = offset
            });
        }

        return brush;
    }

    private static Color ParseColor(string value)
    {
        string hex = value.TrimStart('#');
        if (hex.Length is not (6 or 8))
        {
            throw new FormatException($"Invalid Chroma color '{value}'.");
        }

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
