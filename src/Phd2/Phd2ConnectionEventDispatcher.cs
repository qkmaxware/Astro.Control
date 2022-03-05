namespace Qkmaxware.Astro.Control {

public class Phd2ConnectionEventDispatcher {
    public delegate void Phd2VersionListener(Phd2VersionMessage version);
    public event Phd2VersionListener OnVersionRecieved = delegate {};
    internal void NotifyVersionRecieved(Phd2VersionMessage ver) {
        this.OnVersionRecieved?.Invoke(ver);
    } 

    public delegate void Phd2AppStateListener(Phd2AppStateMessage state);
    public event Phd2AppStateListener OnAppStateChanged = delegate {};
    internal void NotifyAppStateChanged(Phd2AppStateMessage msg) {
        this.OnAppStateChanged?.Invoke(msg);
    }
}

}