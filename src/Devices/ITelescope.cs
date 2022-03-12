
namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Tracking rate abstraction enum
/// </summary>
public enum TrackingRate {
    Sidereal, Solar, Lunar, King, Custom
}

/// <summary>
/// Interface describing device controls for telescope mounts
/// </summary>
public interface ITelescope : IDevice {
    /// <summary>
    /// Synchronize the orientation of the telescope to the given coordinates
    /// </summary>
    /// <param name="ra">right ascension</param>
    /// <param name="dec">declination</param>
    void Sync(double ra, double dec);

    /// <summary>
    /// Automated slew to the given coordinates 
    /// </summary>
    /// <param name="ra">right ascension</param>
    /// <param name="dec">declination</param>
    void Goto(double ra, double dec);

    /// <summary>
    /// Automated slew and tracking of the given coordinates 
    /// </summary>
    /// <param name="ra">right ascension</param>
    /// <param name="dec">declination</param>
    /// <param name="rate">tracking rate</param>
    void Track(double ra, double dec, TrackingRate rate);
}

}