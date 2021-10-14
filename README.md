# Astro.Control
C# INDI Client Library


## Usage
1. Connect to INDI server
```cs
var server = new IndiServer("localhost");
IndiConnection conn;
if (server.TryConnect(out conn)) {
    // ...
}
```
2. Poll for devices
```cs
    conn.QueryProperties();
```
3. Fetch device and use a controller to abstract away the INDI properties
    - use a listener or delay between querying properties and fetching devices to give the server time to respond.
```cs
    var device = conn.GetDeviceByName("Celestron GPS");
    if (device != null) {
        var telescope = new IndiTelescopeController(device);
        telescope.SetSlewRate(SlewRate.Max);
        telescope.Rotate(Direction.North);
    }
```