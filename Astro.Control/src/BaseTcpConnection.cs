using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Qkmaxware.Astro.Control {

/// <summary>
/// Base class for TCP based connections
/// </summary>
public abstract class BaseTcpConnection : IServerConnection {
    /// <summary>
    /// The server that this connection is established to
    /// </summary>
    /// <value>connected server</value>
    public IServerSpecification Server {get; private set;}

    protected TcpClient client;
    protected Stream inputStream;
    private StreamWriter writer;

    /// <summary>
    /// Get or set a logger for the given connection
    /// </summary>
    /// <value>logger</value>
    public IConnectionLogger InputLogger {get;set;}

    /// <summary>
    /// Check if the connection is active
    /// </summary>
    public bool IsConnected => client != null && client.Connected;

    internal BaseTcpConnection (IServerSpecification server) {
        this.Server = server;
    }

    ~BaseTcpConnection() {
        this.Disconnect();
    }

    /// <summary>
    /// Disconnect from the PHD2 server
    /// </summary>
    public void Disconnect() {
        client?.Close();
        client = null;
        inputStream = null;
        writer = null;
        ConnectionClosed();
    }

    /// <summary>
    /// Cleanup after a server disconnection
    /// </summary> 
    protected virtual void ConnectionClosed() {}

    /// <summary>
    /// Attempt to reconnect if no longer connected
    /// </summary>
    public void Connect() {
        if (!IsConnected) {
            try {
                client = new TcpClient(Server.Host, Server.Port);
                if (IsConnected) {
                    NetworkStream stream = client.GetStream();
                    inputStream =  stream;
                    writer = new StreamWriter(stream, Encoding.UTF8);

                    Task.Run(AsyncRead);

                    ConnectionEstablished();
                }
            } catch {
                Disconnect();
            }
        }
    }
    /// <summary>
    /// Initialize object after server connection
    /// </summary> 
    protected virtual void ConnectionEstablished() {}

    protected abstract void AsyncRead();

    protected void Write(string message) {
        if (IsConnected && writer != null) {
            writer.Write(message);
            writer.Flush();
        }
    }
}

}