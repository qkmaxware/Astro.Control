# Astro.Control
C# client library for control of astronomical equipment using remote protocols for interfacing with established astronomical control software including INDI and PHD2. 

- [Astro.Control](#astrocontrol)
  - [INDI](#indi)
    - [Basic Usage](#basic-usage)
    - [Example](#example)
      - [Satellite Tracking](#satellite-tracking)
  - [PHD2](#phd2)
    - [Basic Usage](#basic-usage-1)
  - [ASCOM Alpaca](#ascom-alpaca)
    - [Basic Usage](#basic-usage-2)

## INDI
![](https://indilib.org/templates/rt_antares/custom/images/logo/indi_logo.png)

[INDI](https://indilib.org/) is a simple XML-like communications protocol described for interactive and automated remote control of diverse instrumentation. A device that includes an INDI server such as [Astroberry](https://www.astroberry.io/) is required for the code in this libary to interface with. The server actually controls the equipment while a client running this codebase can send instructions to the server. 

### Basic Usage
1. Connect to the INDI server
```cs
var server = new IndiServer("localhost");
IndiConnection conn;
if (server.TryConnect(out conn)) {
    // ...
}
```
2. Configure connection and assign event listeners (optional)
```cs
    conn.Events.OnDeviceFound += (device) => { ... };
    conn.Events.OnPropertyDefined += (device, property, value) => { ... };
    conn.Events.OnPropertyChanged += (device, property, @old, @new) => { ... };
    // ...
```
3. Poll for devices and their respective properties
   - This task is asynchronous and non-blocking
   - use a listener or delay between querying properties and getting references to devices in order to give the server time to respond. 
   - Devices that are not connected to by the server may be missing important properties until connected to
```cs
    conn.QueryProperties();
```
4. Fetch device's reference to manipulate its INDI properties  
```cs
    IndiDevice device;

    // Fetch a specific device by its known name
    device = conn.Devices.GetDeviceByNameOrNull("Celestron GPS");
    // Fetch a device by its known properties, can use these to determine the "type" of device
    device = conn.Devices.AllDevices().Where((d) => d.Properties.Exists("TELESCOPE_INFO")).FirstOrDefault();
```
5. Connect the device on the INDI server if it is not already connected. 
   - this can trigger more properties to be pulled as not all devices publish all properties until the device is connected.
   - The `AutoConnectDevices` property of an IndiConnection can automatically do this when connectable devices are discovered.
```cs
    // Connect a single device
    device?.Connect();
    // Connect all devices on the server
    foreach (var d in conn.Devices.AllDevices())
        d?.Connect();
```
6. Manipulate the device using a controller to abstract away the manipulation of the device properties
```cs
    if (device != null) {
        var telescope = new IndiTelescopeController(device);
        telescope.SetSlewRate(SlewRate.Max);
        telescope.StartRotating(Direction.North);
    }
```
### Example
#### Satellite Tracking
```cs
// Connect to the server
var server = new IndiServer("localhost");
IndiConnection conn;
if (!server.TryConnect(out conn)) {
    throw new Exception("Failed to connect to the INDI server");
}

// Wait for device to be received by the client
var driver = "Telescope Simulator";
var device = conn.Devices.GetDeviceByNameOrNull(driver);
var retries = 10;
while (retries > 0 && device == null) {
    Thread.Sleep(TimeSpan.FromSeconds(0.5));
    device = conn.Devices.GetDeviceByNameOrNull(driver);
    retries--;
}
if (device == null) {
    throw new Exception("Could not find device driver");
}

// Make sure it is connected
retries = 10;
while (retries > 0 && !device.IsConnected) {
    device.Connect();
    Thread.Sleep(TimeSpan.FromSeconds(0.5));
    retries--;
}
if (!device.IsConnected) {
    throw new Exception("Failed to connect device driver");
}

// Use satellite ephemeris to compute ra & dec relative to observer (another library)
Angle ra = ...;
Angle dec = ...;

// Tell the telescope to track the coordinates
var controller = new IndiTelescopeController(device);
controller.Track(ra: ra, dec: dec, rate: TrackingRate.Sidereal);

// Satellites are fast, optionally recompute the RA and DEC and goto the new coordinates
var tracking = true;
while (tracking) {
    Thread.Sleep(TimeSpan.FromSeconds(1));
    ra = ...;
    dec = ...;
    controller.Track(ra: ra, dec: dec, rate: TrackingRate.Sidereal);
}
```

## PHD2
![](https://openphdguiding.org/wp-content/themes/openphd/images/header.png)

[PHD2](https://openphdguiding.org/) is a telescope guiding software for providing small and continuous adjustments to telescope mounts in order to keep a target imaging area as stable as possible for longer exposure imaging. It is known for its simplicity and ease of use. PHD2 includes a server mode and uses a JSON schema to communicate with a client library such as this one.

### Basic Usage
1. Connect to the PHD2 server
```cs
var server = new Phd2Server("localhost");
Phd2Server conn;
if (server.TryConnect(out conn)) {
    // ...
}
```
2. Configure connection and assign event listeners (optional)
```cs
    conn.Events.OnAppStateChanged += (state) => { ... };
    // ...
```
3. Call RPC methods to interact with the PHD2 server
```cs
    conn.Guide();
```

## ASCOM Alpaca
![](https://www.ascom-standards.org/Images/AlpacaLogo210.png)

[ASCOM Alpaca](https://www.ascom-standards.org/Developer/Alpaca.htm) is a rest based service used for cross platform control and monitoring of devices controlled via the ASCOM platform. 

### Basic Usage
1. Connect to the Alpaca server
```cs
var server = new AscomAlpacaServer("localhost");
AscomAlpacaConnection conn;
if (server.TryConnect(out conn)) {
    // ...
}
```