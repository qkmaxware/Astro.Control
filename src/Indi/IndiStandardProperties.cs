namespace Qkmaxware.Astro.Control {

/// <summary>
/// Standard INDI properties
/// https://indilib.org/develop/developer-manual/101-standard-properties.html
/// </summary>
public static class IndiStandardProperties {
    // General
    public static readonly string Connection = "CONNECTION";
    public static readonly string Port = "DEVICE_PORT";
    public static readonly string LocalSiderealTime = "TIME_LST";
    public static readonly string LocalUtcTime = "TIME_UTC";
    public static readonly string GeographicCoordinate = "GEOGRAPHIC_COORD";
    public static readonly string Atmosphere = "ATMOSPHERE";
    public static readonly string UploadMode = "UPLOAD_MODE";
    public static readonly string UploadSettings = "UPLOAD_SETTINGS";
    public static readonly string ActiveDevices = "ACTIVE_DEVICES";

    // Telescopes
    public static readonly string TelescopeJ2000EquatorialCoordinate = "EQUATORIAL_COORD";
    public static readonly string TelescopeJNowEquatorialCoordinate = "EQUATORIAL_EOD_COORD";
    public static readonly string TelescopeTargetSlewCoordinate = "TARGET_EOD_COORD";
    public static readonly string TelescopeAltAzimuthCoordinate = "HORIZONTAL_COORD";
    public static readonly string TelescopeOnCoordinateSet = "ON_COORD_SET";
    public static readonly string TelescopeMotionNorthSouth = "TELESCOPE_MOTION_NS";
    public static readonly string TelescopeMotionWestEast = "TELESCOPE_MOTION_WE";
    public static readonly string TelescopeTimedGuideNorthSouth = "TELESCOPE_TIMED_GUIDE_NS";
    public static readonly string TelescopeTimedGuideWestEast = "TELESCOPE_TIMED_GUIDE_WE";
    public static readonly string TelescopeSlewRate = "TELESCOPE_SLEW_RATE";
    public static readonly string TelescopePark = "TELESCOPE_PARK";
    public static readonly string TelescopeParkPosition = "TELESCOPE_PARK_POSITION";
    public static readonly string TelescopeParkOption = "TELESCOPE_PARK_OPTION";
    public static readonly string TelescopeAbortMotion = "TELESCOPE_ABORT_MOTION";
    public static readonly string TelescopeTrackRate = "TELESCOPE_TRACK_RATE";
    public static readonly string TelescopeInfo = "TELESCOPE_INFO";
    public static readonly string TelescopePierSide = "TELESCOPE_PIER_SIDE";

    // CCDs
    public static readonly string CcdExposure = "CCD_EXPOSURE";
    public static readonly string CcdAbortExposure = "CCD_ABORT_EXPOSURE";
    public static readonly string CcdFrame = "CCD_FRAME";
    public static readonly string CcdTemperature = "CCD_TEMPERATURE";
    public static readonly string CcdCooler = "CCD_COOLER";
    public static readonly string CcdCoolerPower = "CCD_COOLER_POWER";
    public static readonly string CcdFrameType = "CCD_FRAME_TYPE";
    public static readonly string CcdBinning = "CCD_BINNING";
    public static readonly string CcdCompression = "CCD_COMPRESSION";
    public static readonly string CcdFrameReset = "CCD_FRAME_RESET";
    public static readonly string CcdInfo = "CCD_INFO";
    public static readonly string CcdCfa = "CCD_CFA";
    public static readonly string Ccd1 = "CCD1";

    // Streaming
}

}