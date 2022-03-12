using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Qkmaxware.Astro.Control {

public class JsonRpcClient : BaseTcpConnection {

    public class Request {
        public string jsonrpc => "2.0";
        public string method {get; set;}
        public string id {get; private set;}
        public object @params {get; set;}

        public Request(string method, object args = null) {
            this.method = method;
            this.@params = args;
            this.id = new Guid().ToString();
        }

        internal volatile Response response;
    }

    public class Error {
        public int code {get; set;}
        public string message {get; set;}
        public object data {get; set;}
    }

    public class Response {
        public object result {get; set;}
        public Error error {get; set;}
        public string id {get; set;} 

        public bool IsError() => error != null;

    }

    /// <summary>
    /// Output stream to print all received messages to
    /// </summary>
    public TextWriter OutputStream;

    public JsonRpcClient(IServerSpecification server) : base(server) {}

    private static int bufferSize = 1000;
    protected override void AsyncRead() {
        int depth = 0;
        bool inQuotes = false;
        char lastChar = default(char);

        StringBuilder buffer = new StringBuilder(bufferSize);

        while (this.IsConnected) {
            // Try to read in the next character
            char b = (char)this.inputStream.ReadByte();
            if (b < 0) {
                this.Disconnect();
                return;
            }
            buffer.Append(b);

            // Track if we enter or exist an object
            if (b == '"' && lastChar != '\\') {
                inQuotes = !inQuotes;
            } else if (b == '{' && !inQuotes) {
                depth++;
            } else if (b == '}' && !inQuotes) {
                depth--;
            }

            // When "outer-most" object is closed
            if (depth == 0 && buffer.Length > 0) {
                try {
                    var json = buffer.ToString();                   // Get json
                    if (this.OutputStream != null)
                        this.OutputStream.Write(json);              // Redirect received messages
                    var obj = JsonDocument.Parse(json);             // Parse the object
                    Task.Run(() => process(obj.RootElement));       // Process the message on a new thread
                    buffer.Clear();                                 // Clear the buffer to prepare for future objects
                } catch {}
            }
        }
    }

    private void process(JsonElement node) {
        if (node.ValueKind != JsonValueKind.Object)
            return; // Messages sent from PHD2 are objects

        // Check if is an event message or an RPC call
        JsonElement tag;
        if (node.TryGetProperty("jsonrpc", out tag)) {
            processRpcResponse(node);
        } else {
            processNonRpcResponse(node);
        }
    }

    protected virtual void processNonRpcResponse(JsonElement response) {}
    
    private void processRpcResponse(JsonElement rawResponse) {
        var res = JsonSerializer.Deserialize<Response>(rawResponse.GetRawText());
        Request req;
        if (this.activeRequests.TryRemove(res.id, out req)) {
            // We found a match, handle it with a callback
            req.response = res;
        }
    }

    private ConcurrentDictionary<string, Request> activeRequests = new ConcurrentDictionary<string, Request>();
    protected void sendRpcRequest(Request request) {
        if (request == null)
            return;

        var json = JsonSerializer.Serialize(request);
        if (this.activeRequests.TryAdd(request.id, request)) {
            this.Write(json);
        }
    }

    private Response sendAndWait(Request request) {
        sendRpcRequest(request);
        
        var didTimeOut = false;
        var timeoutAt = DateTime.Now + TimeSpan.FromMinutes(1);
        while(request.response == null) {
            // Spin wait
            // No response yet, check timeout
            if (DateTime.Now > timeoutAt) {
                didTimeOut = true;
                break;
            }
        }
        
        if (didTimeOut || request.response == null) {
            return new Response {
                error = new Error {
                    code = 0,
                    message = "The request has timed out"
                }
            };
        } else {
            return request.response;
        }
    }
    public Task<Response> Send(Request request) {
        return Task.Run(() => sendAndWait(request));
    }
}

}