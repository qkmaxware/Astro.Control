using System;
using System.IO;
using System.Threading.Tasks;
using Qkmaxware.Astro.Control.Devices;
using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Simulated observation dome
/// </summary>
public class SimulatedDome : IDome {
    public bool IsShutterOpen {get; private set;}

    public string Name => ".Net Simulated Dome";

    public bool IsConnected => true;

    public void CloseShutter() {
        IsShutterOpen = false;
    }

    public void Connect(){}

    public void Disconnect(){}

    private Angle direction = Angle.Zero;

    public Angle PointingDirection => direction;

    public void Goto(double rpm, Angle angle) {
        direction = angle;
    }

    public void OpenShutter() {
        IsShutterOpen = true;
    }

    private double rpm;
    private DateTime? startedMovingAt;

    private void resolveMotion() {
        if (startedMovingAt.HasValue) {
            var duration = (DateTime.Now - startedMovingAt.Value).TotalMinutes;
            var delta = Angle.Degrees(360 * duration * rpm);
            this.direction = (this.direction + delta).Wrap();
            startedMovingAt = null;
        }
    }

    public void RotateCCW(double rpm) {
        resolveMotion();
        this.rpm = rpm;
        startedMovingAt = DateTime.Now;
    }

    public void RotateCW(double rpm) {
        resolveMotion();
        this.rpm = -rpm;
        startedMovingAt = DateTime.Now;
    }

    public void Stop() {
        resolveMotion();
        this.rpm = 0;
        startedMovingAt = null;
    }
}

}