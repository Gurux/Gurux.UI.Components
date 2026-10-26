namespace Gurux.UI.Components;

/// <summary>A location displayed on a <see cref="GXMap"/>.</summary>
public class GXMapMarker
{
    /// <summary>Gets or sets the latitude in degrees, from -90 to 90.</summary>
    public double Latitude { get; set; }

    /// <summary>Gets or sets the longitude in degrees, from -180 to 180.</summary>
    public double Longitude { get; set; }

    /// <summary>Gets or sets the marker's accessible name and hover text.</summary>
    public string? Title { get; set; }

    /// <summary>Gets or sets an absolute HTTP or HTTPS URL opened in a new tab on click.</summary>
    public string? Url { get; set; }
}
