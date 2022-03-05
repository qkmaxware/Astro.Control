namespace Qkmaxware.Astro.Control {

/// <summary>
/// Phd2 server information
/// </summary>
public class Phd2Server : IServerSpecification {
    /// <summary>
    /// Default port used by PHD2 servers
    /// </summary>
    public static readonly int DefaultPort = 4400;
    
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
    /// Create a new reference to an PHD2 server
    /// </summary>
    /// <param name="host">host string</param>
    /// <param name="port">port</param>
    public Phd2Server(string host, int port = 4400) {
        this.Host = host;
        this.Port = port;
    }

    /// <summary>
    /// Try to establish a connection to the PHD2 server
    /// </summary>
    /// <param name="conn">connection if successful</param>
    /// <returns>true if connection was successful, false otherwise</returns>
    public bool TryConnect(out Phd2Connection conn) {
        return TryConnect(out conn, null);
    }

    /// <summary>
    /// Try to establish a connection to the PHD2 server
    /// </summary>
    /// <param name="conn">connection if successful</param>
    /// <param name="events">event listeners</param>
    /// <returns>true if connection was successful, false otherwise</returns>
    public bool TryConnect(out Phd2Connection conn, Phd2ConnectionEventDispatcher events) {
        conn = new Phd2Connection(this, events);
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


}