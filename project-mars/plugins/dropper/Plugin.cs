using MarsClient.Plugins;
using System;
using System.Net;

class MarsDropper : Plugin {
    public override string Name => "Dropper";
    public override string Author => "Samuel Barrows";
    public override string Version => "v1";

    public static IDictionary<string, dynamic> PluginMain(string? path){
        //temp vars because params dont work for some reason
        //path = null;
        string url = "https://i.sparow.club/gondolas/squat_gondolas.jpg";
        if (path == null){
            path = Path.GetTempFileName();
        }
        if (url == null){
            IDictionary<string, dynamic> ReturnDict = new Dictionary<string, dynamic>();
            //ReturnDict.Add("ExitCode", 1);
            ReturnDict.Add("Message", $"You need to provide a url");
            return ReturnDict;
        }

        using (var client = new HttpClient()){
            try{
                Console.WriteLine(path);
                client.DownloadFile(url, path);

                IDictionary<string, dynamic> ReturnDict = new Dictionary<string, dynamic>();
                // return an exitcode, 0 for success, 1 for failure
                // ideally, we would return the exitcode of the process or dynamically set it based on the result of the process
                ReturnDict.Add("Status", $"File downloaded to {path}");
                ReturnDict.Add("Message", $"File downloaded to {path}");

                return ReturnDict;
            }catch (Exception e) {
                IDictionary<string, dynamic> ReturnDict = new Dictionary<string, dynamic>();
                //ReturnDict.Add("ExitCode", 1);
                ReturnDict.Add("Message", $"Could not download file. {e}");
                return ReturnDict;
            }
        }
    }
}
