namespace Qkmaxware.Astro.Control {

public class Phd2Profile : Devices.IGuider {
    
    public int Id {get; private set;}
    public string Name {get; private set;}

    public Phd2Connection Source {get; private set;}
    
    public Phd2Profile(int id, string name, Phd2Connection connection) {
        this.Id = id;
        this.Name = name;
        this.Source = connection;
    }

    public void BeginGuiding() {
        if (Source == null)
            return;

        // Send stop guiding
        Source.StopCapture();
        // Select this profile
        Source.SelectProfile(this);
        // Send guide
        Source.Guide();
    }
}

}