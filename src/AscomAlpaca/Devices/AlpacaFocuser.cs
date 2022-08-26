using System.Collections.Generic;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Interface for controlling focusers via ASCOM Alpaca
/// </summary>
public class AlpacaFocuser : AlpacaDevice, IFocuser {

    internal AlpacaFocuser(AlpacaConnection conn, AlpacaConfiguredDevicesResponse.ConfiguredDevice rawDevice) : base(conn, rawDevice.DeviceName, rawDevice.DeviceType, rawDevice.DeviceNumber) {}
    public AlpacaFocuser(AlpacaConnection conn, string deviceName, string deviceType, int deviceNumber) : base(conn, deviceName, deviceType, deviceNumber) {}

     public bool IsMotionReversed => false;

    public int FocusPosition => throw new System.NotImplementedException();

    public void ReverseMotion() {
        // Not supported
    }

    public void ResetMotion() {
        // Not supported because reverse is not supported
    }

    public void StartFocusing(int velocity) {
        // Not supported
    }

    public void StopFocusing() {
        Put<AlpacaMethodResponse>($"{Connection.Server.Host}:{Connection.Server.Port}/focuser/{DeviceNumber}/halt");
    }

    public int GetMaximumFocusPosition() {
        return Get<AlpacaValueResponse<int>>($"{Connection.Server.Host}:{Connection.Server.Port}/focuser/{DeviceNumber}/maxincrement").Value;
    }

    public int GetMinimumFocusPosition() {
        return 0;
    }

    public void GotoFocusPosition(int speed, int position) {
        Put<AlpacaMethodResponse>(
            $"{Connection.Server.Host}:{Connection.Server.Port}/focuser/{DeviceNumber}/move",
            new KeyValuePair<string,string>("Position", position.ToString())
        );
    }
}

}