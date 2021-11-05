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
/// Message sent from client to device
/// </summary>
public abstract class IndiClientMessage {
    /// <summary>
    /// Convert message to an xml before sending
    /// </summary>
    /// <returns>xml string</returns>
    public abstract string EncodeXml();
}

/// <summary>
/// Message to request device properties from an INDI server
/// </summary>
public class IndiGetPropertiesMessage : IndiClientMessage {
    /// <summary>
    /// Name of the device (all if not provided)
    /// </summary>
    public string DeviceName;
    /// <summary>
    /// Name of the property (all if not provided)
    /// </summary>
    public string PropertyName;
    /// <summary>
    /// Get all properties for all devices
    /// </summary>
    public IndiGetPropertiesMessage () : this(null) {}
    /// <summary>
    /// Get all properties for the given device
    /// </summary>
    /// <param name="device">device name</param>
    public IndiGetPropertiesMessage (string device) {
        this.DeviceName = device;
        this.PropertyName = null;
    }
    /// <summary>
    /// Get a specific property from a device
    /// </summary>
    /// <param name="device">device name</param>
    /// <param name="property">property name</param>
    public IndiGetPropertiesMessage (string device, string property) {
        this.DeviceName = device;
        this.PropertyName = property;
    }
    public override string EncodeXml() {
        var el = new XElement("getProperties",
                new XAttribute("version", "1.7"));
        if (!string.IsNullOrEmpty(DeviceName)) {
            el.Add(new XAttribute("device", this.DeviceName));
        }
        if (!string.IsNullOrEmpty(PropertyName)) {
            el.Add(new XAttribute("name", this.PropertyName));
        }
        return el.ToString();
    }
}

/// <summary>
/// New property message to set a value on a remote INDI server
/// </summary>
public class IndiNewPropertyMessage : IndiClientMessage {
    /// <summary>
    /// Device name
    /// </summary>
    public string DeviceName;
    /// <summary>
    /// Property name
    /// </summary>
    public string PropertyName;
    /// <summary>
    /// New value of the property
    /// </summary>
    public IndiValue PropertyValue;
    /// <summary>
    /// Create a new message to update the remote value of a property for a given device
    /// </summary>
    /// <param name="device">device name</param>
    /// <param name="prop">property name</param>
    /// <param name="value">new value</param>
    public IndiNewPropertyMessage(string device, string prop, IndiValue value) {
        this.DeviceName = device;
        this.PropertyName = prop;
        this.PropertyValue = value;
    }
    public override string EncodeXml() {
        var el = PropertyValue.CreateNewElement();
        el.AddOrUpdateAttribute("device", this.DeviceName);
        el.AddOrUpdateAttribute("name", this.PropertyName);
        return el.ToString();
    }
}

/// <summary>
/// The state of blob sending 
/// </summary>
public enum IndiBlobState {
    /// <summary>
    /// Never receive BLOBs on this connection
    /// </summary>
    Never, 
    /// <summary>
    /// Only receive BLOBs on this connection, pauses changes to other properties
    /// </summary>
    Only, 
    /// <summary>
    /// Receive BLOBs on this connection as well as changes to other properties
    /// </summary>
    Also
}

/// <summary>
/// Message sent to control the state of sending or recieving BLOBs
/// </summary>
public class IndiEnableBlobMessage : IndiClientMessage {
    /// <summary>
    /// Device name
    /// </summary>
    public string DeviceName;
    /// <summary>
    /// Property name
    /// </summary>
    public string PropertyName;
    /// <summary>
    /// State of blob handling for this property
    /// </summary>
    public IndiBlobState State; 
    public override string EncodeXml() {
        var node = new XElement(
            "enableBLOB", 
            new XText(this.State.ToString())
        );
        if (this.DeviceName != null) {
            node.Add(new XAttribute("device", this.DeviceName));
        }
        if (this.PropertyName != null) {
            node.Add(new XAttribute("name", this.PropertyName));
        }
        return node.ToString();
    }
}

}