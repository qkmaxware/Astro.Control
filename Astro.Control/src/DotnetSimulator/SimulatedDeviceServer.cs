using System.Collections.Generic;
using System.Linq;
using Qkmaxware.Astro.Control;
using Qkmaxware.Astro.Control.Devices;

namespace Qkmaxware.Astro.Control {

/// <summary>
/// Simulated device source with a simulated connection to the source
/// </summary>
public class SimulatedDeviceServer : IServerSpecification {
    #region Fake Host
    public string Host => "localhost";
    public int Port => 0;
    public bool TryConnect(out IServerConnection connection, IConnectionLogger logger = null) {
        connection = new SimulatedDeviceConnection();
        connection.Connect();
        connection.InputLogger = logger;
        return true;
    }
    #endregion
}

}