using System;
using System.Linq;
using Qkmaxware.Astro.Control.Devices;
using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Controller abstraction for telescope devices
/// </summary>
public class IndiTelescopeController : IndiDeviceController, ITelescope {
    public IndiTelescopeController(IndiDevice device) : base(device) {}

    /// <summary>
    /// The current right ascension of the telescope
    /// </summary>
    /// <returns></returns>
    public Angle RightAscension => Angle.Hours(this.GetPropertyOrDefault<IndiVector<IndiNumberValue>>(IndiStandardProperties.TelescopeJNowEquatorialCoordinate)?.GetItemWithName("RA")?.Value ?? 0);
    /// <summary>
    /// The current declination of the telescope
    /// </summary>
    /// <returns></returns>
    public Angle Declination => Angle.Degrees(this.GetPropertyOrDefault<IndiVector<IndiNumberValue>>(IndiStandardProperties.TelescopeJNowEquatorialCoordinate)?.GetItemWithName("DEC")?.Value ?? 0);

    public bool IsSlewing => GetPropertyOrDefault<IndiVector<IndiSwitchValue>>(IndiStandardProperties.TelescopeStatus)?.IsOn("SCOPE_SLEWING") ?? false;
    public bool IsTracking => GetPropertyOrDefault<IndiVector<IndiSwitchValue>>(IndiStandardProperties.TelescopeStatus)?.IsOn("SCOPE_TRACKING") ?? false;

    private string mode;
    private void setMode(string mode) {
        var vector = GetPropertyOrThrow<IndiVector<IndiSwitchValue>>(IndiStandardProperties.TelescopeOnCoordinateSet);
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

    private bool J2000 = true;
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
    /// Begin rotating the telescope in the desired direction
    /// </summary>
    /// <param name="horizontal">horizontal axis input</param>
    /// <param name="vertical">vertical axis input</param>
    public void Rotate(float horizontal, float vertical) {
        var hDir = MathF.Sign(horizontal);
        var yDir = MathF.Sin(vertical);
        var speed = MathF.Max(MathF.Min(1, MathF.Sqrt(horizontal * horizontal + vertical * vertical)), 0); // 0 to 1 speed

        // Set speed
        var speedVector = GetPropertyOrThrow<IndiVector<IndiSwitchValue>>(IndiStandardProperties.TelescopeSlewRate);
        var index = (int)(speed * (speedVector.Count - 1));  // 0 to index
        speedVector.SwitchTo(index);
        SetProperty(speedVector);

        // Set direction
        var dirVector = GetPropertyOrThrow<IndiVector<IndiSwitchValue>>(IndiStandardProperties.TelescopeMotionWestEast);
        dirVector.GetSwitch("MOTION_WEST").Value = hDir < 0;
        dirVector.GetSwitch("MOTION_EAST").Value = hDir > 0;
        SetProperty(dirVector);

        dirVector = GetPropertyOrThrow<IndiVector<IndiSwitchValue>>(IndiStandardProperties.TelescopeMotionNorthSouth);
        dirVector.GetSwitch("MOTION_NORTH").Value = yDir > 0;
        dirVector.GetSwitch("MOTION_SOUTH").Value = yDir < 0;
        SetProperty(dirVector);
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