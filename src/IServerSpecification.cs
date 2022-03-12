namespace Qkmaxware.Astro.Control {

/// <summary>
/// Interface describing the information needed to connect to a server
/// </summary>
public interface IServerSpecification {
    /// <summary>
    /// Server host ip
    /// </summary>
    /// <value>host</value>
    string Host {get;}
    /// <summary>
    /// Server port
    /// </summary>
    /// <value>port number</value>
    int Port {get;}

    /// <summary>
    /// Try to connect to the remote server
    /// </summary>
    /// <param name="connection">connection if successful</param>
    /// <returns>true if connection was successful</returns>
    bool TryConnect(out IServerConnection connection);
}

}