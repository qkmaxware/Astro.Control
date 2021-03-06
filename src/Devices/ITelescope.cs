
using Qkmaxware.Measurement;

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
    void Sync(Angle ra, Angle dec);

    /// <summary>
    /// Automated slew to the given coordinates 
    /// </summary>
    /// <param name="ra">right ascension</param>
    /// <param name="dec">declination</param>
    void Goto(Angle ra, Angle dec);

    /// <summary>
    /// Automated slew and tracking of the given coordinates 
    /// </summary>
    /// <param name="ra">right ascension</param>
    /// <param name="dec">declination</param>
    /// <param name="rate">tracking rate</param>
    void Track(Angle ra, Angle dec, TrackingRate rate);
}

}