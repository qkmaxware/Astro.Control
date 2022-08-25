using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Qkmaxware.Astro.Control {

public class AlpacaResponse {
    public int? ClientTransactionID {get; set;}
    public int? ServerTransactionID {get; set;}
    public int? ErrorNumber {get; set;}
    public string ErrorMessage {get; set;}
    public bool IsError => ErrorNumber.HasValue && ErrorNumber.Value != 0;
}

public class AlpacaValueResponse<T> : AlpacaResponse {
    public T Value {get; set;}
}

public class AlpacaConfiguredDevicesResponse : AlpacaValueResponse<AlpacaConfiguredDevicesResponse.ConfiguredDevice[]> {
    public class ConfiguredDevice {
        public string DeviceName {get; set;}
        public string DeviceType {get; set;}
        public int DeviceNumber {get; set;}
        public string UniqueID {get; set;}
    }
}
public class AlpacaMethodResponse : AlpacaResponse {}

public enum AlpacaImageSampleDataType {
    Unknown = 0,
    I16 = 1,
    I32 = 2,
    F64 = 3,
}

public class AlpacaImageArrayResponse : AlpacaResponse, IJsonOnDeserialized {
    public AlpacaImageSampleDataType Type {get; set;}
    public int Rank {get; set;}
    public JsonDocument Value {get; set;}

    public bool HasMonochromeData => MonochromePixels != null;
    public double[][] MonochromePixels {get; set;}
    public bool HasColourData => ColourPixels != null;
    public double[][][] ColourPixels {get; set;}

    [OnDeserialized]
    public void OnDeserialized () {
        if (Rank == 3) {
            MonochromePixels = null;
            ColourPixels = JsonSerializer.Deserialize<double[][][]>(Value);
        } else {
            MonochromePixels = JsonSerializer.Deserialize<double[][]>(Value);
            ColourPixels = null;
        }
    }
}

}