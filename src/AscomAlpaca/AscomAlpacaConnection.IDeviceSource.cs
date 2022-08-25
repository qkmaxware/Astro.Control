using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Qkmaxware.Astro.Control.Devices;

namespace Qkmaxware.Astro.Control {

public partial class AlpacaConnection : IDeviceSource {

    private IEnumerable<AlpacaConfiguredDevicesResponse.ConfiguredDevice> ListConfiguredDevices() {
        using (var client = new HttpClient()) {
            var task = client.GetAsync($"{Server.Host}:{Server.Port}/management/v1/configureddevices");
            task.Wait();

            var content = task.Result.Content.ReadAsStringAsync();
            content.Wait();
            var body = content.Result; 

            if (task.Result.StatusCode == System.Net.HttpStatusCode.OK) {
                var devices = JsonSerializer.Deserialize<AlpacaConfiguredDevicesResponse>(body);
                if (devices.IsError || devices.Value == null) {
                    return Enumerable.Empty<AlpacaConfiguredDevicesResponse.ConfiguredDevice>();
                } else {
                    return devices.Value;
                }
            } else {
                return Enumerable.Empty<AlpacaConfiguredDevicesResponse.ConfiguredDevice>();
            }
        }
    }

    public IEnumerable<IDevice> EnumerateAllDevices() {
        foreach (var raw in this.ListConfiguredDevices()) {
            switch (raw.DeviceType) {
                case "Telescope":
                    yield return new AlpacaTelescope(this, raw);
                    break;
                case "Camera":
                    yield return new AlpacaCamera(this, raw);
                    break;
                case "Dome":
                    yield return new AlpacaDome(this, raw);
                    break;
                case "Focuser":
                    yield return new AlpacaFocuser(this, raw);
                    break;
                case "FilterWheel":
                    yield return new AlpacaFilterWheel(this, raw);
                    break;
                default:
                    // IDK what kind of device this is, skip it
                    continue;
            }
        }
    }

    public IEnumerable<IGuider> EnumerateAutoGuiders() => EnumerateAllDevices().OfType<IGuider>();

    public IEnumerable<ICamera> EnumerateCameras() => EnumerateAllDevices().OfType<ICamera>();

    public IEnumerable<IDome> EnumerateDomes() => EnumerateAllDevices().OfType<IDome>();

    public IEnumerable<IFilterWheel> EnumerateFilterWheels() => EnumerateAllDevices().OfType<IFilterWheel>();

    public IEnumerable<IFocuser> EnumerateFocusers() => EnumerateAllDevices().OfType<IFocuser>();

    public IEnumerable<ITelescope> EnumerateTelescopes() => EnumerateAllDevices().OfType<ITelescope>();
}

}