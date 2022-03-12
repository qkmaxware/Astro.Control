using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System;
using System.IO;
using System.Linq;
using System.Numerics;

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

    private static T MapMsg2Obj<T>(T entity, JsonElement node) {
        var attrType = typeof(Phd2MessagePropertyAttribute);
        var fields = entity.GetType().GetFields().Where(field => Attribute.IsDefined(field, attrType));

        foreach (var field in fields) {
            var fieldType = field.FieldType;
            var attrs = field.GetCustomAttributes(attrType, true);
            foreach (var attr in attrs) {
                string propertyName = ((Phd2MessagePropertyAttribute)attr).PropertyName;
                var jsonProperty = node.GetProperty(propertyName);

                // Standard "primitive" types
                if (fieldType == typeof(string)) {
                    field.SetValue(entity, jsonProperty.GetString());
                } 
                else if (fieldType == typeof(int)) {
                    field.SetValue(entity, (int)jsonProperty.GetDouble());
                }
                else if (fieldType == typeof(long)) {
                    field.SetValue(entity, (long)jsonProperty.GetDouble());
                }
                else if (fieldType == typeof(float)) {
                    field.SetValue(entity, (float)jsonProperty.GetDouble());
                }
                else if (fieldType == typeof(long)) {
                    field.SetValue(entity, (long)jsonProperty.GetDouble());
                }
                else if (fieldType == typeof(bool)) {
                    field.SetValue(entity, (bool)jsonProperty.GetBoolean());
                }

                // Custom type mappings
                else if (fieldType == typeof(JsonElement)) {
                    field.SetValue(entity, jsonProperty);
                }
                else if (field == typeof(Vector2)) {
                    var x = jsonProperty.GetArrayLength() > 0 ? jsonProperty[0].GetDouble() : 0;
                    var y = jsonProperty.GetArrayLength() > 1 ? jsonProperty[1].GetDouble() : 0;
                    field.SetValue(entity, new Vector2((float)x, (float)y));
                }
                else if (field == typeof(Vector3)) {
                    var x = jsonProperty.GetArrayLength() > 0 ? jsonProperty[0].GetDouble() : 0;
                    var y = jsonProperty.GetArrayLength() > 1 ? jsonProperty[1].GetDouble() : 0;
                    var z = jsonProperty.GetArrayLength() > 2 ? jsonProperty[2].GetDouble() : 0;
                    field.SetValue(entity, new Vector3((float)x, (float)y, (float)z));
                }
            }
        }
        return entity;
    }

    # region Event Processing
    // https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#event-message-descriptions
    private void processEvent(JsonElement node) {
        // Process common attributes
        string type         = node.GetProperty("Event").GetString();

        // Based on 'type' parse different kinds of messages
        switch (type) {
            case Version: {
                var message = MapMsg2Obj(new Phd2VersionMessage(), node);
                setVersion(message);
                Events.NotifyVersionRecieved(message);
            } break;
            case AppState: {
                var message = MapMsg2Obj(new Phd2AppStateMessage(), node);
                setState(message);
                Events.NotifyAppStateChanged(message);
            } break;
            case Alert: {
                var message = MapMsg2Obj(new Phd2AlertMessage(), node);
                Events.NotifyAlertReceived(message);
            } break;
            case ConfigurationChange: {
                var message = MapMsg2Obj(new Phd2ConfigurationChangeMessage(), node);
                Events.NotifyConfigurationChanged(message);
            } break;

            case LockPositionSet: {
                var message = MapMsg2Obj(new Phd2LockPositionSetMessage(), node);
                Events.NotifyLockPositionSet(message);
            } break;
            case LockPositionShiftLimitReached: {
                var message = MapMsg2Obj(new Phd2LockPositionShiftLimitReachedMessage(), node);
                Events.NotifyLockPositionShiftLimitReached(message);
            } break;
            case LockPositionLost: {
                var message = MapMsg2Obj(new Phd2LockPositionLostMessage(), node);
                Events.NotifyLockPositionLost(message);
            } break;

            case LoopingExposures:  {
                var message = MapMsg2Obj(new Phd2LoopingExposuresIterationMessage(), node);
                Events.NotifyLoopingExposuresIteration(message);
            } break;
            case LoopingExposuresStopped: {
                var message = MapMsg2Obj(new Phd2LoopingExposuresStoppedMessage(), node);
                Events.NotifyLoopingExposuresStopped(message);
            } break;

            case StartCalibration: {
                var message = MapMsg2Obj(new Phd2CalibrationStartMessage(), node);
                Events.NotifyCalibrationStart(message);
            } break;
            case Calibrating: {
                var message = MapMsg2Obj(new Phd2CalibratingMessage(), node);
                Events.NotifyCalibrating(message);
            } break;
            case CalibrationComplete: {
                var message = MapMsg2Obj(new Phd2CalibrationCompleteMessage(), node);
                Events.NotifyCalibrationComplete(message);
            } break;
            case CalibrationFailed: {
                var message = MapMsg2Obj(new Phd2CalibrationFailedMessage(), node);
                Events.NotifyCalibrationFailed(message);
            } break;
            case CalibrationDataFlipped: {
                var message = MapMsg2Obj(new Phd2CalibrationDataFlippedMessage(), node);
                Events.NotifyCalibrationDataFlipped(message);
            } break;

            case StarSelected: {
                var message = MapMsg2Obj(new Phd2StarSelectedMessage(), node);
                Events.NotifyStarSelected(message);
            } break;
            case StartGuiding: {
                var message = MapMsg2Obj(new Phd2StartGuidingMessage(), node);
                Events.NotifyStartGuiding(message);
            } break;
            case GuideParamChange: {
                var message = MapMsg2Obj(new Phd2GuideParametreChangeMessage(), node);
                Events.NotifyGuideParametreChange(message);
            } break;
            case GuideStep: {
                var message = MapMsg2Obj(new Phd2GuideStepMessage(), node);
                Events.NotifyGuideStep(message);
            } break;
            case GuidingStopped: {
                var message = MapMsg2Obj(new Phd2StoppedGuidingMessage(), node);
                Events.NotifyStoppedGuiding(message);
            } break;
            case GuidingDithered: {
                var message = MapMsg2Obj(new Phd2GuidingDitheredMessage(), node);
                Events.NotifyGuidingDithered(message);
            } break;
            case Resumed: {
                var message = MapMsg2Obj(new Phd2ResumedGuidingMessage(), node);
                Events.NotifyResumedGuiding(message);
            } break;
            case Paused: {
                var message = MapMsg2Obj(new Phd2PausedGuidingMessage(), node);
                Events.NotifyPausedGuiding(message);
            } break;
            case StarLost: {
                var message = MapMsg2Obj(new Phd2StarLostMessage(), node);
                Events.NotifyStarLost(message);
            } break;

            case SettleBegin: {
                var message = MapMsg2Obj(new Phd2SettleBeginMessage(), node);
                Events.NotifySettleBegin(message);
            } break;
            case Settling: {
                var message = MapMsg2Obj(new Phd2SettlingMessage(), node);
                Events.NotifySettling(message);
            } break;
            case SettleDone: {
                var message = MapMsg2Obj(new Phd2SettleDoneMessage(), node);
                Events.NotifySettleDone(message);
            } break;

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