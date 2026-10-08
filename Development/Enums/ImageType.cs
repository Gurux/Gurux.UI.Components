namespace Gurux.UI.Components.Enums
{
    /// <summary>
    /// Defines how an image value is interpreted and rendered.
    /// </summary>
    public enum ImageType : byte
    {
        /// <summary>
        /// Detects the rendering type from the image value.
        /// </summary>
        Auto,
        /// <summary>
        /// Treats the value as a raster image URI or Base64 image content.
        /// </summary>
        Raster,
        /// <summary>
        /// Treats the value as SVG markup.
        /// </summary>
        Svg,
        /// <summary>
        /// Treats the value as an icon ligature or icon class identifier.
        /// </summary>
        IconLigature,
        /// <summary>
        /// Treats the value as HTML markup.
        /// </summary>
        Html
    }
}