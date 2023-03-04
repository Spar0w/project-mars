using MarsClient.Plugins;
using System.Diagnostics;
using System.IO;

class MarsUploader : Plugin
{
    public override string Name => "Uploader";
    public override string Author => "Samuel Barrows and Damir Hajrovic";
    public override string Version => "v1";

    [CommandAttributes("SendFileToServer", arguments = 1, description = "Pulls a file from the client and sends it to the server.")]
    public static IDictionary<string, dynamic> SendFileToServer(string filepath)
    {
        IDictionary<string, dynamic> ReturnArray = new Dictionary<string, dynamic>();
        //string temppath = Path.GetTempFileName();

        if (File.Exists(filepath))
        {
            ReturnArray.Add("Status", "File found.");
            ReturnArray.Add("Message", "File found. Sent to Client for processing.");
            string[] FileArray = { filepath };
            ReturnArray.Add("Files", FileArray);
            return ReturnArray;
        }
        else
        {
            ReturnArray.Add("Message", "The specified file was not found.");
            return ReturnArray;
        }

        
    }

}