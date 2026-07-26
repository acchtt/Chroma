using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.UI;

namespace Chroma.Services;

/// <summary>
/// Applies the neon spectrum from the rounded Chroma badge to both WinUI themes.
/// Keeping the palette in code lets the existing XAML styles continue to use
/// ThemeResource keys while the brand colors remain centralized.
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

        resources["CyanColor"] = ParseColor("#00E5FF");
        resources["BlueColor"] = ParseColor("#168CFF");
        resources["VioletColor"] = ParseColor("#8B45FF");
        resources["MagentaColor"] = ParseColor("#FF2DBF");
        resources["PinkColor"] = ParseColor("#FF67D4");
        resources["LimeColor"] = ParseColor("#7CF3A0");
        resources["GoldColor"] = ParseColor("#FFD568");
        resources["LimeBrush"] = Brush("#7CF3A0");
        resources["GoldBrush"] = Brush("#FFD568");

        resources["NeonGradientBrush"] = Gradient(
            ("#00E5FF", 0.00),
            ("#168CFF", 0.24),
            ("#8B45FF", 0.52),
            ("#FF2DBF", 0.82),
            ("#FF67D4", 1.00));

        LinearGradientBrush navigation = Gradient(
            ("#078CFF", 0.00),
            ("#7A35FF", 0.48),
            ("#F022B8", 1.00));
        navigation.Opacity = 0.96;
        resources["NavActiveBrush"] = navigation;

        resources["SliderGradientBrush"] = HorizontalGradient(
            ("#00E5FF", 0.00),
            ("#168CFF", 0.26),
            ("#8B45FF", 0.58),
            ("#FF2DBF", 1.00));

        resources["PrimaryActionGradientBrush"] = HorizontalGradient(
            ("#00C9FF", 0.00),
            ("#168CFF", 0.24),
            ("#8B45FF", 0.52),
            ("#FF2DBF", 0.82),
            ("#FF67D4", 1.00));
    }

    private static void ApplyDarkTheme(ResourceDictionary theme)
    {
        SetColor(theme, "AppBackgroundColor", "AppBackgroundBrush", "#02030D");
        SetColor(theme, "SidebarColor", "SidebarBrush", "#040617");
        SetColor(theme, "PanelColor", "PanelBrush", "#071326");
        SetColor(theme, "PanelRaisedColor", "PanelRaisedBrush", "#0B1B34");
        SetColor(theme, "PanelHoverColor", "PanelHoverBrush", "#10294B");
        SetColor(theme, "StrokeColor", "StrokeBrush", "#29466F");
        SetColor(theme, "TextPrimaryColor", "TextPrimaryBrush", "#F9FAFF");
        SetColor(theme, "TextSecondaryColor", "TextSecondaryBrush", "#B6BED3");
        SetColor(theme, "TextMutedColor", "TextMutedBrush", "#7F8CA6");

        theme["TitleBarBrush"] = Brush("#D9020410");
        theme["ProfileCardBrush"] = Brush("#09182E");
        theme["ProfileIconBrush"] = Brush("#05091A");
        theme["ProfileIconStrokeBrush"] = Brush("#31527D");
        theme["ProfileMenuIconBrush"] = Brush("#E6E9F5");
        theme["DropZoneStrokeBrush"] = Brush("#526B98");
        theme["EditorIconBrush"] = Brush("#050A18");
        theme["InputBrush"] = Brush("#050D20");
        theme["InputStrokeBrush"] = Brush("#365B89");
        theme["DisabledPanelBrush"] = Brush("#071326");
        theme["DisabledPanelStrokeBrush"] = Brush("#26466D");
        theme["DisabledBadgeBrush"] = Brush("#172C4B");
        theme["FooterBrush"] = Brush("#061225");
        theme["FooterStrokeBrush"] = Brush("#27476E");
        theme["StatusCardBrush"] = Brush("#07172D");
        theme["StatusCardStrokeBrush"] = Brush("#2A4B73");
        theme["ButtonHoverOverlayBrush"] = Brush("#16FFFFFF");
        theme["ButtonPressedOverlayBrush"] = Brush("#24FFFFFF");
        theme["NumericHoverBrush"] = Brush("#173A63");
        theme["NumericPressedBrush"] = Brush("#24517F");
        theme["InactiveNavTextBrush"] = Brush("#B0B9CF");
        theme["ProfilesNavIconBrush"] = Brush("#65EEFF");
        theme["SettingsNavIconBrush"] = Brush("#FF73D9");
        theme["AboutNavIconBrush"] = Brush("#B7A5FF");
        theme["FooterSecondaryIconBrush"] = Brush("#91C7FF");
        theme["CyanBrush"] = Brush("#00E5FF");
        theme["MagentaBrush"] = Brush("#FF2DBF");
        theme["PositiveBrush"] = Brush("#6EF2A3");
        theme["DangerBrush"] = Brush("#FF5D84");
        theme["PrimaryActionForegroundBrush"] = Brush("#FFFFFF");
        theme["LogoTaglinePrimaryBrush"] = Brush("#00E5FF");
        theme["LogoTaglineSecondaryBrush"] = Brush("#FF58CE");
    }

    private static void ApplyLightTheme(ResourceDictionary theme)
    {
        SetColor(theme, "AppBackgroundColor", "AppBackgroundBrush", "#F7F8FF");
        SetColor(theme, "SidebarColor", "SidebarBrush", "#FEFCFF");
        SetColor(theme, "PanelColor", "PanelBrush", "#FFFFFF");
        SetColor(theme, "PanelRaisedColor", "PanelRaisedBrush", "#F5F3FF");
        SetColor(theme, "PanelHoverColor", "PanelHoverBrush", "#EDF6FF");
        SetColor(theme, "StrokeColor", "StrokeBrush", "#CBD7EA");
        SetColor(theme, "TextPrimaryColor", "TextPrimaryBrush", "#151527");
        SetColor(theme, "TextSecondaryColor", "TextSecondaryBrush", "#515D78");
        SetColor(theme, "TextMutedColor", "TextMutedBrush", "#7D89A2");

        theme["TitleBarBrush"] = Brush("#FAFBFF");
        theme["ProfileCardBrush"] = Brush("#FFFFFF");
        theme["ProfileIconBrush"] = Brush("#F3F5FF");
        theme["ProfileIconStrokeBrush"] = Brush("#C8D5EA");
        theme["ProfileMenuIconBrush"] = Brush("#53617A");
        theme["DropZoneStrokeBrush"] = Brush("#AABBD4");
        theme["EditorIconBrush"] = Brush("#F3F5FF");
        theme["InputBrush"] = Brush("#FFFFFF");
        theme["InputStrokeBrush"] = Brush("#B7C9E2");
        theme["DisabledPanelBrush"] = Brush("#F6F7FC");
        theme["DisabledPanelStrokeBrush"] = Brush("#CDD8E8");
        theme["DisabledBadgeBrush"] = Brush("#E9EDF8");
        theme["FooterBrush"] = Brush("#FFFFFF");
        theme["FooterStrokeBrush"] = Brush("#CFDAEA");
        theme["StatusCardBrush"] = Brush("#FFFFFF");
        theme["StatusCardStrokeBrush"] = Brush("#CFDAEA");
        theme["ButtonHoverOverlayBrush"] = Brush("#0B1A2F52");
        theme["ButtonPressedOverlayBrush"] = Brush("#141A2F52");
        theme["NumericHoverBrush"] = Brush("#10557DB0");
        theme["NumericPressedBrush"] = Brush("#18557DB0");
        theme["InactiveNavTextBrush"] = Brush("#697790");
        theme["ProfilesNavIconBrush"] = Brush("#008DA8");
        theme["SettingsNavIconBrush"] = Brush("#B9158C");
        theme["AboutNavIconBrush"] = Brush("#6946C7");
        theme["FooterSecondaryIconBrush"] = Brush("#426FB5");
        theme["CyanBrush"] = Brush("#008DA8");
        theme["MagentaBrush"] = Brush("#B9158C");
        theme["PositiveBrush"] = Brush("#13875A");
        theme["DangerBrush"] = Brush("#C63C62");
        theme["PrimaryActionForegroundBrush"] = Brush("#FFFFFF");
        theme["LogoTaglinePrimaryBrush"] = Brush("#008DA8");
        theme["LogoTaglineSecondaryBrush"] = Brush("#A31A83");
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
