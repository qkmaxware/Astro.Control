using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System;
using System.IO;

namespace Qkmaxware.Astro.Control {

public class Phd2Connection : BaseTcpConnection {
    /// <summary>
    /// Event dispatcher which can have its events subscribed to by listeners
    /// </summary>
    /// <value>event dispatcher</value>
    public Phd2ConnectionEventDispatcher Events {get; private set;}

    /// <summary>
    /// Output stream to print all received messages to
    /// </summary>
    public TextWriter OutputStream;

    internal Phd2Connection (Phd2Server server, Phd2ConnectionEventDispatcher events = null) :  base(server) {
        this.Events = events ?? new Phd2ConnectionEventDispatcher();
    }

    private static int bufferSize = 1000;

    protected override void AsyncRead() {
        int depth = 0;
        bool inQuotes = false;
        char lastChar = default(char);

        StringBuilder buffer = new StringBuilder(bufferSize);

        while (this.IsConnected) {
            // Try to read in the next character
            char b = (char)this.inputStream.ReadByte();
            if (b < 0) {
                this.Disconnect();
                return;
            }
            buffer.Append(b);

            // Track if we enter or exist an object
            if (b == '"' && lastChar != '\\') {
                inQuotes = !inQuotes;
            } else if (b == '{' && !inQuotes) {
                depth++;
            } else if (b == '}' && !inQuotes) {
                depth--;
            }

            // When "outer-most" object is closed
            if (depth == 0 && buffer.Length > 0) {
                try {
                    var json = buffer.ToString();                   // Get json
                    if (this.OutputStream != null)
                        this.OutputStream.Write(json);              // Redirect recieved messages
                    var obj = JsonDocument.Parse(json);             // Parse the object
                    Task.Run(() => process(obj.RootElement));       // Process the message on a new thread
                    buffer.Clear();                                 // Clear the buffer to prepare for future objects
                } catch {}
            }
        }
    }

    #region Event Types
    private const string Version = "Version";
    private const string LockPositionSet = "LockPositionSet";
    private const string Calibrating = "Calibrating";
    private const string CalibrationComplete = "CalibrationComplete";
    private const string StarSelected = "StarSelected";
    private const string StartGuiding = "StartGuiding";
    private const string Paused = "Paused";
    private const string StartCalibration = "StartCalibration";
    private const string AppState = "AppState";
    private const string CalibrationFailed = "CalibrationFailed";
    private const string CalibrationDataFlipped = "CalibrationDataFlipped";
    private const string LockPositionShiftLimitReached = "LockPositionShiftLimitReached";
    private const string LoopingExposures = "LoopingExposures";
    private const string LoopingExposuresStopped = "LoopingExposuresStopped";
    private const string SettleBegin = "SettleBegin";
    private const string Settling = "Settling";
    private const string SettleDone = "SettleDone";
    private const string StarLost = "StarLost";
    private const string GuidingStopped = "GuidingStopped";
    private const string Resumed = "Resumed";
    private const string GuideStep = "GuideStep";
    private const string GuidingDithered = "GuidingDithered";
    private const string LockPositionLost = "LockPositionLost";
    private const string Alert = "Alert";
    private const string GuideParamChange = "GuideParamChange";
    private const string ConfigurationChange = "ConfigurationChange";
    #endregion

    private void process(JsonElement node) {
        if (node.ValueKind != JsonValueKind.Object)
            return; // Messages sent from PHD2 are objects
        
        // Check if is an event message or an RPC call
        JsonElement tag;
        if (node.TryGetProperty("Event", out tag)) {
            processEvent(node);
        } else if (node.TryGetProperty("jsonrpc", out tag)) {
            processRpcResponse(node);
        }
        
    }

    # region Event Processing
    // https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#event-message-descriptions
    private void processEvent(JsonElement node) {
        // Process common attributes
        string type         = node.GetProperty("Event").GetString();
        double timestamp    = node.GetProperty("Timestamp").GetDouble();
        string host         = node.GetProperty("Host").GetString();
        string instance     = node.GetProperty("Inst").GetString();

        // Based on 'type' parse different kinds of messages
        switch (type) {
            case Version: {
                var message = new Phd2VersionMessage {
                    Timestamp  = timestamp,
                    HostName   = host,
                    InstanceId = instance,

                    PhdVersion = node.GetProperty("PHDVersion").GetString(),
                    PhdSubversion = node.GetProperty("PHDSubver").GetString(),
                    MsgVersion = node.GetProperty("MsgVersion").GetDouble(),
                    OverlapSupport = node.GetProperty("OverlapSupport").GetBoolean()
                };
                setVersion(message);
                Events.NotifyVersionRecieved(message);
            } break;
            case AppState: {
                var message = new Phd2AppStateMessage {
                    Timestamp  = timestamp,
                    HostName   = host,
                    InstanceId = instance,

                    State = node.GetProperty("State").GetString(),
                };
                setState(message);
                Events.NotifyAppStateChanged(message);
            } break;
            case Alert: break;
            case ConfigurationChange: break;

            case LockPositionSet: break;
            case LockPositionShiftLimitReached: break;
            case LockPositionLost: break;

            case LoopingExposures: break;
            case LoopingExposuresStopped: break;

            case StartCalibration: break;
            case Calibrating: break;
            case CalibrationComplete: break;
            case CalibrationFailed: break;
            case CalibrationDataFlipped: break;

            case StarSelected: break;
            case StartGuiding: break;
            case GuideParamChange: break;
            case GuideStep: break;
            case GuidingStopped: break;
            case GuidingDithered: break;
            case Resumed: break;
            case Paused: break;
            case StarLost: break;

            case SettleBegin: break;
            case Settling: break;
            case SettleDone: break;

        }
    }

    public Phd2VersionMessage versionInfo {get; private set;}
    public string VersionNumber => versionInfo?.PhdVersion + " " + versionInfo?.PhdSubversion;
    private void setVersion(Phd2VersionMessage message) {
        this.versionInfo = message;
    }
    public string State {get; private set;}
    private void setState(Phd2AppStateMessage message) {
        this.State = message.State;
    }
    # endregion

    # region RPC 
    // https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#available-methods
    private void processRpcResponse(JsonElement node) {

    }
    #endregion
}


}