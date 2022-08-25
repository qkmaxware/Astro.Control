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

}