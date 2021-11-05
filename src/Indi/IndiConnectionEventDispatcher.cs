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
/// INDI connection class's event management and dispatching
/// </summary>
public class IndiConnectionEventDispatcher {

    #region Event listener defintions
    public delegate void ServerConnectionListener();
    public delegate void ServerDisconnectionListener();
    public delegate void MessageSentListener(IndiClientMessage message);
    public delegate void MessageReceivedListener(IndiServerMessage message);
        public delegate void DefinePropertyListener(IndiDevice device, string property, IndiValue value);
        public delegate void DeletePropertyListener(IndiDevice device, string property, IndiValue value);
        public delegate void SetPropertyListener(IndiDevice device, string property, IndiValue previous, IndiValue next);
        public delegate void NotificationReceivedListener(IndiDevice device, string message);
    public delegate void DeviceFoundListener(IndiDevice device);
    public delegate void DeviceRemovedListener(IndiDevice device);
    #endregion
    

    public event ServerConnectionListener OnServerConnected = delegate{};
    internal void NotifyServerConnected() {
        this.OnServerConnected?.Invoke();
    }
    public event ServerDisconnectionListener OnServerDisconnected = delegate{};
    internal void NotifyServerDisconnected() {
        this.OnServerConnected?.Invoke();
    }
    public event MessageSentListener OnMessageSent= delegate{};
    internal void NotifyMessageSent(IndiClientMessage message) {
        this.OnMessageSent?.Invoke(message);
    }
    public event MessageReceivedListener OnMessageReceived= delegate{};
    internal void NotifyMessageReceived(IndiServerMessage message) {
        this.OnMessageReceived?.Invoke(message);
    }
        public event DefinePropertyListener OnPropertyDefined= delegate{};
        internal void NotifyPropertyCreated(IndiDevice device, string property, IndiValue value) {
            this.OnPropertyDefined?.Invoke(device, property, value);
        }
        public event DeletePropertyListener OnPropertyDeleted= delegate{};
        internal void NotifyPropertyDeleted(IndiDevice device, string property, IndiValue value) {
            this.OnPropertyDeleted?.Invoke(device, property, value);
        }
        public event SetPropertyListener OnPropertyChanged= delegate{};
        internal void NotifyPropertyChanged(IndiDevice device, string property, IndiValue previous, IndiValue next) {
            this.OnPropertyChanged?.Invoke(device, property, previous, next);
        }
        public event NotificationReceivedListener OnNotificationReceived= delegate{};
        internal void Notify(IndiDevice device, string message) {
            this.OnNotificationReceived?.Invoke(device, message);
        }
    public event DeviceFoundListener OnDeviceFound= delegate{};
    internal void NotifyDeviceFound(IndiDevice device) {
        this.OnDeviceFound?.Invoke(device);
    }
    public event DeviceRemovedListener OnDeviceRemoved= delegate{};
    internal void NotifyDeviceRemoved(IndiDevice device) {
        this.OnDeviceRemoved?.Invoke(device);
    }

}

}