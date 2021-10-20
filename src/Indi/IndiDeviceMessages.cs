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

namespace Qkmaxware.Astro.Control {

/// <summary>
/// Messages from the server to the client
/// </summary>
public abstract class IndiServerMessage {
    /// <summary>
    /// Action to perform when the message is received
    /// </summary>
    /// <param name="connection">connection the message was received over</param>
    public abstract void Process(IndiConnection connection);
    /// <summary>
    /// Encode this message to XML
    /// </summary>
    /// <returns>XML string</returns>
    public abstract string EncodeXml();
}

/// <summary>
/// Set a property on the client with value from the server
/// </summary>
public class IndiSetPropertyMessage : IndiServerMessage {
    /// <summary>
    /// Device name
    /// </summary>
    public string DeviceName;
    /// <summary>
    /// Property name
    /// </summary>
    public string PropertyName;
    /// <summary>
    /// New property value
    /// </summary>
    public IndiValue PropertyValue;
    /// <summary>
    /// Set the given property on all devices
    /// </summary>
    /// <param name="property">property name</param>
    /// <param name="value">new value</param>
    public IndiSetPropertyMessage(string property, IndiValue value) : this(null, property, value) {}
    /// <summary>
    /// Set the given property on the specific device
    /// </summary>
    /// <param name="device">device name</param>
    /// <param name="property">property name</param>
    /// <param name="value">new value</param>
    public IndiSetPropertyMessage(string device, string property, IndiValue value) {
        this.DeviceName = device;
        this.PropertyName = property;
        this.PropertyValue = value;
    }
    public override void Process(IndiConnection connection) {
        /*
        if receive <setXXX> from Device
            change record of value and/or state for the specified Property
        */
        if (string.IsNullOrEmpty(DeviceName)) {
            // Set property for all devices
            foreach (var device in connection.Devices) {
                updateProperty(device.Value, PropertyName, PropertyValue);
            }
        } else {
            var device = connection.GetDeviceByName(DeviceName);
            if (device != null) {
                // Set property on specific device
                updateProperty(device, PropertyName, PropertyValue);
            }
        }
    }

    private void updateProperty(IndiDevice device, string property, IndiValue value) {
        // TODO, fix this to not overwrite values, but just to update the actual "stored" value
        if (device.Properties.Exists(PropertyName)) {
            var oldProp = device.Properties[property];
            if (oldProp is UpdatableIndiValue updatableProp) {
                if (!updatableProp.TryUpdateValue(value)) {
                    device.Properties[property] = PropertyValue;
                }
            } else {
                device.Properties[property] = PropertyValue;
            }
        } else {
            // Do nothing, SET does not make new properties, just updates existing ones
        }
    }

    public override string EncodeXml() {
        return PropertyValue.CreateSetElement().ToString();
    }
}

/// <summary>
/// Define the given property on the client from a definition on the server
/// </summary>
public class IndiDefinePropertyMessage : IndiServerMessage {
    /// <summary>
    /// Device name
    /// </summary>
    public string DeviceName;
    /// <summary>
    /// Property name
    /// </summary>
    public string PropertyName;
    /// <summary>
    /// New property value
    /// </summary>
    public IndiValue PropertyValue;
    /// <summary>
    /// Define a new property for the given device
    /// </summary>
    /// <param name="device">device name</param>
    /// <param name="prop">property name</param>
    /// <param name="value">default value</param>
    public IndiDefinePropertyMessage(string device, string prop, IndiValue value) {
        this.DeviceName = device;
        this.PropertyName = prop;
        this.PropertyValue = value;
    }
    public override void Process(IndiConnection connection) {
        /*
        if receive <defProperty> from Device
            if first time to see this device=
                create new Device record
            if first time to see this device+name combination
                create new Property record within given Device
        */
        if (!string.IsNullOrEmpty(this.DeviceName) && !string.IsNullOrEmpty(this.PropertyName) && this.PropertyValue != null) {
            var device = connection.GetOrCreateDevice(this.DeviceName);
            device.Properties[this.PropertyName] = this.PropertyValue;
        }
    }

    public override string EncodeXml() {
        return PropertyValue.CreateDefinitionElement().ToString();
    }
}

/// <summary>
/// Delete a given property on the client that was removed on the server
/// </summary>
public class IndiDeletePropertyMessage : IndiServerMessage {
    /// <summary>
    /// Device name
    /// </summary>
    public string DeviceName;
    /// <summary>
    /// Property name
    /// </summary>
    public string PropertyName;
    /// <summary>
    /// Check if all properties are to be deleted
    /// </summary>
    /// <returns>true if this message is designed to delete all properties</returns>
    public bool DeleteAllProperties => string.IsNullOrEmpty(PropertyName);
    /// <summary>
    /// Timestamp
    /// </summary>
    public string Timestamp;
    /// <summary>
    /// Notice attached to deletion
    /// </summary>
    public string Message;
    /// <summary>
    /// Delete the given property for the device
    /// </summary>
    /// <param name="device">device name to delete from</param>
    /// <param name="prop">property name (or empty for all properties)</param>
    /// <param name="timestamp">timestamp</param>
    /// <param name="msg">message</param>
    public IndiDeletePropertyMessage(string device, string prop, string timestamp, string msg) {
        this.DeviceName = device;
        this.PropertyName = prop;
        this.Timestamp = timestamp;
        this.Message = msg;
    }

    public override void Process(IndiConnection connection) {
        /*
        if receive <delProperty> from Device
            if includes device= attribute
                if includes name= attribute
                    delete record for just the given Device+name
                else
                    delete all records the given Device
            else
                delete all records for all devices 
        */
        if (string.IsNullOrEmpty(DeviceName)) {
            // Delete property for all devices
            if (DeleteAllProperties) {
                foreach (var device in connection.Devices) {
                    device.Value.Properties.Clear();
                }
            } else {
                foreach (var device in connection.Devices) {
                    device.Value.Properties.Delete(this.PropertyName);
                }
            }
        } else {
            var device = connection.GetDeviceByName(DeviceName);
            if (device != null) {
                // Delete property on specific device
                if (DeleteAllProperties) {
                    device.Properties.Clear();
                } else {
                    device.Properties.Delete(this.PropertyName);
                }
            }
        }
    }

    public override string EncodeXml() {
        var el = new XElement("delProperty");
        if (!string.IsNullOrEmpty(DeviceName)) 
            el.Add(new XAttribute("device", DeviceName));
        if (!string.IsNullOrEmpty(PropertyName)) 
            el.Add(new XAttribute("name", PropertyName));
        if (!string.IsNullOrEmpty(Timestamp)) 
            el.Add(new XAttribute("timestamp", Timestamp));
        if (!string.IsNullOrEmpty(Message)) 
            el.Add(new XAttribute("message", Message));
        return el.ToString();
    }
}

/// <summary>
/// Generic notification message from the server to the client
/// </summary>
public class IndiNotificationMessage : IndiServerMessage {
    /// <summary>
    /// Device name notification came from
    /// </summary>
    public string DeviceName;
    /// <summary>
    /// Timestamp
    /// </summary>
    public string Timestamp;
    /// <summary>
    /// Notification message
    /// </summary>
    public string Message;
    /// <summary>
    /// Create a new notification for the given device
    /// </summary>
    /// <param name="device">device name</param>
    /// <param name="timestamp">timestamp</param>
    /// <param name="message">message</param>
    public IndiNotificationMessage(string device, string timestamp, string message) {
        this.DeviceName = device;
        this.Timestamp = timestamp;
        this.Message = message;
    }
    public override void Process(IndiConnection connection) {}

    public override string EncodeXml() {
        var el = new XElement("message");
        if (!string.IsNullOrEmpty(this.DeviceName))
            el.Add(new XAttribute("device", this.DeviceName));
        if (!string.IsNullOrEmpty(this.Timestamp))
            el.Add(new XAttribute("timestamp", this.Timestamp));
        if (!string.IsNullOrEmpty(this.Message))
            el.Add(new XAttribute("message", this.Message));
        return el.ToString();
    }
}

}