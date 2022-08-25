namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Interface for controlling cameras via ASCOM Alpaca
/// </summary>
public class AlpacaCamera : AlpacaDevice, ICamera {
    internal AlpacaCamera(AlpacaConnection conn, AlpacaConfiguredDevicesResponse.ConfiguredDevice rawDevice) : base(conn, rawDevice.DeviceName, rawDevice.DeviceType, rawDevice.DeviceNumber) {}
    public AlpacaCamera(AlpacaConnection conn, string deviceName, string deviceType, int deviceNumber) : base(conn, deviceName, deviceType, deviceNumber) {}

}

}