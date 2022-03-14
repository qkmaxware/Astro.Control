using System;
using System.Linq;
using Qkmaxware.Astro.Control.Devices;
using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Devices {

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
public class IndiTelescopeController : IndiDeviceController, ITelescope {
    public IndiTelescopeController(IndiDevice device) : base(device) {}

    private string mode;
    private void setMode(string mode) {
        var vector = GetPropertyOrThrow<IndiVector<IndiSwitchValue>>(IndiStandardProperties.TelescopeOnCoordinateSet);;
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

    private bool J2000;
    public void UseJNowCoordinates(bool jnow) {
        J2000 = !jnow;
    }

    /// <summary>
    /// Synchronize the telescope's orientation (pointing direction) 
    /// </summary>
    /// <param name="ra">current RA angle</param>
    /// <param name="dec">current DEC angle</param>
    public void Sync(Angle ra, Angle dec) {
        var vector = this.GetPropertyOrThrow<IndiVector<IndiNumberValue>>(
            J2000 
            ? IndiStandardProperties.TelescopeJ2000EquatorialCoordinate 
            : IndiStandardProperties.TelescopeJNowEquatorialCoordinate
        );
        syncNext();
        vector.GetItemWithName("RA").Value = (double)ra.TotalHours();
        vector.GetItemWithName("DEC").Value = (double)dec.TotalDegrees();
        SetProperty(vector.Name, vector);
    }

    /// <summary>
    /// Set the telescope's slew rate
    /// </summary>
    /// <param name="rate">speed</param>
    public void SetSlewRate(SlewRate rate) {
        var vector = GetPropertyOrThrow<IndiVector<IndiSwitchValue>>(IndiStandardProperties.TelescopeSlewRate);
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
        var vector = GetPropertyOrThrow<IndiVector<IndiSwitchValue>>(IndiStandardProperties.TelescopeMotionWestEast);
        vector.GetSwitch("MOTION_WEST").Value = motion == Direction.West || motion == Direction.NorthWest || motion == Direction.SouthWest;
        vector.GetSwitch("MOTION_EAST").Value = motion == Direction.East || motion == Direction.NorthEast || motion == Direction.SouthEast;
        SetProperty(vector.Name, vector);

        vector = GetPropertyOrThrow<IndiVector<IndiSwitchValue>>(IndiStandardProperties.TelescopeMotionNorthSouth);
        vector.GetSwitch("MOTION_NORTH").Value = motion == Direction.North || motion == Direction.NorthEast || motion == Direction.NorthWest;
        vector.GetSwitch("MOTION_SOUTH").Value = motion == Direction.South || motion == Direction.SouthEast || motion == Direction.SouthWest;
        SetProperty(vector.Name, vector);
    }

    /// <summary>
    /// Instruct the telescope to slew to the given coordinates
    /// </summary>
    /// <param name="ra">desired RA angle</param>
    /// <param name="dec">desired DEC angle</param>
    public void Goto(Angle ra, Angle dec) {
        var vector = this.GetPropertyOrThrow<IndiVector<IndiNumberValue>>(
            J2000 
            ? IndiStandardProperties.TelescopeJ2000EquatorialCoordinate 
            : IndiStandardProperties.TelescopeJNowEquatorialCoordinate
        );
        slewNext();
        vector.GetItemWithName("RA").Value = (double)ra.TotalHours();
        vector.GetItemWithName("DEC").Value = (double)dec.TotalDegrees();
        SetProperty(vector.Name, vector);
    }
    /// <summary>
    /// Instruct the telescope to slew to the given coordinates and track the object
    /// </summary>
    /// <param name="ra">desired RA angle</param>
    /// <param name="dec">desired DEC angle</param>
    /// <param name="rate">tracking rate</param>
    public void Track(Angle ra, Angle dec, TrackingRate rate = TrackingRate.Sidereal) {
        // Set tracking rate
        var rateString = "TRACK_" + rate.ToString().ToUpperInvariant();
        var trackVector = this.GetPropertyOrDefault<IndiVector<IndiSwitchValue>>("TELESCOPE_TRACK_RATE");
        if (trackVector != null) {
            trackVector.SwitchTo(rateString);
            SetProperty(trackVector);
        }

        // Set the tracking position
        var posVector = this.GetPropertyOrThrow<IndiVector<IndiNumberValue>>(
            J2000 
            ? IndiStandardProperties.TelescopeJ2000EquatorialCoordinate 
            : IndiStandardProperties.TelescopeJNowEquatorialCoordinate
        );
        trackNext();
        posVector.GetItemWithName("RA").Value = (double)ra.TotalHours();
        posVector.GetItemWithName("DEC").Value = (double)dec.TotalDegrees();
        SetProperty(posVector);
    }
}

}