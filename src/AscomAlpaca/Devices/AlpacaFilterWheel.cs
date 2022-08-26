using System.Collections.Generic;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Interface for controlling filter wheels via ASCOM Alpaca
/// </summary>
public class AlpacaFilterWheel : AlpacaDevice, IFilterWheel {
    internal AlpacaFilterWheel(AlpacaConnection conn, AlpacaConfiguredDevicesResponse.ConfiguredDevice rawDevice) : base(conn, rawDevice.DeviceName, rawDevice.DeviceType, rawDevice.DeviceNumber) {}
    public AlpacaFilterWheel(AlpacaConnection conn, string deviceName, string deviceType, int deviceNumber) : base(conn, deviceName, deviceType, deviceNumber) {}

    public int CurrentFilterIndex() {
        var res = Get<AlpacaValueResponse<int>>($"{Connection.Server.Host}:{Connection.Server.Port}/filterwheel/{DeviceNumber}/position");
        return res.Value;
    }

    public void ChangeFilterAsync(int index) {
        Put<AlpacaMethodResponse>($"{Connection.Server.Host}:{Connection.Server.Port}/filterwheel/{DeviceNumber}/position", new KeyValuePair<string, string>("Position", index.ToString()));
    }

    public List<string> ListFilterNames() {
        var res = Get<AlpacaValueResponse<string[]>>($"{Connection.Server.Host}:{Connection.Server.Port}/filterwheel/{DeviceNumber}/names");
        if (res.IsError || res.Value == null) {
            return new List<string>();
        }
        else {
            return new List<string>(res.Value);
        }
    }

    public void UpdateFilterNames(List<string> filters) {
        // Can't change filter names for Alpaca... too bad :)
    }
}

}