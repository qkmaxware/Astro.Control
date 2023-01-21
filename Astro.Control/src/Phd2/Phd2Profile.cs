using System;
using Qkmaxware.Astro.Control.Devices;

namespace Qkmaxware.Astro.Control {

public class Phd2Profile : IGuider {
    /// <summary>
    /// Id of the profile on the PHD2 server
    /// </summary>
    /// <value>id</value>
    public int Id {get; private set;}
    /// <summary>
    /// Name of the device being controlled
    /// </summary>
    /// <value>name</value>
    public string Name {get; private set;}
    /// <summary>
    /// PHD2 connection the profile lives on
    /// </summary>
    /// <value>connection</value>
    public Phd2Connection Source {get; private set;}
    
    public Phd2Profile(int id, string name, Phd2Connection connection) {
        this.Id = id;
        this.Name = name;
        this.Source = connection;
    }

    /// <summary>
    /// Connection status of the device
    /// </summary>
    /// <value>true if the device is connected and accessible</value>
    public bool IsConnected => true;
    /// <summary>
    /// Connect the device
    /// </summary>
    public void Connect() {}
    /// <summary>
    /// Disconnect the device
    /// </summary>
    public void Disconnect() {}


    /// <summary>
    /// Check if the auto-guider is currently guiding
    /// </summary>
    /// <value>stateof the auto-guider</value>
    public GuideState State => isGuiding ? GuideState.Guiding : (isInitializing ? GuideState.Calibrating : GuideState.Idle);
    private bool isGuiding => Source != null && Source.State == "Guiding";
    private static string[] initialization_states = new string[]{"Looping", "Calibrating", "Selected"};
    private bool isInitializing => Source != null && Array.IndexOf(initialization_states, Source.State) != -1;

    /// <summary>
    /// Begin guiding with the auto-guider
    /// </summary>
    public void StartGuiding() {
        if (Source == null)
            return;

        // Send stop guiding
        Source.StopCapture();
        // Select this profile
        Source.SelectProfile(this);
        // Send guide
        Source.Guide();
    }
    
    /// <summary>
    /// Stop the auto-guider
    /// </summary>
    public void StopGuiding() {
        if (Source == null)
            return;

        // Send stop guiding
        Source.StopCapture();
    }
}

}