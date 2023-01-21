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
/// INDI server information
/// </summary>
public class IndiServer : IServerSpecification {
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
    /// <param name="logger">logger for connection messages</param>
    /// <returns>true if connection was successful, false otherwise</returns>
    public bool TryConnect(out IServerConnection conn, IConnectionLogger logger = null) {
        conn = new IndiConnection(this, null);
        conn.InputLogger = logger;
        conn.Connect();
        if (conn.IsConnected) {
            return true;
        } else {
            conn.Disconnect();
            conn = null;
            return false;
        }
    }
}

}