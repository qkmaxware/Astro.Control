using System;
using System.IO;
using System.Threading.Tasks;
using Qkmaxware.Astro.Control.Devices;
using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Simulated telescope
/// </summary>
public class SimulatedTelescope : ITelescope {
    public string Name => ".Net Simulated Telescope Mount";

    public bool IsConnected => true;

    public void Connect() { }

    public void Disconnect() { }


    /// <summary>
    /// The current right ascension of the telescope
    /// </summary>
    /// <returns></returns>
    public Angle RightAscension => ra;
    /// <summary>
    /// The current declination of the telescope
    /// </summary>
    /// <returns></returns>
    public Angle Declination => dec;

    public bool IsSlewing => false;
    public bool IsTracking => false;

    private Angle ra = Angle.Zero;
    private Angle dec = Angle.Zero;

    public void Goto(Angle ra, Angle dec) {
        resolveMotion();
        this.ra = ra;
        this.dec = dec;
    }

    private DateTime? motionStartedAt;
    private double vRpm;
    private double hRpm;
    private void resolveMotion() {
        if (motionStartedAt != null) {
            var duration = (DateTime.Now - motionStartedAt.Value).TotalMinutes;
            ra = (ra + Angle.Degrees(360 * duration * hRpm)).Wrap();
            dec = (dec + Angle.Degrees(360 * duration * vRpm)).Wrap();
            motionStartedAt = null;
        }
    }

    public void Rotate(float horizontal, float vertical) {
        resolveMotion();
        vRpm = vertical;
        hRpm = horizontal;
        motionStartedAt = DateTime.Now;
    }

    public void Sync(Angle ra, Angle dec) {
        resolveMotion();
        this.ra = ra;
        this.dec = dec;
    }

    public void Track(Angle ra, Angle dec, TrackingRate rate) {
        resolveMotion();
        this.ra = ra;
        this.dec = dec;
    }
}

}