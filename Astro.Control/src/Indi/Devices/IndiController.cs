using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Qkmaxware.Astro.Control.IndiConnectionEventDispatcher;

namespace Qkmaxware.Astro.Control.Devices {

/// <summary>
/// Base class for wrappers that control INDI devices in particular ways
/// </summary>
public abstract class IndiDeviceController : IDevice, IHasPortDetails, IHasCommunicationsDetails {

    private IndiConnection connection {get; set;}
    private IndiDevice device {get;set;}

    /// <summary>
    /// Name of the controlled INDI device
    /// </summary>
    public string Name {get; private set;}
    /// <summary>
    /// Port used by the INDI device
    /// </summary>
    public string Port {
        get => device.Port;
        set => device.Port = value;
    }
    /// <summary>
    /// Communication baud rate
    /// </summary>
    public int? BaudRate {
        get => device.BaudRate;
        set => device.BaudRate = value;   
    }
    /// <summary>
    /// Enumerate over valid baud rates for the given device
    /// </summary>
    /// <returns>enumerable of baud rates</returns>
    public IEnumerable<int> EnumerateBaudRates() => device.AvailableBaudRates;

    /// <summary>
    /// Create a controller around this given device
    /// </summary>
    /// <param name="device">device</param>
    public IndiDeviceController(IndiDevice device) {
        this.Name = device.Name;
        this.connection = device.Connection;
        this.device = device;
    }

    protected Task AwaitPropertyChange(string property) {
        return Task.Run(async () => {
            // Configure delay task
            bool spin = true;
            SetPropertyListener listener = (device, changed, from, to) => {
                if (this.device == device && property == changed) {
                    // Property on the given device is changed, we don't care what it was changed to. 
                    spin = false;
                }
            };
            this.connection.Events.OnPropertyChanged += listener;
            
            // Spin wait
            while (spin) {
                await Task.Delay(100); // Wait 100ms between checks
            }

            // Cleanup
            this.connection.Events.OnPropertyChanged -= listener;
        });
    }

    /// <summary>
    /// Test if the device has the given property. Useful un verifying if the device is compatible with the controller.
    /// </summary>
    /// <param name="name">name of the property</param>
    /// <returns>true if the device has a property with the given name</returns>
    protected bool HasProperty(string name) {
        var device = this.device;
        return device != null && device.Properties != null && device.Properties.Exists(name);
    }

    /// <summary>
    /// Get a property value or throw an exception
    /// </summary>
    /// <param name="prop">propety to get</param>
    /// <typeparam name="T">property value's type to cast</typeparam>
    /// <returns>property if the property exists and is of the given type</returns>
    protected T GetPropertyOrThrow<T>(string prop) where T:IndiValue {
        T value = default(T);
        if (device.Properties.TryGetValue<T>(prop, out value)) {
            return value;
        } else {
            throw new System.ArgumentException($"Device property '{prop}' is missing or not of type {typeof(T).Name}");
        }
    }

    /// <summary>
    /// Get a property value or return the default value
    /// </summary>
    /// <param name="prop">propety to get</param>
    /// <typeparam name="T">property value's type to cast</typeparam>
    /// <returns>property if the property exists and is of the given type</returns>
    protected T GetPropertyOrDefault<T>(string prop) where T:IndiValue {
        T value = default(T);
        if (device.Properties.TryGetValue<T>(prop, out value)) {
            return value;
        } else {
            return default(T);
        }
    }

    /// <summary>
    /// Get a property value or create one from a template
    /// </summary>
    /// <param name="prop">propety to get</param>
    /// <param name="factory">factory function to use when creating a new value</param>
    /// <typeparam name="T">property value's type to cast</typeparam>
    /// <returns>property if the property exists and is of the given type or a new value created from the factory</returns>
    protected T GetPropertyOrNew<T>(string prop, Func<T> factory) where T:IndiValue {
        T value = default(T);
        if (device.Properties.TryGetValue<T>(prop, out value)) {
            return value;
        } else {
            var created = factory();
            created.Name = prop;
            return created;
        }
    }

    /// <summary>
    /// Change the value of a property
    /// </summary>
    /// <param name="property">property to change</param>
    /// <param name="vector">value to change it to</param>
    protected void SetProperty(string property, IndiValue vector) {
        if (vector != null && !string.IsNullOrEmpty(property))
            this.device.Properties.SetRemoteValue(property, vector);
    }
    
    /// <summary>
    /// Change the value of a vector property, property name interpreted from the vector's 'Name' property
    /// </summary>
    /// <param name="vector">value to change it to</param>
    protected void SetProperty<T>(IndiVector<T> vector) where T:IndiValue {
        this.SetProperty(vector.Name, vector);
    }

    /// <summary>
    /// Send a generic message to this INDI device
    /// </summary>
    /// <param name="message">device message</param>
    protected void SendMessageToDevice(IndiClientMessage message) {
        this.device.Connection.Send(message);
    }

    /// <summary>
    /// Refresh all properties
    /// </summary>
    public void RefreshProperties() {
        this.device.Properties.RefreshAsync();
    }

    /// <summary>
    /// Check if the underlying device is connected or not
    /// </summary>
    public bool IsConnected => this.device.IsConnected;

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

    /// <summary>
    /// Fetch a list of all active devices visible to this device
    /// </summary>
    /// <returns>dictionary of device kind, device name pairs</returns>
    public Dictionary<string, string> GetSnoopedDevices() {
        var vec = this.GetPropertyOrDefault<IndiVector<IndiTextValue>>("ACTIVE_DEVICES");
        if (vec != null) {
            return vec.ToDictionary(
                (text) => text.Name,
                (text) => text.Value
            );
        } else {
            return new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Allow this device to snoop on another
    /// </summary>
    /// <param name="deviceKind">kind of device</param>
    /// <param name="deviceName">name of device to snoop on</param>
    public void SnoopDevice(string deviceKind, string deviceName) {
        var vec = this.GetPropertyOrThrow<IndiVector<IndiTextValue>>("ACTIVE_DEVICES");
        var item = vec.GetItemWithName(deviceKind);
        if (item == null) {
            item = new IndiTextValue();
            item.Name = deviceKind; item.Label = deviceKind;
            vec.Add(item);
        }
        item.Value = deviceName;
        SetProperty(vec);
    }

    /// <summary>
    /// Set the device's time to the current time on the client system
    /// </summary>
    public void SetTimeFromClient() {
        this.device.SetClock(DateTime.Now);
    }

    /// <summary>
    /// Set the device's geographic location
    /// </summary>
    /// <param name="lat">latitude in degrees +N</param>
    /// <param name="lon">longitude in degrees +E</param>
    /// <param name="alt">altitude in meters</param>
    public void SetGeolocation(double lat, double lon, double alt = 0) {
        this.device.SetGeolocation(lat, lon, alt);
    }

    /// <summary>
    /// Explicitly downcast a controller to a basic device
    /// </summary>
    /// <param name="ctrl">controller to downcast</param>
    public static explicit operator IndiDevice(IndiDeviceController ctrl) {
        return ctrl.device;
    }

}

}