using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Dynamic;
using System.Collections.Generic;
using System.Collections;
using Qkmaxware.Astro.Control.Devices;

namespace Qkmaxware.Astro.Control {

/// <summary>
/// Device properties container
/// </summary>
public class IndiPropertiesContainer : IEnumerable<KeyValuePair<string, IndiValue>>{
    private ConcurrentDictionary<string, IndiValue> properties = new ConcurrentDictionary<string, IndiValue>();

    private IndiDevice owner;

    public IndiPropertiesContainer(IndiDevice owner) {
        this.owner = owner;
    }

    /// <summary>
    /// Check if the given property exists
    /// </summary>
    /// <param name="name">property name</param>
    /// <returns>true if property exists</returns>
    public bool Exists(string name) {
        return properties.ContainsKey(name);
    }

    /// <summary>
    /// Request that all properties be refreshed asynchronously
    /// </summary>
    public void RefreshAsync() {
        this.owner.Connection.Send(new IndiGetPropertiesMessage(this.owner.Name));
    }

    /// <summary>
    /// Request that a specific property be refreshed asynchronously
    /// </summary>
    /// <param name="property">name of the property to refresh</param>
    public void RefreshAsync(string property) {
        this.owner.Connection.Send(new IndiGetPropertiesMessage(this.owner.Name));
    }

    /// <summary>
    /// Set the value of a property, sending the change to the INDI server asynconously. It may take a few moments to be updated locally.
    /// </summary>
    /// <param name="property">name of the property to change</param>
    /// <param name="value">value to change the property to</param>
    public void SetAsync(string property, IndiValue value) {
        // Create NewProperty client message
        // Populate device name, property name, and value
        var message = new IndiNewPropertyMessage(this.owner.Name, property, value);
        // Update local copy
        this.properties[property] = value; 
        // Encode and send
        this.owner.Connection.Send(message);
    }

    /// <summary>
    /// Get the cached local value of the given property
    /// </summary>
    /// <param name="property">property whose value to get</param>
    /// <returns>value of the given property if it exists</returns>
    public IndiValue Get(string property) {
        return this.properties[property];
    }

    /// <summary>
    /// Get the cached local value of a given property only if it exists and if of the given type
    /// </summary>
    /// <param name="property">property whose value to get</param>
    /// <param name="value">value of the given property if it exists</param>
    /// <typeparam name="T">type assertion for the type of the property's value</typeparam>
    /// <returns>true if property exists and is of the given type</returns>
    public bool TryGet<T> (string property, out T value) where T:IndiValue {
        value = default(T);
        if (this.Exists(property)) {
            var t = this.Get(property);
            if (t is T) {
                value = (T)t;
                return true;
            } else {
                return false;
            }
        } else {
            return false;
        }
    }
 
    /// <summary>
    /// Clear all properties
    /// </summary>
    public void Clear() {
        this.properties.Clear();
    }   

    /// <summary>
    /// Delete property
    /// </summary>
    /// <param name="property">property name</param>
    public void Delete(string property) {
        IndiValue old;
        this.properties.TryRemove(property, out old);
    }

    /// <summary>
    /// Enumerate over properties
    /// </summary>
    /// <returns>property enumerator</returns>
    public IEnumerator<KeyValuePair<string, IndiValue>> GetEnumerator() {
        return this.properties.GetEnumerator();
    }

    /// <summary>
    /// Enumerate over properties
    /// </summary>
    /// <returns>property enumerator</returns>
    IEnumerator IEnumerable.GetEnumerator() {
        return this.GetEnumerator();
    }

    /// <summary>
    /// Get or set a property by name
    /// </summary>
    /// <value>property value</value>
    public IndiValue this[string key] {
        get => Get(key);
        internal set {
            properties[key] = value;
        }
    }
}

/// <summary>
/// Abstraction for a device accessible over an INDI connection
/// </summary>
public class IndiDevice : IDevice {
    /// <summary>
    /// Name of the device
    /// </summary>
    /// <value>name</value>
    public string Name {get; private set;}
    /// <summary>
    /// Properties associated with this device
    /// </summary>
    /// <returns>property container</returns>
    public IndiDevicePropertiesWrapper Properties {get; private set;}

    /// <summary>
    /// The connection this device can be accessed over
    /// </summary>
    public IndiConnection Connection {get; private set;}

    /// <summary>
    /// Create a new device on the given connection with the given name
    /// </summary>
    /// <param name="name">device name</param>
    /// <param name="connection">connection the device is accessible over</param>
    public IndiDevice(string name, IndiConnection connection) {
        this.Name = name;
        this.Connection = connection;
        this.Properties = new IndiDevicePropertiesWrapper(connection, name);
    }

    /// <summary>
    /// Test if this device is connected
    /// </summary>
    /// <returns>true if the device is connected</returns>
    public bool IsConnected {
        get {
            IndiVector<IndiSwitchValue> vector;
            if (this.Properties.TryGetValue<IndiVector<IndiSwitchValue>>(IndiStandardProperties.Connection, out vector)) {
                return vector.GetSwitch("CONNECT")?.IsOn ?? false;
            } else {
                return false;
            }
        }
    }

    /// <summary>
    /// Connect the device, this may trigger more properties to be pulled from the server
    /// </summary>
    public void Connect() {
        IndiVector<IndiSwitchValue> vector;
        if (this.Properties.TryGetValue<IndiVector<IndiSwitchValue>>(IndiStandardProperties.Connection, out vector)) {
            var connect = vector.GetSwitch("CONNECT");
            if (connect != null && !connect.IsOn) {
                // Only connect if not already connected
                foreach (var toggle in vector) {
                    toggle.Value = toggle == connect;
                }
                this.Properties.SetRemoteValue(vector.Name, vector);
                this.Properties.RefreshAsync();
            }
        }
    }

    /// <summary>
    /// Disconnect the device
    /// </summary>
    public void Disconnect() {
        IndiVector<IndiSwitchValue> vector;
        if (this.Properties.TryGetValue<IndiVector<IndiSwitchValue>>(IndiStandardProperties.Connection, out vector)) {
            var disconnect = vector.GetSwitch("CONNECT");
            if (disconnect != null && !disconnect.IsOn) {
                // Only disconnect if not already disconnected
                foreach (var toggle in vector) {
                    toggle.Value = toggle == disconnect;
                }
                this.Properties.SetRemoteValue(vector.Name, vector);
                this.Properties.RefreshAsync();
            }
        }
    }

    #region Standard Property Manipulators
    /// <summary>
    /// The device's port
    /// </summary>
    /// <returns>port name if the port is found, null otherwise</returns>
    public string Port {
        get {
            IndiVector<IndiTextValue> vector;
            if (this.Properties.TryGetValue<IndiVector<IndiTextValue>>("DEVICE_PORT", out vector)) {
                return vector.GetItemWithName("PORT")?.Value;
            } else {
                return null;
            }
        } set {
            if (string.IsNullOrEmpty(value))
                return;

            IndiVector<IndiTextValue> vector;
            if (this.Properties.TryGetValue<IndiVector<IndiTextValue>>("DEVICE_PORT", out vector)) {
                var portItem = vector.GetItemWithName("PORT");
                if (portItem != null)
                    portItem.Value = value;

                this.Properties.SetRemoteValue(vector.Name, vector);
            } 
        }
    }

    /// <summary>
    /// Device baud rate if applicable
    /// </summary>
    /// <value>baud rate if it exists, null otherwise</value>
    public int? BaudRate {
        get {
            IndiVector<IndiSwitchValue> vector;
            if (this.Properties.TryGetValue<IndiVector<IndiSwitchValue>>("DEVICE_BAUD_RATE", out vector)) {
                var enabled = vector.GetFirstEnabledSwitch();
                if (enabled == null) {
                    return null;
                } else {
                    int rate;
                    if (int.TryParse(enabled.Name, out rate)) {
                        return rate;
                    } else {
                        return null;
                    }
                }
            } else {
                return null;
            }
        }
        set {
            if (!value.HasValue)
                return;

            var strValue = value.Value.ToString();
            IndiVector<IndiSwitchValue> vector;
            if (this.Properties.TryGetValue<IndiVector<IndiSwitchValue>>("DEVICE_BAUD_RATE", out vector)) {
                vector.SwitchTo(strValue);
                this.Properties.SetRemoteValue(vector.Name, vector);
            }
        }
    }
    /// <summary>
    /// List of all valid baud rates
    /// </summary>
    /// <value>collection of valid baud rates or empty if there are none</value>
    public IEnumerable<int> AvailableBaudRates {
        get {
            IndiVector<IndiSwitchValue> vector;
            if (this.Properties.TryGetValue<IndiVector<IndiSwitchValue>>("DEVICE_BAUD_RATE", out vector)) {
                foreach (var swch in vector) {
                    int rate;
                    if (int.TryParse(swch.Name, out rate)) {
                        yield return rate;
                    } 
                }
            }
        }
    }

    /// <summary>
    /// Set the device's clock
    /// </summary>
    public void SetClock(DateTime time) {
        IndiVector<IndiTextValue> vector;
        if (this.Properties.TryGetValue<IndiVector<IndiTextValue>>(IndiStandardProperties.Connection, out vector)) {
            if (vector.IsWritable) {
                vector.GetItemWithName("UTC").Value = time.ToUniversalTime().ToString("o");
                vector.GetItemWithName("OFFSET").Value = TimeZoneInfo.Local.GetUtcOffset(time).TotalHours.ToString();
                
                this.Properties.SetRemoteValue(vector.Name, vector);
            }
        }
    }

    /// <summary>
    /// Set the device's geographic location
    /// </summary>
    /// <param name="lat">latitude in degrees +N</param>
    /// <param name="lon">longitude in degrees +E</param>
    /// <param name="alt">altitude in meters</param>
    public void SetGeolocation(double lat, double lon, double alt = 0) {
        lat = Math.Min(90, Math.Max(-90, lat)); //clamp between -90 and 90
        lon = (lon - (Math.Floor( lon / 360 ) * 360 )) + 0; // normalize between 0 and 360
        IndiVector<IndiNumberValue> vector;
        if (this.Properties.TryGetValue<IndiVector<IndiNumberValue>>(IndiStandardProperties.Connection, out vector)) {
            if (vector.IsWritable) {
                vector.GetItemWithName("LAT").Value = lat;
                vector.GetItemWithName("LONG").Value = lon;
                vector.GetItemWithName("ELEV").Value = Math.Max(0, alt);

                this.Properties.SetRemoteValue(vector.Name, vector);
            }
        }
    }

    #endregion

    // override object.Equals
    public override bool Equals(object obj) {
        if (obj == null) {
            return false;
        }
        if (obj is IndiDevice other) {
            return other.Connection == this.Connection && other.Name == this.Name;
        }
        else {
            return base.Equals(obj);
        }
    }
    
    // override object.GetHashCode
    public override int GetHashCode() {
        return HashCode.Combine(this.Name, this.Connection);
    }
}

}