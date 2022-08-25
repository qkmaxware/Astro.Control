namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Interface for controlling astronomy domes via ASCOM Alpaca
/// </summary>
public class AlpacaDome : AlpacaDevice, IDome {
    internal AlpacaDome(AlpacaConnection conn, AlpacaConfiguredDevicesResponse.ConfiguredDevice rawDevice) : base(conn, rawDevice.DeviceName, rawDevice.DeviceType, rawDevice.DeviceNumber) {}
    public AlpacaDome(AlpacaConnection conn, string deviceName, string deviceType, int deviceNumber) : base(conn, deviceName, deviceType, deviceNumber) {}

}

}