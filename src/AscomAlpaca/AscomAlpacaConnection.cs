namespace Qkmaxware.Astro.Control {

public partial class AlpacaConnection : IServerConnection {
    public bool IsConnected {get; private set;}

    public IServerSpecification Server {get; private set;}
    public IConnectionLogger InputLogger { get; set; }

    public AlpacaConnection(AlpacaServer server, IConnectionLogger logger = null) {
        this.Server = server;
        this.InputLogger = logger;
    }

    public void Connect() {
        this.IsConnected = true;
    }

    public void Disconnect() {
        this.IsConnected = false;
    }
}

}