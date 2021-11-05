using System;
using System.Linq;

namespace Qkmaxware.Astro.Control.Controllers {

/// <summary>
/// Slew rate abstraction enum
/// </summary>
public enum SlewRate {
    Guide, Centering, Find, Max
}

/// <summary>
/// Direction enum
/// </summary>
public enum Direction {
    None, North, South, East, West, NorthEast, NorthWest, SouthEast, SouthWest
}

/// <summary>
/// Controller abstraction for telescope devices
/// </summary>
public class IndiTelescopeController : IndiDeviceController {
    /// <summary>
    /// Check if the telescope knows what direction it is pointing
    /// </summary>
    public bool IsOrientated => AlignmentPointCount > 0;
    /// <summary>
    /// Count of the number of points used to align the telescope's direction
    /// </summary>
    /// <value>Number of synchronized alignment points</value>
    public int AlignmentPointCount {get; private set;}
    /*/// <summary>
    /// Check if the telescope's time has been synchronized
    /// </summary>
    public bool HasTimeBeenSet {get; private set;}
    /// <summary>
    /// Check if the telescope knows its geo-position
    /// </summary>
    public bool IsPositioned {get; private set;}
    /// <summary>
    /// Check if the telescope is fully aligned
    /// </summary>
    public bool IsAligned => HasTimeBeenSet && IsPositioned && IsOrientated;*/

    public IndiTelescopeController(IndiDevice device) : base(device) {}

    private string mode;
    private void setMode(string mode) {
        var vector = GetProperty<IndiVector<IndiSwitchValue>>(IndiStandardProperties.TelescopeOnCoordinateSet);;
        this.mode = mode;
        vector.SwitchTo((toggle) => toggle.Name == mode);
        SetProperty("ON_COORD_SET", vector);
    }
    private void slewNext() {
        if (mode != "SLEW")
            setMode("SLEW");
    }
    private void trackNext() {
        if (mode != "TRACK")
            setMode("TRACK");
    }
    private void syncNext() {
        if (mode != "SYNC")
            setMode("SYNC");
    }

    /// <summary>
    /// Synchronize the telescope's orientation (pointing direction) 
    /// </summary>
    /// <param name="ra">current RA angle</param>
    /// <param name="dec">current DEC angle</param>
    /// <param name="J2000"></param>
    public void SetOrientation(double ra, double dec, bool J2000 = false) {
        var vector = this.GetProperty<IndiVector<IndiNumberValue>>(
            J2000 
            ? IndiStandardProperties.TelescopeJ2000EquatorialCoordinate 
            : IndiStandardProperties.TelescopeJNowEquatorialCoordinate
        );
        syncNext();
        vector.GetItemWithName("RA").Value = ra;
        vector.GetItemWithName("DEC").Value = dec;
        SetProperty(vector.Name, vector);
        this.AlignmentPointCount++;
    }

    /// <summary>
    /// Set the telescope's slew rate
    /// </summary>
    /// <param name="rate">speed</param>
    public void SetSlewRate(SlewRate rate) {
        var vector = GetProperty<IndiVector<IndiSwitchValue>>(IndiStandardProperties.TelescopeSlewRate);
        var index = (int)(((int)rate / 3f) * (vector.Count - 1)); 
        vector.SwitchTo(index);
        SetProperty(vector.Name, vector);
    }

    /// <summary>
    /// Stop the current rotation
    /// </summary>
    public void StopRotation() {
        StartRotating(Direction.None);
    }

    /// <summary>
    /// Begin telescope rotation in the given direction
    /// </summary>
    /// <param name="motion">direction of rotation</param>
    public void StartRotating(Direction motion) {
        var vector = GetProperty<IndiVector<IndiSwitchValue>>(IndiStandardProperties.TelescopeMotionWestEast);
        vector.GetSwitch("MOTION_WEST").Value = motion == Direction.West || motion == Direction.NorthWest || motion == Direction.SouthWest;
        vector.GetSwitch("MOTION_EAST").Value = motion == Direction.East || motion == Direction.NorthEast || motion == Direction.SouthEast;
        SetProperty(vector.Name, vector);

        vector = GetProperty<IndiVector<IndiSwitchValue>>(IndiStandardProperties.TelescopeMotionNorthSouth);
        vector.GetSwitch("MOTION_NORTH").Value = motion == Direction.North || motion == Direction.NorthEast || motion == Direction.NorthWest;
        vector.GetSwitch("MOTION_SOUTH").Value = motion == Direction.South || motion == Direction.SouthEast || motion == Direction.SouthWest;
        SetProperty(vector.Name, vector);
    }

    /// <summary>
    /// Instruct the telescope to slew to the given coordinates
    /// </summary>
    /// <param name="raDegrees">desired RA angle</param>
    /// <param name="decDegrees">desired DEC angle</param>
    /// <param name="J2000"></param>
    public void GotoOrientation(double raDegrees, double decDegrees, bool J2000 = false) {
        var vector = this.GetProperty<IndiVector<IndiNumberValue>>(
            J2000 
            ? IndiStandardProperties.TelescopeJ2000EquatorialCoordinate 
            : IndiStandardProperties.TelescopeJNowEquatorialCoordinate
        );
        slewNext();
        vector.GetItemWithName("RA").Value = raDegrees;
        vector.GetItemWithName("DEC").Value = decDegrees;
        SetProperty(vector.Name, vector);
    }
}

}