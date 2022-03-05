using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Qkmaxware.Astro.Control.Controllers {

using IndiSwitchVector = IndiVector<IndiSwitchValue>;

/// <summary>
/// Controller abstraction for CCD devices
/// </summary>
public class IndiCameraController : IndiDeviceController {
    public IndiCameraController(IndiDevice device) : base(device) {}

    public void EnableCooling(bool isCooling) {
        var property = "CCD_COOLER";
        var vector = GetPropertyOrThrow<IndiSwitchVector>(property);
        vector.SwitchTo(isCooling ? "COOLER_ON" : "COOLER_OFF");
        SetProperty(property, vector);
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
            DeviceName = this.DeviceName,
            PropertyName = "CCD1",
            State = state
        };
        this.SendMessageToDevice(message);
    }

    public void ExposeSync(float seconds) {
        var property = "CCD_EXPOSURE";
        var vector = this.GetPropertyOrNew<IndiVector<IndiNumberValue>>(property, () => {
            var v = new IndiVector<IndiNumberValue>(property);
            v.Add(new IndiNumberValue { Name = "CCD_EXPOSURE_VALUE", Value = seconds });
            return v;
        });
        vector.GetItemWithName("CCD_EXPOSURE_VALUE").Value = seconds;

        SetProperty(property, vector);
        Thread.Sleep(TimeSpan.FromSeconds(seconds));
    }

    public CancellationTokenSource ExposeAsync(float seconds) {
        var cancel = new CancellationTokenSource();
        Task.Run(() => {
            var property = "CCD_EXPOSURE";
            var vector = this.GetPropertyOrNew<IndiVector<IndiNumberValue>>(property, () => {
                var v = new IndiVector<IndiNumberValue>(property);
                v.Add(new IndiNumberValue { Name = "CCD_EXPOSURE_VALUE", Value = seconds });
                return v;
            });
            vector.GetItemWithName("CCD_EXPOSURE_VALUE").Value = seconds;

            SetProperty(property, vector);
            var end = DateTime.Now + TimeSpan.FromSeconds(seconds);
            while(DateTime.Now < end) {
                if (cancel.IsCancellationRequested) {
                    SetProperty("CCD_ABORT_EXPOSURE", new IndiNumberValue { Name = "ABORT", Value = seconds });
                    break;
                }
            }
        });
        return cancel;
    }

}

}