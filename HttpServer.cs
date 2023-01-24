using System.Net;
using System.Text.Json.Nodes;

namespace ChirpStack_Http;

public class HttpServer
{
    private const int Port = 8080;
    private HttpListener? _listener;
    
    /// <summary>
    /// Starts the server, which continuously listens to incoming HTTP-requests.
    /// These requests are sensor data pushed to this server from chirpstack.
    /// Chirpstack knows the address of this server from the HTTP-integration made on chirpstack. 
    /// </summary>
    private void Start()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://*:" + Port.ToString() + "/");
        _listener.Start();
        Receive();
    }

    /// <summary>
    /// Stops the server from listening to HTTP-requests on port 8080.
    /// </summary>
    private void Stop()
    {
        _listener?.Stop();
    }
    
    /// <summary>
    /// Begins asynchronously retrieving an incoming request.
    /// </summary>
    private void Receive()
    {
        _listener?.BeginGetContext(ListenerCallback, _listener);
    }
    
    /// <summary>
    /// Retrieves the incoming client request, e.g. the incoming sensor data from chirpstack.
    /// Every time a sensor sends a message to chirpstack, chirpstack will forward that message
    /// to this server and the message is retrieved and parsed in this method. 
    /// </summary>
    /// <param name="result"></param>
    private void ListenerCallback(IAsyncResult result)
    {
        if (_listener is not {IsListening: true}) return;
        var context = _listener.EndGetContext(result);
        var request = context.Request;

        // do something with the request
        Console.WriteLine($"{request.Url}");
        var arr = request.Url?.ToString().Split("=");
        var reqType = arr?[1];
        
        if (request.HasEntityBody && reqType == "up")
        {
            // This is an uplink message, which means that the message contain sensor data
            var body = request.InputStream;
            var encoding = request.ContentEncoding;
            var reader = new StreamReader(body, encoding);
            if (request.ContentType != null)
            {
                Console.WriteLine("Client data content type {0}", request.ContentType);
            }
            Console.WriteLine("Client data content length {0}", request.ContentLength64);

            Console.WriteLine("Start of data:");
            var s = reader.ReadToEnd();
            var json = JsonNode.Parse(s);
                
            Console.WriteLine(s);
            Console.WriteLine(json?["data"]);
            
            // Parse the data and save it to a file. 
            ParseAndSave(json?["data"]?.ToString());
                
            Console.WriteLine("End of data:");
            reader.Close();
            body.Close();
        } else if (request.HasEntityBody && reqType == "txack")
        {
            // This is a TX-ACK message, which indicate that the configuration sent to
            // the device was received and that the device was configured.
            Console.WriteLine("Device successfully configured!");
        } else if (request.HasEntityBody && reqType == "error")
        {
            Console.WriteLine("Error occured...");
        }
        
        // Start listening for new requests. 
        Receive();
    }
    
    /// <summary>
    /// Parses and saves the payload to a CSV-file. This method is a temporary method to demonstrate how data
    /// from two sensors with 10 samples each can be parsed. This method is only tested on the pulse-counter
    /// setting on the ELT-2 sensor with two switches connected to it.
    /// </summary>
    /// <param name="payload"></param>
    private static void ParseAndSave(string? payload)
    {
        Console.WriteLine(payload);
        if (payload != null)
        {
            byte[] bytes = Convert.FromBase64String(payload);
            Console.WriteLine(bytes);
        
            var hex = BitConverter.ToString(bytes);
            Console.WriteLine(hex);
        
            payload = hex.Replace("-", "");
            Console.WriteLine(payload);
            payload = ReversePayload(payload, 12);
            Console.WriteLine(payload);
            
            var data = "Timestamp, Sw 1, Sw 2\n";
            var str = $"{"Time",5},{"Sw 1",5},{"Sw 2",5}";
            Console.WriteLine(str);
            var counter = 6;
            for (var i = 0; i < payload.Length; i+=12)
            {
                var sample = payload.Substring(i, 12);
                var switchOne = Convert.ToInt32(sample.Substring(sample.Length - 4, 4), 16);
                var switchTwo = Convert.ToInt32(sample.Substring(2, 4), 16);
                data += counter + "," + switchOne + "," + switchTwo + "\n";
                
                Console.WriteLine($"{counter,5},{switchOne,5},{switchTwo,5}");
                counter += 6;
                
            }
            const string filePath = "/Users/damo/RiderProjects/chirpstack_http/chirpstack_http/File.csv";
            File.WriteAllText(filePath, data);
        }
    }
    
    /// <summary>
    /// Runs the server and allows the developer to shut it down by pressing CTRL+C. 
    /// </summary>
    public void Run()
    {
        // https://thoughtbot.com/blog/using-httplistener-to-build-a-http-server-in-csharp
        var keepRunning = true;
        Console.CancelKeyPress += delegate(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            keepRunning = false;
        };

        Console.WriteLine("Starting HTTP listener...");

        Start();

        while (keepRunning) { }

        Stop();

        Console.WriteLine("Exiting gracefully...");
    }
    
    /// <summary>
    /// Reverses the payload, since the ELT-2 appends the latest sample to the beginning of the queue.
    /// Example: From SAMPLE_2,SAMPLE_1 to SAMPLE_1,SAMPLE_2. 
    /// </summary>
    /// <param name="payload">The payload in hexadecimal format</param>
    /// <param name="sizeOfSample">The size of each sample in bytes, where each byte is represented by two hex values, e.g. FF or EF</param>
    /// <returns></returns>
    private static string ReversePayload(string payload, int sizeOfSample)
    {
        var s = "";

        for (var i = payload.Length; i > 0; i-=sizeOfSample)
        {
            var sample = payload.Substring(i - sizeOfSample, sizeOfSample);
            s += sample;
        }
        return s;
    }
}
