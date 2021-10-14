using System;
using System.Linq;

namespace Qkmaxware.Astro.Control.Controllers {

/// <summary>
/// Base class for wrappers that control INDI devices in particular ways
/// </summary>
public abstract class IndiDeviceController {

    private IndiDevice device;

    public IndiDeviceController(IndiDevice device) {
        this.device = device;
    }

    protected T GetProperty<T>(string prop) where T:IndiValue, new() {
        if (device.Properties.HasProperty(prop)) {
            var t = device.Properties[prop];
            if (t is T) {
                return (T)device.Properties[prop];
            } else {
                throw new System.ArgumentException($"Device property '{prop}' is not of type {typeof(T).Name}");
            }
        } else {
            throw new System.ArgumentException($"Device is missing required property '{prop}'");
        }
    }

    protected void SetProperty(string property, IndiValue vector) {
        this.device.ChangeRemoteProperty(property, vector);
    }

    public void RefreshProperties() {
        this.device.RefreshProperties();
    }

    public void Connect() {
        IndiVector<IndiSwitchValue> vector = GetProperty<IndiVector<IndiSwitchValue>>(IndiStandardProperties.Connection);
        var connect = vector.GetSwitch("CONNECT");
        if (connect != null && !connect.IsOn) {
            // Only connect if not already connected
            foreach (var toggle in vector) {
                toggle.Value = toggle == connect;
            }
            this.device.ChangeRemoteProperty(vector.Name, vector);
            this.device.RefreshProperties();
        }
    }

    public void Disconnect() {
        IndiVector<IndiSwitchValue> vector = GetProperty<IndiVector<IndiSwitchValue>>(IndiStandardProperties.Connection);
        var disconnect = vector.GetSwitch("CONNECT");
        if (disconnect != null && !disconnect.IsOn) {
            // Only disconnect if not already disconnected
            foreach (var toggle in vector) {
                toggle.Value = toggle == disconnect;
            }
            this.device.ChangeRemoteProperty(vector.Name, vector);
            this.device.RefreshProperties();
        }
    }

}

}