namespace Qkmaxware.Astro.Control {

/// <summary>
/// Class representing a server hosting ASCOM Alpaca 
/// </summary>
public class AlpacaServer : IServerSpecification {
    /// <summary>
    /// Default port used by Alpaca servers
    /// </summary>
    public static readonly int DefaultPort = 7843;
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
    /// Create a new reference to an Alpaca server
    /// </summary>
    /// <param name="host">host string</param>
    /// <param name="port">alpaca discovery port</param>
    public AlpacaServer(string host, int port = 7843) {
        this.Host = host;
        this.Port = port;
    }


    /// <summary>
    /// Try to establish a connection to the Alpaca server
    /// </summary>
    /// <param name="conn">connection if successful</param>
    /// <param name="logger">logger for connection messages</param>
    /// <returns>true if connection was successful, false otherwise</returns>
    public bool TryConnect(out IServerConnection conn, IConnectionLogger logger = null) {
        conn = new AlpacaConnection(this, logger);
        conn.Connect();
        // TODO assert that the server supports the API version implemented
        return conn.IsConnected;
    }
}

}