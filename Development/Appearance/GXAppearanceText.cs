using System.Globalization;
using System.Resources;

namespace Gurux.UI.Components.Appearance;

internal static class GXAppearanceText
{
    private static readonly ResourceManager Resources = new("Gurux.UI.Components.Appearance.GXAppearanceResources", typeof(GXAppearanceText).Assembly);
    internal static string Get(string key) => Resources.GetString(key, CultureInfo.CurrentUICulture) ?? key;
}
