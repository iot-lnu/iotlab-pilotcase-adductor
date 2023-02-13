# Pilotcase - Adductor AB

A simple code base for interacting with ChirpStack's REST API. 
## Target Framework
- .net core SDK: 6.0

## Run the example
To run this code, start by initializing `ip`, `deviceEui`, `token`, and `commandFilePath`. Then run `program.cs` in your preferred .NET environment.  

```c#
using ChirpStack_Http;

const string ip = "Gateway_IP";
const int port = 8080;

// DEVEUI of the device, which we want to write downlinks to and read uplinks from
const string deviceEui = "DEVEUI"; 

// Chirpstack REST API JWT Token
const string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhcG...bCeqBrh7g";

// The path to the commands file
const string commandFilePath = "/Users/USER/RiderProjects/chirpstack_http/chirpstack_http/commands.txt";

var url = $"http://{ip}:{port}/api/devices/{deviceEui}/queue";

// Initiate a connection to the Chirpstack REST API
var con = new Connection(url, token);

// Create a downlink generator object to help us creating downlink hex-strings
var dlg = new DownLinkGenerator(commandFilePath);

// Generate a downlink that changes the sampling period to once every 60 seconds, and reboot
var str = dlg.CreateDownlink("SplPer", 60, true);

// Send the generated downlink hex-string
con.Send(str);

// Start a server that listens to all uplinks the chirpstack receives
var server = new HttpServer();
server.Run();
```