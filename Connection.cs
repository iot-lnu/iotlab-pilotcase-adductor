using System.Net.Http.Headers;

namespace ChirpStack_Http;
using System.Text;

public class Connection
{
    private readonly HttpClient _client;
    private readonly string _url;
    
    public Connection(string url, string token = "")
    {
        _url = url;
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
    
    /// <summary>
    /// Takes a hex-string and converts it to an 8-bit unsigned integer.
    /// For example, 3E would be converted to 62. 
    /// </summary>
    /// <param name="inputHex">The input in hexadecimal format as string</param>
    /// <returns>The resulting byte array</returns>
    private static byte[] HexStringToByteArray(string inputHex)
    {
        byte[] resultantArray = new byte[inputHex.Length / 2];
        for (var i = 0; i < resultantArray.Length; i++)
        {
            resultantArray[i] = Convert.ToByte(inputHex.Substring(i * 2, 2), 16);
        }
        return resultantArray;
    }

    /// <summary>
    /// Takes a command in hex and sends it to the chirpstack using POST-request.
    /// </summary>
    /// <param name="commandInHexString">The command in hexadecimal format as string</param>
    public async void Send(string commandInHexString)
    {
        byte[] d = HexStringToByteArray(commandInHexString);
        string base64 = Convert.ToBase64String(d);
        var json = $@"{{
          ""deviceQueueItem"": {{ 
             ""confirmed"": false,
             ""data"": ""{base64}"",
             ""fPort"": 6
	        }}
        }}";
        
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(_url, data);
        
        Console.WriteLine("Status Code: " + response.StatusCode);
    }
}