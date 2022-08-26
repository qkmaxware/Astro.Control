using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Interface for controlling telescopes via ASCOM Alpaca
/// </summary>
public class AlpacaTelescope : AlpacaDevice, ITelescope {
        
    internal AlpacaTelescope(AlpacaConnection conn, AlpacaConfiguredDevicesResponse.ConfiguredDevice rawDevice) : base(conn, rawDevice.DeviceName, rawDevice.DeviceType, rawDevice.DeviceNumber) {}
    public AlpacaTelescope(AlpacaConnection conn, string deviceName, string deviceType, int deviceNumber) : base(conn, deviceName, deviceType, deviceNumber) {}

    public Angle RightAscension {
        get {
            return Angle.Hours(Get<AlpacaValueResponse<double>>($"{Connection.Server.Host}:{Connection.Server.Port}/telescope/{DeviceNumber}/rightascension").Value);
        }
    }

    public Angle Declination {
        get {
            return Angle.Degrees(Get<AlpacaValueResponse<double>>($"{Connection.Server.Host}:{Connection.Server.Port}/telescope/{DeviceNumber}/declination").Value);
        }
    }


    public void Rotate(float horizontal, float vertical) {
        Put<AlpacaMethodResponse>(
            $"{Connection.Server.Host}:{Connection.Server.Port}/telescope/{DeviceNumber}/abortslew"
        );
        Put<AlpacaMethodResponse>(
            $"{Connection.Server.Host}:{Connection.Server.Port}/telescope/{DeviceNumber}/moveaxis",
            new KeyValuePair<string,string>("Axis", "0"),
            new KeyValuePair<string,string>("Rate", horizontal),
        );
        Put<AlpacaMethodResponse>(
            $"{Connection.Server.Host}:{Connection.Server.Port}/telescope/{DeviceNumber}/moveaxis",
            new KeyValuePair<string,string>("Axis", "1"),
            new KeyValuePair<string,string>("Rate", vertical),
        );
    }

    public void Sync(Angle ra, Angle dec) {
        Put<AlpacaMethodResponse>(
            $"{Connection.Server.Host}:{Connection.Server.Port}/telescope/{DeviceNumber}/synctocoordinates",
            new KeyValuePair<string,string>("RightAscension", ((double)ra.TotalHours()).ToString()),
            new KeyValuePair<string,string>("Declination", ((double)dec.TotalDegrees()).ToString()),
        );
    }

    public void Goto(Angle ra, Angle dec) {
        Put<AlpacaMethodResponse>(
            $"{Connection.Server.Host}:{Connection.Server.Port}/telescope/{DeviceNumber}/targetrightascension",
            new KeyValuePair<string,string>("TargetRightAscension", ((double)ra.TotalHours()).ToString())
        );
        Put<AlpacaMethodResponse>(
            $"{Connection.Server.Host}:{Connection.Server.Port}/telescope/{DeviceNumber}/targetdeclination",
            new KeyValuePair<string,string>("TargetDeclination", ((double)dec.TotalDegrees()).ToString())
        );
        Put<AlpacaMethodResponse>(
            $"{Connection.Server.Host}:{Connection.Server.Port}/telescope/{DeviceNumber}/tracking",
            new KeyValuePair<string,string>("Tracking", false.ToString())
        );
        // TODO goto rate
        Put<AlpacaMethodResponse>(
            $"{Connection.Server.Host}:{Connection.Server.Port}/telescope/{DeviceNumber}/slewtotargetasync"
        );
    }

    public void Track(Angle ra, Angle dec, TrackingRate rate) {
        Put<AlpacaMethodResponse>(
            $"{Connection.Server.Host}:{Connection.Server.Port}/telescope/{DeviceNumber}/targetrightascension",
            new KeyValuePair<string,string>("TargetRightAscension", ((double)ra.TotalHours()).ToString())
        );
        Put<AlpacaMethodResponse>(
            $"{Connection.Server.Host}:{Connection.Server.Port}/telescope/{DeviceNumber}/targetdeclination",
            new KeyValuePair<string,string>("TargetDeclination", ((double)dec.TotalDegrees()).ToString())
        );
        Put<AlpacaMethodResponse>(
            $"{Connection.Server.Host}:{Connection.Server.Port}/telescope/{DeviceNumber}/slewtotargetasync"
        );
        // TODO tracking rate
        Put<AlpacaMethodResponse>(
            $"{Connection.Server.Host}:{Connection.Server.Port}/telescope/{DeviceNumber}/tracking",
            new KeyValuePair<string,string>("Tracking", true.ToString())
        );
    }
}

}