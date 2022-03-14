using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Qkmaxware.Measurement;

namespace Qkmaxware.Astro.Control.Devices {

using IndiSwitchVector = IndiVector<IndiSwitchValue>;

/// <summary>
/// Controller abstraction for CCD devices
/// </summary>
public class IndiCameraController : IndiDeviceController, ICamera {
    public IndiCameraController(IndiDevice device) : base(device) {}

    private static readonly string ccd_cooling_property = "CCD_COOLER";
    /// <summary>
    /// Test if the camera's cooling is enabled
    /// </summary>
    /// <value>true if cooling is enabled</value>
    public bool IsCoolingEnabled {
        get {
            var vector = GetPropertyOrThrow<IndiSwitchVector>(ccd_cooling_property);
            var on = vector.GetSwitch("COOLER_ON");
            if (on != null && on.IsOn) {
                return true;
            } else {
                return false;
            }
        }
    }
    private void enableCooling(bool isCooling) {
        var vector = GetPropertyOrThrow<IndiSwitchVector>(ccd_cooling_property);
        vector.SwitchTo(isCooling ? "COOLER_ON" : "COOLER_OFF");
        SetProperty(ccd_cooling_property, vector);
    }
    /// <summary>
    /// Enable camera sensor cooling
    /// </summary>
    public void EnableCooling() {
        enableCooling(true);
    }
    /// <summary>
    /// Disable camera sensor cooling
    /// </summary>
    public void DisableCooling() {
        enableCooling(false);
    }

    public void UseRAW() {
        var property = "CCD_COMPRESSION";
        var vector = GetPropertyOrThrow<IndiSwitchVector>(property);
        vector.SwitchTo("CCD_RAW");
        SetProperty(property, vector);
    }
    
    public void UseCompression() {
        var property = "CCD_COMPRESSION";
        var vector = GetPropertyOrThrow<IndiSwitchVector>(property);
        vector.SwitchTo("CCD_COMPRESS");
        SetProperty(property, vector);
    }

    public void EnableReceivingImageBlobs(IndiBlobState state = IndiBlobState.Also) {
        var message = new IndiEnableBlobMessage {
            DeviceName = this.Name,
            PropertyName = "CCD1",
            State = state
        };
        this.SendMessageToDevice(message);
    }

    public CancellationTokenSource Expose(Duration timespan) {
        double seconds = (double)timespan.TotalSeconds();

        // Enable receiving the image
        EnableReceivingImageBlobs(IndiBlobState.Also);

        var cancel = new CancellationTokenSource();
        Task.Run(() => {
            // Take the image
            var property = "CCD_EXPOSURE";
            var vector = this.GetPropertyOrNew<IndiVector<IndiNumberValue>>(property, () => {
                var v = new IndiVector<IndiNumberValue>(property);
                v.Add(new IndiNumberValue { Name = "CCD_EXPOSURE_VALUE", Value = seconds });
                return v;
            });
            vector.GetItemWithName("CCD_EXPOSURE_VALUE").Value = seconds;

            // Wait for the image to be done (or if it is cancelled)
            SetProperty(property, vector);
            var end = DateTime.Now + TimeSpan.FromSeconds(seconds);
            while(DateTime.Now < end) {
                if (cancel.IsCancellationRequested) {
                    SetProperty("CCD_ABORT_EXPOSURE", new IndiNumberValue { Name = "ABORT", Value = seconds });
                    break;
                }
            }

            // Disable receiving of images
            EnableReceivingImageBlobs(IndiBlobState.Never);
        });
        return cancel;
    }

}

}