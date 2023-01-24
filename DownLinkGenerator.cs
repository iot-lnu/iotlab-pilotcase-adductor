namespace ChirpStack_Http;


public class DownLinkGenerator
{
    private List<Setting> settings;
    
    /// <summary>
    /// The constructor. It parses the given file and stores each line as
    /// an individual setting. The each line in the file should consist of
    /// three properties: ID, NAME and BYTES where ID is the hexadecimal
    /// value of the command, NAME is name of the command according to elsys'
    /// command naming convention and BYTES is the size of the command, e.g.
    /// that hold the value of the configuration.
    /// 
    /// Explained payload
    /// Raw bytes (hex)	    Part	        Value	    Description
    /// 3E	                Header		                Must always be present
    /// 05	                Total length	5	        Length of payload excluding header and length (2 bytes)
    /// 140000003C	        SplPer	        60	        5 bytes
    /// Resulting payload
    /// 3E05140000003C
    /// </summary>
    /// <param name="settingsFilePath">The path to the file containing the supported configurations</param>
    public DownLinkGenerator(string settingsFilePath)
    {
        IEnumerable<string> lines = ReadLines(settingsFilePath);
        settings = new List<Setting>();

        foreach (string line in lines)
        {
            var values = line.Split(' ');
            var id = strToInt(values[0], true);
            var name = values[1];
            var bytes = strToInt(values[2], false);
            settings.Add(new Setting(id, name, bytes));
        }
    }

    private static IEnumerable<string> ReadLines(string settingFilePath)
    {
        return File.ReadAllLines($@"{settingFilePath}");
    }

    private int strToInt(string str, bool hex)
    {
        var res = -1;
        var numBase = hex ? 16 : 10;

        try
        {
            res = Convert.ToInt32(str, numBase);
        }
        catch (FormatException e)
        {
            Console.WriteLine(e.Message);
        }
        return res;
    }
    
    /// <summary>
    /// Creates a hexadecimal configuration string using the given command name and value.
    /// </summary>
    /// <param name="name">The name of the command, e.g. SplPer for Timebase</param>
    /// <param name="value">The actual value of the command</param>
    /// <param name="reboot">If the device should reboot once it is configured</param>
    /// <returns></returns>
    public string CreateDownlink(string name, int value, bool reboot=false)
    {
        Setting? setting = settings.Find(r => r.Name.ToLower() == name.ToLower());
        
        if (setting != null)
        {
            var header = "3E";
            var payload = setting.Id.ToString($"X{2}");
            payload += value.ToString($"X{setting.Bytes * 2}");

            var length = payload.Length / 2;

            if (reboot)
            {
                length += 1;
                payload += "FE";
            }

            var payloadLength = length.ToString($"x{2}");

            var downlink = header + payloadLength + payload;
            
            Console.WriteLine("Downlink: " + downlink);

            return downlink;

        }
        
        return "";
    }
}