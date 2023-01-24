// https://www.one-tab.com/page/uAWM4_M_RmiZrYeT3QwJpQ
using ChirpStack_Http;

const string ip = "192.168.43.114";
const int port = 8080;
const string deviceEui = "a81758fffe07340c";
const string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhcGlfa2V5X2lkIjoiNDlkYzY2M2YtY2Q2Zi00YmZkLTk1MmItNjBjOWNhZDYzNjZhIiwiYXVkIjoiYXMiLCJpc3MiOiJhcyIsIm5iZiI6MTY2NzgyODE1NCwic3ViIjoiYXBpX2tleSJ9.owSq7LHV0AY1Lms576MKatSzf0blCpLaIfbCeqBrh7g";
const string commandFilePath = "/Users/damo/RiderProjects/chirpstack_http/chirpstack_http/commands.txt";

var url = $"http://{ip}:{port}/api/devices/{deviceEui}/queue";

var con = new Connection(url, token);
var dlg = new DownLinkGenerator(commandFilePath);

// SplPer
var str = dlg.CreateDownlink("SplPer", 60, false);
con.Send(str);

var server = new HttpServer();
server.Run();





