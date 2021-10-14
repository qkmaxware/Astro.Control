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
using System.Text.RegularExpressions;
using System.Threading;

namespace Qkmaxware.Astro.Control {

/// <summary>
/// INDI server information
/// </summary>
public class IndiServer {
    /// <summary>
    /// Default port used by INDI servers
    /// </summary>
    public static readonly int DefaultPort = 7624;
    /// <summary>
    /// Server host ip
    /// </summary>
    /// <value>host</value>
    public string Host {get; private set;}
    /// <summary>
    /// Server port
    /// </summary>
    /// <value>port number</value>
    public int Port {get; private set;}

    /// <summary>
    /// Create a new reference to an INDI server
    /// </summary>
    /// <param name="host">host string</param>
    /// <param name="port">port</param>
    public IndiServer(string host, int port = 7624) {
        this.Host = host;
        this.Port = port;
    }

    /// <summary>
    /// Try to establish a connection to the INDI server
    /// </summary>
    /// <param name="conn">connection if successful</param>
    /// <returns>true if connection was successful, false otherwise</returns>
    public bool TryConnect(out IndiConnection conn) {
        return TryConnect(out conn, new IIndiListener[0]);
    }

    /// <summary>
    /// Try to establish a connection to the INDI server
    /// </summary>
    /// <param name="conn">connection if successful</param>
    /// <param name="listeners">list of INDI listeners to automatically subscribe to the connection if successful</param>
    /// <returns>true if connection was successful, false otherwise</returns>
    public bool TryConnect(out IndiConnection conn, params IIndiListener[] listeners) {
        conn = new IndiConnection(this);
        foreach (var listener in listeners) {
            conn.Subscribe(listener);
        }
        conn.ReConnect();
        if (conn.IsConnected) {
            return true;
        } else {
            conn.Disconnect();
            conn = null;
            return false;
        }
    }
}

/// <summary>
/// Connection from client machine to INDI server
/// </summary>
public class IndiConnection : Notifier<IIndiListener> {
    private IndiServer server;
    private TcpClient client;
    private StreamReader reader;
    private StreamWriter writer;

    /// <summary>
    /// Output stream to print all received messages to
    /// </summary>
    public StreamWriter OutputStream;

    /// <summary>
    /// Check if the connection is active
    /// </summary>
    public bool IsConnected => client != null && client.Connected;

    /// <summary>
    /// Devices acquired from this connection
    /// </summary>
    /// <typeparam name="string">device name</typeparam>
    /// <typeparam name="IndiDevice">device</typeparam>
    /// <returns>all devices</returns>
    public ConcurrentDictionary<string, IndiDevice> Devices {get; private set;} = new ConcurrentDictionary<string, IndiDevice>();

    internal IndiConnection (IndiServer server) {
        var builder = new UriBuilder();
        this.server = server;
    }

    ~IndiConnection() {
        this.Disconnect();
    }

    /// <summary>
    /// Add a device to this connection
    /// </summary>
    /// <param name="device">device</param>
    public void AddDevice(IndiDevice device) {
        this.Devices.AddOrUpdate(device.Name, device, (key, old) => device);
        foreach (var sub in this.Subscribers) {
            sub.OnAddDevice(device);
        }
    }

    /// <summary>
    /// Remove device from this connection
    /// </summary>
    /// <param name="device">device</param>
    public void RemoveDevice(IndiDevice device) {
        IndiDevice deleted;
        this.Devices.TryRemove(device.Name, out deleted);
        foreach (var sub in this.Subscribers) {
            sub.OnRemoveDevice(deleted);
        }
    }

    /// <summary>
    /// Remove a device with the given name
    /// </summary>
    /// <param name="name">name of device</param>
    public void RemoveDeviceByName(string name) {
        RemoveDevice(GetDeviceByName(name));
    }

    /// <summary>
    /// Get a device with the given name
    /// </summary>
    /// <param name="name">name of device</param>
    /// <returns>device or null</returns>
    public IndiDevice GetDeviceByName(string name) {
        IndiDevice device;
        if (Devices.TryGetValue(name, out device)) {
            return device;
        } else {
            return null;
        }
    }

    /// <summary>
    /// Get a device with the given name, or create it if it doesn't exist
    /// </summary>
    /// <param name="name">name of device</param>
    /// <returns>existing or new device</returns>
    public IndiDevice GetOrCreateDevice(string name) {
        int b4 = Devices.Count;
        var device = Devices.GetOrAdd(name, new IndiDevice(name, this));
        if (Devices.Count > b4) {
            foreach (var sub in this.Subscribers) {
                sub.OnAddDevice(device);
            }
        }
        return device;
    }

    /// <summary>
    /// Query all properties for all devices
    /// </summary>
    public void QueryProperties() {
        this.Send(new IndiGetPropertiesMessage());
    }

    /// <summary>
    /// Attempt to reconnect if no longer connected
    /// </summary>
    public void ReConnect() {
        if (!IsConnected) {
            try {
                client = new TcpClient(server.Host, server.Port);
                if (IsConnected) {
                    NetworkStream stream = client.GetStream();
                    reader = new StreamReader(stream, Encoding.UTF8);
                    writer = new StreamWriter(stream, Encoding.UTF8);

                    Task.Run(asyncRead);

                    foreach (var sub in this.Subscribers) {
                        sub.OnConnect(this.server);
                    }
                }
            } catch {
                Disconnect();
            }
        }
    }

    /// <summary>
    /// Disconnect from the INDI server
    /// </summary>
    public void Disconnect() {
        client?.Close();
        client = null;
        reader = null;
        writer = null;
        foreach (var sub in this.Subscribers) {
            sub.OnDisconnect(this.server);
        }
    }

    /// <summary>
    /// Send a message to the INDI server
    /// </summary>
    /// <param name="message">message to send</param>
    public void Send(IndiClientMessage message) {
        this.SendXml(message.EncodeXml());
        foreach (var sub in this.Subscribers) {
            sub.OnMessageSent(message);
        }
    }

    /// <summary>
    /// Receive a message from the server
    /// </summary>
    /// <param name="message">received message</param>
    public void Receive(IndiDeviceMessage message) {
        // actually do the correct action based on the message
        // adding devices or updating properties etc
        // allow blockers to continue
        if (message != null) {
            try {
                message.Process(this);
                foreach (var sub in this.Subscribers) {
                    sub.OnMessageReceived(message);
                }
            } catch {
                // Suppress all errors for handlers
            }
        }
    }

    /// <summary>
    /// Send a raw XML message to the INDI server
    /// </summary>
    /// <param name="xml">xml message</param>
    public void SendXml(string xml) {
        if (IsConnected) {
            writer.Write(xml);
            writer.Flush();
        }
    }

    private void asyncRead() {
        StringBuilder str = new StringBuilder(client.Available);
        while(IsConnected) {
            try {
                while (reader != null && ((NetworkStream)reader.BaseStream).DataAvailable) {
                    char[] buffer = new char[client.Available];
                    var charactersRead = reader.ReadBlock(buffer, 0, buffer.Length);
                    for(var i = 0; i < charactersRead; i++) {
                        var c = buffer[i];
                        if (XmlConvert.IsXmlChar(c)) {
                            str.Append(c);
                            if (OutputStream != null) {
                                OutputStream.Write(c);
                            }
                        }
                    }

                    var xml = str.ToString();
                    //var now = DateTime.Now.ToString("dd-MM-yyy HH.mm.ss");
                    //using (var fs = new StreamWriter($"{now}.data.log")) {
                        //fs.Write(xml);
                        //Thread.Sleep(1500);
                    //}
                    
                    //try {
                    if (tryParseXml(xml)) {
                        str.Clear();
                    }
                    //} catch (Exception ex) {
                        //Console.WriteLine(ex.Message);
                        //Console.WriteLine(ex.StackTrace);
                    //}
                }
            } catch {
                continue;
            }
        }
    }

    private bool tryParseXml(string xmllike) {
        // Parse XML 
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
                    Receive(
                        new IndiSetPropertyMessage(
                            element.GetAttribute("device"),
                            element.GetAttribute("name"),
                            value
                        )
                    );
                }
                else if (element.Name.StartsWith("def")) {
                    var value = parseIndiValue(element);
                    Receive(
                        new IndiDefinePropertyMessage(
                            element.GetAttribute("device"),
                            element.GetAttribute("name"),
                            value
                        )
                    );
                } 
                else if (element.Name == ("delProperty")) {
                    Receive(
                        new IndiDeletePropertyMessage(
                            element.GetAttribute("device"),
                            element.GetAttribute("name"),
                            element.GetAttribute("timestamp"),
                            element.GetAttribute("message")
                        )
                    );
                }
                else if (element.Name == ("message")) {
                    Receive(
                        new IndiNotificationMessage(
                            element.GetAttribute("device"),
                            element.GetAttribute("timestamp"),
                            element.GetAttribute("message")
                        )
                    );
                } 
                // Fallback
                else {}
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
            return new IndiSwitchValue {
                Name = name,
                Label = label,
                Switch = name,
                Value = value.InnerText == "On"
            };
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