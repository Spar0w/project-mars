using MarsClient.Plugins;
using System;
using System.Diagnostics;

class Processes : Plugin {
    public override string Name => "Processes";
    public override string Author => "Samuel Barrows";
    public override string Version => "1";
    
    public static IDictionary<string, dynamic> PluginMain(string processPath){
        IDictionary<string, dynamic> ReturnDict = new Dictionary<string, dynamic>();

        string processName = processPath.Split(@"\").Last();
    
        Processes p = new Processes();
        int status = -1;
        string message = null;
        //List<string> message = null;
        try{
            (status, message) = p.StartProc(processPath);
        } catch (Exception e){
            ReturnDict.Add("Message", $"Exception: {e}");
            return ReturnDict;
        }
        if (status == 0){
            Console.WriteLine(message);
            ReturnDict.Add("Status", status);
            ReturnDict.Add("Message", message);
        } else {
            ReturnDict.Add("Message", message);
        }
        return ReturnDict;
    }

    public (int, string) StartProc(string processPath){
        //start a process
        Process proc = new Process();
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.FileName = processPath;
        proc.StartInfo.CreateNoWindow = false;

        try{
            proc.Start();
            return (0, $"{processPath} started successfully!");
        } catch (Exception e) {
            Console.WriteLine(e);
            return (1, $"Failed to start {processPath}. Exception: {e}");
        }
    }

}