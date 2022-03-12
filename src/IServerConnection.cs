namespace Qkmaxware.Astro.Control {

/// <summary>
/// Interface describing an active connection to a server
/// </summary>
public interface IServerConnection {
    /// <summary>
    /// Test that the connection is active
    /// </summary>
    /// <returns>true of the connection is active</returns>
    bool IsConnected {get;}
    /// <summary>
    /// Disconnect from the remote server
    /// </summary>
    void Disconnect();
    /// <summary>
    /// Establish connection
    /// </summary>
    void Connect();
}

}