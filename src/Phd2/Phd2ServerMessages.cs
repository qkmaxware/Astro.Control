namespace Qkmaxware.Astro.Control {

/// <summary>
/// Base class for all messages sent from a PHD2 server
/// </summary>
public abstract class Phd2ServerMessage {
    public double Timestamp;
    public string HostName;
    public string InstanceId;
}

public class Phd2VersionMessage : Phd2ServerMessage {
    public string PhdVersion;
    public string PhdSubversion;
    public double MsgVersion;
    public bool OverlapSupport;
}

public class Phd2AppStateMessage : Phd2ServerMessage {
    public string State;
}

}