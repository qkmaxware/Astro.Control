namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Interface for controlling focusers via ASCOM Alpaca
/// </summary>
public class AlpacaFocuser : AlpacaDevice, IFocuser {
    internal AlpacaFocuser(AlpacaConnection conn, AlpacaConfiguredDevicesResponse.ConfiguredDevice rawDevice) : base(conn, rawDevice.DeviceName, rawDevice.DeviceType, rawDevice.DeviceNumber) {}
    public AlpacaFocuser(AlpacaConnection conn, string deviceName, string deviceType, int deviceNumber) : base(conn, deviceName, deviceType, deviceNumber) {}

}

}