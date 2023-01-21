using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;

namespace Qkmaxware.Astro.Control.Devices {

public abstract class AlpacaDevice : IDevice {
    public AlpacaConnection Connection {get; private set;}
    
    public string DeviceType {get; private set;}
    public int DeviceNumber {get; private set;}

    public string Name {get; private set;}

    public bool IsConnected {
        get {
            return Get<AlpacaValueResponse<bool>>($"{Connection.Server.Host}:{Connection.Server.Port}/{DeviceType}/{DeviceNumber}/connected").Value;
        }
    }

    public AlpacaDevice(AlpacaConnection conn, string deviceName, string deviceType, int deviceNumber) {
        this.Connection = conn;
        this.Name = deviceName;
        this.DeviceType = deviceType;
        this.DeviceNumber = deviceNumber;
    }

    protected static T Get<T>(string url) where T:AlpacaResponse {
        using (var client = new HttpClient()) {
            var task = client.GetAsync(url);
            task.Wait();

            var content = task.Result.Content.ReadAsStringAsync();
            content.Wait();
            var body = content.Result; 

            if (task.Result.StatusCode == System.Net.HttpStatusCode.OK) {
                return JsonSerializer.Deserialize<T>(body);
            } else {
                throw new HttpRequestException(body);
            }
        }
    }

    protected static T Put<T>(string url, params KeyValuePair<string,string>[] args) where T:AlpacaResponse {
        using (var client = new HttpClient()) {
            var dict = new Dictionary<string, string>();
            dict.Add("Content-Type", "application/x-www-form-urlencoded");
            foreach (var arg in args) {
                dict.Add(arg.Key, arg.Value);
            }

            var task = client.PutAsync(url, new FormUrlEncodedContent(dict));
            task.Wait();

            var content = task.Result.Content.ReadAsStringAsync();
            content.Wait();
            var body = content.Result; 

            if (task.Result.StatusCode == System.Net.HttpStatusCode.OK) {
                return JsonSerializer.Deserialize<T>(body);
            } else {
                throw new HttpRequestException(body);
            }
        }
    }

    private void setConnection(bool isConnected) {
        Put<AlpacaMethodResponse>($"{Connection.Server.Host}:{Connection.Server.Port}/{DeviceType}/{DeviceNumber}/connected", new KeyValuePair<string,string>("Connected", isConnected ? "true" : "false"));
    }
    public void Connect() {
        setConnection(true);
    }

    public void Disconnect() {
        setConnection(false);
    }
}

}