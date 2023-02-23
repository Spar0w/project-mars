using MarsClient.Plugins;
using System;
using System.Diagnostics;

class Processes : Plugin {
    public override string Name => "Stop Process";
    public override string Author => "Samuel Barrows";
    public override string Version => "1";
    
    public static IDictionary<string, dynamic> PluginMain(string pr){
        IDictionary<string, dynamic> ReturnDict = new Dictionary<string, dynamic>();

        string processName = pr.Split(@"\").Last();
    
        Processes p = new Processes();
        int status = -1;
        string message = null;
        //List<string> message = null;
        try{
            (status, message) = p.StopProc(pr);
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

    public (int, string) StopProc(string p){
        //try to get a process
        //attempt to get the process using the ID.
        //the user may have put in the name of the process, so we try that next
        Process proc = null;
        try{
            int pId = Int32.Parse(p);
            proc = Process.GetProcessById(pId);
        } catch (FormatException){
            Process[] procs = Process.GetProcessesByName(p);
            //just get the first one who cares
            proc = procs[0];
        }

        //kill the process and respond to the server
        try{
            proc.Kill();
            return (0, $"{p} stopped successfully!");
        } catch (Exception e) {
            Console.WriteLine(e);
            return (1, $"Failed to stop {p}. Exception: {e}");
        }
    }

}