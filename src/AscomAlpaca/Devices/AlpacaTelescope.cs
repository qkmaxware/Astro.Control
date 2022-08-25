namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Interface for controlling telescopes via ASCOM Alpaca
/// </summary>
public class AlpacaTelescope : AlpacaDevice, ITelescope {
    internal AlpacaTelescope(AlpacaConnection conn, AlpacaConfiguredDevicesResponse.ConfiguredDevice rawDevice) : base(conn, rawDevice.DeviceName, rawDevice.DeviceType, rawDevice.DeviceNumber) {}
    public AlpacaTelescope(AlpacaConnection conn, string deviceName, string deviceType, int deviceNumber) : base(conn, deviceName, deviceType, deviceNumber) {}

}

}