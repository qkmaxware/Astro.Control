namespace Qkmaxware.Astro.Control {

/// <summary>
/// A simple logger to emit received messages over a connection
/// </summary>
public interface IConnectionLogger {
    /// <summary>
    /// Write a received message to the logger
    /// </summary>
    /// <param name="message">message object</param>
    public void Write(object message);
}

}