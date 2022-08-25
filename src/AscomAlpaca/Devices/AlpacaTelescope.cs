using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Interface for controlling telescopes via ASCOM Alpaca
/// </summary>
public class AlpacaTelescope : AlpacaDevice, ITelescope {
        
    internal AlpacaTelescope(AlpacaConnection conn, AlpacaConfiguredDevicesResponse.ConfiguredDevice rawDevice) : base(conn, rawDevice.DeviceName, rawDevice.DeviceType, rawDevice.DeviceNumber) {}
    public AlpacaTelescope(AlpacaConnection conn, string deviceName, string deviceType, int deviceNumber) : base(conn, deviceName, deviceType, deviceNumber) {}

    public Angle RightAscension => throw new System.NotImplementedException();

    public Angle Declination => throw new System.NotImplementedException();


    public void Rotate(float horizontal, float vertical)
    {
        throw new System.NotImplementedException();
    }

    public void Sync(Angle ra, Angle dec)
    {
        throw new System.NotImplementedException();
    }

    public void Goto(Angle ra, Angle dec)
    {
        throw new System.NotImplementedException();
    }

    public void Track(Angle ra, Angle dec, TrackingRate rate)
    {
        throw new System.NotImplementedException();
    }
}

}