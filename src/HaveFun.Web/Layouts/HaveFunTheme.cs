using MudBlazor;

namespace HaveFun.Web;

public static class HaveFunTheme
{
    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#2563eb",
            Secondary = "#0f766e",
            AppbarBackground = "#2563eb",
            Background = "#f8fafc",
            Surface = "#ffffff"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "6px",
            DrawerWidthLeft = "240px"
        }
    };
}
