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
}


}