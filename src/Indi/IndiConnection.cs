using System;
using System.Linq;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Dynamic;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

namespace Qkmaxware.Astro.Control {

/// <summary>
/// Connection from client machine to INDI server
/// </summary>
public partial class IndiConnection : BaseTcpConnection {
    /// <summary>
    /// Flag to indicate if disconnected devices should be connected automatically when discovered
    /// </summary>
    /// <value>true to auto connect devices</value>
    public bool AutoConnectDevices {get; set;}

    /// <summary>
    /// Devices acquired from this connection
    /// </summary>
    /// <typeparam name="string">device name</typeparam>
    /// <typeparam name="IndiDevice">device</typeparam>
    /// <returns>all devices</returns>
    public IndiDeviceCollection Devices {get; private set;}

    /// <summary>
    /// Event dispatcher which can have its events subscribed to by listeners
    /// </summary>
    /// <value>event dispatcher</value>
    public IndiConnectionEventDispatcher Events {get; private set;}

    internal IndiConnection (IndiServer server, IndiConnectionEventDispatcher eventDispatcher = null) : base(server) {
        var builder = new UriBuilder();
        this.Events = eventDispatcher ?? new IndiConnectionEventDispatcher();
        this.Devices = new IndiDeviceCollection(this);
    }

    ~IndiConnection() {
        this.Disconnect();
    }

    /// <summary>
    /// Get a device with the given name, or create it if it doesn't exist
    /// </summary>
    /// <param name="name">name of device</param>
    /// <returns>existing or new device</returns>
    internal IndiDevice GetOrCreateDevice(string name) {
        int b4 = Devices.Count;
        this.Devices.ReserveDeviceNameIfNotExists(name);
        var device = new IndiDevice(name, this);
        if (Devices.Count > b4) {
            this.Events.NotifyDeviceFound(device);
        }
        return device;
    }

    /// <summary>
    /// Query all properties for all devices
    /// </summary>
    public void QueryProperties() {
        this.Send(new IndiGetPropertiesMessage());
    }

    protected override void ConnectionEstablished() {
        // Send a query properties message once the connection is established
        this.QueryProperties(); 

        // Trigger a notification
        this.Events.NotifyServerConnected();
    }

    protected override void ConnectionClosed() {
        // Trigger a notification
        this.Events.NotifyServerDisconnected();
    }

    /// <summary>
    /// Send a message to the INDI server
    /// </summary>
    /// <param name="message">message to send</param>
    public void Send(IndiClientMessage message) {
        var xml = message.EncodeXml();
        this.SendXml(xml);
        this.Events.NotifyMessageSent(message);
    }

    /// <summary>
    /// Process a message from the server
    /// </summary>
    /// <param name="message">received message</param>
    public void Process(IndiServerMessage message) {
        if (message != null) {
            // Handle events listening to messages
            // Dispatch different events based on the message type
            try {
                // In case of change events, store a reference to the old value before doing the update
                IndiValue old = message is IndiSetPropertyMessage setter ? this.Devices.GetDeviceByNameOrNull(setter.DeviceName)?.Properties?.GetValueOrNull(setter.PropertyName) : null;
                
                // Actually do the correct action based on the message
                message.Process(this);

                // Generic notification of the message
                this.Events.NotifyMessageReceived(message);
                
                // Specific notification based on message type
                switch (message) {
                    case IndiSetPropertyMessage smsg: {
                        var device = this.Devices.GetDeviceByNameOrNull(smsg.DeviceName);
                        this.Events.NotifyPropertyChanged(
                            device, 
                            smsg.PropertyName,
                            old,
                            smsg.PropertyValue
                        );
                        break;
                    }
                    case IndiDefinePropertyMessage dmsg: {
                        var device = this.Devices.GetDeviceByNameOrNull(dmsg.DeviceName);
                        this.Events.NotifyPropertyCreated(device, dmsg.PropertyName, dmsg.PropertyValue);
                        break;   
                    }
                    case IndiDeletePropertyMessage delmsg: {
                        var device = this.Devices.GetDeviceByNameOrNull(delmsg.DeviceName);
                        this.Events.NotifyPropertyDeleted(device, delmsg.PropertyName, device.Properties?.GetValueOrNull(delmsg.PropertyName));
                        break; 
                    }
                    case IndiNotificationMessage note: {
                        var device = this.Devices.GetDeviceByNameOrNull(note.DeviceName);
                        this.Events.Notify(device, note.Message);
                        break;
                    }
                }   
            } catch {
                // Suppress all errors for handlers
            }

            // If the auto connect flag is set and we are defining a connection property
            if (message is IndiDefinePropertyMessage propDef) {
                if (this.AutoConnectDevices && !string.IsNullOrEmpty(propDef.DeviceName) && propDef.PropertyName == IndiStandardProperties.Connection) {
                    IndiDevice device;
                    if(this.Devices.TryGetDeviceByName(propDef.DeviceName, out device)) {
                        device.Connect();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Send a raw XML message to the INDI server
    /// </summary>
    /// <param name="xml">xml message</param>
    public void SendXml(string xml) {
        this.Write(xml);
    }
    private int bufferSize = 32768;
    private int BufferSize {
        get => bufferSize;
        set => Math.Max(value, 1); // Must be a positive non-zero number
    }
    protected override void AsyncRead() {
        StringBuilder message = new StringBuilder(BufferSize);
        while(IsConnected) {
            try {
                byte[] buffer = new byte[BufferSize];
                var bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0) {
                    var str = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    message.Append(str);
                    if (InputLogger != null) {
                        InputLogger.Write(str);
                    }

                    var xml = message.ToString();

                    List<IndiServerMessage> parsed;
                    if (tryParseXml(xml, out parsed)) {
                        foreach (var parsedMessage in parsed) {
                            Process(parsedMessage);
                        }
                        message.Clear();
                    }
                }

                Thread.Sleep(100); // Allow time for more data to come in
            } catch {
                Disconnect();
                break;
            }
        }
    }

    private bool tryParseXml(string xmllike, out List<IndiServerMessage> messages) {
        // Parse XML 
        messages = new List<IndiServerMessage>();
        XmlDocument xmlDocument = new XmlDocument();
        try {  
            xmlDocument.LoadXml("<document>" + xmllike + "</document>");
        } catch {
            return false;
        }

        // Translate XML
        foreach (var child in xmlDocument.DocumentElement.ChildNodes) {
           
            if (child is XmlElement element) {
                if (element.Name.StartsWith("set")) {
                    var value = parseIndiValue(element);
                    messages.Add(
                        new IndiSetPropertyMessage(
                            element.GetAttribute("device"),
                            element.GetAttribute("name"),
                            value
                        )
                    );
                }
                else if (element.Name.StartsWith("def")) {
                    var value = parseIndiValue(element);
                    messages.Add(
                        new IndiDefinePropertyMessage(
                            element.GetAttribute("device"),
                            element.GetAttribute("name"),
                            value
                        )
                    );
                } 
                else if (element.Name == ("delProperty")) {
                    messages.Add(
                        new IndiDeletePropertyMessage(
                            element.GetAttribute("device"),
                            element.GetAttribute("name"),
                            element.GetAttribute("timestamp"),
                            element.GetAttribute("message")
                        )
                    );
                }
                else if (element.Name == ("message")) {
                    messages.Add(
                        new IndiNotificationMessage(
                            element.GetAttribute("device"),
                            element.GetAttribute("timestamp"),
                            element.GetAttribute("message")
                        )
                    );
                } 
                // Fallback
                else {

                }
            }
            
        }

        return true;
    }

    private IndiValue parseIndiValueVector<T>(string label, XmlElement value) where T:IndiValue {
        var vector = new IndiVector<T> {
            Name = value.GetAttribute("name"),
            Label = label,
            Group = value.GetAttribute("group"),
            State = value.GetAttribute("state"),
            Permissions = value.GetAttribute("perm"),
            Rule = value.GetAttribute("rule"),
            Timeout = value.GetAttribute("timeout"),
            Timestamp = value.GetAttribute("timestamp"),
            Comment = value.GetAttribute("message")
        };
        foreach (var child in value.ChildNodes) {
            if (child is XmlElement element) {
                var member = parseIndiValue(element);
                if (member is T valid)
                    vector.Add(valid);
            }
        }
        return vector;
    }
    private IndiValue parseIndiValue(XmlElement value) {
        var label = value.GetAttribute("label");
        if (string.IsNullOrEmpty(label)) {
            label = value.GetAttribute("name");
        }
        var name = value.GetAttribute("name");

        if (value.Name.EndsWith("TextVector")) {
            return parseIndiValueVector<IndiTextValue>(label, value);
        } 
        else if (value.Name.EndsWith("Text")) {
            return new IndiTextValue {
                Name = name,
                Label = label,
                Value = value.InnerText
            };
        } 
        else if (value.Name.EndsWith("NumberVector")) {
            return parseIndiValueVector<IndiNumberValue>(label, value);
        } 
        else if (value.Name.EndsWith("Number")) {
            var result = parseIndiDouble(value.InnerText, value.GetAttribute("format"));

            double min, max, step;
            double.TryParse(value.GetAttribute("min"), out min);    // will be set to 0 be default if not provided (ie SET)
            double.TryParse(value.GetAttribute("max"), out max);    // will be set to 0 be default if not provided (ie SET)
            double.TryParse(value.GetAttribute("step"), out step);  // will be set to 0 be default if not provided (ie SET)
            
            return new IndiNumberValue {
                Name = name,
                Label = label,
                Value = result,
                Min = min,
                Max = max,
                Step = step
            };
        }
        else if (value.Name.EndsWith("SwitchVector")) {
            return parseIndiValueVector<IndiSwitchValue>(label, value);
        } 
        else if (value.Name.EndsWith("Switch")) {
            var s = new IndiSwitchValue {
                Name = name,
                Label = label,
                Switch = name,
                Value = value.InnerText.Trim() == "On"
            };
            return s;
        }
        // TODO handle lights
        else if (value.Name.EndsWith("LightVector")) {
            return parseIndiValueVector<IndiLightValue>(label, value);
        } 
        else if (value.Name.EndsWith("Light")) {
            return new IndiLightValue{
                Name = name,
                Label = label,
            };
        }
        // TODO handle blobs
        else if (value.Name.EndsWith("BLOBVector")) {
            return parseIndiValueVector<IndiBlobValue>(label, value);
        } 
        else if (value.Name.EndsWith("BLOB")) {
            return new IndiBlobValue {
                Name = name,
                Label = label,
                Value = value.InnerText
            };
        }

        // Default fallthrough case
        else {
            return null;
        }
    }

    private static Regex numberFormat = new Regex(@"(?<d>[+-]?\d+(?:\.\d+)?(?:[Ee][+-]?\d+)?)(?:\:(?<m>\d+(?:\.\d+)))?(?:\:(?<s>\d+(?:\.\d+)))?");
    private double parseIndiDouble(string str, string format) {
        /*
            Can be printf formats 
                https://www.cplusplus.com/reference/cstdio/printf/
            Or custom formats
                %<w>.<f>m
                <w> is the total field width
                <f> is the width of the fraction. valid values are:
                    9 -> :mm:ss.ss
                    8 -> :mm:ss.s
                    6 -> :mm:ss
                    5 -> :mm.m
                    3 -> :mm
        */
        if (string.IsNullOrEmpty(str)) {
            return 0;
        }
        
        // Hex based formats
        if (format == "x" || format == "X") {
            return int.Parse(str, System.Globalization.NumberStyles.HexNumber);
        }
        // Decimal based formats 
        var match = numberFormat.Match(str);
        double degrees = double.Parse(match.Groups["d"].Value, System.Globalization.NumberStyles.Float);
        if(match.Groups["m"].Success) {
            degrees += double.Parse(match.Groups["m"].Value) * 60;
        }
        if (match.Groups["s"].Success) {
            degrees += double.Parse(match.Groups["s"].Value) * 3600;
        }
        return degrees;
    }
}

}