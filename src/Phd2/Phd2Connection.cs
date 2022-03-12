using System.Text.Json;
using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using Qkmaxware.Measurement;
using System.Collections;

namespace Qkmaxware.Astro.Control {

public partial class Phd2Connection : JsonRpcClient {
    /// <summary>
    /// Event dispatcher which can have its events subscribed to by listeners
    /// </summary>
    /// <value>event dispatcher</value>
    public Phd2ConnectionEventDispatcher Events {get; private set;}

    internal Phd2Connection (Phd2Server server, Phd2ConnectionEventDispatcher events = null) :  base(server) {
        this.Events = events ?? new Phd2ConnectionEventDispatcher();
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

    protected override void processNonRpcResponse(JsonElement node) {
        // only process object messages with an "event" tag
        JsonElement tag;
        if (node.TryGetProperty("Event", out tag)) {
            processEvent(node);
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
                this.setState("Looping");
            } break;
            case LoopingExposuresStopped: {
                var message = MapMsg2Obj(new Phd2LoopingExposuresStoppedMessage(), node);
                Events.NotifyLoopingExposuresStopped(message);
                this.setState("Stopped");
            } break;

            case StartCalibration: {
                var message = MapMsg2Obj(new Phd2CalibrationStartMessage(), node);
                Events.NotifyCalibrationStart(message);
                this.setState("Calibrating");
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
                this.setState("Selected");
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
                this.setState("Guiding");
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
                this.setState("Paused");
            } break;
            case StarLost: {
                var message = MapMsg2Obj(new Phd2StarLostMessage(), node);
                Events.NotifyStarLost(message);
                this.setState("LostLock");
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
    private void setState(string state) {
        this.State = state;
    }
    # endregion

    # region RPC 
    // https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#available-methods
    public void CaptureSingleFrame(Duration exposure) {
        var req = new JsonRpcClient.Request("capture_single_frame", (double)exposure.TotalMilliseconds());
        this.Send(req);
    } 
    public void ClearCalibration() {
        var req = new JsonRpcClient.Request("clear_calibration", "both");
        this.Send(req);
    }
    public void FlipCalibration() {
        var req = new JsonRpcClient.Request("flip_calibration");
        this.Send(req);
    }
    public void StopCapture () {
        var req = new JsonRpcClient.Request("stop_capture");
        this.Send(req);
    }
    public void SelectProfile(Phd2Profile profile) {
        this.SelectProfileWithId(profile.Id);
    }
    public void SelectProfileWithId(int id) {
        var req = new JsonRpcClient.Request("set_profile", id);
        this.Send(req);
    }
    public void Guide(double pixelStableGuideDistance = 1.5, double settleTime = 10, double settlingTimeout = 60, bool recalibrate = false) {
        // First transmit stop_capture to "start fresh" as stated here https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#guide-method
        var req = new JsonRpcClient.Request("guide", new Dictionary<string, object> {
            {"settle", new Dictionary<string,object> {
                {"pixels",  pixelStableGuideDistance}, 
                {"time",    settleTime}, 
                {"timeout", settlingTimeout}
            }},
            {"recalibrate", recalibrate}
        });
        this.Send(req);
    }
    private List<Phd2Profile> cachedProfiles = new List<Phd2Profile>();
    public IEnumerable<Phd2Profile> EnumerateProfiles => cachedProfiles.AsReadOnly();
    public void FetchRemoteProfiles() {
        var req = new JsonRpcClient.Request("get_profiles");
        var task = this.Send(req);
        task.Wait();
        var profiles = task.Result.result;
        cachedProfiles.Clear();

        if (profiles != null && profiles is IEnumerable profileArray) {
            foreach (var profile in profileArray) {
                if (profile is IDictionary<string, object> profileObject) {
                    this.cachedProfiles.Add(new Phd2Profile((int)profileObject["id"], profileObject["name"]?.ToString(), this));
                }
            }
        }
    }
    #endregion
}


}