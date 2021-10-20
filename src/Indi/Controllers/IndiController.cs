using System;
using System.Linq;

namespace Qkmaxware.Astro.Control.Controllers {

/// <summary>
/// Base class for wrappers that control INDI devices in particular ways
/// </summary>
public abstract class IndiDeviceController {

    private IndiDevice device;

    /// <summary>
    /// Create a controller around this given device
    /// </summary>
    /// <param name="device">device</param>
    public IndiDeviceController(IndiDevice device) {
        this.device = device;
    }

    /// <summary>
    /// Get a value or throw an exception
    /// </summary>
    /// <param name="prop">propety to get</param>
    /// <typeparam name="T">property value's type to cast</typeparam>
    /// <returns>property if the property exists and is of the given type</returns>
    protected T GetProperty<T>(string prop) where T:IndiValue, new() {
        T value = default(T);
        if (device.Properties.TryGet<T>(prop, out value)) {
            return value;
        } else {
            throw new System.ArgumentException($"Device property '{prop}' is missing or not of type {typeof(T).Name}");
        }
    }

    /// <summary>
    /// Change the value of a property
    /// </summary>
    /// <param name="property">property to change</param>
    /// <param name="vector">value to change it to</param>
    protected void SetProperty(string property, IndiValue vector) {
        this.device.Properties.SetAsync(property, vector);
    }

    /// <summary>
    /// Refresh all properties
    /// </summary>
    public void RefreshProperties() {
        this.device.Properties.RefreshAsync();
    }

    /// <summary>
    /// Connect the underlying device if not connected
    /// </summary>
    public void Connect() {
        this.device.Connect();
    }

    /// <summary>
    /// Disconnect the underlying device
    /// </summary>
    public void Disconnect() {
        this.device.Disconnect();
    }

}

}