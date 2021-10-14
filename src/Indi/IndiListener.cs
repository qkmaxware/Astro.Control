namespace Qkmaxware.Astro.Control {
    
/// <summary>
/// Interface for any listeners that can subscribe to INDI connection events
/// </summary>
public interface IIndiListener {
    /// <summary>
    /// Listener to connection events
    /// </summary>
    /// <param name="server">server connected to</param>
    void OnConnect(IndiServer server);
    /// <summary>
    /// Listener to dis-connection events
    /// </summary>
    /// <param name="server">server disconnected from</param>
    void OnDisconnect(IndiServer server);
    /// <summary>
    /// Listener whenever client sends a message
    /// </summary>
    /// <param name="message">message sent</param>
    void OnMessageSent(IndiClientMessage message);
    /// <summary>
    /// Listener whenever client receives a message
    /// </summary>
    /// <param name="message">message received</param>
    void OnMessageReceived(IndiDeviceMessage message);
    /// <summary>
    /// Listener whenever a new device is defined
    /// </summary>
    /// <param name="device">device added</param>
    void OnAddDevice(IndiDevice device);
    /// <summary>
    /// Listener whenever a device is deleted
    /// </summary>
    /// <param name="device">device removed</param>
    void OnRemoveDevice(IndiDevice device);
}

/// <summary>
/// Base class with empty implementations of all listener functions
/// </summary>
public class BaseIndiListener : IIndiListener {
    public virtual void OnConnect(IndiServer server) {}
    public virtual void OnDisconnect(IndiServer server) {}

    public virtual void OnMessageSent(IndiClientMessage message) {}
    public virtual void OnMessageReceived(IndiDeviceMessage message) {
        switch (message) {
            case IndiSetPropertyMessage smsg:
                OnSetProperty(smsg); break;
            case IndiDefinePropertyMessage dmsg:
                OnDefineProperty(dmsg); break;
            case IndiDeletePropertyMessage delmsg:
                OnDeleteProperty(delmsg); break;
            case IndiNotificationMessage note:
                OnNotification(note); break;
        }   
    }

    public virtual void OnAddDevice(IndiDevice device){}
    public virtual void OnRemoveDevice(IndiDevice device) {}

    /// <summary>
    /// When a define property message is received
    /// </summary>
    /// <param name="message">message</param>
    public virtual void OnDefineProperty(IndiDefinePropertyMessage message){}
    /// <summary>
    /// When a delete property message is received
    /// </summary>
    /// <param name="message">message</param>
    public virtual void OnDeleteProperty(IndiDeletePropertyMessage message){}
    /// <summary>
    /// When a notification message is received
    /// </summary>
    /// <param name="message">message</param>
    public virtual void OnNotification(IndiNotificationMessage message){}
    /// <summary>
    /// When a set property message is received
    /// </summary>
    /// <param name="message">message</param>
    public virtual void OnSetProperty(IndiSetPropertyMessage message){}
}

}