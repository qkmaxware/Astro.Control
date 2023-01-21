using System.Numerics;
using System.Text.Json;

namespace Qkmaxware.Astro.Control {

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring
# region PHD2 Message Types

/// <summary>
/// Base class Phd2for all messages sent from a PHD2 server
/// </summary>
public abstract class Phd2ServerMessage {
    [Phd2MessageProperty("Timestamp")]
    public double Timestamp;
    [Phd2MessageProperty("Host")]
    public string HostName;
    [Phd2MessageProperty("Inst")]
    public string InstanceId;
}

public class Phd2VersionMessage : Phd2ServerMessage {
    [Phd2MessageProperty("PHDVersion")]
    public string PhdVersion;
    [Phd2MessageProperty("PHDSubver")]
    public string PhdSubversion;
    [Phd2MessageProperty("MsgVersion")]
    public double MessageVersion;
    [Phd2MessageProperty("OverlapSupport")]
    public bool OverlapSupport;
}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#appstate
public class Phd2AppStateMessage : Phd2ServerMessage {
    [Phd2MessageProperty("State")]
    /// <summary>
    /// One of Stopped, Selected, Calibrating, Guiding, LostLock, Paused, Looping
    /// </summary>
    public string State;
}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#lockpositionset
public class Phd2LockPositionSetMessage : Phd2ServerMessage {
    [Phd2MessageProperty("X")]
    public double X;
    [Phd2MessageProperty("Y")]
    public double Y;
}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#calibrating
public class Phd2CalibratingMessage : Phd2ServerMessage {
    [Phd2MessageProperty("Mount")]
    public string MountName;
    [Phd2MessageProperty("dir")]
    public string CalibrationDirection;
    [Phd2MessageProperty("dist")]
    public double StartingDistance;
    [Phd2MessageProperty("dx")]
    public double StartingOffsetX;
    [Phd2MessageProperty("dy")]
    public double StartingOffsetY;
    [Phd2MessageProperty("pos")]    
    public Vector2 CalibrationStarCoordinates;
    [Phd2MessageProperty("step")]
    public double StepSize;
    [Phd2MessageProperty("State")]
    public string State;
}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#startcalibration
public class Phd2CalibrationStartMessage : Phd2ServerMessage {
    [Phd2MessageProperty("Mount")]
    public string MountName;
}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#calibrationcomplete
public class Phd2CalibrationCompleteMessage : Phd2ServerMessage {
    [Phd2MessageProperty("Mount")]
    public string MountName;
}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#starselected
public class Phd2StarSelectedMessage : Phd2ServerMessage {
    [Phd2MessageProperty("X")]
    public double LockX;
    [Phd2MessageProperty("Y")]
    public double LockY;
}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#startguiding
public class Phd2StartGuidingMessage : Phd2ServerMessage {}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#paused
public class Phd2PausedGuidingMessage : Phd2ServerMessage {}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#calibrationfailed
public class Phd2CalibrationFailedMessage : Phd2ServerMessage {
    [Phd2MessageProperty("Reason")]
    public string Message;
}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#calibrationdataflipped
public class Phd2CalibrationDataFlippedMessage : Phd2ServerMessage {
    [Phd2MessageProperty("Mount")]
    public string MountName;
}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#lockpositionshiftlimitreached
public class Phd2LockPositionShiftLimitReachedMessage : Phd2ServerMessage {}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#loopingexposures
public class Phd2LoopingExposuresIterationMessage : Phd2ServerMessage {
    [Phd2MessageProperty("Frame")]
    public int Frame;
}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#loopingexposuresstopped
public class Phd2LoopingExposuresStoppedMessage : Phd2ServerMessage {}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#settlebegin
public class Phd2SettleBeginMessage : Phd2ServerMessage {}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#settling
public class Phd2SettlingMessage : Phd2ServerMessage {
    [Phd2MessageProperty("Distance")]
    public double Distance;
    [Phd2MessageProperty("Time")]
    public double EllapsedTime;
    [Phd2MessageProperty("SettleTime")]
    public double SettleTime;
    [Phd2MessageProperty("StarLocked")]
    public bool IsStarLocked;
}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#settledone
public class Phd2SettleDoneMessage : Phd2ServerMessage {
    [Phd2MessageProperty("Status")]
    public double Status;
    [Phd2MessageProperty("Error")]
    public string ErrorMessage;
    [Phd2MessageProperty("TotalFrames")]
    public int TotalFrames;
    [Phd2MessageProperty("DroppedFrames")]
    public int DroppedFrames;
}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#starlost
public class Phd2StarLostMessage : Phd2ServerMessage {
    [Phd2MessageProperty("Frame")]
    public int FrameNumber;
    [Phd2MessageProperty("Time")]
    public double Time;
    [Phd2MessageProperty("StarMass")]
    public double StarMass;
    [Phd2MessageProperty("SNR")]
    public double SNR;
    [Phd2MessageProperty("AvgDist")]
    public double AverageDistance;
    [Phd2MessageProperty("ErrorCode")]
    public int ErrorCode;
    [Phd2MessageProperty("Status")]
    public string ErrorMessage;
}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#guidingstopped
public class Phd2StoppedGuidingMessage : Phd2ServerMessage {}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#resumed
public class Phd2ResumedGuidingMessage : Phd2ServerMessage {}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#guidestep
public class Phd2GuideStepMessage : Phd2ServerMessage {
    [Phd2MessageProperty("Frame")]
    public int Frame;
    [Phd2MessageProperty("Time")]
    public double Time;
    [Phd2MessageProperty("Mount")]
    public string MountName;
    [Phd2MessageProperty("dx")]
    public double PixelOffsetX;
    [Phd2MessageProperty("dy")]
    public double PixelOffsetY;
    [Phd2MessageProperty("RADistanceRaw")]
    public double RawDistanceRA;
    [Phd2MessageProperty("DecDistanceRaw")]
    public double RawDistanceDec;
    [Phd2MessageProperty("RADistanceGuide")]
    public double GuideDistanceRA;
    [Phd2MessageProperty("DecDistanceGuide")]
    public double GuideDistanceDec;
    [Phd2MessageProperty("RADuration")]
    public double DurationRA;
    [Phd2MessageProperty("RADirection")]
    public string DirectionRA;
    [Phd2MessageProperty("DECDuration")]
    public double DurationDec;
    [Phd2MessageProperty("DECDirection")]
    public string DirectionDec;
    [Phd2MessageProperty("StarMass")]
    public double StarMass;
    [Phd2MessageProperty("SNR")]
    public double SNR;
    [Phd2MessageProperty("HFD")]
    public double HFD;
    [Phd2MessageProperty("AvgDist")]
    public double AverageDistance;
    [Phd2MessageProperty("RALimited")]
    public bool IsRALimited;
    [Phd2MessageProperty("DecLimited")]
    public bool IsDecLimited;
    [Phd2MessageProperty("ErrorCode")]
    public int ErrorCode;
}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#guidingdithered
public class Phd2GuidingDitheredMessage : Phd2ServerMessage {
    [Phd2MessageProperty("dx")]
    public double DitherOffsetX;
    [Phd2MessageProperty("dy")]
    public double DitherOffsetY;
}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#lockpositionlost
public class Phd2LockPositionLostMessage : Phd2ServerMessage { }

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#alert
public class Phd2AlertMessage : Phd2ServerMessage {
    [Phd2MessageProperty("Type")]
    public string MessageType;
    [Phd2MessageProperty("Msg")]
    public string Message;
}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#guideparamchange
public class Phd2GuideParametreChangeMessage : Phd2ServerMessage {
    [Phd2MessageProperty("Name")]
    public string ParametreName;
    [Phd2MessageProperty("Value")]
    public JsonElement Value;
}

// https://github.com/OpenPHDGuiding/phd2/wiki/EventMonitoring#configurationchange
public class Phd2ConfigurationChangeMessage : Phd2ServerMessage { }

#endregion


}