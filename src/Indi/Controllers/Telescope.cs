using System;
using System.Linq;

namespace Qkmaxware.Astro.Control.Controllers {

public enum SlewRate {
    Guide, Centering, Find, Max
}

public enum Direction {
    None, North, South, East, West, NorthEast, NorthWest, SouthEast, SouthWest
}

/// <summary>
/// Controller abstraction for telescope devices
/// </summary>
public class IndiTelescopeController : IndiDeviceController {
    public bool IsPositioned {get; private set;}
    public bool IsOrientated {get; private set;}
    public bool IsAligned => IsPositioned && IsOrientated;

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

    public void SetTimeToClient() {
        var vector = this.GetProperty<IndiVector<IndiTextValue>>(IndiStandardProperties.LocalUtcTime);
        var time = DateTime.Now;
        if (vector.IsWritable) {
            vector.GetItemWithName("UTC").Value = time.ToUniversalTime().ToShortTimeString();
            vector.GetItemWithName("OFFSET").Value = TimeZoneInfo.Local.GetUtcOffset(time).TotalHours.ToString();
            SetProperty(vector.Name, vector);
        }
    }

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
        this.IsOrientated = true;
    }

    public void SetLocation(double lat, double lon, double alt) {
        var vector = GetProperty<IndiVector<IndiNumberValue>>(IndiStandardProperties.GeographicCoordinate);
        vector.GetItemWithName("LAT").Value = lat;
        vector.GetItemWithName("LONG").Value = lon;
        vector.GetItemWithName("ELEV").Value = Math.Max(0, alt);
        SetProperty(vector.Name, vector);
        this.IsPositioned = true;
    }

    public void SetSlewRate(SlewRate rate) {
        var vector = GetProperty<IndiVector<IndiSwitchValue>>(IndiStandardProperties.TelescopeSlewRate);
        var index = (int)(((int)rate / 3f) * (vector.Count - 1)); 
        vector.SwitchTo(index);
        SetProperty(vector.Name, vector);
    }

    public void ClearMotion() {
        Rotate(Direction.None);
    }

    public void Rotate(Direction motion) {
        var vector = GetProperty<IndiVector<IndiSwitchValue>>(IndiStandardProperties.TelescopeMotionWestEast);
        vector.GetSwitch("MOTION_WEST").Value = motion == Direction.West || motion == Direction.NorthWest || motion == Direction.SouthWest;
        vector.GetSwitch("MOTION_EAST").Value = motion == Direction.East || motion == Direction.NorthEast || motion == Direction.SouthEast;
        SetProperty(vector.Name, vector);

        vector = GetProperty<IndiVector<IndiSwitchValue>>(IndiStandardProperties.TelescopeMotionNorthSouth);
        vector.GetSwitch("MOTION_NORTH").Value = motion == Direction.North || motion == Direction.NorthEast || motion == Direction.NorthWest;
        vector.GetSwitch("MOTION_SOUTH").Value = motion == Direction.South || motion == Direction.SouthEast || motion == Direction.SouthWest;
        SetProperty(vector.Name, vector);
    }

    public void Goto(double raDegrees, double decDegrees, bool J2000 = false) {
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