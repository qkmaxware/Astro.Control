using System.Collections.Generic;
using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Devices {

public enum AlpacaShutterState {
    Open = 0,
    Closed = 1,
    Opening = 2,
    Closing = 3,
    Error = 4
}

/// <summary>
/// Interface for controlling astronomy domes via ASCOM Alpaca
/// </summary>
public class AlpacaDome : AlpacaDevice, IDome {
       
    internal AlpacaDome(AlpacaConnection conn, AlpacaConfiguredDevicesResponse.ConfiguredDevice rawDevice) : base(conn, rawDevice.DeviceName, rawDevice.DeviceType, rawDevice.DeviceNumber) {}
    public AlpacaDome(AlpacaConnection conn, string deviceName, string deviceType, int deviceNumber) : base(conn, deviceName, deviceType, deviceNumber) {}

    public bool IsShutterOpen {
        get {
            var res = Get<AlpacaValueResponse<AlpacaShutterState>>($"{Connection.Server.Host}:{Connection.Server.Port}/dome/{DeviceNumber}/shutterstatus");
            if (res.IsError || res.Value == AlpacaShutterState.Error)
                return false;
            
            return res.Value switch {
                AlpacaShutterState.Open => true,
                AlpacaShutterState.Opening => true,
                AlpacaShutterState.Closed => false,
                AlpacaShutterState.Closing => false,
                _ => false
            };
        }
    }

    public Angle PointingDirection {
        get {
            var res = Get<AlpacaValueResponse<double>>($"{Connection.Server.Host}:{Connection.Server.Port}/dome/{DeviceNumber}/azimuth");
            return Angle.Degrees(res.Value);
        }
    }

    public void OpenShutter() {
        Put<AlpacaMethodResponse>($"{Connection.Server.Host}:{Connection.Server.Port}/dome/{DeviceNumber}/openshutter");
    }

    public void CloseShutter() {
        Put<AlpacaMethodResponse>($"{Connection.Server.Host}:{Connection.Server.Port}/dome/{DeviceNumber}/closeshutter");
    }

    public void Goto(double rpm, Angle angle) {
        Put<AlpacaMethodResponse>(
            $"{Connection.Server.Host}:{Connection.Server.Port}/dome/{DeviceNumber}/slewtoazimuth",
            new KeyValuePair<string,string>("Azimuth", ((double)angle.TotalDegrees()).ToString())
        );

    }

    public void RotateCW(double rpm) {
        // Doesn't support direction based rotation
    }

    public void RotateCCW(double rpm) {
        // Doesn't support direction based rotation
    }

    public void Stop() {
        Put<AlpacaMethodResponse>($"{Connection.Server.Host}:{Connection.Server.Port}/dome/{DeviceNumber}/abortslew");
    }
}

}